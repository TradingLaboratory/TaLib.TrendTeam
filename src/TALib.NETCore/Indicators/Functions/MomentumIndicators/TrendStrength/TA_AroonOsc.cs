// Название файла: TA_AroonOsc.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - основная категория)
// TrendIndicators (альтернатива для акцента на оценке силы тренда)
// Oscillators (альтернатива для группировки осцилляторов)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Aroon Oscillator (Momentum Indicators) — Осциллятор Арун (Индикаторы моментума)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High) для расчета индикатора</param>
    /// <param name="inLow">Массив минимальных цен (Low) для расчета индикатора</param>
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
    /// <param name="optInTimePeriod">Период времени для расчета индикаторов Aroon Up и Aroon Down (по умолчанию 14)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Осциллятор Арун — это индикатор моментума, рассчитывающий разницу между индикаторами <b>Aroon Up</b> и <b>Aroon Down</b>
    /// для оценки силы и направления текущего тренда.
    /// </para>
    /// <para>
    /// <b>Формула расчета:</b>
    /// <code>
    /// Aroon Up   = 100 * (Period - DaysSinceHighestHigh) / Period
    /// Aroon Down = 100 * (Period - DaysSinceLowestLow) / Period
    /// Aroon Osc  = Aroon Up - Aroon Down
    /// </code>
    /// где:
    /// - <c>DaysSinceHighestHigh</c> — количество периодов с момента достижения максимального значения цены (High) за указанный период
    /// - <c>DaysSinceLowestLow</c> — количество периодов с момента достижения минимального значения цены (Low) за указанный период
    /// </para>
    /// <para>
    /// <b>Интерпретация значений:</b>
    /// <list type="bullet">
    ///   <item><description>Значения выше 0: восходящий тренд (Aroon Up сильнее Aroon Down)</description></item>
    ///   <item><description>Значения ниже 0: нисходящий тренд (Aroon Down сильнее Aroon Up)</description></item>
    ///   <item><description>Значения около +50..+100: сильный восходящий тренд</description></item>
    ///   <item><description>Значения около -50..-100: сильный нисходящий тренд</description></item>
    ///   <item><description>Значения около 0: отсутствие четкого тренда, боковое движение</description></item>
    ///   <item><description>Пересечение нулевой линии: потенциальный сигнал разворота тренда</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Особенности использования:</b>
    /// - Рекомендуемый период: 14 (стандартный), но может варьироваться от 10 до 25 в зависимости от временного интервала
    /// - Индикатор особенно эффективен на трендовых рынках
    /// - Для повышения надежности сигналов рекомендуется использовать в комбинации с другими индикаторами тренда
    /// </para>
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
    /// Возвращает период обратного просмотра (lookback) для индикатора <see cref="AroonOsc{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета индикатора (по умолчанию 14)</param>
    /// <returns>
    /// Количество баров, необходимых до первого валидного значения индикатора.
    /// Возвращает -1 при некорректном значении периода (меньше 2).
    /// </returns>
    /// <remarks>
    /// Период обратного просмотра равен значению <paramref name="optInTimePeriod"/>,
    /// так как для расчета первого значения требуется анализировать указанное количество предыдущих баров.
    /// </remarks>
    [PublicAPI]
    public static int AroonOscLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Вспомогательный метод для совместимости с абстрактным API (работа с массивами вместо Span)
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

        // Проверка корректности входного диапазона и длины массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимально допустимого значения периода (должен быть >= 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Алгоритм расчета основан на функции Aroon, но вместо вывода отдельных значений
         * Aroon Up и Aroon Down, вычисляется их разница — осциллятор:
         *   AroonOsc = AroonUp - AroonDown
         *
         * После арифметического упрощения формула преобразуется в:
         *   AroonOsc = factor * (highestIdx - lowestIdx)
         * где:
         *   factor = 100 / optInTimePeriod
         *   highestIdx — индекс бара с максимальным значением High в окне периода
         *   lowestIdx — индекс бара с минимальным значением Low в окне периода
         */

        // Расчет периода обратного просмотра (количество баров до первого валидного значения)
        var lookbackTotal = AroonOscLookback(optInTimePeriod);
        // Смещение начального индекса с учетом периода обратного просмотра
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после смещения начальный индекс превышает конечный — нет данных для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация переменных для скользящего окна расчета
        var outIdx = 0;                    // Индекс в выходном массиве
        var today = startIdx;              // Текущий обрабатываемый бар
        var trailingIdx = startIdx - lookbackTotal; // Начало скользящего окна (размером optInTimePeriod)

        // Кэширование индексов и значений экстремумов для оптимизации расчетов
        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;

        // Предварительно рассчитанный множитель для преобразования в проценты (100 / период)
        var factor = FunctionHelpers.Hundred<T>() / T.CreateChecked(optInTimePeriod);

        // Основной цикл расчета осциллятора для каждого бара в диапазоне
        while (today <= endIdx)
        {
            // Обновление индекса и значения минимального Low в скользящем окне
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);

            // Обновление индекса и значения максимального High в скользящем окне
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            /* Расчет значения осциллятора Арун:
             * Используется упрощенная формула после алгебраического преобразования:
             *   AroonOsc = factor * (highestIdx - lowestIdx)
             * где разница индексов отражает относительную силу восходящего и нисходящего компонентов
             */
            var arron = factor * T.CreateChecked(highestIdx - lowestIdx);

            // Запись рассчитанного значения в выходной массив
            // (запись выполняется последней для поддержки работы с одинаковыми входным/выходным буферами)
            outReal[outIdx++] = arron;

            // Сдвиг скользящего окна на один бар вперед
            trailingIdx++;
            today++;
        }

        // Формирование выходного диапазона с валидными значениями
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
