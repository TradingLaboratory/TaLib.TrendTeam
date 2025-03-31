// CalcPriceOscillator.cs
namespace TALib;
internal static partial class FunctionHelpers
{

    /// <summary>
    /// Рассчитывает ценовой осциллятор на основе разности/отношения двух скользящих средних.
    /// Поддерживает режимы вывода: абсолютные значения (Fast MA - Slow MA) или процентное отношение ((Fast MA - Slow MA)/Slow MA * 100).
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (например, цены закрытия)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения осциллятора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastPeriod">Период быстрой скользящей средней (должен быть меньше медленного периода)</param>
    /// <param name="optInSlowPeriod">Период медленной скользящей средней (должен быть больше быстрого периода)</param>
    /// <param name="optInMethod">Метод расчета скользящих средних (SMA, EMA, и др.)</param>
    /// <param name="tempBuffer">Временный буфер для хранения промежуточных значений быстрой скользящей средней</param>
    /// <param name="doPercentageOutput">
    /// Флаг режима вывода:  
    /// - <c>true</c> — результат в процентах ((Fast MA - Slow MA)/Slow MA * 100)  
    /// - <c>false</c> — абсолютная разность (Fast MA - Slow MA)
    /// </param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcPriceOscillator<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        Core.MAType optInMethod,
        Span<T> tempBuffer,
        bool doPercentageOutput) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        // Корректировка периодов: медленная MA должна иметь больший период
        if (optInSlowPeriod < optInFastPeriod)
        {
            (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
        }

        // Расчет быстрой скользящей средней во временный буфер
        var retCode = Functions.Ma(inReal, inRange, tempBuffer, out var outRange2, optInFastPeriod, optInMethod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Расчет медленной скользящей средней в выходной массив
        retCode = Functions.Ma(inReal, inRange, outReal, out var outRange1, optInSlowPeriod, optInMethod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Синхронизация индексов между быстрой и медленной MA
        for (int i = 0, j = outRange1.Start.Value - outRange2.Start.Value;
             i < outRange1.End.Value - outRange1.Start.Value;
             i++, j++)
        {
            if (doPercentageOutput)
            {
                // Процентный осциллятор: ((Fast MA - Slow MA) / Slow MA) * 100
                var tempReal = outReal[i];
                outReal[i] = !T.IsZero(tempReal)
                    ? (tempBuffer[j] - tempReal) / tempReal * T.CreateChecked(100)
                    : T.Zero;
            }
            else
            {
                // Абсолютный осциллятор: Fast MA - Slow MA
                outReal[i] = tempBuffer[j] - outReal[i];
            }
        }

        outRange = new Range(outRange1.Start.Value, outRange1.End.Value); // Формирование выходного диапазона

        return retCode;
    }

}
