//Sinh.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива для базовых математических операций)
//MathFunctions (альтернатива для специализированных математических функций)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Trigonometric Sinh (Math Transform) — Векторное гиперболическое преобразование Sinh (Математические преобразования)
    /// <para>
    /// Применяет функцию гиперболического синуса (hyperbolic sine) к каждому элементу входного массива.
    /// </para>
    /// <para>
    /// Функция имеет ограниченное применение в классическом техническом анализе.
    /// Может использоваться в проприетарных или экспериментальных моделях анализа рынка.
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <param name="inReal">Входные данные для преобразования (цены, значения индикаторов или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения после преобразования.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </param>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу выполнения расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте, иначе соответствующий код ошибки.
    /// </returns>
    /// <remarks>
    /// SINH применяет функцию гиперболического синуса к входным данным для специализированных аналитических целей.
    /// <para>
    /// Функция имеет ограниченное распространение в практике технического анализа.
    /// Может встречаться в проприетарных или экспериментальных моделях.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sinh<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SinhImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для метода <see cref="Sinh{T}">Sinh</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчёта не требуется исторических данных.</returns>
    [PublicAPI]
    public static int SinhLookback() => 0;

    /// <remarks>
    /// Для обеспечения совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sinh<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SinhImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode SinhImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона значением [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности границ и достаточности данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Применение гиперболического синуса к каждому элементу входного диапазона
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Расчёт гиперболического синуса текущего значения: sinh(x) = (e^x - e^(-x)) / 2
            outReal[outIdx++] = T.Sinh(inReal[i]);
        }

        // Формирование выходного диапазона: все обработанные элементы имеют валидные значения
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
