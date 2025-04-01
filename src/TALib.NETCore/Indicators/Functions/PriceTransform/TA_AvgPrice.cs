//Название файла: TA_AvgPrice.cs
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
    /// <param name="inOpen">Входные данные открытия цен.</param>
    /// <param name="inHigh">Входные данные максимальных цен.</param>
    /// <param name="inLow">Входные данные минимальных цен.</param>
    /// <param name="inClose">Входные данные закрытия цен.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inOpen[outRange.Start + i]</c>, <c>inHigh[outRange.Start + i]</c>, <c>inLow[outRange.Start + i]</c>, <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/>, <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inOpen.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inOpen"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Средняя Цена — это простая трансформация цен, которая вычисляет среднее арифметическое значение цен открытия,
    /// максимума, минимума и закрытия для заданного диапазона.
    /// Она предоставляет единственное значение, представляющее среднюю цену ценной бумаги за период.
    /// <para>
    /// Эта функция полезна для сглаживания колебаний цен и получения представительной цены для анализа.
    /// Комбинирование её с скользящими средними или осцилляторами может помочь в выявлении зон справедливой стоимости или возможностей для возврата к среднему.
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
    /// Возвращает период обратного просмотра для <see cref="AvgPrice{T}">AvgPrice</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого расчета не требуется исторических данных.</returns>
    [PublicAPI]
    public static int AvgPriceLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
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
        // Инициализация диапазона выходных данных
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inOpen.Length, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Получение начального и конечного индексов для обработки
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи в выходной массив
        var outIdx = 0;

        // Вычисление средней цены для каждого элемента в заданном диапазоне
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Вычисление средней цены: (High + Low + Close + Open) / 4
            outReal[outIdx++] = (inHigh[i] + inLow[i] + inClose[i] + inOpen[i]) / FunctionHelpers.Four<T>();
        }

        // Обновление диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
