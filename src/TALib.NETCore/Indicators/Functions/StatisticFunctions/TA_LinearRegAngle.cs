//Название файла: TA_LinearRegAngle.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//RegressionAnalysis (альтернатива для акцента на регрессионном анализе)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Linear Regression Angle (Statistic Functions) — Угол линейной регрессии (Статистические функции)
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Угол линейной регрессии вычисляет угол наклона линии наилучшего соответствия для серии точек данных
    /// за указанный период. Он предоставляет перспективу направления и величины тренда в градусах,
    /// помогая определить силу и направление тренда.
    /// <para>
    /// Функция может указывать на силу тренда. Подтверждение с помощью ADX или индикаторов объема может уменьшить неправильное толкование
    /// незначительных изменений цены как значимых движений.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
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
    ///       Преобразовать наклон (m) в угол в градусах с использованием функции арктангенса:
    ///       <code>
    ///         Angle = RadiansToDegrees(Atan(m))
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительный угол указывает на восходящий тренд, где цены растут.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательный угол указывает на нисходящий тренд, где цены падают.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Угол, близкий к нулю, указывает на отсутствие значительного тренда в данных за период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Чем больше абсолютное значение угла, тем сильнее тренд.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode LinearRegAngle<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegAngleImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="LinearRegAngle{T}">LinearRegAngle</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int LinearRegAngleLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode LinearRegAngle<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegAngleImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode LinearRegAngleImpl<T>(
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

        /* Линейная регрессия — это концепция, также известная как "метод наименьших квадратов" или "наилучшее соответствие".
         * Линейная регрессия пытается подогнать прямую линию между несколькими точками данных таким образом, чтобы
         * расстояние между каждой точкой данных и линией было минимальным.
         *
         * Для каждой точки прямая линия над указанным предыдущим периодом баров определяется в терминах y = b + m * x:
         *
         * Возвращает 'm' в градусах.
         */

        var lookbackTotal = LinearRegAngleLookback(optInTimePeriod);
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
            T sumXY = T.Zero, sumY = T.Zero;
            for (var i = optInTimePeriod; i-- != 0;)
            {
                var tempValue1 = inReal[today - i];
                sumY += tempValue1;
                sumXY += T.CreateChecked(i) * tempValue1;
            }

            var m = (timePeriod * sumXY - sumX * sumY) / divisor;
            outReal[outIdx++] = T.RadiansToDegrees(T.Atan(m));
            today++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
