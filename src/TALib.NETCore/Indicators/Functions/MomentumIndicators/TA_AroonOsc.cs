//Название файла: TA_AroonOsc.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendStrength (альтернатива для акцента на силе тренда)
//Oscillators (альтернатива для акцента на осцилляторах)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Aroon Oscillator (Momentum Indicators) — Осциллятор Арун (Индикаторы моментума)
    /// </summary>
    /// <param name="inHigh">Входные данные для расчета индикатора (максимальные цены)</param>
    /// <param name="inLow">Входные данные для расчета индикатора (минимальные цены)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c> и <c>inLow[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> и <paramref name="inLow"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Осциллятор Арун — это индикатор моментума, который измеряет разницу между индикаторами Aroon Up и Aroon Down,
    /// чтобы оценить силу и направление тренда.
    /// <para>
    /// Функция полезна для определения силы и направления тренда и может также сигнализировать о потенциальных разворотах
    /// при пересечении нулевой линии. Обычно используется в сочетании с другими техническими индикаторами
    /// для повышения надежности торговых сигналов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить максимальную максимум и минимальную минимум в указанном периоде времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать Aroon Up и Aroon Down:
    /// <code>
    /// Aroon Up = 100 * (Time Period - Дни с момента максимального максимума) / Time Period
    /// Aroon Down = 100 * (Time Period - Дни с момента минимального минимума) / Time Period
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать Осциллятор Арун как разницу между Aroon Up и Aroon Down:
    ///       <code>
    ///         Aroon Oscillator = Aroon Up - Aroon Down
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения указывают на то, что Aroon Up сильнее, чем Aroon Down, что свидетельствует о восходящем тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения указывают на то, что Aroon Down сильнее, чем Aroon Up, что свидетельствует о нисходящем тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Чем ближе значение к +100 или -100, тем сильнее соответствующий тренд.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения около 0 указывают на отсутствие четкого тренда или боковое движение рынка.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode AroonOsc<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AroonOscImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="AroonOsc{T}">AroonOsc</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int AroonOscLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode AroonOsc<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AroonOscImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode AroonOscImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Этот код почти идентичен функции Aroon, за исключением того,
         * что вместо вывода AroonUp и AroonDown отдельно, строится осциллятор из обоих.
         *
         *   AroonOsc = AroonUp - AroonDown
         *
         */

        // Эта функция использует оптимизированный по скорости алгоритм для логики min/max.
        // Возможно, сначала нужно посмотреть, как работает Min/Max, и эта функция станет понятнее.

        var lookbackTotal = AroonOscLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжаем расчет для запрашиваемого диапазона.
        // Алгоритм позволяет использовать один и тот же буфер для входных и выходных данных.
        var outIdx = 0;
        var today = startIdx;
        var trailingIdx = startIdx - lookbackTotal;

        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;
        var factor = FunctionHelpers.Hundred<T>() / T.CreateChecked(optInTimePeriod);
        while (today <= endIdx)
        {
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            /* Осциллятор рассчитывается следующим образом:
             *   AroonUp   = factor * (optInTimePeriod - (today - highestIdx))
             *   AroonDown = factor * (optInTimePeriod - (today - lowestIdx))
             *   AroonOsc  = AroonUp - AroonDown
             *
             * Арифметическое упрощение дает:
             *   Aroon = factor * (highestIdx - lowestIdx)
             */
            var arron = factor * T.CreateChecked(highestIdx - lowestIdx);

            // Входной и выходной буферы могут быть одним и тем же, поэтому запись в выходной буфер выполняется последней.
            outReal[outIdx++] = arron;

            trailingIdx++;
            today++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
