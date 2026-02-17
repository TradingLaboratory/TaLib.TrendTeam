//Название файла *.cs
//Группы к которым можно отнести индикатор:
//PriceTransform (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу трансформации)
//BasicIndicators (альтернатива для акцента на базовых индикаторах)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Average Price (Price Transform) — Средняя Цена (Трансформация Цен)
    /// </summary>
    /// <param name="inOpen">
    /// Входные данные цен открытия (Open).  
    /// Массив содержит цены открытия для каждого бара в анализируемом периоде.
    /// </param>
    /// <param name="inHigh">
    /// Входные данные максимальных цен (High).  
    /// Массив содержит максимальные цены для каждого бара в анализируемом периоде.
    /// </param>
    /// <param name="inLow">
    /// Входные данные минимальных цен (Low).  
    /// Массив содержит минимальные цены для каждого бара в анализируемом периоде.
    /// </param>
    /// <param name="inClose">
    /// Входные данные цен закрытия (Close).  
    /// Массив содержит цены закрытия для каждого бара в анализируемом периоде.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// <para>
    /// - Если не указан, обрабатываются все элементы входных массивов (<paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>).  
    /// - Все входные массивы должны иметь одинаковую длину.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует средней цене для бара с индексом <c>inRange.Start + i</c>.  
    /// - Значения вычисляются по формуле: <c>(Open + High + Low + Close) / 4</c>.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения индикатора:  
    /// <para>
    /// - <b>Start</b>: индекс первого элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inOpen.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,  
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.  
    /// <para>
    /// - Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении.  
    /// - Возвращает соответствующий код ошибки в противном случае (например, <see cref="Core.RetCode.OutOfRangeParam"/>).
    /// </para>
    /// </returns>
    /// <remarks>
    /// Средняя Цена (Average Price) — это простая трансформация цен, которая вычисляет среднее арифметическое значение  
    /// цен открытия (Open), максимума (High), минимума (Low) и закрытия (Close) для каждого бара.  
    /// <para>
    /// Формула расчета: <c>AvgPrice = (Open + High + Low + Close) / 4</c>
    /// </para>
    /// <para>
    /// Эта функция полезна для сглаживания колебаний цен и получения представительной цены для анализа.  
    /// Средняя цена предоставляет единственное значение, представляющее среднюю цену ценной бумаги за период,  
    /// что может быть более информативным, чем использование только цены закрытия.
    /// </para>
    /// <para>
    /// Комбинирование её со скользящими средними или осцилляторами может помочь в выявлении зон справедливой стоимости  
    /// или возможностей для возврата к среднему (mean reversion).
    /// </para>
    /// <para>
    /// <b>Lookback период:</b> 0 (не требуется исторических данных для расчета).  
    /// <b>outRange:</b> Диапазон индексов входных данных, для которых рассчитаны валидные значения индикатора.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode AvgPrice<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AvgPriceImpl(inOpen, inHigh, inLow, inClose, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (Lookback) для <see cref="AvgPrice{T}">AvgPrice</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого расчета не требуется исторических данных.  
    /// Средняя цена вычисляется для каждого бара независимо на основе данных текущего бара.
    /// </returns>
    [PublicAPI]
    public static int AvgPriceLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API.  
    /// Этот метод обеспечивает совместимость с версиями API, использующими массивы вместо Span.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode AvgPrice<T>(
        T[] inOpen,
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AvgPriceImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outReal, out outRange);

    private static Core.RetCode AvgPriceImpl<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация диапазона выходных данных (outRange)
        // Начальное значение указывает на пустой диапазон
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона (inRange)
        // ValidateInputRange проверяет, что все входные массивы имеют достаточную длину
        // и что диапазон inRange находится в допустимых пределах
        if (FunctionHelpers.ValidateInputRange(inRange, inOpen.Length, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Получение начального и конечного индексов для обработки из валидированного диапазона
        // startIdx - индекс первого бара для расчета
        // endIdx - индекс последнего бара для расчета
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи в выходной массив outReal
        // Начинается с 0 и увеличивается для каждого рассчитанного значения
        var outIdx = 0;

        // Вычисление средней цены для каждого элемента в заданном диапазоне
        // Цикл проходит по всем барам от startIdx до endIdx включительно
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Вычисление средней цены по формуле: (Open + High + Low + Close) / 4
            // inOpen[i] - цена открытия текущего бара
            // inHigh[i] - максимальная цена текущего бара
            // inLow[i] - минимальная цена текущего бара
            // inClose[i] - цена закрытия текущего бара
            // FunctionHelpers.Four<T>() возвращает константу 4 для типа T
            outReal[outIdx++] = (inHigh[i] + inLow[i] + inClose[i] + inOpen[i]) / FunctionHelpers.Four<T>();
        }

        // Обновление диапазона выходных данных (outRange)
        // Start - индекс первого бара во входных данных с валидным значением
        // End - индекс последнего бара во входных данных с валидным значением (startIdx + outIdx)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
