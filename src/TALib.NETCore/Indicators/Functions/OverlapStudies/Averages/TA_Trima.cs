// Trima.cs
// Группы, к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// StatisticFunctions (альтернатива для статистических расчетов)
// TrendIndicators (альтернатива для индикаторов тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Triangular Moving Average (Overlap Studies) — Треугольная скользящая средняя (Индикаторы перекрытия)
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
    /// <param name="optInTimePeriod">Период времени (количество баров) для расчета индикатора</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Треугольная скользящая средняя (TRIMA) — это взвешенная скользящая средняя, которая придает больший вес
    /// точкам данных, расположенным ближе к центру указанного периода. В отличие от обычной взвешенной скользящей средней (WMA),
    /// которая акцентирует внимание на последних ценах, TRIMA симметрично распределяет веса, подчеркивая среднюю часть набора данных
    /// для получения более сглаженного среднего значения.
    /// </para>
    /// <para>
    /// Индикатор может давать более сглаженную меру тренда по сравнению с <see cref="Sma{T}">SMA</see> или <see cref="Ema{T}">EMA</see>.
    /// Интеграция с индикаторами импульса может предоставить более четкие сигналы за счет снижения шума на графике.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого периода вычисляется взвешенная скользящая средняя следующим образом:
    ///       - Для нечетного периода TRIMA эквивалентна простой скользящей средней (SMA) от другой SMA.
    ///       - Для четного периода TRIMA использует скорректированные веса для сглаживания данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Числитель обновляется динамически для каждого последующего расчета путем вычитания уходящих значений,
    ///       добавления новых значений и соответствующей корректировки весов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Взвешенная сумма нормализуется путем деления на общий весовой коэффициент для периода.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Trima<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        TrimaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период отката (lookback) для индикатора <see cref="Trima{T}">Trima</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета индикатора</param>
    /// <returns>Количество периодов, необходимых до того, как можно будет рассчитать первое валидное выходное значение.</returns>
    [PublicAPI]
    public static int TrimaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Trima<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        TrimaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode TrimaImpl<T>(
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

        /* Треугольная скользящая средняя (TRIMA) является взвешенной скользящей средней. В отличие от WMA,
         * которая придает больший вес последним ценовым барам, треугольная средняя придает больший вес
         * данным в середине указанного периода.
         *
         * Примеры:
         *   Для временного ряда = {a, b, c, d, e, f...} ("a" — самая старая цена)
         *
         *   1-е значение TRIMA с периодом 4: ((1 * a) + (2 * b) + (2 * c) + (1 * d)) / 6
         *   2-е значение TRIMA с периодом 4: ((1 * b) + (2 * c) + (2 * d) + (1 * e)) / 6
         *
         *   1-е значение TRIMA с периодом 5: ((1 * a) + (2 * b) + (3 * c) + (2 * d) + (1 * e)) / 9
         *   2-е значение TRIMA с периодом 5: ((1 * b) + (2 * c) + (3 * d) + (2 * e) + (1 * f)) / 9
         *
         * Общепринятая реализация
         * ─────────────────────────────────
         * С помощью алгебры можно доказать, что TRIMA эквивалентна вычислению SMA от другой SMA.
         * Следующие правила описывают эту эквивалентность:
         *
         *   (1) Для четного периода: TRIMA(x, period) = SMA(SMA(x, period / 2), (period / 2) + 1)
         *   (2) Для нечетного периода: TRIMA(x, period) = SMA(SMA(x, (period + 1) / 2), (period + 1) / 2)
         *
         * Иными словами:
         *   (1) Период 4 преобразуется в: TRIMA(x, 4) = SMA(SMA(x, 2), 3)
         *   (2) Период 5 преобразуется в: TRIMA(x, 5) = SMA(SMA(x, 3), 3)
         *
         * SMA от SMA — это алгоритм, обычно встречающийся в учебной литературе.
         *
         * Реализация в библиотеке
         * ──────────────
         * Выходные значения совпадают с общепринятой реализацией.
         *
         * Для оптимизации скорости и избежания выделения памяти библиотека использует более эффективный алгоритм,
         * чем обычная двойная SMA.
         *
         * Расчет перехода от одного значения TRIMA к следующему выполняется с помощью 4 небольших корректировок
         * (ниже показан пример для периода 4):
         *
         * TRIMA на момент "d": ((1 * a) + (2 * b) + (2 * c) + (1 * d)) / 6
         * TRIMA на момент "e": ((1 * b) + (2 * c) + (2 * d) + (1 * e)) / 6
         *
         * Чтобы перейти от TRIMA "d" к "e", выполняются следующие операции:
         *   1) "a" и "b" вычитаются из числителя.
         *   2) "d" добавляется к числителю.
         *   3) "e" добавляется к числителю.
         *   4) TRIMA рассчитывается как числитель / 6
         *   5) Последовательность повторяется для следующего выходного значения
         *
         * Эти операции соответствуют шагам, реализованным в библиотеке:
         *   1) выполняется через numeratorSub
         *   2) выполняется через numeratorAdd
         *   3) получается из последнего входного значения
         *   4) Вычисление и запись TRIMA в выходной массив
         *   5) Повтор для следующего выходного значения
         *
         * numeratorAdd и numeratorSub должны корректироваться на каждой итерации.
         *
         * Обновление numeratorSub требует значений из входных данных на позициях trailingIdx и middleIdx.
         *
         * Обновление numeratorAdd требует значений из входных данных на позициях middleIdx и todayIdx.
         */

        var lookbackTotal = TrimaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        int outIdx;
        if (optInTimePeriod % 2 != 0)
        {
            ProcessOdd(inReal, startIdx, endIdx, optInTimePeriod, lookbackTotal, outReal, out outIdx);
        }
        else
        {
            ProcessEven(inReal, startIdx, endIdx, optInTimePeriod, lookbackTotal, outReal, out outIdx);
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void ProcessOdd<T>(
        ReadOnlySpan<T> inReal,
        int startIdx,
        int endIdx,
        int optInTimePeriod,
        int lookbackTotal,
        Span<T> outReal,
        out int outIdx) where T : IFloatingPointIeee754<T>
    {
        /* Расчет коэффициента нормализации — 1 деленное на сумму весов.
         *
         * Сумма весов рассчитывается следующим образом:
         *
         * Простая сумма ряда 1 + 2 + 3 + ... + n выражается как n * (n + 1) / 2
         *
         * На основе этой логики можно вывести формулу для "треугольного" ряда в зависимости от четности периода.
         *
         * Формула для нечетного периода:
         *   период = 5, где n = (int)(period / 2) = 2
         *   формула для "треугольного" ряда:
         *     1 + 2 + 3 + 2 + 1 = (n * (n + 1)) + n + 1
         *                       = (n + 1) * (n + 1)
         *                       = 3 * 3 = 9
         *
         * Формула для четного периода:
         *   период = 6, где n = (int)(period / 2) = 3
         *   формула для "треугольного" ряда:
         *     1 + 2 + 3 + 3 + 2 + 1 = n * (n + 1)
         *                           = 3 * 4 = 12
         */

        // Вычисления полностью выполняются с целыми числами, преобразование в double происходит только при присвоении фактору
        var i = optInTimePeriod >> 1; // Эквивалентно делению периода на 2 с округлением вниз
        var ti = T.CreateChecked(i);
        var factor = (ti + T.One) * (ti + T.One); // Коэффициент нормализации для нечетного периода: (n+1)²
        factor = T.One / factor; // Обратное значение для нормализации

        var trailingIdx = startIdx - lookbackTotal; // Индекс самого старого бара в окне расчета
        var middleIdx = trailingIdx + i; // Индекс центрального бара (левая граница центра)
        var todayIdx = middleIdx + i; // Индекс текущего бара (правая граница окна)
        T numerator = T.Zero, numeratorSub = T.Zero; // Числитель и компонент для вычитания
        T tempReal;
        // Накопление числителя для левой половины окна (от центра к началу)
        for (i = middleIdx; i >= trailingIdx; i--)
        {
            tempReal = inReal[i];
            numeratorSub += tempReal; // Накопление суммы для вычитания
            numerator += numeratorSub; // Добавление взвешенной суммы в числитель
        }

        var numeratorAdd = T.Zero; // Компонент для добавления
        middleIdx++;
        // Накопление числителя для правой половины окна (от центра к концу)
        for (i = middleIdx; i <= todayIdx; i++)
        {
            tempReal = inReal[i];
            numeratorAdd += tempReal; // Накопление суммы для добавления
            numerator += numeratorAdd; // Добавление взвешенной суммы в числитель
        }

        // Значение на позиции trailingIdx сохраняется в tempReal для обработки случая,
        // когда выходной и входной буферы могут указывать на одну и ту же область памяти
        outIdx = 0;
        tempReal = inReal[trailingIdx++]; // Сохранение значения для будущего вычитания
        outReal[outIdx++] = numerator * factor; // Первое валидное значение индикатора
        todayIdx++;

        // Основной цикл расчета последующих значений индикатора
        while (todayIdx <= endIdx)
        {
            numerator -= numeratorSub; // Шаг 1: вычитание уходящей левой части
            numeratorSub -= tempReal; // Коррекция компонента вычитания
            tempReal = inReal[middleIdx++]; // Новое значение в центре окна
            numeratorSub += tempReal; // Обновление компонента вычитания

            numerator += numeratorAdd; // Шаг 2: добавление накопленной правой части
            numeratorAdd -= tempReal; // Коррекция компонента добавления
            tempReal = inReal[todayIdx++]; // Новое текущее значение
            numeratorAdd += tempReal; // Обновление компонента добавления

            numerator += tempReal; // Шаг 3: добавление нового текущего значения

            tempReal = inReal[trailingIdx++]; // Сохранение уходящего значения для следующей итерации
            outReal[outIdx++] = numerator * factor; // Расчет и запись значения индикатора
        }
    }

    private static void ProcessEven<T>(
        ReadOnlySpan<T> inReal,
        int startIdx,
        int endIdx,
        int optInTimePeriod,
        int lookbackTotal,
        Span<T> outReal,
        out int outIdx) where T : IFloatingPointIeee754<T>
    {
        /* Логика очень похожа на нечетный случай, за исключением:
         *   - расчета коэффициента нормализации (отличается формула)
         *   - охвата числителя в numeratorSub и numeratorAdd (смещены границы)
         *   - порядка корректировки numeratorAdd в основном цикле
         */
        var i = optInTimePeriod >> 1; // Половина периода
        var ti = T.CreateChecked(i);
        var factor = ti * (ti + T.One); // Коэффициент нормализации для четного периода: n * (n + 1)
        factor = T.One / factor; // Обратное значение для нормализации

        var trailingIdx = startIdx - lookbackTotal; // Индекс самого старого бара в окне расчета
        var middleIdx = trailingIdx + i - 1; // Индекс центрального бара (смещен для четного периода)
        var todayIdx = middleIdx + i; // Индекс текущего бара
        T numerator = T.Zero, numeratorSub = T.Zero; // Числитель и компонент для вычитания
        T tempReal;
        // Накопление числителя для левой половины окна
        for (i = middleIdx; i >= trailingIdx; i--)
        {
            tempReal = inReal[i];
            numeratorSub += tempReal;
            numerator += numeratorSub;
        }

        var numeratorAdd = T.Zero; // Компонент для добавления
        middleIdx++;
        // Накопление числителя для правой половины окна
        for (i = middleIdx; i <= todayIdx; i++)
        {
            tempReal = inReal[i];
            numeratorAdd += tempReal;
            numerator += numeratorAdd;
        }

        // Значение на позиции trailingIdx сохраняется в tempReal для обработки случая,
        // когда выходной и входной буферы могут указывать на одну и ту же область памяти
        outIdx = 0;
        tempReal = inReal[trailingIdx++];
        outReal[outIdx++] = numerator * factor; // Первое валидное значение индикатора
        todayIdx++;

        // Основной цикл расчета последующих значений индикатора для четного периода
        while (todayIdx <= endIdx)
        {
            numerator -= numeratorSub; // Шаг 1: вычитание уходящей левой части
            numeratorSub -= tempReal; // Коррекция компонента вычитания
            tempReal = inReal[middleIdx++]; // Новое значение в центре окна
            numeratorSub += tempReal; // Обновление компонента вычитания

            numeratorAdd -= tempReal; // Коррекция компонента добавления (порядок отличается от нечетного случая)
            numerator += numeratorAdd; // Шаг 2: добавление накопленной правой части
            tempReal = inReal[todayIdx++]; // Новое текущее значение
            numeratorAdd += tempReal; // Обновление компонента добавления

            numerator += tempReal; // Шаг 3: добавление нового текущего значения

            tempReal = inReal[trailingIdx++]; // Сохранение уходящего значения для следующей итерации
            outReal[outIdx++] = numerator * factor; // Расчет и запись значения индикатора
        }
    }
}
