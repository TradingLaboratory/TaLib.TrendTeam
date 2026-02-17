//Название файла: TA_Var.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//VolatilityIndicators (альтернатива, если требуется группировка по типу индикатора)
//DispersionMeasures (предложение для подпапки внутри StatisticFunctions)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Variance (Statistic Functions) — Дисперсия (Статистические функции)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени для расчета дисперсии.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция дисперсии измеряет разброс или дисперсию набора значений вокруг их среднего.
    /// Она дает представление о волатильности или изменчивости данных за указанный период.
    /// <para>
    /// Функция может использоваться для оценки риска или в количественных стратегиях.
    /// Комбинирование с стандартным отклонением или Бета может уточнить понимание условий волатильности.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить среднее (среднее значение) точек данных за указанный период времени:
    ///       <code>
    ///         Mean = Sum(DataPoints) / TimePeriod
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить квадрат разности каждой точки данных от среднего:
    ///       <code>
    ///         Squared Difference = (Value - Mean)²
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить дисперсию, усреднив эти квадраты разностей:
    ///       <code>
    ///         Variance = Σ(Squared Differences) / Period
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокое значение указывает на большую изменчивость или волатильность данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшее значение свидетельствует о том, что точки данных ближе к среднему, указывая на меньшую волатильность.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Дисперсия часто используется как основа для других индикаторов, таких как стандартное отклонение.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Var<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
        VarImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="Var{T}">Var</see>.
    /// Это количество баров, которое необходимо пропустить в начале входных данных для получения первого валидного значения индикатора.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета дисперсии.</param>
    /// <returns>Индекс первого бара во входных данных, для которого доступно валидное значение индикатора.</returns>
    [PublicAPI]
    public static int VarLookback(int optInTimePeriod = 5) => optInTimePeriod < 1 ? -1 : optInTimePeriod - 1;

    /// <summary>
    /// Реализация метода Var для совместимости с абстрактным API.
    /// </summary>
    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Var<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
        VarImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode VarImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением по умолчанию
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка валидности периода времени
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Вычисление дисперсии с использованием вспомогательного метода
        return FunctionHelpers.CalcVariance(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outReal, out outRange,
            optInTimePeriod);
    }
}
