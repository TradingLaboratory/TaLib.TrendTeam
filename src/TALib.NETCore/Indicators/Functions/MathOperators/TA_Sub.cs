
//Название файла TA_Sub.cs

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Векторная арифметическая операция вычитания (Математические операторы)
    /// </summary>
    /// <param name="inReal0">Первый набор входных значений.</param>
    /// <param name="inReal1">Второй набор входных значений.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal0"/> и <paramref name="inReal1"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal0"/> и <paramref name="inReal1"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal0"/> и <paramref name="inReal1"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// SUB вычитает один ряд данных из другого, что полезно для индикаторов спрэда или сравнительного анализа.
    /// <para>
    /// Функция является фундаментальной операцией в разработке пользовательских индикаторов.
    /// Интеграция её в стратегии парного трейдинга или межрыночного сравнения может дать инсайты относительной силы.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sub<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SubImpl(inReal0, inReal1, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период предыстории для <see cref="Sub{T}">Sub</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого вычисления не требуется исторических данных.</returns>
    [PublicAPI]
    public static int SubLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sub<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SubImpl<T>(inReal0, inReal1, inRange, outReal, out outRange);

    private static Core.RetCode SubImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var outIdx = 0;
        // Вычитание значений из одного массива из другого в указанном диапазоне
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = inReal0[i] - inReal1[i];
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}

