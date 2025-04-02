//Название файла: TA_Ceil.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу операции)
//DataPreprocessing (альтернатива для акцента на предобработке данных)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Ceil (Math Transform) — Векторное округление вверх (Математическое преобразование)
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Округление каждой точки данных вверх до ближайшего целого числа, в основном для предварительной обработки данных.
    /// <para>
    /// Функция не предоставляет прямых торговых сигналов.
    /// Её полезность проявляется в разработке пользовательских индикаторов или в подготовке данных для сложных преобразований.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Ceil<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        CeilImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Ceil{T}">Ceil</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого вычисления не требуется исторических данных.</returns>
    [PublicAPI]
    public static int CeilLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Ceil<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        CeilImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode CeilImpl<T>(
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

        var (startIdx, endIdx) = rangeIndices;

        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Округление каждого элемента вверх до ближайшего целого числа
            outReal[outIdx++] = T.Ceiling(inReal[i]);
        }

        // Установка диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
