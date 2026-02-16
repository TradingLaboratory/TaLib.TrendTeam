//Название файла: TA_MinIndex.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по типу индикатора)
//IndexCalculations (альтернатива для акцента на расчетах индексов)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Index of lowest value over a specified period (Math Operators) — Индекс минимального значения за указанный период (Математические операторы)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outInteger">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outInteger[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени (Time Period) — количество баров для анализа.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция MinIndex вычисляет индекс минимального значения в ряду данных за указанный период.
    /// Обычно используется в техническом анализе для определения, где произошло минимальное значение в скользящем окне данных.
    /// <para>
    /// Функция <see cref="Min{T}">Min</see> может быть использована для получения самого минимального значения, а не его индекса.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить диапазон индексов для оценки минимального значения на основе входного диапазона и периода времени:
    ///       <code>
    ///         Range = [trailingIdx, today]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определить индекс минимального значения в диапазоне:
    ///       <code>
    ///         LowestIndex = IndexOfMin(inReal[i] for i in Range)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходные данные предоставляют относительный индекс внутри входного диапазона, где было найдено минимальное значение в каждом временном окне.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Примечание о lookback периоде</b>:
    /// <para>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// Все бары в исходных данных с индексом меньше чем lookback будут пропущены, чтобы посчитать первое валидное значение индикатора.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MinIndex<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<int> outInteger,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinIndexImpl(inReal, inRange, outInteger, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback period) для <see cref="MinIndex{T}">MinIndex</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени (Time Period) — количество баров для анализа.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения индикатора.</returns>
    /// <remarks>
    /// Lookback период определяет, сколько баров нужно пропустить в начале данных перед тем, как можно будет рассчитать первое валидное значение индикатора.
    /// Для MinIndex lookback период равен <c>optInTimePeriod - 1</c>.
    /// </remarks>
    [PublicAPI]
    public static int MinIndexLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MinIndex<T>(
        T[] inReal,
        Range inRange,
        int[] outInteger,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinIndexImpl<T>(inReal, inRange, outInteger, out outRange, optInTimePeriod);

    private static Core.RetCode MinIndexImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<int> outInteger,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализируем outRange как пустой диапазон, начинающийся с 0
        outRange = Range.EndAt(0);

        // Проверяем корректность входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлекаем начальный и конечный индексы из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Проверяем, что период времени не меньше минимально допустимого значения (2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Вычисляем общий lookback период - количество баров, которые нужно пропустить перед первым валидным значением
        var lookbackTotal = MinIndexLookback(optInTimePeriod);
        // Корректируем начальный индекс с учётом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, данных недостаточно для расчёта
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжаем вычисление для запрашиваемого диапазона.
        // Алгоритм позволяет использовать один и тот же буфер для входных и выходных данных.

        // Индекс для записи результатов в выходной массив outInteger
        var outIdx = 0;
        // Текущий индекс бара, для которого производится расчёт
        var today = startIdx;
        // Начальный индекс скользящего окна (trailing index) - определяет начало периода анализа
        var trailingIdx = startIdx - lookbackTotal;

        // Индекс минимального значения в текущем окне
        var lowestIdx = -1;
        // Минимальное значение в текущем окне
        var lowest = T.Zero;

        // Основной цикл расчёта индикатора для каждого бара в диапазоне
        while (today <= endIdx)
        {
            // Вычисляем индекс и значение минимума в диапазоне [trailingIdx, today]
            // Функция обновляет lowestIdx и lowest для текущего окна
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

            // Записываем индекс минимального значения в выходной массив
            outInteger[outIdx++] = lowestIdx;
            // Сдвигаем начало окна на один бар вперёд
            trailingIdx++;
            // Переходим к следующему бару
            today++;
        }

        // Сохраняем outRange относительно входных данных перед возвратом.
        // outRange.Start - индекс первого элемента inReal с валидным значением
        // outRange.End - индекс последнего элемента inReal с валидным значением
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
