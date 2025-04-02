//Название файла: TA_LinearRegIntercept.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//TrendAnalysis (альтернатива, если требуется группировка по типу анализа)
//RegressionMetrics (альтернатива для акцента на регрессионных метриках)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Linear Regression Intercept (Statistic Functions) — Линейная регрессия пересечения (Статистические функции)
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
    /// Линейная регрессия пересечения вычисляет точку пересечения y линии наилучшего подхода для серии данных
    /// за указанный период. Она представляет собой значение линии, когда значение x (индекс) равно нулю, предоставляя информацию
    /// о базовом уровне данных.
    /// <para>
    /// Её можно использовать в сочетании с наклоном для лучшего понимания характеристик тренда данных.
    /// Функция может служить ориентиром. Комбинирование её с другими метриками на основе регрессии или ценовыми паттернами
    /// может дать более глубокие инсайты.
    /// </para>
    ///
    /// <b>Шаги вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить суммы значений X (индексные позиции), квадратов X и произведения X и Y (входные значения)
    ///       за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить наклон (m) линии регрессии по формуле:
    ///       <code>
    ///         m = (n * Sum(XY) - Sum(X) * Sum(Y)) / (n * Sum(X^2) - (Sum(X))^2)
    ///       </code>
    ///       где n — период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить точку пересечения y (b) линии регрессии по формуле:
    ///       <code>
    ///         b = (Sum(Y) - m * Sum(X)) / n
    ///       </code>
    ///       где Sum(Y) — сумма входных значений, а Sum(X) — сумма индексных позиций.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Точка пересечения y указывает на начальный уровень серии данных относительно линии регрессии.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Изменения в точке пересечения со временем могут сигнализировать о сдвигах в базовом уровне данных.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode LinearRegIntercept<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegInterceptImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="LinearRegIntercept{T}">LinearRegIntercept</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int LinearRegInterceptLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode LinearRegIntercept<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegInterceptImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode LinearRegInterceptImpl<T>(
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

        /* Линейная регрессия — это концепция, также известная как "метод наименьших квадратов" или "наилучшая подгонка."
         * Линейная регрессия пытается подогнать прямую линию между несколькими точками данных таким образом, чтобы
         * расстояние между каждой точкой данных и линией было минимальным.
         *
         * Для каждой точки прямая линия за предыдущий период баров определяется в терминах y = b + m * x:
         *
         * Возвращает 'b'
         */

        var lookbackTotal = LinearRegInterceptLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var today = startIdx;

        var timePeriod = T.CreateChecked(optInTimePeriod);
        var sumX = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * 0.5);
        var sumXSqr = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * (optInTimePeriod * 2 - 1) / 6.0);
        var divisor = sumX * sumX - timePeriod * sumXSqr;
        while (today <= endIdx)
        {
            var sumXY = T.Zero;
            var sumY = T.Zero;
            for (var i = optInTimePeriod; i-- != 0;)
            {
                var tempValue1 = inReal[today - i];
                sumY += tempValue1;
                sumXY += T.CreateChecked(i) * tempValue1;
            }

            var m = (timePeriod * sumXY - sumX * sumY) / divisor;
            outReal[outIdx++] = (sumY - m * sumX) / timePeriod;
            today++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
