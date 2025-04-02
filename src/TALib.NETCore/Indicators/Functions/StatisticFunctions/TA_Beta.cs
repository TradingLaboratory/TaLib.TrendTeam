//Название файла: TA_Beta
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//VolatilityIndicators (альтернатива, если требуется группировка по типу индикатора)
//RiskManagement (альтернатива для акцента на управлении рисками)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Beta (Statistic Functions) — Бета (Статистические функции)
    /// </summary>
    /// <param name="inReal0">Входные данные для расчета индикатора (цены акций или других финансовых инструментов)</param>
    /// <param name="inReal1">Входные данные для расчета индикатора (цены бенчмарка или рыночные цены)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal0"/> и <paramref name="inReal1"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal0"/> и <paramref name="inReal1"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal0"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция Бета рассчитывает коэффициент бета, который измеряет волатильность акции относительно бенчмарка.
    /// <para>
    /// Функция может использоваться в управлении рисками и выборе портфеля.
    /// Комбинирование с другими мерами риска или методами нормализации может оптимизировать выбор инструментов.
    /// </para>
    ///
    /// <b>Формула расчета</b>:
    /// <code>
    ///   Beta = Covariance(Security, Market) / Variance(Market)
    /// </code>
    /// где ковариация и дисперсия выводятся из процентных изменений цен акции и рынка
    /// за указанный период времени.
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение 1 указывает, что цена акции движется вместе с рынком.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение больше 1 предполагает, что акция более волатильна, чем рынок.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение меньше 1 указывает, что акция менее волатильна, чем рынок.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Beta<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
        BetaImpl(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Beta{T}">Beta</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int BetaLookback(int optInTimePeriod = 5) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Beta<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 5) where T : IFloatingPointIeee754<T> =>
        BetaImpl<T>(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode BetaImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Алгоритм Бета измеряет волатильность акции относительно индекса. Цены акций задаются в inReal0,
         * а цены индекса — в inReal1. Размер этих векторов должен быть одинаковым.
         * Алгоритм заключается в вычислении изменений между ценами в обоих векторах, а затем в "построении" этих изменений
         * как точек на евклидовой плоскости. Значение x точки — это доходность рынка, а значение y — доходность акции.
         * Значение бета — это наклон линейной регрессии через эти точки. Бета равная 1 — это просто линия y=x,
         * то есть акция варьируется точно с рынком. Бета меньше единицы означает, что акция варьируется меньше,
         * чем рынок, а бета больше единицы — что акция варьируется больше рынка.
         */

        var lookbackTotal = BetaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        T sxy, sx, sy;
        var sxx = sxy = sx = sy = T.Zero;
        var trailingIdx = startIdx - lookbackTotal;
        var trailingLastPriceX = inReal0[trailingIdx]; // та же цена, что и lastPriceX, но используется для удаления элементов из суммирования
        var lastPriceX = trailingLastPriceX; // последняя цена, считанная из inReal0
        var trailingLastPriceY = inReal1[trailingIdx]; // та же цена, что и lastPriceY, но используется для удаления элементов из суммирования
        var lastPriceY = trailingLastPriceY; /* последняя цена, считанная из inReal1 */

        var i = ++trailingIdx;
        while (i < startIdx)
        {
            UpdateSummation(inReal0, inReal1, ref lastPriceX, ref lastPriceY, ref i, ref sxx, ref sxy, ref sx, ref sy);
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);

        var outIdx = 0;
        do
        {
            UpdateSummation(inReal0, inReal1, ref lastPriceX, ref lastPriceY, ref i, ref sxx, ref sxy, ref sx, ref sy);

            UpdateTrailingSummation(inReal0, inReal1, ref trailingLastPriceX, ref trailingLastPriceY, ref trailingIdx, ref sxx, ref sxy,
                ref sx, ref sy, timePeriod, outReal, ref outIdx);
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void UpdateSummation<T>(
        ReadOnlySpan<T> real0,
        ReadOnlySpan<T> real1,
        ref T lastPriceX,
        ref T lastPriceY,
        ref int idx,
        ref T sxx,
        ref T sxy,
        ref T sx,
        ref T sy) where T : IFloatingPointIeee754<T>
    {
        var tmpReal = real0[idx];
        var x = !T.IsZero(lastPriceX) ? (tmpReal - lastPriceX) / lastPriceX : T.Zero;
        lastPriceX = tmpReal;

        tmpReal = real1[idx++];
        var y = !T.IsZero(lastPriceY) ? (tmpReal - lastPriceY) / lastPriceY : T.Zero;
        lastPriceY = tmpReal;

        sxx += x * x;
        sxy += x * y;
        sx += x;
        sy += y;
    }

    private static void UpdateTrailingSummation<T>(
        ReadOnlySpan<T> real0,
        ReadOnlySpan<T> real1,
        ref T trailingLastPriceX,
        ref T trailingLastPriceY,
        ref int trailingIdx,
        ref T sxx,
        ref T sxy,
        ref T sx,
        ref T sy,
        T timePeriod,
        Span<T> outReal,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        // Всегда считываем trailing перед записью выходного значения, так как входной и выходной буферы могут быть одинаковыми.
        var tmpReal = real0[trailingIdx];
        var x = !T.IsZero(trailingLastPriceX) ? (tmpReal - trailingLastPriceX) / trailingLastPriceX : T.Zero;
        trailingLastPriceX = tmpReal;

        tmpReal = real1[trailingIdx++];
        var y = !T.IsZero(trailingLastPriceY) ? (tmpReal - trailingLastPriceY) / trailingLastPriceY : T.Zero;
        trailingLastPriceY = tmpReal;

        tmpReal = timePeriod * sxx - sx * sx;
        outReal[outIdx++] = !T.IsZero(tmpReal) ? (timePeriod * sxy - sx * sy) / tmpReal : T.Zero;

        // Удаляем расчет, начиная с trailingIdx.
        sxx -= x * x;
        sxy -= x * y;
        sx -= x;
        sy -= y;
    }
}
