//Название файла: TA_Cosh.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу математических операций)
//TrigonometricFunctions (альтернатива для акцента на тригонометрических функциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Trigonometric Cosh (Math Transform) — Векторная Тригонометрическая Cosh (Математическое Преобразование)
    /// </summary>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
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
    /// <returns>
    /// Возвращает значение типа <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Cosh применяет гиперболическую косинусную функцию к каждой точке данных в ряду, в основном для продвинутого математического моделирования,
    /// а не для стандартного технического анализа.
    /// <para>
    /// Функция редко используется самостоятельно для генерации сигналов. Она может быть интегрирована в специализированные или проприетарные модели,
    /// в сочетании с другими математическими преобразованиями.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Cosh<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        CoshImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Cosh{T}">Cosh</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого вычисления не требуется исторических данных.</returns>
    [PublicAPI]
    public static int CoshLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Cosh<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        CoshImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode CoshImpl<T>(
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
        // Применение гиперболической косинусной функции к каждому элементу в указанном диапазоне
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = T.Cosh(inReal[i]);
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
