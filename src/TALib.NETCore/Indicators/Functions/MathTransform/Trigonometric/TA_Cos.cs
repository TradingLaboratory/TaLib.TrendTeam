//Название файла: TA_Cos.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу математических операций)
//TrigonometricFunctions (альтернатива для акцента на тригонометрических функциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Cos (Math Transform) — Косинус (Математическое преобразование)
    /// </summary>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора — массив числовых значений.
    /// <para>
    /// Может содержать цены (Open, High, Low, Close), значения других индикаторов
    /// или любые другие временные ряды, к которым применима тригонометрическая функция.
    /// </para>
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Позволяет вычислять индикатор только для части входных данных.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Содержит результаты вычисления косинуса для каждого элемента входного диапазона.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении,
    /// или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Функция Cos применяет косинусную функцию к каждой точке данных в ряду.
    /// <para>
    /// В основном используется для продвинутого математического моделирования,
    /// а не для стандартного технического анализа.
    /// </para>
    /// <para>
    /// Функция редко используется самостоятельно для генерации торговых сигналов.
    /// Она может быть интегрирована в специализированные или проприетарные модели
    /// в сочетании с другими математическими преобразованиями.
    /// </para>
    /// <para>
    /// <b>Lookback период:</b> 0 — для вычисления не требуется исторических данных,
    /// так как косинус вычисляется для каждого значения независимо.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Cos<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        CosImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (Lookback) для <see cref="Cos{T}">Cos</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для вычисления косинуса не требуется исторических данных.
    /// <para>
    /// Каждый элемент выходного массива вычисляется независимо от предыдущих значений.
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int CosLookback() => 0;

    /// <remarks>
    /// Вспомогательный метод для совместимости с абстрактным API.
    /// <para>
    /// Обеспечивает работу с массивами вместо Span&lt;T&gt; для обратной совместимости.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Cos<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        CosImpl<T>(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Основная реализация вычисления косинуса для входных данных.
    /// </summary>
    /// <typeparam name="T">
    /// Числовой тип данных, реализующий <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <param name="inReal">Входной массив данных для вычисления косинуса.</param>
    /// <param name="inRange">Диапазон индексов во входном массиве для обработки.</param>
    /// <param name="outReal">Выходной массив для хранения результатов вычислений.</param>
    /// <param name="outRange">Диапазон валидных выходных значений (индексы первой и последней ячейки).</param>
    /// <returns>Код результата операции <see cref="Core.RetCode"/>.</returns>
    private static Core.RetCode CosImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона - по умолчанию пустой диапазон
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        // Возвращает null если диапазон некорректен или выходит за границы массива
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Получение начального и конечного индексов диапазона для обработки
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Вычисление косинуса для каждого элемента в заданном диапазоне
        // Каждая точка данных обрабатывается независимо от других
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = T.Cos(inReal[i]);
        }

        // Установка диапазона выходных данных
        // Start - индекс первого обработанного элемента
        // End - индекс следующего за последним обработанным элементом
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
