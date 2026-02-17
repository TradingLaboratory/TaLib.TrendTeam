//Название файла: TA_Beta.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - основная категория, 100%)
//VolatilityIndicators (альтернатива, 70%)
//RiskMetrics (предлагаемая категория, 60%)
//Рекомендуемая подпапка (если будет создана внутри StatisticFunctions): Regression

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Beta (Statistic Functions) — Бета (Статистические функции)
    /// </summary>
    /// <param name="inReal0">Входные данные: цены финансового инструмента (акции, облигации и т.д.)</param>
    /// <param name="inReal1">Входные данные: цены бенчмарка (рыночный индекс, эталонный актив)</param>
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
    /// <param name="optInTimePeriod">Период времени для расчёта (количество баров). Минимальное значение: 1.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция рассчитывает коэффициент Бета (β), измеряющий чувствительность доходности актива к изменениям доходности рынка (бенчмарка).
    /// Используется в оценке систематического риска, портфельном анализе и CAPM-моделях.
    /// </para>
    /// <para>
    /// <b>Формула расчёта</b>:
    /// <code>
    ///   Beta = Covariance(Asset_Returns, Market_Returns) / Variance(Market_Returns)
    /// </code>
    /// где доходности (Returns) рассчитываются как процентные изменения цен за период.
    /// </para>
    /// <para>
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item><description>Beta = 1: актив движется синхронно с рынком.</description></item>
    ///   <item><description>Beta &gt; 1: актив более волатилен рынка (агрессивный).</description></item>
    ///   <item><description>Beta &lt; 1: актив менее волатилен рынка (консервативный).</description></item>
    ///   <item><description>Beta &lt; 0: актив движется против рынка (хеджирующий инструмент).</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Важно</b>: Для корректного расчёта требуется достаточное количество данных (≥ optInTimePeriod). 
    /// При нулевой дисперсии рынка (плоский тренд) возвращается 0.
    /// </para>
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
    /// Возвращает период обратного просмотра (lookback) для метода <see cref="Beta{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчёта.</param>
    /// <returns>
    /// Количество баров, необходимых до первого валидного значения индикатора. 
    /// При некорректном периоде (меньше 1) возвращает -1.
    /// </returns>
    [PublicAPI]
    public static int BetaLookback(int optInTimePeriod = 5) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Вспомогательный метод для совместимости с устаревшим массивным API.
    /// Перенаправляет вызов в реализацию на базе Span.
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

    /// <summary>
    /// Основная реализация расчёта коэффициента Бета.
    /// </summary>
    /// <remarks>
    /// Алгоритм:
    /// 1. Рассчитывает процентные изменения (доходности) для актива (inReal0) и рынка (inReal1).
    /// 2. Строит скользящее окно размером optInTimePeriod для накопления сумм: 
    ///    Sxx (сумма квадратов доходностей рынка), Sxy (сумма произведений доходностей), Sx, Sy.
    /// 3. Применяет формулу линейной регрессии: Beta = (n*Sxy - Sx*Sy) / (n*Sxx - Sx²).
    /// 4. Использует технику "бегущего окна" (trailing index) для эффективного обновления сумм.
    /// 
    /// Примечание: При делении на ноль (нулевая дисперсия рынка) возвращается 0.
    /// </remarks>
    private static Core.RetCode BetaImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и равенства длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода расчёта
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* 
         * Коэффициент Бета измеряет систематический риск актива относительно бенчмарка.
         * inReal0 — цены актива, inReal1 — цены рыночного индекса (бенчмарка).
         * Алгоритм:
         * 1. Рассчитываются процентные изменения (доходности) для актива (y) и рынка (x).
         * 2. Точки (x, y) интерпретируются как данные для линейной регрессии.
         * 3. Бета = наклон (slope) регрессионной прямой y = βx + α.
         *    - β = 1: актив повторяет рынок.
         *    - β > 1: актив агрессивнее рынка.
         *    - β < 1: актив консервативнее рынка.
         *    - β < 0: актив движется против рынка.
         */

        // Общий период обратного просмотра (минимальное количество баров для первого валидного значения)
        var lookbackTotal = BetaLookback(optInTimePeriod);
        // Смещение startIdx для обеспечения достаточного количества данных
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация сумм для расчёта регрессии
        T sxx, sxy, sx, sy; // Sxx = Σx², Sxy = Σx·y, Sx = Σx, Sy = Σy
        sxx = sxy = sx = sy = T.Zero;

        // Индекс начала скользящего окна (для удаления устаревших значений)
        var trailingIdx = startIdx - lookbackTotal;

        // Цена актива (inReal0) на начало окна — для расчёта удаления из сумм
        var trailingLastPriceX = inReal0[trailingIdx];
        // Текущая последняя цена актива (для расчёта новых доходностей)
        var lastPriceX = trailingLastPriceX;

        // Цена бенчмарка (inReal1) на начало окна
        var trailingLastPriceY = inReal1[trailingIdx];
        // Текущая последняя цена бенчмарка
        var lastPriceY = trailingLastPriceY;

        // Предварительное заполнение сумм до startIdx (инициализация окна)
        var i = ++trailingIdx;
        while (i < startIdx)
        {
            UpdateSummation(inReal0, inReal1, ref lastPriceX, ref lastPriceY, ref i, ref sxx, ref sxy, ref sx, ref sy);
        }

        // Преобразование целочисленного периода в числовой тип для вычислений
        var timePeriod = T.CreateChecked(optInTimePeriod);

        var outIdx = 0; // Индекс записи в выходной массив
        // Основной цикл: расчёт Бета для каждого бара от startIdx до endIdx
        do
        {
            // Добавление нового бара в суммы
            UpdateSummation(inReal0, inReal1, ref lastPriceX, ref lastPriceY, ref i, ref sxx, ref sxy, ref sx, ref sy);
            // Удаление старого бара из сумм и запись результата
            UpdateTrailingSummation(inReal0, inReal1, ref trailingLastPriceX, ref trailingLastPriceY, ref trailingIdx, ref sxx, ref sxy,
                ref sx, ref sy, timePeriod, outReal, ref outIdx);
        } while (i <= endIdx);

        // Установка диапазона валидных значений в выходном массиве
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Обновляет суммы (Sxx, Sxy, Sx, Sy) добавлением нового бара.
    /// </summary>
    /// <remarks>
    /// Рассчитывает процентные изменения (доходности) для актива и рынка на текущем шаге:
    /// x = (Close_market_current - Close_market_prev) / Close_market_prev
    /// y = (Close_asset_current - Close_asset_prev) / Close_asset_prev
    /// Обновляет накопленные суммы для регрессионного расчёта.
    /// При нулевой предыдущей цене доходность принимается равной 0.
    /// </remarks>
    private static void UpdateSummation<T>(
        ReadOnlySpan<T> real0,      // Цены актива
        ReadOnlySpan<T> real1,      // Цены бенчмарка
        ref T lastPriceX,           // Последняя цена актива (для расчёта y)
        ref T lastPriceY,           // Последняя цена бенчмарка (для расчёта x)
        ref int idx,                // Текущий индекс (инкрементируется внутри метода)
        ref T sxx,                  // Σx²
        ref T sxy,                  // Σx·y
        ref T sx,                   // Σx
        ref T sy) where T : IFloatingPointIeee754<T> // Σy
    {
        // Чтение текущей цены актива и расчёт доходности (y)
        var tmpReal = real0[idx];
        var y = !T.IsZero(lastPriceX) ? (tmpReal - lastPriceX) / lastPriceX : T.Zero;
        lastPriceX = tmpReal; // Обновление последней цены актива

        // Чтение текущей цены бенчмарка, расчёт доходности (x) и переход к следующему индексу
        tmpReal = real1[idx++];
        var x = !T.IsZero(lastPriceY) ? (tmpReal - lastPriceY) / lastPriceY : T.Zero;
        lastPriceY = tmpReal; // Обновление последней цены бенчмарка

        // Накопление сумм для регрессии
        sxx += x * x; // Σx²
        sxy += x * y; // Σx·y
        sx += x;      // Σx
        sy += y;      // Σy
    }

    /// <summary>
    /// Обновляет суммы удалением старого бара из окна и записывает значение Бета.
    /// </summary>
    /// <remarks>
    /// 1. Считывает данные на позиции trailingIdx (старый бар, покидающий окно).
    /// 2. Рассчитывает его доходности (x_trail, y_trail).
    /// 3. Вычисляет Бета по формуле: (n*Sxy - Sx*Sy) / (n*Sxx - Sx²).
    /// 4. Удаляет вклад старого бара из сумм (коррекция окна).
    /// 
    /// Важно: Чтение trailing происходит ДО записи в outReal, так как буферы могут пересекаться.
    /// При делении на ноль (нулевая дисперсия рынка) записывается 0.
    /// </remarks>
    private static void UpdateTrailingSummation<T>(
        ReadOnlySpan<T> real0,        // Цены актива
        ReadOnlySpan<T> real1,        // Цены бенчмарка
        ref T trailingLastPriceX,     // Цена актива на начало удаляемого бара
        ref T trailingLastPriceY,     // Цена бенчмарка на начало удаляемого бара
        ref int trailingIdx,          // Индекс удаляемого бара (инкрементируется)
        ref T sxx,                    // Σx² (текущее окно)
        ref T sxy,                    // Σx·y
        ref T sx,                     // Σx
        ref T sy,                     // Σy
        T timePeriod,                 // Размер окна (n) в числовом типе
        Span<T> outReal,              // Выходной массив для записи Бета
        ref int outIdx) where T : IFloatingPointIeee754<T> // Индекс записи в outReal
    {
        // Чтение цены актива на позиции trailingIdx и расчёт доходности удаляемого бара (y_trail)
        var tmpReal = real0[trailingIdx];
        var y_trail = !T.IsZero(trailingLastPriceX) ? (tmpReal - trailingLastPriceX) / trailingLastPriceX : T.Zero;
        trailingLastPriceX = tmpReal; // Обновление цены актива для следующего шага

        // Чтение цены бенчмарка, расчёт доходности (x_trail) и переход к следующему trailing индексу
        tmpReal = real1[trailingIdx++];
        var x_trail = !T.IsZero(trailingLastPriceY) ? (tmpReal - trailingLastPriceY) / trailingLastPriceY : T.Zero;
        trailingLastPriceY = tmpReal;

        // Расчёт знаменателя формулы Бета: n*Sxx - (Sx)²
        tmpReal = timePeriod * sxx - sx * sx;
        // Расчёт и запись значения Бета; при нулевом знаменателе — 0
        outReal[outIdx++] = !T.IsZero(tmpReal) ? (timePeriod * sxy - sx * sy) / tmpReal : T.Zero;

        // Коррекция сумм: удаление вклада устаревшего бара из окна
        sxx -= x_trail * x_trail; // Σx² -= x_trail²
        sxy -= x_trail * y_trail; // Σx·y -= x_trail·y_trail
        sx -= x_trail;            // Σx -= x_trail
        sy -= y_trail;            // Σy -= y_trail
    }
}
