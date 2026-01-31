// Файл: Tan.cs
// Группы, к которым можно отнести функцию:
// MathTransform (существующая папка - идеальное соответствие категории)
// MathOperators (альтернатива для базовых математических операций)
// MathFunctions (альтернатива для расширенных математических функций)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Trigonometric Tan (Math Transform) — Векторный тригонометрический тангенс (Математические преобразования)
    /// <para>
    /// Применяет тригонометрическую функцию тангенса к каждому элементу входного массива.
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <param name="inReal">Входные данные для преобразования (временной ряд значений)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения после применения функции тангенса.  
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
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция TAN применяет тригонометрическую функцию тангенса, обычно используемую в специализированных или экспериментальных сценариях анализа.
    /// <para>
    /// Функция не применяется самостоятельно в классическом техническом анализе. Рассматривайте её использование только в сложных, кастомных моделях.
    /// </para>
    /// <para>
    /// Обратите внимание: тангенс не определён для углов ±90° + 180°·k (где k — целое число), что может привести к бесконечным значениям при определённых входных данных.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Tan<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TanImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для функции <see cref="Tan{T}">Tan</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчёта тангенса не требуется исторических данных.</returns>
    [PublicAPI]
    public static int TanLookback() => 0;

    /// <remarks>
    /// Для обеспечения совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Tan<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TanImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode TanImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и достаточности данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Применение функции тангенса к каждому элементу входного диапазона
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Расчёт тангенса текущего значения: outReal[outIdx] = tan(inReal[i])
            outReal[outIdx++] = T.Tan(inReal[i]);
        }

        // Формирование выходного диапазона: все обработанные элементы имеют валидные значения
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
