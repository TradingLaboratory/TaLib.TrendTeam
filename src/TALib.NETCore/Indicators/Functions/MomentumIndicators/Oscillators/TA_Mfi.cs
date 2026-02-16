// Название файла: TA_Mfi.cs
// Рекомендуемое размещение:
// Основная папка: MomentumIndicators
// Подпапка: Oscillators (существующая подпапка для осцилляторов)
// Альтернативные варианты подпапок (если требуется специализация):
// 1. MoneyFlow
// 2. VolumeOscillators
// 3. FlowIndicators

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Money Flow Index (Momentum Indicators) — Индекс Денежного Потока (Индикаторы Импульса)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High) для каждого бара.</param>
    /// <param name="inLow">Массив минимальных цен (Low) для каждого бара.</param>
    /// <param name="inClose">Массив цен закрытия (Close) для каждого бара.</param>
    /// <param name="inVolume">Массив объемов (Volume) для каждого бара.</param>
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
    /// <param name="optInTimePeriod">Период времени для расчета MFI (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индекс Денежного Потока (MFI) — это импульсный осциллятор, который измеряет силу денежного потока, входящего и выходящего из ценной бумаги за определенный период.
    /// Он объединяет данные цен и объемов, чтобы указать на давление покупателей или продавцов,
    /// и часто используется для определения перекупленности или перепроданности.
    /// </para>
    /// <para>
    /// MFI похож на <see cref="Rsi{T}">RSI</see>, но включает данные объемов, что делает его более чувствительным к объемам торгов.
    /// Его комбинирование с трендовыми индикаторами или <see cref="Obv{T}">OBV</see> может усилить интерпретацию рыночных условий.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить типичную цену (Typical Price) для каждого бара:
    ///       <code>
    ///         Typical Price = (High + Low + Close) / 3
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать сырой денежный поток (Raw Money Flow) для каждого бара:
    ///       <code>
    ///         Raw Money Flow = Typical Price * Volume
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определить, является ли сырой денежный поток положительным или отрицательным, сравнивая текущую типичную цену с предыдущей:
    ///       - Если текущая типичная цена больше предыдущей — положительный денежный поток (Positive Money Flow).
    ///       - Если текущая типичная цена меньше предыдущей — отрицательный денежный поток (Negative Money Flow).
    ///       - Если цены равны — денежный поток равен нулю.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Накопить положительные и отрицательные денежные потоки за указанный период времени (<paramref name="optInTimePeriod"/>).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить Индекс Денежного Потока (MFI) с использованием формулы:
    ///       <code>
    ///         MFI = 100 * (Positive Money Flow / (Positive Money Flow + Negative Money Flow))
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение выше 80 указывает на перекупленность (overbought), предполагая возможный разворот тренда или откат.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение ниже 20 указывает на перепроданность (oversold), предполагая возможный разворот тренда или отскок.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расхождения (divergence) между MFI и движением цены могут сигнализировать о возможных разворотах тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Резкие движения MFI за пределы 80/20 зон часто предшествуют сильным ценовым движениям.
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
    /// Возвращает период обратного просмотра (lookback period) для <see cref="Mfi{T}">MFI</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета MFI (по умолчанию 14).</param>
    /// <returns>
    /// Количество периодов, необходимых до первого вычисленного значения индикатора.
    /// Включает базовый период + нестабильный период (unstable period) для сглаживания Уайлдера.
    /// </returns>
    [PublicAPI]
    public static int MfiLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Mfi);

    /// <remarks>
    /// Для совместимости с абстрактным API (массивы вместо Span)
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

        // Проверка корректности входного диапазона и длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимально допустимого периода (должен быть >= 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет общего периода обратного просмотра (включая нестабильный период)
        var lookbackTotal = MfiLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс превышает конечный — нет данных для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;

        // Циклический буфер для хранения денежных потоков за период (negative, positive)
        var moneyFlow = new (T negative, T positive)[optInTimePeriod];

        // Текущий индекс в циклическом буфере денежных потоков
        var mflowIdx = 0;
        // Максимальный допустимый индекс в буфере (для циклического сдвига)
        var maxIdxMflow = optInTimePeriod - 1;

        // Накопить положительные и отрицательные денежные потоки в начальном периоде
        // Начинаем с позиции, предшествующей первому валидному значению (учитывая lookback)
        var today = startIdx - lookbackTotal;
        // Расчет типичной цены (Typical Price) для первого бара в периоде
        var prevValue = (inHigh[today] + inLow[today] + inClose[today]) / FunctionHelpers.Three<T>();

        // Сумма положительных денежных потоков за период
        var posSumMF = T.Zero;
        // Сумма отрицательных денежных потоков за период
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
            // Пропустить нестабильный период (unstable period)
            // Выполнить обработку данных, но не записывать результаты в выходной массив
            today = SkipMfiUnstablePeriod(inHigh, inLow, inClose, inVolume, today, startIdx, moneyFlow, maxIdxMflow, ref posSumMF,
                ref mflowIdx, ref negSumMF, ref prevValue);
        }

        // Основной цикл расчета MFI для оставшихся баров
        while (today <= endIdx)
        {
            // Удалить устаревший денежный поток из суммы (скользящее окно)
            posSumMF -= moneyFlow[mflowIdx].positive;
            negSumMF -= moneyFlow[mflowIdx].negative;

            // Обновить денежные потоки для текущего бара
            UpdateMoneyFlow(inHigh, inLow, inClose, inVolume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow,
                ref mflowIdx);

            // Расчет значения MFI по формуле: 100 * (положительный поток / общий поток)
            var tempValue1 = posSumMF + negSumMF;
            outReal[outIdx++] = tempValue1 >= T.One ? FunctionHelpers.Hundred<T>() * (posSumMF / tempValue1) : T.Zero;

            // Циклический сдвиг индекса буфера
            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }

        // Установка диапазона валидных значений в выходном массиве
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Накопление начальных денежных потоков для заполнения скользящего окна периода.
    /// </summary>
    /// <param name="high">Массив максимальных цен (High).</param>
    /// <param name="low">Массив минимальных цен (Low).</param>
    /// <param name="close">Массив цен закрытия (Close).</param>
    /// <param name="volume">Массив объемов (Volume).</param>
    /// <param name="today">Текущий индекс обрабатываемого бара (изменяется по ссылке).</param>
    /// <param name="prevValue">Предыдущая типичная цена (изменяется по ссылке).</param>
    /// <param name="posSumMF">Сумма положительных денежных потоков (изменяется по ссылке).</param>
    /// <param name="negSumMF">Сумма отрицательных денежных потоков (изменяется по ссылке).</param>
    /// <param name="moneyFlow">Циклический буфер для хранения денежных потоков за период.</param>
    /// <param name="mflowIdx">Текущий индекс в буфере денежных потоков (изменяется по ссылке).</param>
    /// <param name="maxIdxMflow">Максимальный индекс в буфере.</param>
    /// <param name="timePeriod">Период времени для расчета.</param>
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
        // Заполнение буфера денежных потоков за весь период перед началом основного расчета
        for (var i = timePeriod; i > 0; i--)
        {
            UpdateMoneyFlow(high, low, close, volume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow, ref mflowIdx);

            // Циклический сдвиг индекса буфера
            if (++mflowIdx > maxIdxMflow)
            {
                mflowIdx = 0;
            }
        }
    }

    /// <summary>
    /// Пропуск нестабильного периода (unstable period) для сглаживания Уайлдера.
    /// </summary>
    /// <param name="high">Массив максимальных цен (High).</param>
    /// <param name="low">Массив минимальных цен (Low).</param>
    /// <param name="close">Массив цен закрытия (Close).</param>
    /// <param name="volume">Массив объемов (Volume).</param>
    /// <param name="today">Текущий индекс обрабатываемого бара.</param>
    /// <param name="startIdx">Индекс первого бара с валидным значением.</param>
    /// <param name="moneyFlow">Циклический буфер для хранения денежных потоков.</param>
    /// <param name="maxIdxMflow">Максимальный индекс в буфере.</param>
    /// <param name="posSumMF">Сумма положительных денежных потоков (изменяется по ссылке).</param>
    /// <param name="mFlowIdx">Текущий индекс в буфере (изменяется по ссылке).</param>
    /// <param name="negSumMF">Сумма отрицательных денежных потоков (изменяется по ссылке).</param>
    /// <param name="prevValue">Предыдущая типичная цена (изменяется по ссылке).</param>
    /// <returns>Обновленный индекс текущего бара после пропуска нестабильного периода.</returns>
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
        // Пропуск баров в нестабильном периоде: обновление сумм без записи в выходной массив
        while (today < startIdx)
        {
            // Удаление устаревшего денежного потока из суммы
            posSumMF -= moneyFlow[mFlowIdx].positive;
            negSumMF -= moneyFlow[mFlowIdx].negative;

            // Обновление денежных потоков для текущего бара
            UpdateMoneyFlow(high, low, close, volume, ref today, ref prevValue, ref posSumMF, ref negSumMF, moneyFlow, ref mFlowIdx);

            // Циклический сдвиг индекса буфера
            if (++mFlowIdx > maxIdxMflow)
            {
                mFlowIdx = 0;
            }
        }

        return today;
    }

    /// <summary>
    /// Обновление денежных потоков для текущего бара.
    /// </summary>
    /// <param name="high">Массив максимальных цен (High).</param>
    /// <param name="low">Массив минимальных цен (Low).</param>
    /// <param name="close">Массив цен закрытия (Close).</param>
    /// <param name="volume">Массив объемов (Volume).</param>
    /// <param name="today">Текущий индекс обрабатываемого бара (изменяется по ссылке, инкрементируется после обработки).</param>
    /// <param name="prevValue">Предыдущая типичная цена (изменяется по ссылке).</param>
    /// <param name="posSumMF">Сумма положительных денежных потоков (изменяется по ссылке).</param>
    /// <param name="negSumMF">Сумма отрицательных денежных потоков (изменяется по ссылке).</param>
    /// <param name="moneyFlow">Циклический буфер для хранения денежных потоков.</param>
    /// <param name="mflowIdx">Текущий индекс в буфере (изменяется по ссылке).</param>
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
        // Расчет типичной цены (Typical Price) для текущего бара
        var tempValue1 = (high[today] + low[today] + close[today]) / FunctionHelpers.Three<T>();
        // Сравнение с предыдущей типичной ценой для определения направления потока
        var tempValue2 = tempValue1 - prevValue;
        // Сохранение текущей типичной цены как предыдущей для следующей итерации
        prevValue = tempValue1;
        // Расчет сырого денежного потока: типичная цена * объем
        tempValue1 *= volume[today++];
        if (tempValue2 < T.Zero)
        {
            // Отрицательный денежный поток (цена снизилась)
            moneyFlow[mflowIdx].negative = tempValue1;
            negSumMF += tempValue1;
            moneyFlow[mflowIdx].positive = T.Zero;
        }
        else if (tempValue2 > T.Zero)
        {
            // Положительный денежный поток (цена выросла)
            moneyFlow[mflowIdx].positive = tempValue1;
            posSumMF += tempValue1;
            moneyFlow[mflowIdx].negative = T.Zero;
        }
        else
        {
            // Нулевой денежный поток (цена не изменилась)
            moneyFlow[mflowIdx].positive = T.Zero;
            moneyFlow[mflowIdx].negative = T.Zero;
        }
    }
}
