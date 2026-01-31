// Название файла: Sqrt.cs
// Группы, к которым можно отнести индикатор:
// MathTransform (существующая папка - идеальное соответствие категории)
// MathOperators (альтернатива для базовых математических операций)
// StatisticFunctions (альтернатива при использовании в статистических расчётах)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Sqrt (Math Transform) — Квадратный корень (Математическое преобразование)
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта: временной ряд значений (цены, волатильность, другие индикаторы)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения результата операции квадратного корня.  
    /// - Длина массива равна <c>outRange.End.Value - outRange.Start.Value</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start.Value + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal.Length</c> (последний элемент входных данных), если расчёт успешен.  
    /// - Если входные данные некорректны, возвращается <c>Range.EndAt(0)</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата операции <see cref="Core.RetCode"/>:  
    /// - <see cref="Core.RetCode.Success"/> — расчёт успешно выполнен.  
    /// - <see cref="Core.RetCode.OutOfRangeParam"/> — некорректный диапазон входных данных.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция применяет математическую операцию квадратного корня к каждому элементу входного массива.
    /// Используется в расчётах волатильности (например, при вычислении стандартного отклонения) и других статистических показателей.
    /// </para>
    /// <para>
    /// Применение квадратного корня позволяет нормализовать распределение данных и улучшить интерпретацию показателей риска.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sqrt<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SqrtImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период задержки (lookback) для функции <see cref="Sqrt{T}"/>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчёта квадратного корня не требуется исторических данных.</returns>
    [PublicAPI]
    public static int SqrtLookback() => 0;

    /// <remarks>
    /// Для обеспечения совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sqrt<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SqrtImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode SqrtImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением (0 элементов)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности start/end индексов относительно длины inReal
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов для обработки из валидированного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив outReal
        var outIdx = 0;

        // Применение функции квадратного корня к каждому элементу входного диапазона
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Расчёт квадратного корня от текущего элемента входного массива
            outReal[outIdx++] = T.Sqrt(inReal[i]);
        }

        // Формирование выходного диапазона: 
        // Start = startIdx (первый обработанный элемент входных данных)
        // End = startIdx + outIdx (последний обработанный элемент)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
