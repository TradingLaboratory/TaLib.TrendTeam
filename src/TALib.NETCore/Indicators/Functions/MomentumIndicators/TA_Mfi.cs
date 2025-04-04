//Название файла TA_Mfi.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//VolumeIndicators (альтернатива, если требуется группировка по типу индикатора)
//MoneyFlow (альтернатива для акцента на денежных потоках)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Money Flow Index (Momentum Indicators) — Индекс Денежного Потока (Индикаторы Импульса)
    /// </summary>
    /// <param name="inHigh">Входные данные максимальных цен.</param>
    /// <param name="inLow">Входные данные минимальных цен.</param>
    /// <param name="inClose">Входные данные цен закрытия.</param>
    /// <param name="inVolume">Входные данные объемов.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>, <paramref name="inVolume"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>, <paramref name="inVolume"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Индекс Денежного Потока (MFI) — это импульсный осциллятор, который измеряет силу денежного потока, входящего и выходящего из ценной бумаги за определенный период.
    /// Он объединяет данные цен и объемов, чтобы указать на давление покупателей или продавцов,
    /// и часто используется для определения перекупленности или перепроданности.
    /// <para>
    /// MFI похож на <see cref="Rsi{T}">RSI</see>, но включает данные объемов.
    /// Его комбинирование с трендовыми индикаторами или <see cref="Obv{T}">OBV</see> может усилить интерпретацию.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить типичную цену для каждого бара:
    ///       <code>
    ///         Типичная Цена = (High + Low + Close) / 3
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать сырой денежный поток для каждого бара:
    ///       <code>
    ///         Денежный Поток = Типичная Цена * Volume
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определить, является ли сырой денежный поток положительным или отрицательным, сравнивая текущую типичную цену с предыдущей:
    ///       - Если текущая типичная цена больше предыдущей, она способствует положительному денежному потоку.
    ///       - Если меньше, то отрицательному.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Накопить положительные и отрицательные денежные потоки за указанный период времени (`optInTimePeriod`).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить Индекс Денежного Потока с использованием формулы:
    ///       <code>
    ///         MFI = 100 * (Положительный Денежный Поток / (Положительный Денежный Поток + Отрицательный Денежный Поток))
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение выше 80 указывает на перекупленность, предполагая возможный разворот тренда или откат.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение ниже 20 указывает на перепроданность, предполагая возможный разворот тренда или отскок.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расхождения между MFI и движением цены могут сигнализировать о возможных разворотах тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Mfi<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MfiImpl(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Mfi{T}">Mfi</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int MfiLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Mfi);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Mfi<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        T[] inVolume,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MfiImpl<T>(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MfiImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = MfiLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;

        var moneyFlow = new (T negative, T positive)[optInTimePeriod];

        var mflowIdx = 0;
        var maxIdxMflow = optInTimePeriod - 1;

        // Накопить положительные и отрицательные денежные потоки в начальном периоде.
        var today = startIdx - lookbackTotal;
        var prevValue = (inHigh[today] + inLow[today] + inClose[today]) / FunctionHelpers.Three<T>();

        var posSumMF = T.Zero;
        var negSumMF = T.Zero;
        today++;
        AccumulateInitialMoneyFlow(inHigh, inLow, inClose, inVolume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
            ref mflowIdx, maxIdxMflow, optInTimePeriod);

        /* Следующие два уравнения эквивалентны:
         *   MFI = 100 - (100 / 1 + (posSumMF / negSumMF))
         *   MFI = 100 * (posSumMF / (posSumMF + negSumMF))
         * Вторая формула используется здесь для оптимизации скорости.
         */
        if (today > startIdx)
        {
            var tempValue1 = posSumMF + negSumMF;
            outReal[outIdx++] = tempValue1 >= T.One ? FunctionHelpers.Hundred<T>() * (posSumMF / tempValue1) : T.Zero;
        }
        else
        {
            // Пропустить нестабильный период. Выполнить обработку, но не записывать в выходные данные.
            today = SkipMfiUnstablePeriod(inHigh, inLow, inClose, inVolume, today, startIdx, moneyFlow, maxIdxMflow, ref posSumMF,
                ref mflowIdx, ref negSumMF, ref prevValue);
        }

        while (today <= endIdx)
        {
            posSumMF -= moneyFlow[mflowIdx].positive;
            negSumMF -= moneyFlow[mflowIdx].negative;

            UpdateMoneyFlow(inHigh, inLow, inClose, inVolume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
                ref mflowIdx);

            var tempValue1 = posSumMF + negSumMF;
            outReal[outIdx++] = tempValue1 >= T.One ? FunctionHelpers.Hundred<T>() * (posSumMF / tempValue1) : T.Zero;

            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void AccumulateInitialMoneyFlow<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        ref int today,
        ref T prevValue,
        ref T posSumMF,
        ref T negSumMF,
        (T negative, T positive)[] moneyFlow,
        ref int mflowIdx,
        int maxIdxMflow,
        int timePeriod) where T : IFloatingPointIeee754<T>
    {
        for (var i = timePeriod; i > 0; i--)
        {
            UpdateMoneyFlow(high, low, close, volume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow, ref mflowIdx);

            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }
    }

    private static int SkipMfiUnstablePeriod<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        int today,
        int startIdx,
        (T negative, T positive)[] moneyFlow,
        int maxIdxMflow,
        ref T posSumMF,
        ref int mFlowIdx,
        ref T negSumMF,
        ref T prevValue) where T : IFloatingPointIeee754<T>
    {
        while (today < startIdx)
        {
            posSumMF -= moneyFlow[mFlowIdx].positive;
            negSumMF -= moneyFlow[mFlowIdx].negative;

            UpdateMoneyFlow(high, low, close, volume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow, ref mFlowIdx);

            if (++mFlowIdx > maxIdxMflow)
            {
                mFlowIdx = 0;
            }
        }

        return today;
    }

    private static void UpdateMoneyFlow<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        ref int today,
        ref T prevValue,
        ref T posSumMF,
        ref T negSumMF,
        (T negative, T positive)[] moneyFlow,
        ref int mflowIdx) where T : IFloatingPointIeee754<T>
    {
        var tempValue1 = (high[today] + low[today] + close[today]) / FunctionHelpers.Three<T>();
        var tempValue2 = tempValue1 - prevValue;
        prevValue = tempValue1;
        tempValue1 *= volume[today++];
        if (tempValue2 < T.Zero)
        {
            moneyFlow[mflowIdx].negative = tempValue1;
            negSumMF += tempValue1;
            moneyFlow[mflowIdx].positive = T.Zero;
        }
        else if (tempValue2 > T.Zero)
        {
            moneyFlow[mflowIdx].positive = tempValue1;
            posSumMF += tempValue1;
            moneyFlow[mflowIdx].negative = T.Zero;
        }
        else
        {
            moneyFlow[mflowIdx].positive = T.Zero;
            moneyFlow[mflowIdx].negative = T.Zero;
        }
    }
}
