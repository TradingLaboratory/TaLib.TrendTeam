//Название файла: Tsf.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории, линейная регрессия является статистическим методом)
//OverlapStudies (альтернатива ≥80%, результат накладывается на график цены как линия. Похожа на Avg)
//ForecastIndicators (альтернативное название для новой категории прогнозирующих индикаторов)


using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Time Series Forecast (Statistic Functions) — Прогноз временного ряда (Статистические функции)
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта индикатора (обычно цены закрытия <see cref="Close"/>, но могут быть и другие временные ряды)</param>
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
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчёт успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени для расчёта линейной регрессии (минимум 2 бара)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция <b>Time Series Forecast (TSF)</b> рассчитывает прогноз будущего значения на основе линейной регрессии,
    /// применяя метод наименьших квадратов для определения наилучшей прямой линии, аппроксимирующей данные за указанный период.
    /// </para>
    /// <para>
    /// Прогнозируемое значение соответствует точке на линии регрессии для следующего бара после окончания периода расчёта.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление суммы входных значений (<c>Σ(y)</c>) и суммы произведений индексов на значения (<c>Σ(x·y)</c>) за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определение наклона (<c>m</c>) и смещения (<c>b</c>) линии линейной регрессии методом наименьших квадратов:
    /// <code>
    /// Наклон (m) = (N * Σ(x·y) - Σ(x) * Σ(y)) / (N * Σ(x²) - (Σ(x))²)
    /// Смещение (b) = (Σ(y) - m * Σ(x)) / N
    /// где N — количество баров в периоде, x — индексы баров (0, 1, 2, ..., N-1)
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт прогнозируемого значения для следующего бара по формуле: <c>y = b + m * N</c>,
    ///       где N — длина периода (следующий индекс после окончания периода расчёта).
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Возрастающее значение индикатора указывает на восходящий тренд.</description>
    ///   </item>
    ///   <item>
    ///     <description>Убывающее значение индикатора указывает на нисходящий тренд.</description>
    ///   </item>
    ///   <item>
    ///     <description>TSF можно использовать для прогнозирования краткосрочных движений цены и оценки силы тренда.</description>
    ///   </item>
    ///   <item>
    ///     <description>Рекомендуется использовать в сочетании с другими индикаторами тренда или импульса для подтверждения сигналов.</description>
    ///   </item>
    /// </list>
    ///
    /// <b>Особенности</b>:
    /// <para>
    /// Lookback-период равен <c>optInTimePeriod - 1</c>, то есть первое валидное значение индикатора появляется
    /// на баре с индексом <c>optInTimePeriod - 1</c> (считая с нуля).
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Tsf<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        TsfImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает lookback-период для метода <see cref="Tsf{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчёта линейной регрессии.</param>
    /// <returns>
    /// Количество периодов, необходимых до появления первого валидного значения индикатора.
    /// Для TSF lookback = optInTimePeriod - 1 (минимум 1 бар при периоде 2).
    /// Возвращает -1, если указанный период меньше 2.
    /// </returns>
    [PublicAPI]
    public static int TsfLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Tsf<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        TsfImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode TsfImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального допустимого периода (минимум 2 бара для расчёта линейной регрессии)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Линейная регрессия — статистический метод, также известный как "метод наименьших квадратов" или "наилучшая аппроксимация".
         * Линейная регрессия пытается провести прямую линию через набор точек данных таким образом,
         * чтобы минимизировать расстояние между каждой точкой и линией.
         *
         * Для каждой точки рассчитывается прямая линия за указанный предыдущий период в виде уравнения y = b + m * x:
         *
         * Возвращается прогнозируемое значение: b + m * (period)
         * где period = optInTimePeriod (следующий индекс после окончания периода расчёта)
         */

        // Расчёт минимального индекса для первого валидного значения (lookback period)
        var lookbackTotal = TsfLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс превышает конечный — нет данных для расчёта
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0; // Индекс в выходном массиве
        var today = startIdx; // Текущий индекс обрабатываемого бара

        // Преобразование целочисленного периода в числовой тип T
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Предварительный расчёт суммы индексов: Σ(x) = 0 + 1 + 2 + ... + (N-1) = N*(N-1)/2
        var sumX = timePeriod * (timePeriod - T.One) * T.CreateChecked(0.5);

        // Предварительный расчёт суммы квадратов индексов: Σ(x²) = 0² + 1² + ... + (N-1)² = N*(N-1)*(2N-1)/6
        var sumXSqr = timePeriod * (timePeriod - T.One) * (timePeriod * FunctionHelpers.Two<T>() - T.One) / T.CreateChecked(6);

        // Расчёт делителя для формулы наклона: divisor = N * Σ(x²) - (Σ(x))²
        var divisor = sumX * sumX - timePeriod * sumXSqr;

        // Основной цикл расчёта индикатора для каждого бара
        while (today <= endIdx)
        {
            var sumXY = T.Zero; // Сумма произведений индексов на значения: Σ(x·y)
            var sumY = T.Zero;  // Сумма значений: Σ(y)

            // Цикл по периоду для расчёта сумм за окно регрессии
            for (var i = optInTimePeriod; i-- != 0;)
            {
                // Получение значения из входного массива (с конца периода к началу)
                var tempValue1 = inReal[today - i];
                sumY += tempValue1;                 // Накопление суммы значений Σ(y)
                sumXY += T.CreateChecked(i) * tempValue1; // Накопление суммы произведений Σ(x·y)
            }

            // Расчёт наклона линии регрессии: m = (N * Σ(x·y) - Σ(x) * Σ(y)) / divisor
            var m = (timePeriod * sumXY - sumX * sumY) / divisor;

            // Расчёт смещения (пересечение с осью Y): b = (Σ(y) - m * Σ(x)) / N
            var b = (sumY - m * sumX) / timePeriod;

            // Расчёт прогнозируемого значения для следующего бара: y = b + m * N
            // где N = timePeriod — индекс следующего бара после окончания периода
            outReal[outIdx++] = b + m * timePeriod;
            today++;
        }

        // Установка диапазона валидных значений в выходном массиве
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
