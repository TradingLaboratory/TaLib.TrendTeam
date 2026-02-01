// WillR.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// Oscillators (альтернатива для группировки осцилляторов)
// OverboughtOversold (альтернатива для акцента на зонах перекупленности/перепроданности)


namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Williams' %R (Momentum Indicators) — Индекс Уильямса %R (Осцилляторы импульса)
    /// </summary>
    /// <param name="inHigh">Входной диапазон максимальных цен (High) за каждый период.</param>
    /// <param name="inLow">Входной диапазон минимальных цен (Low) за каждый период.</param>
    /// <param name="inClose">Входной диапазон цен закрытия (Close) за каждый период.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">
    /// Диапазон для хранения рассчитанных значений индикатора.
    /// - Содержит ТОЛЬКО валидные значения индикатора.
    /// - Длина равна <c>outRange.End.Value - outRange.Start.Value</c> (если <c>outRange</c> корректен).
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения индикатора:
    /// - <b>Start</b>: индекс первого элемента входных данных с валидным значением в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента входных данных с валидным значением в <paramref name="outReal"/>.
    /// - Если данных недостаточно для расчёта (например, длина входных данных меньше периода индикатора), возвращается диапазон <c>[0, 0)</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (количество баров для определения Highest High и Lowest Low).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индекс Уильямса %R — осциллятор импульса, измеряющий уровень цены закрытия (Close) относительно диапазона
    /// между максимальной (Highest High) и минимальной (Lowest Low) ценами за заданный период.
    /// Используется для выявления зон перекупленности и перепроданности на рынке.
    /// </para>
    /// <para>
    /// Индикатор наиболее эффективен на рынках с боковым трендом (флэт).
    /// Для повышения надёжности сигналов разворота рекомендуется подтверждение с помощью трендовых индикаторов или объёмов.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение максимальной цены (Highest High) и минимальной цены (Lowest Low) за заданный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт значения %R по формуле:
    ///       <code>
    ///         %R = ((Highest High - Close) / (Highest High - Lowest Low)) * -100
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><description>Highest High — максимальная цена (High) за период</description></item>
    ///         <item><description>Lowest Low — минимальная цена (Low) за период</description></item>
    ///         <item><description>Close — цена закрытия текущего бара</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения выше -20 (ближе к 0) указывают на зону перекупленности — потенциальная точка для продажи.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения ниже -80 (ближе к -100) указывают на зону перепроданности — потенциальная точка для покупки.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение уровня -50 может сигнализировать о смене краткосрочного импульса.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode WillR<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        WillRImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период запаздывания (lookback) для индикатора <see cref="WillR{T}">WillR</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>
    /// Количество периодов, необходимых до первого валидного значения индикатора.
    /// Для %R равно <c>optInTimePeriod - 1</c> (так как для расчёта требуется анализ предыдущих <c>optInTimePeriod</c> баров).
    /// </returns>
    [PublicAPI]
    public static int WillRLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode WillR<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        WillRImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode WillRImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением (без валидных данных)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и равенства длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода: должен быть не менее 2 для расчёта диапазона цен
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчёт периода запаздывания (количество баров, необходимых до первого валидного значения)
        var lookbackTotal = WillRLookback(optInTimePeriod);
        // Корректировка начального индекса с учётом периода запаздывания
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — расчёт не требуется
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Начало расчёта для запрошенного диапазона.
        // Алгоритм допускает использование одного и того же буфера для входных и выходных данных.
        var outIdx = 0;                      // Индекс записи в выходной массив
        var today = startIdx;                // Текущий обрабатываемый бар
        var trailingIdx = startIdx - lookbackTotal; // Начальный индекс скользящего окна

        // Кэширование индексов и значений экстремумов для оптимизации расчётов
        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;

        // Основной цикл расчёта индикатора для каждого бара в диапазоне
        while (today <= endIdx)
        {
            // Обновление минимальной цены (Lowest Low) в текущем окне периода
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            // Обновление максимальной цены (Highest High) в текущем окне периода
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            // Предварительный расчёт знаменателя формулы с учётом умножения на -100
            // Формула: %R = ((Highest High - Close) / (Highest High - Lowest Low)) * -100
            // Преобразовано: %R = (Highest High - Close) / ((Highest High - Lowest Low) / -100)
            var diff = (highest - lowest) / (T.NegativeOne * FunctionHelpers.Hundred<T>());

            // Расчёт значения %R с защитой от деления на ноль
            outReal[outIdx++] = !T.IsZero(diff) ? (highest - inClose[today]) / diff : T.Zero;

            // Сдвиг скользящего окна на один бар вперёд
            trailingIdx++;
            today++;
        }

        // Формирование выходного диапазона: от первого валидного индекса до последнего рассчитанного
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
