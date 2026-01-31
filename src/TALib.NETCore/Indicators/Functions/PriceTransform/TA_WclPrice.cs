// WclPrice.cs
// Группы к которым можно отнести индикатор:
// PriceTransform (существующая папка - идеальное соответствие категории)
// OverlapStudies (альтернатива, так как индикатор отображается на графике цен)
// PriceIndicators (альтернатива для акцента на работе с ценовыми данными)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Weighted Close Price (Price Transform) — Взвешенная цена закрытия (Преобразование цены)
    /// </summary>
    /// <param name="inHigh">Входной диапазон максимальных цен (High) за каждый период.</param>
    /// <param name="inLow">Входной диапазон минимальных цен (Low) за каждый период.</param>
    /// <param name="inClose">Входной диапазон цен закрытия (Close) за каждый период.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">
    /// Диапазон для хранения рассчитанных значений взвешенной цены закрытия.
    /// - Длина выходного диапазона равна количеству обработанных периодов.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента входных данных с валидным значением в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента входных данных с валидным значением в <paramref name="outReal"/>.
    /// - Для данного индикатора <c>outRange.Start == inRange.Start</c>, так как lookback период равен 0.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата выполнения <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, иначе соответствующий код ошибки.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Взвешенная цена закрытия (Weighted Close Price) — это преобразование цены, которое вычисляет
    /// взвешенное среднее значение максимальной (High), минимальной (Low) и цены закрытия (Close),
    /// придавая двойной вес цене закрытия как наиболее значимой для анализа.
    /// </para>
    /// <para>
    /// Данный индикатор подчеркивает важность цены закрытия, которая часто считается ключевой ценой
    /// торгового периода. Он улучшает анализ динамики закрытия и может выявлять паттерны, которые
    /// другие преобразования цен упускают. Комбинация с индикаторами импульса (например,
    /// <see cref="Rsi{T}">RSI</see>) или трендовыми инструментами (например, <see cref="Macd{T}">MACD</see>)
    /// повышает его аналитическую ценность.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого периода вычисляется взвешенная цена закрытия по формуле:
    ///       <code>
    ///         WCL = (High + Low + 2 * Close) / 4
    ///       </code>
    ///       где:
    ///       - <c>High</c> — максимальная цена периода,
    ///       - <c>Low</c> — минимальная цена периода,
    ///       - <c>Close</c> — цена закрытия периода.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode WclPrice<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        WclPriceImpl(inHigh, inLow, inClose, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для индикатора <see cref="WclPrice{T}">WclPrice</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчета не требуется предыдущих исторических данных.</returns>
    [PublicAPI]
    public static int WclPriceLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode WclPrice<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        WclPriceImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange);

    private static Core.RetCode WclPriceImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и проверка соответствия длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов для обработки
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Расчет взвешенной цены закрытия по формуле: (High + Low + 2 * Close) / 4
            outReal[outIdx++] = (inHigh[i] + inLow[i] + inClose[i] * FunctionHelpers.Two<T>()) / FunctionHelpers.Four<T>();
        }

        // Формирование выходного диапазона: от начального индекса до последнего обработанного элемента
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
