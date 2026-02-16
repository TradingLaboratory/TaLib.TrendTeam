//Название файла: TA_Div.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по типу функций)
//ArithmeticFunctions (альтернатива для акцента на арифметических операциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Arithmetic Div (Math Operators) — Векторное арифметическое деление (Математические операторы)
    /// </summary>
    /// <param name="inReal0">
    /// Первый массив входных данных (числитель). 
    /// Цены, другие индикаторы или временные ряды.
    /// </param>
    /// <param name="inReal1">
    /// Второй массив входных данных (знаменатель). 
    /// Цены, другие индикаторы или временные ряды.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив входных данных.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i]</c> и <c>inReal1[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента входных массивов, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента входных массивов, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
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
    /// Возвращает период предыстории (lookback) для <see cref="Div{T}">Div</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого расчета не требуется исторических данных. 
    /// Первое валидное значение может быть рассчитано сразу для первого бара входных данных.
    /// </returns>
    [PublicAPI]
    public static int DivLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API.
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
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение индексов начала и конца обработки из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Поэлементное деление входных данных в указанном диапазоне
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = inReal0[i] / inReal1[i];
        }

        // Установка диапазона выходных данных на основе обработанных индексов
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
