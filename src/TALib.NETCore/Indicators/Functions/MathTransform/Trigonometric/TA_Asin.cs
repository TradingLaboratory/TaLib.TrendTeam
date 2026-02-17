//Название файла: TA_Asin.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу математических операций)
//TrigonometricFunctions (альтернатива для акцента на тригонометрических функциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Trigonometric ASin (Math Transform) — Векторная тригонометрическая ASin (Математическое преобразование)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// <para>
    /// Для функции ASin входные значения должны находиться в диапазоне от -1 до 1 включительно,
    /// так как арксинус определён только для этих значений.
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
    /// - Содержит результаты применения функции арксинуса к каждому элементу входного диапазона.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// - Для ASin lookback период равен 0, поэтому валидные значения доступны для всех элементов входного диапазона.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// ASin применяет функцию арксинуса (обратного синуса) к каждой точке данных в ряду, в основном для продвинутого математического моделирования,
    /// а не для стандартного технического анализа.
    /// <para>
    /// Функция редко используется самостоятельно для генерации сигналов. Она может быть интегрирована в специализированные или проприетарные модели,
    /// в комбинации с другими математическими преобразованиями.
    /// </para>
    /// <para>
    /// Математическая формула: <c>outReal[i] = arcsin(inReal[i])</c>
    /// </para>
    /// <para>
    /// Область определения: входные значения должны быть в диапазоне [-1, 1].
    /// Область значений: результат находится в диапазоне [-π/2, π/2] радиан (от -1.5708 до 1.5708).
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Asin<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AsinImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период предыстории (lookback) для <see cref="Asin{T}">Asin</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого вычисления не требуется исторических данных.
    /// <para>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// Все бары в исходных данных с индексом меньше чем lookback будут пропущены, чтобы посчитать первое валидное значение индикатора.
    /// Для ASin первое валидное значение доступно сразу для первого элемента входных данных.
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int AsinLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Этот метод обеспечивает совместимость с версиями API, использующими массивы вместо Span&lt;T&gt;.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Asin<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AsinImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode AsinImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация outRange - диапазон выходных данных по умолчанию (пустой диапазон)
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
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Применение функции арксинуса к каждому элементу входных данных
            // Формула: outReal[outIdx] = arcsin(inReal[i])
            // Результат в радианах в диапазоне [-π/2, π/2]
            outReal[outIdx++] = T.Asin(inReal[i]);
        }

        // Установка диапазона выходных данных
        // Start: индекс первого элемента inReal, для которого есть валидное значение
        // End: индекс последнего элемента inReal, для которого есть валидное значение
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
