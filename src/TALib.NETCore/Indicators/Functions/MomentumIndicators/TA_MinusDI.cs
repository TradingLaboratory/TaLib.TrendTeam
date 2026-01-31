// MinusDI.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// TrendIndicators (альтернатива для акцента на определении направления тренда)
// VolatilityIndicators (альтернатива, так как использует истинный диапазон)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Minus Directional Indicator (Momentum Indicators) — Минус Направленный Индикатор (-DI) (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входной диапазон максимальных цен (High) за каждый период.</param>
    /// <param name="inLow">Входной диапазон минимальных цен (Low) за каждый период.</param>
    /// <param name="inClose">Входной диапазон цен закрытия (Close) за каждый период.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">
    /// Диапазон для хранения рассчитанных значений индикатора -DI.
    /// - Содержит ТОЛЬКО валидные значения индикатора.
    /// - Длина диапазона равна <c>outRange.End.Value - outRange.Start.Value</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения индикатора:
    /// - <b>Start</b>: индекс первого элемента входных данных с валидным значением -DI.
    /// - <b>End</b>: индекс последнего элемента входных данных с валидным значением -DI.
    /// - Если данных недостаточно для расчёта, возвращается диапазон <c>[0, 0)</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте или соответствующий код ошибки.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Минус Направленный Индикатор (-DI) измеряет силу нисходящего движения цены за заданный период.
    /// Является компонентом Системы Направленного Движения (Directional Movement System) Уэллса Уайлдера.
    /// </para>
    /// <para>
    /// Индикатор может использоваться совместно с <see cref="PlusDI{T}">+DI</see> и <see cref="Adx{T}">ADX</see>
    /// для комплексной оценки силы и направления тренда. Подтверждение сигналов дополнительными индикаторами
    /// снижает риск ложной интерпретации.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление однопериодного отрицательного направленного движения (-DM1) как разницы между текущим минимумом (Low)
    ///       и предыдущим минимумом, при условии что эта разница превышает положительное направленное движение (+DM1).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт Истинного Диапазона (True Range, TR), измеряющего полное ценовое движение за период с учётом гэпов и волатильности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение сглаживания по Уайлдеру для суммирования -DM и TR за указанный период по формулам:
    /// <code>
    /// Today's -DM(n) = Previous -DM(n) - (Previous -DM(n) / TimePeriod) + Today's -DM1
    /// Today's TR(n)  = Previous TR(n)  - (Previous TR(n) / TimePeriod) + Today's TR
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление Минус Направленного Индикатора (-DI) по формуле:
    ///       <code>
    ///         -DI = (-DM(n) / TR(n)) * 100
    ///       </code>
    ///       где -DM(n) и TR(n) — сглаженные значения за период.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост значения -DI указывает на усиление нисходящего импульса.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Снижение значения -DI свидетельствует об ослаблении нисходящего импульса.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сравнение <c>+DI</c> (из <see cref="PlusDI{T}">+DI</see>) и <c>-DI</c> помогает определить направление тренда:
    ///       если <c>+DI > -DI</c> — тренд восходящий, если <c>-DI > +DI</c> — тренд нисходящий.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MinusDI<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MinusDIImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период ожидания (lookback period) для индикатора <see cref="MinusDI{T}">-DI</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int MinusDILookback(int optInTimePeriod = 14) => optInTimePeriod switch
    {
        < 1 => -1,
        > 1 => optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.MinusDI),
        _ => 1
    };

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MinusDI<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MinusDIImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MinusDIImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода расчёта
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Расчёт однопериодного направленного движения (DM1):
         * 
         * DM1 основан на наибольшей части сегодняшнего диапазона, выходящей за пределы вчерашнего диапазона.
         * 
         * Семь случаев расчёта +DM1 и -DM1 за один период:
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
         *  │  │ +DM1=0
         * B│  │ -DM1=0
         *    D│
         * 
         * В случаях 3 и 4 правило: наименьшая разница между (C-A) и (B-D) определяет,
         * какой из +DM или -DM будет равен нулю.
         * 
         * В случае 7: (C-A) и (B-D) равны, поэтому оба значения +DM и -DM равны нулю.
         * 
         * Правила остаются теми же, когда A=B и C=D (максимумы равны минимумам).
         * 
         * При расчёте DM за период > 1: сначала суммируются однопериодные значения DM за нужный период.
         * Например, для -DM14 суммируются -DM1 за первые 14 дней (фактически 13 значений,
         * так как для первого дня нет расчёта DM!).
         * Последующие значения рассчитываются с использованием сглаживания по Уайлдеру:
         * 
         *                                     Предыдущий -DM14
         *   Сегодняшний -DM14 = Предыдущий -DM14 - ─────────────── + Сегодняшний -DM1
         *                                              14
         * 
         * Расчёт -DI14 выполняется по формуле:
         * 
         *             -DM14
         *   -DI14 =  ──────── * 100
         *              TR14
         * 
         * Расчёт TR14:
         * 
         *                                  Предыдущий TR14
         *   Сегодняшний TR14 = Предыдущий TR14 - ───────────── + Сегодняшний TR1
         *                                             14
         * 
         * Первый TR14 — сумма первых 14 значений TR1. Подробнее о расчёте истинного диапазона см. функцию TRange.
         * 
         * Источник: New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        // Расчёт минимального количества баров для получения первого валидного значения
        var lookbackTotal = MinusDILookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback периода нет данных для расчёта — выход с успехом (пустой результат)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Особый случай: период = 1 (без сглаживания)
        if (optInTimePeriod == 1)
        {
            /* Без сглаживания расчёт выполняется напрямую для каждого бара:
             *          -DM1
             *   -DI1 = ──── * 100
             *           TR1
             */
            return CalcMinusDIForPeriodOne(inHigh, inLow, inClose, startIdx, endIdx, outReal, out outRange);
        }

        // Инициализация индексов и переменных для расчёта
        var today = startIdx;          // Текущий индекс обработки
        var outBegIdx = today;         // Индекс первого валидного значения в выходных данных
        today = startIdx - lookbackTotal;  // Сдвиг назад для инициализации сглаживания

        var timePeriod = T.CreateChecked(optInTimePeriod);  // Период расчёта как числовой тип T
        T prevMinusDM = T.Zero, prevTR = T.Zero, _ = T.Zero;  // Переменные для хранения сглаженных значений

        // Инициализация значений -DM и TR за начальный период
        FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
            ref _, ref prevMinusDM, ref prevTR);

        // Пропуск нестабильного периода (минимум одна итерация обязательна для первого расчёта)
        for (var i = 0; i < Core.UnstablePeriodSettings.Get(Core.UnstableFunc.MinusDI) + 1; i++)
        {
            today++;
            // Обновление сглаженных значений -DM и TR
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref _,
                ref prevMinusDM, ref prevTR, timePeriod);
        }

        // Расчёт первого значения -DI (с защитой от деления на ноль)
        if (!T.IsZero(prevTR))
        {
            var (minusDI, _) = FunctionHelpers.CalcDI(prevMinusDM, _, prevTR);
            outReal[0] = minusDI;
        }
        else
        {
            outReal[0] = T.Zero;
        }

        var outIdx = 1;  // Индекс в выходном массиве

        // Основной цикл расчёта для оставшихся баров
        while (today < endIdx)
        {
            today++;
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref _,
                ref prevMinusDM, ref prevTR, timePeriod);

            // Расчёт -DI с защитой от деления на ноль
            if (!T.IsZero(prevTR))
            {
                var (minusDI, _) = FunctionHelpers.CalcDI(prevMinusDM, _, prevTR);
                outReal[outIdx++] = minusDI;
            }
            else
            {
                outReal[outIdx++] = T.Zero;
            }
        }

        // Формирование выходного диапазона с валидными значениями
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static Core.RetCode CalcMinusDIForPeriodOne<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        int startIdx,
        int endIdx,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация предыдущих значений для расчёта разниц
        var today = startIdx - 1;
        var prevHigh = high[today];
        var prevLow = low[today];
        var prevClose = close[today];
        var outIdx = 0;

        // Цикл расчёта для каждого бара без сглаживания
        while (today < endIdx)
        {
            today++;
            // Расчёт разниц между текущими и предыдущими максимумами/минимумами
            var (diffP, diffM) = FunctionHelpers.CalcDeltas(high, low, today, ref prevHigh, ref prevLow);
            // Расчёт истинного диапазона (True Range)
            var tr = FunctionHelpers.TrueRange(prevHigh, prevLow, prevClose);

            // Расчёт -DI1: если отрицательное движение превышает положительное и TR ≠ 0
            outReal[outIdx++] = diffM > T.Zero && diffP < diffM && !T.IsZero(tr) ? diffM / tr : T.Zero;
            prevClose = close[today];
        }

        // Формирование выходного диапазона
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
