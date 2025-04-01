//Название файла TA_Stoch.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//Oscillators (альтернатива, если требуется группировка по типу индикатора)
//TrendIndicators (альтернатива для акцента на трендовых индикаторах)

// файл TA_Stoch.cs
namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Stochastic Oscillator (Momentum Indicators) — Стохастический осциллятор (индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимумы).</param>
    /// <param name="inLow">Массив входных цен Low (минимумы).</param>
    /// <param name="inClose">Массив входных цен Close (закрытия).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// <para>- Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.</para>
    /// </param>
    /// <param name="outSlowK">
    /// Массив, содержащий ТОЛЬКО валидные значения линии %K (медленной).
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outSlowK[i]</c> соответствует <c>inClose[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outSlowD">
    /// Массив, содержащий ТОЛЬКО валидные значения линии %D (медленной).
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outSlowD[i]</c> соответствует <c>inClose[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// <para>- <b>Start</b>: индекс первого элемента <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outSlowK"/> и <paramref name="outSlowD"/>.</para>
    /// <para>- <b>End</b>: индекс последнего элемента <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outSlowK"/> и <paramref name="outSlowD"/>.</para>
    /// <para>- Гарантируется: <c>End == inClose.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.</para>
    /// <para>- Если данных недостаточно (например, длина <paramref name="inClose"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.</para>
    /// </param>
    /// <param name="optInFastKPeriod">Период для расчета быстрой линии %K.</param>
    /// <param name="optInSlowKPeriod">Период сглаживания для преобразования Fast %K в Slow %K.</param>
    /// <param name="optInSlowKMAType">Тип скользящей средней для сглаживания Fast %K.</param>
    /// <param name="optInSlowDPeriod">Период для расчета линии %D.</param>
    /// <param name="optInSlowDMAType">Тип скользящей средней для сглаживания Slow %K.</param>
    /// <typeparam name="T">Числовой тип данных (float/double).</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>).</returns>
    /// <remarks>
    /// Стохастический осциллятор определяет положение цены закрытия относительно ценового диапазона за указанный период.
    /// Позволяет идентифицировать условия перекупленности/перепроданности и потенциальные развороты.
    /// <para>
    /// Индикатор состоит из двух линий:
    /// <c>%K</c> (сырой стохастик) и <c>%D</c> (сигнальная линия).
    /// Реализована "медленная" версия осциллятора с дополнительным сглаживанием.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет сырого значения %K:
    ///       <code>
    ///         %K = 100 * ((Close - LowestLow) / (HighestHigh - LowestLow))
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><description><b>Close</b> - цена закрытия текущего периода</description></item>
    ///         <item><description><b>LowestLow</b> - минимальный минимум за период Fast %K</description></item>
    ///         <item><description><b>HighestHigh</b> - максимальный максимум за период Fast %K</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item><description>Сглаживание %K за период Slow %K с использованием выбранного типа MA</description></item>
    ///   <item><description>Сглаживание Slow %K за период Slow %D с использованием выбранного типа MA</description></item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item><description>Значения выше 80: зона перекупленности (потенциальный разворот вниз)</description></item>
    ///   <item><description>Значения ниже 20: зона перепроданности (потенциальный рост)</description></item>
    ///   <item><description>Пересечения %K и %D:
    ///     <list type="bullet">
    ///       <item><description>%K ↑ выше %D - возможный сигнал на покупку</description></item>
    ///       <item><description>%K ↓ ниже %D - возможный сигнал на продажу</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>Дивергенция между осциллятором и ценой указывает на ослабление тренда</description></item>
    /// </list>
    /// </remarks>

    [PublicAPI]
    public static Core.RetCode Stoch<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outSlowK,
        Span<T> outSlowD,
        out Range outRange,
        int optInFastKPeriod = 5,
        int optInSlowKPeriod = 3,
        Core.MAType optInSlowKMAType = Core.MAType.Sma,
        int optInSlowDPeriod = 3,
        Core.MAType optInSlowDMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        StochImpl(inHigh, inLow, inClose, inRange, outSlowK, outSlowD, out outRange, optInFastKPeriod, optInSlowKPeriod, optInSlowKMAType,
            optInSlowDPeriod, optInSlowDMAType);

    /// <summary>
    /// Возвращает период "просмотра назад" для функции <see cref="Stoch{T}"/>.
    /// </summary>
    /// <returns>Количество периодов, необходимых для первого расчета.</returns>
    [PublicAPI]
    public static int StochLookback(
        int optInFastKPeriod = 5,
        int optInSlowKPeriod = 3,
        Core.MAType optInSlowKMAType = Core.MAType.Sma,
        int optInSlowDPeriod = 3,
        Core.MAType optInSlowDMAType = Core.MAType.Sma)
    {
        if (optInFastKPeriod < 1 || optInSlowKPeriod < 1 || optInSlowDPeriod < 1)
        {
            return -1;
        }
        var retValue = optInFastKPeriod - 1;
        retValue += MaLookback(optInSlowKPeriod, optInSlowKMAType);
        retValue += MaLookback(optInSlowDPeriod, optInSlowDMAType);
        return retValue;
    }

    // Реализация метода
    private static Core.RetCode StochImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outSlowK,
        Span<T> outSlowD,
        out Range outRange,
        int optInFastKPeriod,
        int optInSlowKPeriod,
        Core.MAType optInSlowKMAType,
        int optInSlowDPeriod,
        Core.MAType optInSlowDMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (startIdx, endIdx) = rangeIndices;
        if (optInFastKPeriod < 1 || optInSlowKPeriod < 1 || optInSlowDPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* В стохастике определяются 4 линии:
         *   FastK, FastD, SlowK и SlowD
         *
         *   FastK(Kperiod) = 100*(Close - LowestLow)/(HighestHigh - LowestLow)
         *   FastD = Скользящая средняя FastK за FastDperiod
         *   SlowK = Скользящая средняя FastK за SlowKperiod
         *   SlowD = Скользящая средняя SlowK за SlowDperiod
         */
        var lookbackK = optInFastKPeriod - 1;
        var lookbackDSlow = MaLookback(optInSlowDPeriod, optInSlowDMAType);
        var lookbackTotal = StochLookback(optInFastKPeriod, optInSlowKPeriod, optInSlowKMAType, optInSlowDPeriod, optInSlowDMAType);
        startIdx = Math.Max(startIdx, lookbackTotal);
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var trailingIdx = startIdx - lookbackTotal;
        var today = trailingIdx + lookbackK;

        Span<T> tempBuffer;
        if (outSlowK == inHigh || outSlowK == inLow || outSlowK == inClose)
        {
            tempBuffer = outSlowK;
        }
        else if (outSlowD == inHigh || outSlowD == inLow || outSlowD == inClose)
        {
            tempBuffer = outSlowD;
        }
        else
        {
            tempBuffer = new T[endIdx - today + 1];
        }

        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;
        while (today <= endIdx)
        {
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);
            var diff = (highest - lowest) / FunctionHelpers.Hundred<T>();
            tempBuffer[outIdx++] = !T.IsZero(diff) ? (inClose[today] - lowest) / diff : T.Zero;
            trailingIdx++;
            today++;
        }

        var retCode = MaImpl(tempBuffer, Range.EndAt(outIdx - 1), tempBuffer, out outRange, optInSlowKPeriod, optInSlowKMAType);
        if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
        {
            return retCode;
        }
        var nbElement = outRange.End.Value - outRange.Start.Value;

        retCode = MaImpl(tempBuffer, Range.EndAt(nbElement - 1), outSlowD, out outRange, optInSlowDPeriod, optInSlowDMAType);
        nbElement = outRange.End.Value - outRange.Start.Value;

        tempBuffer.Slice(lookbackDSlow, nbElement).CopyTo(outSlowK);
        if (retCode != Core.RetCode.Success)
        {
            outRange = Range.EndAt(0);
            return retCode;
        }
        outRange = new Range(startIdx, startIdx + nbElement);
        return Core.RetCode.Success;
    }
}
