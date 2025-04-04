//Название файла: TA_MinMax.cs
// Правильное название метода должно быть LowestHighest и помещаться должен в (OverlapStudies)
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по типу индикатора)
//PriceTransform (альтернатива для акцента на трансформации цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MinMax (Math Operators) — Минимальные и максимальные значения за указанный период (Математические операторы)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMin">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMin[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outMax">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMax[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMin"/> и <paramref name="outMax"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMin"/> и <paramref name="outMax"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция MinMax рассчитывает минимальные и максимальные значения в ряду данных за указанный период.
    /// Она часто используется в техническом анализе для определения экстремумов в скользящем окне данных.
    /// <para>
    /// Используйте функции <see cref="Min{T}">Min</see> или <see cref="Max{T}">Max</see>, если требуется только минимальное или максимальное значение соответственно.
    /// Используйте функцию <see cref="MinMaxIndex{T}">MinMaxIndex</see>, если требуются индексы минимальных и максимальных значений вместо самих значений.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить диапазон индексов для оценки минимальных и максимальных значений на основе входного диапазона и периода времени:
    ///       <code>
    ///         Range = [trailingIdx, today]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определить минимальные и максимальные значения в диапазоне:
    /// <code>
    /// Lowest = Min(inReal[i] for i in Range)
    /// Highest = Max(inReal[i] for i in Range)
    /// </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <paramref name="outMin"/> содержит минимальные значения для каждого скользящего периода времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <paramref name="outMax"/> содержит максимальные значения для каждого скользящего периода времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Эти значения могут использоваться для отслеживания уровней поддержки и сопротивления или выявления трендов.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MinMax<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMin,
        Span<T> outMax,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxImpl(inReal, inRange, outMin, outMax, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предварительного просмотра для <see cref="MinMax{T}">MinMax</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    [PublicAPI]
    public static int MinMaxLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MinMax<T>(
        T[] inReal,
        Range inRange,
        T[] outMin,
        T[] outMax,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxImpl<T>(inReal, inRange, outMin, outMax, out outRange, optInTimePeriod);

    private static Core.RetCode MinMaxImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMin,
        Span<T> outMax,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = MinMaxLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжить расчет для запрашиваемого диапазона.
        // Алгоритм позволяет использовать один и тот же буфер для входных и выходных данных.
        var outIdx = 0;
        var today = startIdx;
        var trailingIdx = startIdx - lookbackTotal;

        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;
        while (today <= endIdx)
        {
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

            outMax[outIdx] = highest;
            outMin[outIdx++] = lowest;
            trailingIdx++;
            today++;
        }

        // Сохранить outBegIdx относительно входных данных перед возвратом.
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
