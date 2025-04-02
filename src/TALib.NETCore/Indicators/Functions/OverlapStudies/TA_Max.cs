// Называние файла: TA_Max.cs
// Правильное название метода должно быть Highest и помещаться должен в (OverlapStudies)
// Группы к которым можно отнести индикатор:
// MathOperators (существующая папка - идеальное соответствие категории)
// StatisticFunctions (альтернатива, если требуется группировка по типу индикатора)
// ExtremaFunctions (альтернатива для акцента на поиске экстремумов)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Highest value over a specified period (Math Operators) — Максимальное значение за указанный период (Математические операторы)
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
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция максимального значения вычисляет максимальное значение за указанный период.
    /// Эта функция часто используется в техническом анализе для определения наивысшей точки в временном ряду за скользящее окно.
    /// <para>
    /// Она часто используется для определения пиков или уровней сопротивления в наборе данных.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Инициализировать максимальное значение и его индекс для первого периода в указанном диапазоне.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Перебрать точки данных в заданном диапазоне:
    ///       - Обновить максимальное значение, если текущая точка данных выше, чем ранее зарегистрированное максимальное значение.
    ///       - Если максимальное значение выходит за пределы скользящего окна, пересчитать максимальное значение для нового окна.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сохранить вычисленное максимальное значение для каждой точки данных в выходном массиве.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходной массив представляет максимальное значение внутри скользящего окна, определенного параметром <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Max<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MaxImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Max{T}">Max</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого выходного значения.</returns>
    [PublicAPI]
    public static int MaxLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Max<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MaxImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MaxImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
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

        var lookbackTotal = MaxLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжить вычисление для запрашиваемого диапазона.
        // Алгоритм позволяет входным и выходным данным быть одним и тем же буфером.
        var outIdx = 0;
        var today = startIdx;
        var trailingIdx = startIdx - lookbackTotal;

        var highestIdx = -1;
        var highest = T.Zero;
        while (today <= endIdx)
        {
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);

            outReal[outIdx++] = highest;
            trailingIdx++;
            today++;
        }

        // Сохранить outBegIdx относительно входных данных вызывающей стороны перед возвратом.
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
