// CalcMACD.cs
namespace TALib;
internal static partial class FunctionHelpers
{

    /// <summary>
    /// Рассчитывает индикатор MACD (Moving Average Convergence Divergence) с сигнальной линией и гистограммой.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (например, цены закрытия)</param>
    /// <param name="inRange">Диапазон входных данных для обработки</param>
    /// <param name="outMacd">Массив для значений основной линии MACD (Fast EMA - Slow EMA)</param>
    /// <param name="outMacdSignal">Массив для значений сигнальной линии (EMA от MACD)</param>
    /// <param name="outMacdHist">Массив для гистограммы MACD (MACD - Signal)</param>
    /// <param name="outRange">Диапазон выходных данных (индексы первого и последнего валидных значений)</param>
    /// <param name="optInFastPeriod">Период быстрой EMA (по умолчанию 12)</param>
    /// <param name="optInSlowPeriod">Период медленной EMA (по умолчанию 26)</param>
    /// <param name="optInSignalPeriod">Период сигнальной линии (по умолчанию 9)</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcMACD<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMacd,
        Span<T> outMacdSignal,
        Span<T> outMacdHist,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        int optInSignalPeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Проверка и корректировка периодов: медленный период должен быть больше быстрого
        if (optInSlowPeriod < optInFastPeriod)
        {
            (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
        }

        T k1; // Коэффициент сглаживания для медленной EMA (k = 2/(period + 1))
        T k2; // Коэффициент сглаживания для быстрой EMA

        // Обработка стандартного случая MACD(26,12,9)
        if (optInSlowPeriod != 0)
        {
            k1 = T.CreateChecked(2) / (T.CreateChecked(optInSlowPeriod) + T.One); // k1 = 2/(period + 1)
        }
        else
        {
            optInSlowPeriod = 26; // Установка стандартного значения 26
            k1 = T.CreateChecked(0.075); // k1 = 2/(26+1) ≈ 0.075
        }

        if (optInFastPeriod != 0)
        {
            k2 = T.CreateChecked(2) / (T.CreateChecked(optInFastPeriod) + T.One); // k2 = 2/(period + 1)
        }
        else
        {
            optInFastPeriod = 12; // Установка стандартного значения 12
            k2 = T.CreateChecked(0.15); // k2 = 2/(12+1) ≈ 0.15
        }

        // Расчет lookback-периодов
        var lookbackSignal = Functions.EmaLookback(optInSignalPeriod); // Для сигнальной линии
        var lookbackTotal = Functions.MacdLookback(optInFastPeriod, optInSlowPeriod, optInSignalPeriod); // Общий lookback
        startIdx = Math.Max(startIdx, lookbackTotal); // Корректировка начального индекса

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделение временных буферов для EMA
        var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
        Span<T> fastEMABuffer = new T[tempInteger]; // Буфер для быстрой EMA
        Span<T> slowEMABuffer = new T[tempInteger]; // Буфер для медленной EMA

        /* Расчет медленной EMA
         * 
         * Сдвигаем начальный индекс для обеспечения данных, необходимых для сигнального периода.
         * Это гарантирует, что выходные данные будут начинаться с запрошенного 'startIdx'.
         */
        tempInteger = startIdx - lookbackSignal;
        var retCode = CalcExponentialMA(inReal, new Range(tempInteger, endIdx), slowEMABuffer, out var outRange1, optInSlowPeriod, k1);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Расчет быстрой EMA
        retCode = CalcExponentialMA(inReal, new Range(tempInteger, endIdx), fastEMABuffer, out _, optInFastPeriod, k2);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement1 = outRange1.End.Value - outRange1.Start.Value; // Количество элементов в буферах EMA

        // Расчет основной линии MACD: Fast EMA - Slow EMA
        for (var i = 0; i < nbElement1; i++)
        {
            fastEMABuffer[i] -= slowEMABuffer[i];
        }

        // Копирование результата в выходной массив MACD
        fastEMABuffer.Slice(lookbackSignal, endIdx - startIdx + 1).CopyTo(outMacd);

        // Расчет сигнальной линии (EMA от MACD)
        var k1Period = T.CreateChecked(2) / (T.CreateChecked(optInSignalPeriod) + T.One); // Коэффициент для сигнальной EMA
        retCode = CalcExponentialMA(fastEMABuffer, Range.EndAt(nbElement1 - 1), outMacdSignal, out var outRange2, optInSignalPeriod, k1Period);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement2 = outRange2.End.Value - outRange2.Start.Value; // Количество элементов сигнальной линии

        // Расчет гистограммы: MACD - Signal
        for (var i = 0; i < nbElement2; i++)
        {
            outMacdHist[i] = outMacd[i] - outMacdSignal[i];
        }

        outRange = new Range(startIdx, startIdx + nbElement2); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

}
