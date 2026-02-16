//Название файла: TA_Add.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//Arithmetic (рекомендуемая подпапка для базовых арифметических операций)
//ElementWiseOperations (альтернатива для операций поэлементной обработки)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Arithmetic Addition (Math Operators) — Векторное арифметическое сложение (Математические операторы)
    /// <para>
    /// Выполняет поэлементное сложение двух временных рядов: <c>outReal[i] = inReal0[i] + inReal1[i]</c>.
    /// </para>
    /// </summary>
    /// <param name="inReal0">Первый входной временной ряд (цены, значения индикаторов или другие числовые последовательности)</param>
    /// <param name="inReal1">Второй входной временной ряд (должен иметь ту же длину, что и <paramref name="inReal0"/>)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатываются все элементы входных массивов.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения результата сложения.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i] + inReal1[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в исходных данных, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal0"/>/<paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal0"/>/<paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>), 
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата из <see cref="Core.RetCode"/>.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении операции.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Базовая арифметическая операция для работы с временными рядами. Не является индикатором технического анализа 
    /// в классическом понимании, но широко применяется для:
    /// - Построения составных индикаторов
    /// - Комбинирования сигналов от разных источников
    /// - Корректировки значений индикаторов
    /// - Создания пользовательских математических преобразований
    /// </para>
    /// <para>
    /// Особенности:
    /// - Не требует lookback периода (расчет возможен с первого бара)
    /// - Требует равной длины входных массивов
    /// - Работает с любыми числовыми типами с плавающей точкой
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
    /// Возвращает период lookback для операции <see cref="Add{T}"/>.
    /// </summary>
    /// <returns>Всегда 0, так как для поэлементного сложения не требуется предыстория данных.</returns>
    /// <remarks>
    /// Lookback период = 0 означает, что первое валидное значение 
    /// доступно сразу для первого элемента входных данных (бар с индексом 0).
    /// </remarks>
    [PublicAPI]
    public static int AddLookback() => 0;

    /// <remarks>
    /// Реализация для совместимости с устаревшим API (массивы вместо Span)
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
        outRange = Range.EndAt(0); // Инициализация диапазона выходных данных значением [0, 0)

        // Проверка корректности входных диапазонов и получение индексов начала и конца обработки
        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices; // Начальный и конечный индексы для обработки данных

        var outIdx = 0; // Индекс для записи результатов в выходной массив outReal
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Поэлементное сложение значений из двух входных временных рядов
            outReal[outIdx++] = inReal0[i] + inReal1[i];
        }

        // Формирование диапазона валидных выходных данных: от startIdx до последнего обработанного индекса
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
