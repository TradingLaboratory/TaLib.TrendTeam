// Название файла: PlusDM.cs
// Рекомендуемые папки для размещения:
// MomentumIndicators (основная категория - идеальное соответствие)
// TrendIndicators (альтернатива для группировки по трендовым индикаторам)
// DirectionalMovement (специализированная группа для компонентов системы направленного движения)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// _Plus Directional Movement (+DM) (Momentum Indicators) — Положительное направленное движение (+DM) (Индикаторы импульса)_
    /// </summary>
    /// <param name="inHigh">Входной диапазон цен High (максимальные цены баров)</param>
    /// <param name="inLow">Входной диапазон цен Low (минимальные цены баров)</param>
    /// <param name="inRange">
    /// Диапазон индексов, определяющий часть данных для расчёта во входных диапазонах.  
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">Диапазон для хранения рассчитанных значений индикатора +DM</param>
    /// <param name="outRange">
    /// Диапазон индексов, представляющий валидные данные во выходном диапазоне:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c>, если расчёт успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период сглаживания (по умолчанию 14)</param>
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
    /// Функция расчёта положительного направленного движения (+DM) за указанный период времени.
    /// Является компонентом системы направленного движения (Directional Movement System) Уэллса Уайлдера
    /// и используется для оценки силы восходящего движения цены.
    /// </para>
    /// <para>
    /// Используется совместно с <see cref="MinusDM{T}">-DM</see> для расчёта направленных индикаторов
    /// (<see cref="PlusDI{T}">+DI</see>, <see cref="MinusDI{T}">-DI</see>) или среднего индекса направленного движения (<see cref="Adx{T}">ADX</see>).
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитывается положительное направленное движение за один период как <c>+DM = Текущий High - Предыдущий High</c>,
    ///       если это значение больше соответствующего отрицательного движения (<c>Предыдущий Low - Текущий Low</c>) и положительно;
    ///       в противном случае <c>+DM = 0</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Если период больше 1, суммируются значения <c>+DM</c> за указанный период,
    ///       а результаты сглаживаются методом Уайлдера (Wilder's smoothing):
    ///       <code>
    ///         +DM(n) = (Предыдущий +DM(n-1) * (Period - 1) + Текущий +DM) / Period
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Высокое значение указывает на сильный восходящий импульс.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Низкое или нулевое значение указывает на слабый или отсутствующий восходящий импульс.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode PlusDM<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        PlusDMImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback period) для <see cref="PlusDM{T}">+DM</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период сглаживания (по умолчанию 14)</param>
    /// <returns>Количество периодов, необходимых до расчёта первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int PlusDMLookback(int optInTimePeriod = 14) => optInTimePeriod switch
    {
        < 1 => -1,
        > 1 => optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.PlusDM) - 1,
        _ => 1
    };

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode PlusDM<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        PlusDMImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode PlusDMImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* +DM1 (за один период) основывается на наибольшей части сегодняшнего диапазона,
         * которая находится вне вчерашнего диапазона.
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
         *  │  │ +DM1 = 0
         * B│  │ -DM1 = 0
         *    D│
         *
         * В случаях 3 и 4 правило гласит: наименьшая разница между (C-A) и (B-D)
         * определяет, какой из +DM или -DM равен нулю.
         *
         * В случае 7 значения (C-A) и (B-D) равны, поэтому оба +DM и -DM равны нулю.
         *
         * Правила остаются теми же, когда A=B и C=D (когда максимумы равны минимумам).
         *
         * При расчёте DM за период > 1, однопериодные значения DM сначала суммируются.
         * Например, для +DM14 суммируются +DM1 за первые 14 дней
         * (фактически 13 значений, так как для первого дня нет значения DM!).
         * Последующие значения DM рассчитываются с использованием сглаживания по Уайлдеру:
         *
         *                                     Предыдущий +DM14
         *   Сегодняшний +DM14 = Предыдущий +DM14 - ─────────────── + Сегодняшний +DM1
         *                                            14
         *
         * Расчёт +DI14 выполняется следующим образом:
         *
         *             +DM14
         *   +DI14 =  ───────
         *             TR14
         *
         * Расчёт TR14 (истинного диапазона за 14 периодов):
         *
         *                                  Предыдущий TR14
         *   Сегодняшний TR14 = Предыдущий TR14 - ───────────── + Сегодняшний TR1
         *                                         14
         *
         *   Первый TR14 — это сумма первых 14 значений TR1.
         *   См. функцию TRange для расчёта истинного диапазона (True Range).
         *
         * Источник:
         *    New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        // Расчёт общего периода задержки (включая нестабильный период)
        var lookbackTotal = PlusDMLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback не осталось данных для расчёта
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Обработка специального случая: период = 1 (без сглаживания)
        if (optInTimePeriod == 1)
        {
            // Без сглаживания. Просто рассчитываем +DM1 для каждого бара.
            return CalcPlusDMForPeriodOne(inHigh, inLow, startIdx, endIdx, outReal, out outRange);
        }

        // Индекс первого валидного значения в выходном массиве
        var outBegIdx = startIdx;
        // Текущий индекс в исходных данных (начинаем с позиции перед первым валидным значением)
        var today = startIdx - lookbackTotal;

        // Преобразование целочисленного периода в числовой тип T
        var timePeriod = T.CreateChecked(optInTimePeriod);
        // Переменная для хранения предыдущего сглаженного значения +DM
        T prevPlusDM = T.Zero, _ = T.Zero;

        // Инициализация значений +DM и -DM для начального периода
        FunctionHelpers.InitDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, out var prevHigh, ref today, out var prevLow, out var _,
            timePeriod, ref prevPlusDM, ref _, ref _);

        // Пропуск нестабильного периода (для достижения стабильных значений сглаживания)
        for (var i = 0; i < Core.UnstablePeriodSettings.Get(Core.UnstableFunc.PlusDM); i++)
        {
            today++;
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, ref today, ref prevHigh, ref prevLow, ref _, ref prevPlusDM,
                ref _, ref _, timePeriod);
        }

        // Сохранение первого валидного значения
        outReal[0] = prevPlusDM;
        var outIdx = 1;

        // Основной цикл расчёта +DM для оставшихся баров
        while (today < endIdx)
        {
            today++;
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, ReadOnlySpan<T>.Empty, ref today, ref prevHigh, ref prevLow, ref _, ref prevPlusDM,
                ref _, ref _, timePeriod);
            outReal[outIdx++] = prevPlusDM;
        }

        // Установка диапазона валидных выходных значений
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Вспомогательный метод для расчёта +DM без сглаживания (период = 1).
    /// </summary>
    /// <param name="high">Диапазон цен High</param>
    /// <param name="low">Диапазон цен Low</param>
    /// <param name="startIdx">Начальный индекс для расчёта</param>
    /// <param name="endIdx">Конечный индекс для расчёта</param>
    /// <param name="outReal">Выходной диапазон для сохранения значений +DM</param>
    /// <param name="outRange">Диапазон валидных индексов в выходном массиве</param>
    /// <typeparam name="T">Числовой тип данных</typeparam>
    /// <returns>Код результата операции</returns>
    private static Core.RetCode CalcPlusDMForPeriodOne<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        int startIdx,
        int endIdx,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Начинаем с предыдущего бара перед первым валидным значением
        var today = startIdx - 1;
        var prevHigh = high[today];
        var prevLow = low[today];
        var outIdx = 0;

        // Расчёт +DM1 для каждого бара
        while (today < endIdx)
        {
            today++;
            // Расчёт разниц: diffP = текущий High - предыдущий High, diffM = предыдущий Low - текущий Low
            var (diffP, diffM) = FunctionHelpers.CalcDeltas(high, low, today, ref prevHigh, ref prevLow);
            // +DM1 = diffP, если он положителен и больше diffM; иначе 0
            outReal[outIdx++] = diffP > T.Zero && diffP > diffM ? diffP : T.Zero;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
