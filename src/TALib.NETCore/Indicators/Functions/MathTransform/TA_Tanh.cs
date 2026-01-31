//Название файла: Tanh.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива для базовых математических операций)
//NonLinearTransforms (альтернатива для нелинейных преобразований)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Trigonometric Tanh (Math Transform) — Векторный гиперболический тангенс (Математическое преобразование)
    /// <para>
    /// Применяет гиперболическую функцию тангенса (tanh) к каждому элементу входного массива.
    /// </para>
    /// <para>
    /// Результат преобразования лежит в диапазоне [-1, 1], что может использоваться для нормализации данных
    /// или создания нелинейных преобразований в исследовательских моделях.
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
    /// Массив, содержащий ТОЛЬКО валидные значения после применения функции tanh.  
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
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении, иначе соответствующий код ошибки.
    /// </returns>
    /// <remarks>
    /// Функция tanh редко применяется напрямую в классическом техническом анализе.
    /// <para>
    /// Может использоваться в исследовательских моделях, нейросетях или для нормализации данных
    /// в диапазон [-1, 1] перед применением других алгоритмов.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Tanh<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TanhImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для метода <see cref="Tanh{T}"/>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчёта не требуется исторических данных.</returns>
    [PublicAPI]
    public static int TanhLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Tanh<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TanhImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode TanhImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона значением [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и достаточности данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Применение гиперболического тангенса к текущему элементу входного массива
            outReal[outIdx++] = T.Tanh(inReal[i]);
        }

        // Формирование выходного диапазона: все обработанные элементы имеют валидные значения
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
