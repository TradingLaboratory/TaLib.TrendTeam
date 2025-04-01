//Файл TA_Bbands.cs
namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Полосы Боллинджера (Скользящие исследования)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>- Если не указан, обрабатывается весь массив <paramref name="inReal"/>.</para>
    /// </param>
    /// <param name="outRealUpperBand">
    /// Массив, содержащий ТОЛЬКО валидные значения верхней полосы.
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outRealUpperBand[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outRealMiddleBand">
    /// Массив, содержащий ТОЛЬКО валидные значения средней полосы.
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outRealMiddleBand[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outRealLowerBand">
    /// Массив, содержащий ТОЛЬКО валидные значения нижней полосы.
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outRealLowerBand[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>- <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outRealUpperBand"/>, <paramref name="outRealMiddleBand"/> и <paramref name="outRealLowerBand"/>.</para>
    /// <para>- <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outRealUpperBand"/>, <paramref name="outRealMiddleBand"/> и <paramref name="outRealLowerBand"/>.</para>
    /// <para>- Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.</para>
    /// <para>- Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.</para>
    /// </param>
    /// <param name="optInTimePeriod">Период расчета.</param>
    /// <param name="optInNbDevUp">
    /// Множитель стандартного отклонения для верхней полосы:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Большие значения увеличивают расстояние от средней полосы, снижая чувствительность к незначительным колебаниям цены.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшие значения сокращают расстояние, повышая реакцию на изменения цены.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>Значения выше 5 редко используются из-за потери практической значимости.</para>
    /// </param>
    /// <param name="optInNbDevDn">
    /// Множитель стандартного отклонения для нижней полосы:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Большие значения увеличивают расстояние от средней полосы, снижая вероятность сигналов перепроданности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшие значения сокращают расстояние, повышая чувствительность к снижению цены.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>Значения выше 5 редко используются из-за потери практической значимости.</para>
    /// </param>
    /// <param name="optInMAType">Тип скользящей средней.</param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата <see cref="Core.RetCode"/>.
    /// <para>Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или код ошибки.</para>
    /// </returns>
    /// <remarks>
    /// Полосы Боллинджера — индикатор на основе волатильности, использующий скользящую среднюю и стандартные отклонения
    /// для формирования верхней и нижней "полос" вокруг цены. Полосы расширяются/сужаются при изменении волатильности,
    /// предоставляя информацию о потенциальных зонах перекупленности/перепроданности, периодах консолидации и прорывах.
    /// <para>Используется в торговых стратегиях для идентификации возможностей прорыва, продолжения тренда или разворота.</para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Средняя полоса = скользящая средняя входных значений за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Стандартное отклонение входных значений за тот же период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Верхняя/нижняя полосы рассчитываются по формулам:
    /// <code>
    /// Upper Band = Middle Band + (Standard Deviation * NbDevUp)
    /// Lower Band = Middle Band - (Standard Deviation * NbDevDn)
    /// </code>
    ///       <para>Где:</para>
    ///       <list type="bullet">
    ///         <item><description>Middle Band — средняя полоса (скользящая средняя)</description></item>
    ///         <item><description>Standard Deviation — стандартное отклонение</description></item>
    ///         <item><description>NbDevUp/NbDevDn — множители для верхней/нижней полос</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Приближение цены к верхней полосе указывает на возможную перекупленность (риск коррекции).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Приближение цены к нижней полосе указывает на возможную перепроданность (потенциал роста).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сужение полос (низкая волатильность) предшествует прорывам. Расширение (высокая волатильность) — подтверждает тренд.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>


    [PublicAPI]
    public static Core.RetCode Bbands<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outRealUpperBand,
        Span<T> outRealMiddleBand,
        Span<T> outRealLowerBand,
        out Range outRange,
        int optInTimePeriod = 5,
        double optInNbDevUp = 2.0,
        double optInNbDevDn = 2.0,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        BbandsImpl(inReal, inRange, outRealUpperBand, outRealMiddleBand, outRealLowerBand, out outRange, optInTimePeriod, optInNbDevUp,
            optInNbDevDn, optInMAType);

    /// <summary>
    /// Возвращает lookback-период для <see cref="Bbands{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета.</param>
    /// <param name="optInMAType">Тип скользящей средней.</param>
    /// <returns>Количество периодов, необходимых для расчета первого валидного значения.</returns>
    [PublicAPI]
    public static int BbandsLookback(int optInTimePeriod = 5, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInTimePeriod < 2 ? -1 : MaLookback(optInTimePeriod, optInMAType);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Bbands<T>(
        T[] inReal,
        Range inRange,
        T[] outRealUpperBand,
        T[] outRealMiddleBand,
        T[] outRealLowerBand,
        out Range outRange,
        int optInTimePeriod = 5,
        double optInNbDevUp = 2.0,
        double optInNbDevDn = 2.0,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        BbandsImpl<T>(inReal, inRange, outRealUpperBand, outRealMiddleBand, outRealLowerBand, out outRange, optInTimePeriod, optInNbDevUp,
            optInNbDevDn, optInMAType);

    private static Core.RetCode BbandsImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outRealUpperBand,
        Span<T> outRealMiddleBand,
        Span<T> outRealLowerBand,
        out Range outRange,
        int optInTimePeriod,
        double optInNbDevUp,
        double optInNbDevDn,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (_, endIdx) = rangeIndices;
        if (optInTimePeriod < 2 || optInNbDevUp < 0 || optInNbDevDn < 0)
        {
            return Core.RetCode.BadParam;
        }

        // Определение двух временных буферов для оптимизации памяти
        Span<T> tempBuffer1 = outRealMiddleBand;
        Span<T> tempBuffer2;
        if (inReal == outRealUpperBand)
        {
            tempBuffer2 = outRealLowerBand;
        }
        else
        {
            tempBuffer2 = outRealUpperBand;
            if (inReal == outRealMiddleBand)
            {
                tempBuffer1 = outRealLowerBand;
            }
        }

        // Проверка на наложение входных/выходных буферов
        if (tempBuffer1 == inReal || tempBuffer2 == inReal)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет средней полосы (скользящая средняя)
        var retCode = MaImpl(inReal, inRange, tempBuffer1, out outRange, optInTimePeriod, optInMAType);
        if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
        {
            return retCode;
        }

        var nbElement = outRange.End.Value - outRange.Start.Value;

        // Оптимизация для SMA: повторное использование расчетов
        if (optInMAType == Core.MAType.Sma)
        {
            CalcStandardDeviation(inReal, tempBuffer1, outRange, tempBuffer2, optInTimePeriod);
        }
        else
        {
            // Расчет стандартного отклонения для других типов MA
            retCode = StdDevImpl(inReal, new Range(outRange.Start.Value, endIdx), tempBuffer2, out outRange, optInTimePeriod, 1.0);
            if (retCode != Core.RetCode.Success)
            {
                outRange = Range.EndAt(0);
                return retCode;
            }
        }

        // Копирование MA в среднюю полосу (если буферы различаются)
        if (tempBuffer1 != outRealMiddleBand)
        {
            tempBuffer1[..nbElement].CopyTo(outRealMiddleBand);
        }

        var nbDevUp = T.CreateChecked(optInNbDevUp);
        var nbDevDn = T.CreateChecked(optInNbDevDn);

        // Расчет верхней/нижней полос в зависимости от множителей
        if (optInNbDevUp.Equals(optInNbDevDn))
        {
            CalcEqualBands(tempBuffer2, outRealMiddleBand, outRealUpperBand, outRealLowerBand, nbElement, nbDevUp);
        }
        else
        {
            CalcDistinctBands(tempBuffer2, outRealMiddleBand, outRealUpperBand, outRealLowerBand, nbElement, nbDevUp, nbDevDn);
        }

        return Core.RetCode.Success;
    }

    private static void CalcStandardDeviation<T>(
        ReadOnlySpan<T> real,
        ReadOnlySpan<T> movAvg,
        Range movAvgRange,
        Span<T> outReal,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        var startSum = movAvgRange.Start.Value + 1 - optInTimePeriod;
        var endSum = movAvgRange.Start.Value;
        var periodTotal2 = T.Zero;

        // Начальная сумма квадратов для первого расчета
        for (var outIdx = startSum; outIdx < endSum; outIdx++)
        {
            var tempReal = real[outIdx];
            tempReal *= tempReal;
            periodTotal2 += tempReal;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);
        for (var outIdx = 0; outIdx < movAvgRange.End.Value - movAvgRange.Start.Value; outIdx++, startSum++, endSum++)
        {
            var tempReal = real[endSum];
            tempReal *= tempReal;
            periodTotal2 += tempReal;

            // Среднее квадратов минус квадрат среднего (дисперсия)
            var meanValue2 = periodTotal2 / timePeriod;
            tempReal = real[startSum];
            tempReal *= tempReal;
            periodTotal2 -= tempReal;

            tempReal = movAvg[outIdx];
            tempReal *= tempReal;
            meanValue2 -= tempReal;

            // Квадратный корень дисперсии = стандартное отклонение
            outReal[outIdx] = meanValue2 > T.Zero ? T.Sqrt(meanValue2) : T.Zero;
        }
    }

    private static void CalcEqualBands<T>(
        ReadOnlySpan<T> tempBuffer,
        ReadOnlySpan<T> realMiddleBand,
        Span<T> realUpperBand,
        Span<T> realLowerBand,
        int nbElement,
        T nbDevUp) where T : IFloatingPointIeee754<T>
    {
        if (nbDevUp.Equals(T.One))
        {
            // Прямое сложение/вычитание стандартного отклонения
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = tempBuffer[i];
                var tempReal2 = realMiddleBand[i];
                realUpperBand[i] = tempReal2 + tempReal;
                realLowerBand[i] = tempReal2 - tempReal;
            }
        }
        else
        {
            // Умножение стандартного отклонения на общий множитель
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = tempBuffer[i] * nbDevUp;
                var tempReal2 = realMiddleBand[i];
                realUpperBand[i] = tempReal2 + tempReal;
                realLowerBand[i] = tempReal2 - tempReal;
            }
        }
    }

    private static void CalcDistinctBands<T>(
        ReadOnlySpan<T> tempBuffer,
        ReadOnlySpan<T> realMiddleBand,
        Span<T> realUpperBand,
        Span<T> realLowerBand,
        int nbElement,
        T nbDevUp,
        T nbDevDn) where T : IFloatingPointIeee754<T>
    {
        if (nbDevUp.Equals(T.One))
        {
            // Только нижняя полоса использует множитель
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = tempBuffer[i];
                var tempReal2 = realMiddleBand[i];
                realUpperBand[i] = tempReal2 + tempReal;
                realLowerBand[i] = tempReal2 - tempReal * nbDevDn;
            }
        }
        else if (nbDevDn.Equals(T.One))
        {
            // Только верхняя полоса использует множитель
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = tempBuffer[i];
                var tempReal2 = realMiddleBand[i];
                realLowerBand[i] = tempReal2 - tempReal;
                realUpperBand[i] = tempReal2 + tempReal * nbDevUp;
            }
        }
        else
        {
            // Раздельные множители для верхней и нижней полос
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = tempBuffer[i];
                var tempReal2 = realMiddleBand[i];
                realUpperBand[i] = tempReal2 + tempReal * nbDevUp;
                realLowerBand[i] = tempReal2 - tempReal * nbDevDn;
            }
        }
    }
}
