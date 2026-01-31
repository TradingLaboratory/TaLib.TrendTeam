// Файл: MinusDM.cs
// Группы, к которым относится индикатор:
// MomentumIndicators (основная категория - индикатор импульса в системе направленного движения)
// TrendIndicators (альтернатива ≥70% - оценка силы нисходящего тренда)
// VolatilityIndicators (альтернатива ≥50% - анализ изменчивости цен в контексте направленного движения)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Minus Directional Movement (Momentum Indicators) — Отрицательное направленное движение (-DM) (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High) за каждый период.</param>
    /// <param name="inLow">Массив минимальных цен (Low) за каждый период.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатываются все доступные данные.
    /// </param>
    /// <param name="outReal">
    /// Массив для хранения рассчитанных значений -DM.
    /// - Содержит ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End.Value - outRange.Start.Value</c>.
    /// - Каждый элемент <c>outReal[i]</c> соответствует входным данным с индексом <c>outRange.Start + i</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов входных данных, для которых рассчитаны валидные значения индикатора:
    /// - <b>Start</b>: индекс первого бара во входных данных, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего бара во входных данных, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Если данных недостаточно для расчёта (например, длина входных данных меньше периода индикатора), возвращается диапазон <c>[0, 0)</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (количество баров для сглаживания). Значение по умолчанию: 14.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успешность расчёта:
    /// - <see cref="Core.RetCode.Success"/> — расчёт выполнен успешно;
    /// - <see cref="Core.RetCode.OutOfRangeParam"/> — недопустимый диапазон входных данных;
    /// - <see cref="Core.RetCode.BadParam"/> — недопустимое значение периода (меньше 1).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция расчёта отрицательного направленного движения (-DM) за указанный период.
    /// Является компонентом системы направленного движения Уайлдера (Directional Movement System)
    /// и используется для оценки силы нисходящего движения цены.
    /// </para>
    /// <para>
    /// Используется совместно с <see cref="PlusDM{T}">+DM</see> для расчёта направленных индикаторов
    /// (<see cref="PlusDI{T}">+DI</see>, <see cref="MinusDI{T}">-DI</see>) или среднего индекса направленного движения (<see cref="Adx{T}">ADX</see>).
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого периода рассчитывается отрицательное направленное движение:
    ///       <c>-DM = Previous Low - Current Low</c>, если это значение положительно
    ///       И превышает соответствующее положительное движение (<c>Current High - Previous High</c>);
    ///       в противном случае <c>-DM = 0</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Если период больше 1, значения <c>-DM</c> суммируются за указанный период,
    ///       а затем применяется сглаживание по методу Уайлдера (Wilder's smoothing):
    ///       <code>
    ///         -DM(n) = (Previous -DM(n-1) * (Period - 1) + Current -DM) / Period
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Высокое значение -DM указывает на сильный нисходящий импульс цены.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Низкое или нулевое значение -DM свидетельствует о слабом или отсутствующем нисходящем импульсе.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MinusDM<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MinusDMImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает необходимый lookback-период для расчёта индикатора <see cref="MinusDM{T}">-DM</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>
    /// Количество баров, необходимых перед первым валидным значением индикатора.
    /// - Для периода = 1: lookback = 1 (требуется предыдущий бар для расчёта разницы).
    /// - Для периода > 1: lookback = optInTimePeriod + нестабильный период - 1.
    /// - При недопустимом периоде (< 1): возвращает -1.
    /// </returns>
    [PublicAPI]
    public static int MinusDMLookback(int optInTimePeriod = 14) => optInTimePeriod switch
    {
        < 1 => -1,
        > 1 => optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.MinusDM) - 1,
        _ => 1
    };

    /// <remarks>
    /// Для обеспечения совместимости с абстрактным API (перегрузка для массивов вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MinusDM<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MinusDMImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MinusDMImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и длины массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода (должен быть >= 1)
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Расчёт -DM1 (за один период) основан на наибольшей части сегодняшнего диапазона,
         * находящейся ВНЕ вчерашнего диапазона.
         *
         * Следующие 7 случаев объясняют расчёт +DM и -DM за один период:
         *
         * Случай 1:                       Случай 2:
         *    C│                        A│
         *     │                         │ C│
         *     │ +DM1 = (C-A)           B│  │ +DM1 = 0
         *     │ -DM1 = 0                   │ -DM1 = (B-D)
         * A│  │                           D│
         *  │ D│
         * B│
         *
         * Случай 3:                       Случай 4:
         *    C│                           C│
         *     │                        A│  │
         *     │ +DM1 = (C-A)            │  │ +DM1 = 0
         *     │ -DM1 = 0               B│  │ -DM1 = (B-D)
         * A│  │                            │
         *  │  │                           D│
         * B│  │
         *    D│
         *
         * Случай 5:                      Случай 6:
         * A│                           A│ C│
         *  │ C│ +DM1 = 0                │  │  +DM1 = 0
         *  │  │ -DM1 = 0                │  │  -DM1 = 0
         *  │ D│                         │  │
         * B│                           B│ D│
         *
         *
         * Случай 7:
         *
         *    C│
         * A│  │
         *  │  │ +DM=0
         * B│  │ -DM=0
         *    D│
         *
         * В случаях 3 и 4 правило: наименьшая разница между (C-A) и (B-D) определяет,
         * какой из +DM или -DM будет равен нулю.
         *
         * В случае 7: (C-A) и (B-D) равны, поэтому оба значения (+DM и -DM) равны нулю.
         *
         * Правила остаются теми же, когда A=B и C=D (когда максимумы равны минимумам).
         *
         * При расчёте -DM за период > 1: сначала суммируются однопериодные значения -DM1
         * за указанный период. Например, для -DM14 суммируются значения -DM1 за первые 14 дней
         * (фактически 13 значений, так как для первого дня расчёт невозможен!).
         * Последующие значения рассчитываются с использованием сглаживания по Уайлдеру:
         *
         *                                     Предыдущее -DM14
         *   Сегодняшнее -DM14 = Предыдущее -DM14 - ─────────────── + Сегодняшнее -DM1
         *                                            14
         *
         * Источник: New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        // Расчёт минимального количества баров, необходимых для первого валидного значения
        var lookbackTotal = MinusDMLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback не осталось данных для расчёта — выход с успешным статусом (пустой результат)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Обработка специального случая: период = 1 (без сглаживания)
        if (optInTimePeriod == 1)
        {
            // Без сглаживания: простой расчёт -DM1 для каждого бара
            return CalcMinusDMForPeriodOne(inHigh, inLow, startIdx, endIdx, outReal, out outRange);
        }

        // Индекс первого валидного значения в выходном массиве
        var outBegIdx = startIdx;
        // Текущий индекс обработки (начинаем с позиции до lookback для инициализации)
        var today = startIdx - lookbackTotal;

        // Преобразование целочисленного периода в числовой тип T для расчётов
        var timePeriod = T.CreateChecked(optInTimePeriod);
        // Накопленное сглаженное значение -DM за предыдущий период
        T prevMinusDM = T.Zero, _ = T.Zero;

        // Инициализация значений -DM и истинного диапазона (TR) для первых баров
        FunctionHelpers.InitDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, out var prevHigh, ref today, out var prevLow, out var _,
            timePeriod, ref _, ref prevMinusDM, ref _);

        // Пропуск нестабильного периода (для достижения стабильных значений сглаживания)
        for (var i = 0; i < Core.UnstablePeriodSettings.Get(Core.UnstableFunc.MinusDM); i++)
        {
            today++;
            // Обновление значений -DM и TR для текущего бара
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, ref today, ref prevHigh, ref prevLow, ref _, ref _,
                ref prevMinusDM, ref _, timePeriod);
        }

        // Сохранение первого стабильного значения -DM в выходной массив
        outReal[0] = prevMinusDM;
        var outIdx = 1;

        // Основной цикл расчёта -DM для оставшихся баров
        while (today < endIdx)
        {
            today++;
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, ref today, ref prevHigh, ref prevLow, ref _, ref _,
                ref prevMinusDM, ref _, timePeriod);
            outReal[outIdx++] = prevMinusDM;
        }

        // Формирование диапазона валидных значений в выходном массиве
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static Core.RetCode CalcMinusDMForPeriodOne<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        int startIdx,
        int endIdx,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Начало расчёта с предыдущего бара (для сравнения текущих и предыдущих цен)
        var today = startIdx - 1;
        var prevHigh = high[today];
        var prevLow = low[today];
        var outIdx = 0;

        // Цикл расчёта -DM1 для каждого бара без сглаживания
        while (today < endIdx)
        {
            today++;
            // Расчёт разниц: положительной (diffP) и отрицательной (diffM) направленных движений
            var (diffP, diffM) = FunctionHelpers.CalcDeltas(high, low, today, ref prevHigh, ref prevLow);
            // -DM1 = diffM, если он положителен И превышает положительное движение (diffP); иначе 0
            outReal[outIdx++] = diffM > T.Zero && diffP < diffM ? diffM : T.Zero;
        }

        // Формирование диапазона валидных значений (начинается с startIdx)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
