//Название файла: TA_Add.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//ArithmeticOperations (альтернатива для акцента на базовых операциях)
//BasicMath (альтернатива для общих математических функций)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Векторное арифметическое сложение (Math Operators)
    /// </summary>
    /// <param name="inReal0">Первый набор входных значений.</param>
    /// <param name="inReal1">Второй набор входных значений.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатываются все элементы входных массивов.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий результаты поэлементного сложения.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i] + inReal1[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых выполнены расчеты:  
    /// - <b>Start</b>: индекс первого элемента, для которого есть результат.  
    /// - <b>End</b>: индекс последнего элемента, для которого есть результат.  
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> при успешном расчете.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>), 
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата из <see cref="Core.RetCode"/>.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете.
    /// </returns>
    /// <remarks>
    /// Функция выполняет поэлементное сложение двух числовых рядов.  
    /// Применяется для создания составных индикаторов или модификации сигналов.  
    /// <para>
    /// Не генерирует торговые сигналы напрямую, но полезна для:  
    /// - Построения пользовательских индикаторов  
    /// - Комбинирования различных технических показателей  
    /// - Корректировки сигналов в торговых стратегиях
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Add<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AddImpl(inReal0, inReal1, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для <see cref="Add{T}"/>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчета не требуется исторических данных.</returns>
    /// <remarks>
    /// Lookback период = 0 означает, что первое валидное значение 
    /// доступно сразу для первого элемента входных данных.
    /// </remarks>
    [PublicAPI]
    public static int AddLookback() => 0;

    /// <remarks>
    /// Реализация для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Add<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AddImpl<T>(inReal0, inReal1, inRange, outReal, out outRange);

    private static Core.RetCode AddImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация диапазона выходных данных

        // Проверка корректности входных диапазонов
        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices; // Начальный и конечный индексы для обработки

        var outIdx = 0; // Индекс для записи результатов в выходной массив
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = inReal0[i] + inReal1[i]; // Поэлементное сложение
        }

        outRange = new Range(startIdx, startIdx + outIdx); // Формирование диапазона валидных данных

        return Core.RetCode.Success;
    }
}
