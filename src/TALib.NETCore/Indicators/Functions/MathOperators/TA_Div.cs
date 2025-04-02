//Название файла: TA_Div.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//ArithmeticFunctions (альтернатива, если требуется группировка по типу функций)
//ElementWiseOperations (альтернатива для акцента на элементных операциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Arithmetic Division (Math Operators) — Векторное деление (Математические операторы)
    /// </summary>
    /// <param name="inReal0">Первый набор входных значений.</param>
    /// <param name="inReal1">Второй набор входных значений.</param>
    /// <param name="inRange">
    /// Диапазон индексов, определяющий часть данных, которая будет рассчитана в пределах входных наборов.
    /// </param>
    /// <param name="outReal">
    /// Набор для хранения рассчитанных значений.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов, представляющий допустимые данные в пределах выходных наборов.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция делит один ряд данных на другой поэлементно, что полезно для создания соотношений или нормализации индикаторов.
    /// <para>
    /// Функция является строительным блоком для пользовательских индикаторов или сравнений на основе соотношений между инструментами.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Div<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        DivImpl(inReal0, inReal1, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период предыстории для <see cref="Div{T}">Div</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого расчета не требуется исторических данных.</returns>
    [PublicAPI]
    public static int DivLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Div<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        DivImpl<T>(inReal0, inReal1, inRange, outReal, out outRange);

    private static Core.RetCode DivImpl<T>(
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
        // Поэлементное деление входных данных
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = inReal0[i] / inReal1[i];
        }

        // Установка диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
