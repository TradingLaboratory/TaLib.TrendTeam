//Название файла: TA_Floor.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу операций)
//DataNormalization (альтернатива для акцента на нормализации данных)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Floor (Math Transform) — Векторное округление вниз (Математическое преобразование)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция округляет каждую точку данных вниз до ближайшего целого числа, служа инструментом для предварительной обработки данных.
    /// <para>
    /// Функция полезна для разработки пользовательских индикаторов или нормализации данных, не предоставляя прямых торговых сигналов.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Floor<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        FloorImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Floor{T}">Floor</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого расчета не требуется исторических данных.</returns>
    [PublicAPI]
    public static int FloorLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Floor<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        FloorImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode FloorImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Получение начального и конечного индексов диапазона
        var (startIdx, endIdx) = rangeIndices;

        var outIdx = 0;
        // Округление каждого элемента в диапазоне вниз до ближайшего целого числа
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = T.Floor(inReal[i]);
        }

        // Установка диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
