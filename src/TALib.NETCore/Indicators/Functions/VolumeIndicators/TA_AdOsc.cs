// TA_AdOsc.cs
// Группы к которым можно отнести индикатор:
// VolumeIndicators (существующая папка - идеальное соответствие категории)
// Oscillators (альтернатива для группировки по типу индикатора)
// MomentumIndicators (альтернатива для акцента на импульсе)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Осциллятор Chaikin A/D (индикаторы объема)
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимумы)</param>
    /// <param name="inLow">Массив входных цен Low (минимумы)</param>
    /// <param name="inClose">Массив входных цен Close (закрытия)</param>
    /// <param name="inVolume">Массив входных данных объема торгов</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив
    /// </param>
    /// <param name="outReal">Массив для сохранения рассчитанных значений осциллятора</param>
    /// <param name="outRange">
    /// Диапазон индексов с валидными данными в выходном массиве:  
    /// - Start: индекс первого валидного значения (>= lookback периода)  
    /// - End: индекс последнего обработанного элемента
    /// </param>
    /// <param name="optInFastPeriod">Период для расчета быстрой EMA (по умолчанию 3)</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной EMA (по умолчанию 10)</param>
    /// <typeparam name="T">Числовой тип данных (float/double)</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>)</returns>
    /// <remarks>
    /// Осциллятор Chaikin A/D измеряет импульс линии Накопления/Распределения, 
    /// сравнивая краткосрочные и долгосрочные интервалы денежного потока.
    /// <para>
    /// Позволяет выявлять изменения баланса покупок/продаж до их отражения в ценах.
    /// Может подтверждать тренды или предсказывать пробои уровней.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет линии Накопления/Распределения (A/D Line):
    ///       <code>
    ///         A/D Line = ((Close - Low) - (High - Close)) / (High - Low) * Volume
    ///       </code>
    ///       Где:  
    ///       - Close, Low, High - цены закрытия, минимума и максимума  
    ///       - Volume - объем торгов
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет экспоненциальных скользящих средних (EMA) для A/D Line:
    ///       <code>
    ///         EMA(Fast) = α * A/D Line + (1 - α) * EMA(Fast-1)
    ///         EMA(Slow) = β * A/D Line + (1 - β) * EMA(Slow-1)
    ///       </code>
    ///       Где α = 2/(FastPeriod+1), β = 2/(SlowPeriod+1)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Осциллятор как разница EMA:
    ///       <code>
    ///         AdOsc = EMA(Fast) - EMA(Slow)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация</b>:
    /// <list type="bullet">
    ///   <item><description>Рост значений ⇒ доминирование покупателей</description></item>
    ///   <item><description>Падение значений ⇒ давление продавцов</description></item>
    ///   <item><description>Дивергенция с ценой ⇒ возможный разворот тренда</description></item>
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
    /// Возвращает период "просмотра назад" для осциллятора Chaikin A/D
    /// </summary>
    /// <param name="optInFastPeriod">Период быстрой EMA</param>
    /// <param name="optInSlowPeriod">Период медленной EMA</param>
    /// <returns>Минимальное количество баров для первого валидного значения</returns>
    [PublicAPI]
    public static int AdOscLookback(int optInFastPeriod = 3, int optInSlowPeriod = 10)
    {
        var slowestPeriod = optInFastPeriod < optInSlowPeriod ? optInSlowPeriod : optInFastPeriod;
        return optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : EmaLookback(slowestPeriod);
    }

    /// <summary>
    /// Вариант метода для массивов (совместимость с абстрактным API)
    /// </summary>
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

        // Проверка корректности входных диапазонов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (startIdx, endIdx) = rangeIndices;

        // Проверка валидности периодов EMA
        if (optInFastPeriod < 2 || optInSlowPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Определение периода "просмотра назад"
        var lookbackTotal = AdOscLookback(optInFastPeriod, optInSlowPeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация индексов
        var outBegIdx = startIdx;
        var today = startIdx - lookbackTotal;

        // Расчет коэффициентов сглаживания для EMA
        var fastK = FunctionHelpers.Two<T>() / (T.CreateChecked(optInFastPeriod) + T.One); // α для быстрой EMA
        var oneMinusFastK = T.One - fastK;
        var slowK = FunctionHelpers.Two<T>() / (T.CreateChecked(optInSlowPeriod) + T.One); // α для медленной EMA
        var oneMinusSlowK = T.One - slowK;

        // Инициализация линии Накопления/Распределения
        var ad = T.Zero;
        ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);

        // Начальные значения EMA
        var fastEMA = ad; // Начальное значение быстрой EMA
        var slowEMA = ad; // Начальное значение медленной EMA

        // Пропуск нестабильного периода (до startIdx)
        while (today < startIdx)
        {
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);
            fastEMA = fastK * ad + oneMinusFastK * fastEMA; // Обновление быстрой EMA
            slowEMA = slowK * ad + oneMinusSlowK * slowEMA; // Обновление медленной EMA
        }

        // Основной цикл расчета осциллятора
        var outIdx = 0;
        while (today <= endIdx)
        {
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);
            fastEMA = fastK * ad + oneMinusFastK * fastEMA;
            slowEMA = slowK * ad + oneMinusSlowK * slowEMA;
            outReal[outIdx++] = fastEMA - slowEMA; // Разница EMA = осциллятор
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);
        return Core.RetCode.Success;
    }
}
