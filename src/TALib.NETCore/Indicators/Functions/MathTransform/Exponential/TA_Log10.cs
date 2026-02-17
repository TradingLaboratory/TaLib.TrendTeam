//Название файла: TA_Log10.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу операций)
//StatisticFunctions (альтернатива для акцента на статистических функциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Log10 (Math Transform) — Логарифм по основанию 10 (Математическое преобразование)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// <para>
    /// Для данного метода это массив числовых значений, к которым применяется логарифмическое преобразование.
    /// </para>
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет起始ный и конечный индексы для вычислений.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
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
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// LOG10 применяет логарифмическое преобразование по основанию 10, аналогичное LN.
    /// <para>
    /// Функция может помочь в нормализации масштаба данных. Использование её в более широких количественных стратегиях
    /// может улучшить интерпретацию данных и их сопоставимость.
    /// </para>
    /// <para>
    /// Логарифм по основанию 10 полезен для работы с данными, имеющими экспоненциальный характер роста,
    /// например, цены активов или объёмы торгов.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Log10<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        Log10Impl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback period) для <see cref="Log10{T}">Log10</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого вычисления не требуется исторических данных.
    /// <para>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно получить валидное значение индикатора.
    /// Для Log10 все бары могут быть обработаны немедленно без необходимости в предыдущих значениях.
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int Log10Lookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Этот метод обеспечивает совместимость с массивами вместо Span&lt;T&gt;.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Log10<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        Log10Impl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode Log10Impl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация outRange - диапазон выходных данных по умолчанию
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        // ValidateInputRange возвращает кортеж (startIdx, endIdx) или null при ошибке
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Вычисление логарифма по основанию 10 для каждого элемента в указанном диапазоне
        // T.Log10() - встроенный метод для вычисления десятичного логарифма
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = T.Log10(inReal[i]);
        }

        // Установка диапазона выходных данных
        // Start: начальный индекс во входных данных
        // End: индекс после последнего обработанного элемента (startIdx + количество обработанных элементов)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
