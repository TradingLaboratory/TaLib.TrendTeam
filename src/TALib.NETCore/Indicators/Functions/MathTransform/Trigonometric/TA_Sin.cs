//Название файла: TA_Sin.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//TrigonometricFunctions (альтернатива, если требуется группировка по типу функции)
//AdvancedAnalysis (альтернатива для акцента на продвинутом анализе)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Trigonometric Sin (Math Transform) — Векторная тригонометрическая функция SIN (Математическое преобразование)
    /// </summary>
    /// <typeparam name="T">
    /// Тип числовых данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// <para>
    /// Для функции SIN входные данные представляют собой значения в радианах, 
    /// к которым применяется тригонометрическая функция синуса.
    /// </para>
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет начальную и конечную позиции для вычисления значений индикатора.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Содержит результаты применения функции синуса к входным данным.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// SIN применяет синусоидальную функцию к данным, обычно для продвинутого или экспериментального анализа.
    /// <para>
    /// Функция редко используется самостоятельно. Её применение обычно возникает при разработке сложных индикаторов или анализе циклов.
    /// </para>
    /// <para>
    /// <b>Математическая формула:</b> <c>outReal[i] = sin(inReal[i])</c>
    /// </para>
    /// <para>
    /// <b>Область значений:</b> результат функции синуса находится в диапазоне [-1, 1].
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sin<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SinImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период предыстории (lookback period) для <see cref="Sin{T}">Sin</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого вычисления не требуется исторических данных.
    /// <para>
    /// Функция SIN является точечным преобразованием — каждое выходное значение зависит только от соответствующего входного значения.
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int SinLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Этот метод обеспечивает совместимость с версиями API, использующими массивы вместо Span&lt;T&gt;.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sin<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        SinImpl<T>(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Основная реализация функции SIN.
    /// </summary>
    /// <typeparam name="T">Тип числовых данных, реализующий <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <param name="inReal">Входные данные (значения в радианах).</param>
    /// <param name="inRange">Диапазон обрабатываемых данных.</param>
    /// <param name="outReal">Выходной массив для результатов вычисления.</param>
    /// <param name="outRange">Диапазон валидных выходных значений.</param>
    /// <returns>Код результата операции <see cref="Core.RetCode"/>.</returns>
    private static Core.RetCode SinImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона - по умолчанию пустой диапазон
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        // ValidateInputRange возвращает кортеж (startIdx, endIdx) если диапазон валиден, или null если нет
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Счётчик для записи результатов в выходной массив
        var outIdx = 0;

        // Основной цикл вычисления: применяем функцию синуса к каждому элементу входных данных
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Применение синусоидальной функции к каждому элементу входных данных
            // T.Sin() - стандартная функция синуса для типа T, реализующего IFloatingPointIeee754
            outReal[outIdx++] = T.Sin(inReal[i]);
        }

        // Установка выходного диапазона: от startIdx до startIdx + количество вычисленных значений
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
