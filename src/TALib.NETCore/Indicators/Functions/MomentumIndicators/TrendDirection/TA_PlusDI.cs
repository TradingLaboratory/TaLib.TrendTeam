// PlusDI.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// TrendIndicators (альтернатива для акцента на определении направления тренда)
// DirectionalMovement (специализированная группа для индикаторов системы направленного движения)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Plus Directional Indicator (+DI) (Momentum Indicators) — Индикатор положительного направленного движения (+DI) (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входные данные: максимальные цены (High) за каждый период</param>
    /// <param name="inLow">Входные данные: минимальные цены (Low) за каждый период</param>
    /// <param name="inClose">Входные данные: цены закрытия (Close) за каждый период</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатываются все данные во входных массивах.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора +DI.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c> (и аналогично для Low/Close).
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина входных данных меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период сглаживания для расчета индикатора (по умолчанию 14)</param>
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
    /// Индикатор положительного направленного движения (+DI) измеряет силу восходящего движения цены за заданный период времени.
    /// Является частью системы направленного движения (Directional Movement System) Уэллса Уайлдера.
    /// </para>
    /// <para>
    /// Функция может использоваться совместно с <see cref="PlusDM{T}">+DM</see> и <see cref="Adx{T}">ADX</see> для получения
    /// полной картины силы тренда. Подтверждение сигналов дополнительными индикаторами снижает риск неверной интерпретации.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет однопериодного положительного направленного движения (+DM1) как разницы между текущим максимумом (High) и предыдущим максимумом,
    ///       при условии, что эта разница превышает отрицательное направленное движение (-DM1).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление истинного диапазона (True Range, TR), представляющего полное ценовое движение за период с учетом гэпов и волатильности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение сглаживания Уайлдера для расчета сглаженных значений +DM и TR за указанный период:
    /// <code>
    /// Today's +DM(n) = Previous +DM(n) - (Previous +DM(n) / TimePeriod) + Today's +DM1
    /// Today's TR(n)  = Previous TR(n)  - (Previous TR(n) / TimePeriod) + Today's TR
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет индикатора +DI как отношения сглаженного +DM к сглаженному TR, выраженное в процентах:
    ///       <code>
    ///         +DI = (+DM(n) / TR(n)) * 100
    ///       </code>
    ///       где <c>+DM(n)</c> и <c>TR(n)</c> — сглаженные значения за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост значения +DI указывает на усиление восходящего импульса.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Падение значения +DI свидетельствует об ослаблении восходящего импульса.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сравнение <c>+DI</c> и <c>-DI</c> (из <see cref="MinusDI{T}">-DI</see>) помогает определить направление тренда:
    ///       если <c>+DI > -DI</c>, тренд восходящий; если <c>-DI > +DI</c>, тренд нисходящий.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode PlusDI<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        PlusDIImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback) для индикатора <see cref="PlusDI{T}">+DI</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период сглаживания индикатора</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного значения индикатора</returns>
    [PublicAPI]
    public static int PlusDILookback(int optInTimePeriod = 14) => optInTimePeriod switch
    {
        < 1 => -1,
        > 1 => optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.PlusDI),
        _ => 1
    };

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode PlusDI<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        PlusDIImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode PlusDIImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона для всех массивов цен
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода сглаживания
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Расчет +DM1 (однопериодного направленного движения) основан на наибольшей части сегодняшнего диапазона,
         * которая находится вне вчерашнего диапазона.
         *
         * Следующие 7 случаев объясняют, как рассчитываются +DM и -DM за один период:
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
         * В случаях 3 и 4 правило гласит: наименьшая разница между (C-A) и (B-D) определяет,
         * какой из +DM или -DM будет равен нулю.
         *
         * В случае 7 значения (C-A) и (B-D) равны, поэтому оба +DM и -DM равны нулю.
         *
         * Правила остаются теми же, когда A=B и C=D (когда максимумы равны минимумам).
         *
         * При расчете DM за период > 1, однопериодные значения DM за требуемый период сначала суммируются.
         * Например, для -DM14 суммируются -DM1 за первые 14 дней
         * (это 13 значений, так как для первого дня DM не рассчитывается!)
         * Последующие значения DM рассчитываются с использованием сглаживания Уайлдера:
         *
         *                                     Предыдущий +DM14
         *   Сегодняшний +DM14 = Предыдущий +DM14 - ─────────────── + Сегодняшний +DM1
         *                                            14
         *
         * Расчет +DI14 выполняется следующим образом:
         *
         *             +DM14
         *   +DI14 =  ──────── * 100
         *              TR14
         *
         * Расчет TR14:
         *
         *                                  Предыдущий TR14
         *   Сегодняшний TR14 = Предыдущий TR14 - ─────────────── + Сегодняшний TR1
         *                                             14
         *
         *   Первый TR14 — это сумма первых 14 значений TR1. См. функцию TRange для расчета истинного диапазона.
         *
         * Источник:
         *    New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        // Расчет минимального количества баров, необходимых для первого валидного значения
        var lookbackTotal = PlusDILookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учета lookback периода нет данных для расчета — выход
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Специальный случай для периода = 1 (без сглаживания)
        if (optInTimePeriod == 1)
        {
            /* Без сглаживания расчет выполняется напрямую для каждого бара:
             *          +DM1
             *   +DI1 = ──── * 100
             *           TR1
             */
            return CalcPlusDIForPeriodOne(inHigh, inLow, inClose, startIdx, endIdx, outReal, out outRange);
        }

        // Инициализация индексов и переменных для сглаживания
        var today = startIdx;
        var outBegIdx = today; // Индекс первого валидного значения в выходных данных
        today = startIdx - lookbackTotal; // Начало расчета с учетом периода задержки

        var timePeriod = T.CreateChecked(optInTimePeriod);
        T prevPlusDM = T.Zero, prevTR = T.Zero, _ = T.Zero; // prevMinusDM не используется в +DI, но требуется для вызова общих функций

        // Инициализация первых значений +DM и TR за период сглаживания
        FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
            ref prevPlusDM, ref _, ref prevTR);

        // Пропуск нестабильного периода (минимум 1 итерация для расчета первого значения)
        for (var i = 0; i < Core.UnstablePeriodSettings.Get(Core.UnstableFunc.PlusDI) + 1; i++)
        {
            today++;
            // Обновление значений +DM и TR с применением сглаживания Уайлдера
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref _, ref prevTR, timePeriod);
        }

        // Расчет первого значения +DI (с защитой от деления на ноль)
        if (!T.IsZero(prevTR))
        {
            var (_, plusDI) = FunctionHelpers.CalcDI(_, prevPlusDM, prevTR);
            outReal[0] = plusDI;
        }
        else
        {
            outReal[0] = T.Zero;
        }

        var outIdx = 1; // Индекс для записи в выходной массив

        // Основной цикл расчета +DI для оставшихся баров
        while (today < endIdx)
        {
            today++;
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref _, ref prevTR, timePeriod);
            if (!T.IsZero(prevTR))
            {
                var (_, plusDI) = FunctionHelpers.CalcDI(_, prevPlusDM, prevTR);
                outReal[outIdx++] = plusDI;
            }
            else
            {
                outReal[outIdx++] = T.Zero;
            }
        }

        // Установка диапазона валидных значений в выходных данных
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static Core.RetCode CalcPlusDIForPeriodOne<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        int startIdx,
        int endIdx,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация предыдущих значений цен для расчета разниц
        var today = startIdx - 1;
        var prevHigh = high[today];
        var prevLow = low[today];
        var prevClose = close[today];
        var outIdx = 0;

        // Расчет +DI для периода = 1 (без сглаживания)
        while (today < endIdx)
        {
            today++;
            // Расчет разниц между текущими и предыдущими максимумами/минимумами
            var (diffP, diffM) = FunctionHelpers.CalcDeltas(high, low, today, ref prevHigh, ref prevLow);
            // Расчет истинного диапазона (TR)
            var tr = FunctionHelpers.TrueRange(prevHigh, prevLow, prevClose);

            // Расчет +DI1: +DM1 / TR1 (с защитой от деления на ноль и условия +DM1 > -DM1)
            outReal[outIdx++] = diffP > T.Zero && diffP > diffM && !T.IsZero(tr) ? diffP / tr : T.Zero;
            prevClose = close[today];
        }

        // Установка диапазона валидных значений
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
