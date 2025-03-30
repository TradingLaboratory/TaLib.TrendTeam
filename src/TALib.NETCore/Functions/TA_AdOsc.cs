//Файл TA_AdOsc.cs

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Осциллятор Chaikin A/D (индикаторы объема)
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимумы).</param>
    /// <param name="inLow">Массив входных цен Low (минимумы).</param>
    /// <param name="inClose">Массив входных цен Close (закрытия).</param>
    /// <param name="inVolume">Массив входных данных объема торгов.</param>
    /// <param name="inRange">Диапазон индексов для вычислений во входных данных.</param>
    /// <param name="outReal">Массив для сохранения рассчитанных значений осциллятора.</param>
    /// <param name="outRange">Диапазон индексов с валидными данными в выходном массиве.</param>
    /// <param name="optInFastPeriod">Период для расчета быстрой экспоненциальной скользящей средней (EMA).</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной экспоненциальной скользящей средней (EMA).</param>
    /// <typeparam name="T">Числовой тип данных (float/double).</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>).</returns>
    /// <remarks>
    /// Осциллятор Chaikin A/D измеряет импульс линии Накопления/Распределения, сравнивая краткосрочные и долгосрочные интервалы денежного потока.
    /// <para>
    /// Позволяет выявлять изменения покупательского/продавеческого давления до их проявления в ценовых данных.
    /// Может подтверждать тренды, выявленные по цене, или служить опережающим индикатором пробоев.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет линии Накопления/Распределения:
    ///       <code>
    ///         A/D Line = ((Close - Low) - (High - Close)) / (High - Low) * Volume
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет быстрой EMA (экспоненциальной скользящей средней) для линии A/D.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет медленной EMA для линии A/D.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Осциллятор Chaikin A/D как разница между быстрой и медленной EMA:
    ///       <code>
    ///         AdOsc = EMA(Fast, A/D Line) - EMA(Slow, A/D Line)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост значений указывает на усиление покупательского давления (потенциальный рост цены).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Падение значений указывает на усиление продавеческого давления (потенциальное снижение цены).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Дивергенция между осциллятором и ценой сигнализирует о возможных разворотах тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode AdOsc<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod = 3,
        int optInSlowPeriod = 10) where T : IFloatingPointIeee754<T> =>
        AdOscImpl(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod);

    /// <summary>
    /// Возвращает период "просмотра назад" для функции <see cref="AdOsc{T}"/>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для быстрой EMA.</param>
    /// <param name="optInSlowPeriod">Период для медленной EMA.</param>
    /// <returns>Количество периодов, необходимых для первого расчета.</returns>
    [PublicAPI]
    public static int AdOscLookback(int optInFastPeriod = 3, int optInSlowPeriod = 10)
    {
        var slowestPeriod = optInFastPeriod < optInSlowPeriod ? optInSlowPeriod : optInFastPeriod;

        return optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : EmaLookback(slowestPeriod);
    }

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode AdOsc<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        T[] inVolume,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInFastPeriod = 3,
        int optInSlowPeriod = 10) where T : IFloatingPointIeee754<T> =>
        AdOscImpl<T>(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod);

    private static Core.RetCode AdOscImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInFastPeriod < 2 || optInSlowPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Переменная fastEMA не обязательно является самой быстрой EMA.
         * Аналогично, slowEMA не обязательно самая медленная EMA.
         *
         * AdOsc всегда равен (fastEMA - slowEMA) независимо от указанных периодов. Например:
         *   ADOSC(3, 10) = EMA(3, AD) - EMA(10, AD)
         * тогда как
         *   ADOSC(10, 3) = EMA(10, AD) - EMA(3, AD)
         *
         * Это позволяет экспериментировать с нестандартными параметрами.
         */

        var lookbackTotal = AdOscLookback(optInFastPeriod, optInSlowPeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx;
        var today = startIdx - lookbackTotal;

        var fastK = FunctionHelpers.Two<T>() / (T.CreateChecked(optInFastPeriod) + T.One);
        var oneMinusFastK = T.One - fastK;

        var slowK = FunctionHelpers.Two<T>() / (T.CreateChecked(optInSlowPeriod) + T.One);
        var oneMinusSlowK = T.One - slowK;

        // Инициализация линии Накопления/Распределения
        var ad = T.Zero;
        ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);

        var fastEMA = ad;
        var slowEMA = ad;

        // Пропуск нестабильного периода
        while (today < startIdx)
        {
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);
            fastEMA = fastK * ad + oneMinusFastK * fastEMA;
            slowEMA = slowK * ad + oneMinusSlowK * slowEMA;
        }

        // Основной расчет
        var outIdx = 0;
        while (today <= endIdx)
        {
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);
            fastEMA = fastK * ad + oneMinusFastK * fastEMA;
            slowEMA = slowK * ad + oneMinusSlowK * slowEMA;

            outReal[outIdx++] = fastEMA - slowEMA;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
