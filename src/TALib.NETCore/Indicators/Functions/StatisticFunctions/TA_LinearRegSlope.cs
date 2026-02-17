//Название файла: TA_LinearRegSlope.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//MomentumIndicators (альтернатива, если требуется группировка по типу индикатора)
//TrendDirection (альтернатива для акцента на направлении тренда)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Linear Regression Slope (Statistic Functions) — Линейная регрессия наклона (Статистические функции)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// Обычно используются цены <see cref="Close"/>, но могут применяться любые числовые данные.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет subset данных для вычисления индикатора.
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
    /// <param name="optInTimePeriod">
    /// Период времени для расчета линейной регрессии.
    /// <para>
    /// - Определяет количество баров, используемых для вычисления наклона линии регрессии.
    /// - Минимальное допустимое значение: 2.
    /// - Рекомендуемое значение по умолчанию: 14.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Линейная регрессия наклона вычисляет наклон линии наилучшего соответствия для серии данных
    /// за указанный период. Она предоставляет информацию о направлении и скорости изменения тренда в данных.
    /// <para>
    /// Функция может указывать на восходящий или нисходящий тренд. Подтверждение значений наклона с помощью индикаторов тренда или импульса может
    /// уменьшить вероятность неправильного прочтения рыночных условий.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисляются суммы значений X (индексные позиции), квадратов X и произведения X и Y (входные значения)
    ///       за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисляется наклон (m) линии регрессии по формуле:
    ///       <code>
    ///         m = (n * Sum(XY) - Sum(X) * Sum(Y)) / (n * Sum(X^2) - (Sum(X))^2)
    ///       </code>
    ///       где n — это период времени (<paramref name="optInTimePeriod"/>).
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительный наклон указывает на восходящий тренд, где значения увеличиваются со временем.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательный наклон указывает на нисходящий тренд, где значения уменьшаются со временем.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Величина наклона отражает силу тренда — чем больше абсолютное значение, тем сильнее тренд.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Примечания</b>:
    /// <para>
    /// - Lookback период: <c>optInTimePeriod - 1</c> (необходимо для первого валидного значения индикатора).
    /// - Все бары с индексом меньше lookback будут пропущены при расчете.
    /// - Индикатор чувствителен к выбору периода — меньшие периоды дают более волатильные значения.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode LinearRegSlope<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegSlopeImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="LinearRegSlope{T}">LinearRegSlope</see>.
    /// </summary>
    /// <param name="optInTimePeriod">
    /// Период времени для расчета индикатора.
    /// <para>
    /// - Определяет количество баров, используемых в расчете линейной регрессии.
    /// - Минимальное допустимое значение: 2.
    /// </para>
    /// </param>
    /// <returns>
    /// Количество периодов, необходимых до первого вычисленного значения индикатора.
    /// <para>
    /// - Возвращает <c>optInTimePeriod - 1</c> для валидного периода.
    /// - Возвращает <c>-1</c> если период меньше 2 (невалидный параметр).
    /// </para>
    /// </returns>
    /// <remarks>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно получить валидное значение индикатора.
    /// Все бары с индексом меньше lookback будут пропущены при расчете.
    /// </remarks>
    [PublicAPI]
    public static int LinearRegSlopeLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Этот метод обеспечивает совместимость с версиями API, использующими массивы вместо Span.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode LinearRegSlope<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegSlopeImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode LinearRegSlopeImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона - по умолчанию пустой диапазон
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Проверка валидности периода времени (минимум 2 периода для расчета регрессии)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Линейная регрессия — это концепция, также известная как "метод наименьших квадратов" или "наилучшее соответствие."
         * Линейная регрессия пытается подогнать прямую линию между несколькими точками данных таким образом, чтобы
         * расстояние между каждой точкой данных и линией было минимальным.
         *
         * Для каждой точки прямая линия над указанным предыдущим периодом баров определяется в виде y = b + m * x:
         *
         * Возвращает 'm' (наклон линии регрессии)
         */

        // Расчет lookback периода - количество баров, необходимых для первого валидного значения
        var lookbackTotal = LinearRegSlopeLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс превышает конечный - нет данных для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;
        // Текущий индекс обрабатываемого бара во входных данных
        var today = startIdx;

        // Период времени в типе T для математических операций
        var timePeriod = T.CreateChecked(optInTimePeriod);
        // Сумма значений X (индексных позиций): Sum(X) = n * (n-1) / 2
        var sumX = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * 0.5);
        // Сумма квадратов значений X: Sum(X^2) = n * (n-1) * (2n-1) / 6
        var sumXSqr = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * (optInTimePeriod * 2 - 1) / 6.0);
        // Знаменатель формулы наклона: (Sum(X))^2 - n * Sum(X^2)
        var divisor = sumX * sumX - timePeriod * sumXSqr;

        // Основной цикл расчета наклона линейной регрессии для каждого бара
        while (today <= endIdx)
        {
            // Сумма произведений X и Y (индекс * значение цены)
            T sumXY = T.Zero;
            // Сумма значений Y (цен) за период
            T sumY = T.Zero;

            // Внутренний цикл для вычисления сумм за период регрессии
            for (var i = optInTimePeriod; i-- != 0;)
            {
                // Текущее значение цены на позиции (today - i)
                var tempValue1 = inReal[today - i];
                // Накопление суммы значений Y (цен)
                sumY += tempValue1;
                // Накопление суммы произведений X * Y (индекс * цена)
                sumXY += T.CreateChecked(i) * tempValue1;
            }

            // Расчет наклона линейной регрессии по формуле:
            // m = (n * Sum(XY) - Sum(X) * Sum(Y)) / (n * Sum(X^2) - (Sum(X))^2)
            outReal[outIdx++] = (timePeriod * sumXY - sumX * sumY) / divisor;
            // Переход к следующему бару
            today++;
        }

        // Установка выходного диапазона - индексы входных данных с валидными значениями индикатора
        outRange = new Range(startIdx, startIdx + outIdx);

        // Возврат кода успешного выполнения
        return Core.RetCode.Success;
    }
}
