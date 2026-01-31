// UltOsc.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// VolatilityIndicators (альтернатива через использование True Range)
// MultiTimeframeIndicators (альтернатива для акцента на мультивременных рамках)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Ultimate Oscillator (Momentum Indicators) — Окончательный осциллятор (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входной диапазон максимальных цен (High) для расчета индикатора.</param>
    /// <param name="inLow">Входной диапазон минимальных цен (Low) для расчета индикатора.</param>
    /// <param name="inClose">Входной диапазон цен закрытия (Close) для расчета индикатора.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">
    /// Диапазон для хранения рассчитанных значений индикатора.
    /// - Содержит ТОЛЬКО валидные значения индикатора.
    /// - Длина диапазона равна <c>outRange.End.Value - outRange.Start.Value</c> (если <c>outRange</c> корректен).
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов, представляющий валидные данные в выходном диапазоне:
    /// - <b>Start</b>: индекс первого элемента во входных данных, для которого рассчитано валидное значение.
    /// - <b>End</b>: индекс последнего элемента во входных данных, для которого рассчитано валидное значение.
    /// - Гарантируется: <c>End == inRange.End</c>, если расчет успешен и данных достаточно.
    /// </param>
    /// <param name="optInTimePeriod1">Краткосрочный период усреднения (по умолчанию 7).</param>
    /// <param name="optInTimePeriod2">Среднесрочный период усреднения (по умолчанию 14).</param>
    /// <param name="optInTimePeriod3">Долгосрочный период усреднения (по умолчанию 28).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успешность расчета:
    /// - <see cref="Core.RetCode.Success"/> при успешном расчете,
    /// - соответствующий код ошибки в случае неудачи.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Ultimate Oscillator — осциллятор импульса, разработанный Ларри Уильямсом для оценки соотношения
    /// давления покупателей и продавцов в нескольких временных рамках. Индикатор помогает выявлять
    /// потенциальные развороты цены и состояния перекупленности/перепроданности путем комбинации
    /// краткосрочных, среднесрочных и долгосрочных усредненных значений истинного диапазона (True Range)
    /// и покупательского давления (Buying Pressure).
    /// </para>
    /// <para>
    /// Окончательный осциллятор комбинирует несколько временных рамок для минимизации ложных сигналов,
    /// вызванных краткосрочными рыночными колебаниями, сохраняя при этом достаточную реактивность
    /// на значимые движения цены. Функция предоставляет более сбалансированный взгляд на импульс.
    /// Интеграция с подтверждением тренда или индикаторами объема может усилить достоверность сигналов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого периода рассчитываются истинный диапазон (TR) и покупательское давление (BP):
    /// <code>
    /// True Range (TR) = Maximum(High - Low, Absolute(High - Previous Close), Absolute(Low - Previous Close)).
    /// Buying Pressure (BP) = Close - Minimum(Low, Previous Close).
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитываются усредненные значения BP и TR для краткосрочного, среднесрочного и долгосрочного периодов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисляется значение Ultimate Oscillator как взвешенное среднее трех временных периодов:
    ///       <code>
    ///         UO = 100 * [(4 * (Short BP/TR)) + (2 * (Medium BP/TR)) + (1 * (Long BP/TR))] / (4 + 2 + 1).
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения выше 70 указывают на состояние перекупленности, сигнализируя о потенциальных возможностях для продажи.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения ниже 30 указывают на состояние перепроданности, сигнализируя о потенциальных возможностях для покупки.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Осциллятор может подтверждать тренды при использовании совместно с ценовым действием или другими индикаторами.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode UltOsc<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod1 = 7,
        int optInTimePeriod2 = 14,
        int optInTimePeriod3 = 28) where T : IFloatingPointIeee754<T> =>
        UltOscImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod1, optInTimePeriod2, optInTimePeriod3);

    /// <summary>
    /// Возвращает период задержки (lookback period) для функции <see cref="UltOsc{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod1">Краткосрочный период усреднения.</param>
    /// <param name="optInTimePeriod2">Среднесрочный период усреднения.</param>
    /// <param name="optInTimePeriod3">Долгосрочный период усреднения.</param>
    /// <returns>
    /// Количество периодов, необходимых до первого валидного значения индикатора.
    /// Возвращает -1 при недопустимых значениях периодов.
    /// </returns>
    [PublicAPI]
    public static int UltOscLookback(int optInTimePeriod1 = 7, int optInTimePeriod2 = 14, int optInTimePeriod3 = 28) =>
        optInTimePeriod1 < 1 || optInTimePeriod2 < 1 || optInTimePeriod3 < 1
            ? -1
            : SmaLookback(Math.Max(Math.Max(optInTimePeriod1, optInTimePeriod2), optInTimePeriod3)) + 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode UltOsc<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod1 = 7,
        int optInTimePeriod2 = 14,
        int optInTimePeriod3 = 28) where T : IFloatingPointIeee754<T> =>
        UltOscImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod1, optInTimePeriod2, optInTimePeriod3);

    private static Core.RetCode UltOscImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod1,
        int optInTimePeriod2,
        int optInTimePeriod3) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности границ и соответствия длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности входных периодов (должны быть положительными)
        if (optInTimePeriod1 < 1 || optInTimePeriod2 < 1 || optInTimePeriod3 < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет минимального индекса для первого валидного значения (учитывая период задержки)
        var lookbackTotal = UltOscLookback(optInTimePeriod1, optInTimePeriod2, optInTimePeriod3);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если данных недостаточно для расчета хотя бы одного значения
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Сортировка периодов по возрастанию для корректной работы алгоритма
        SortTimePeriods(ref optInTimePeriod1, ref optInTimePeriod2, ref optInTimePeriod3);

        // Расчет начальных сумм (накопленных значений) для трех временных периодов
        var totals1 = CalcPrimeTotals(inLow, inHigh, inClose, optInTimePeriod1, startIdx);
        var totals2 = CalcPrimeTotals(inLow, inHigh, inClose, optInTimePeriod2, startIdx);
        var totals3 = CalcPrimeTotals(inLow, inHigh, inClose, optInTimePeriod3, startIdx);

        // Константа для нормализации результата (сумма весовых коэффициентов: 4 + 2 + 1 = 7)
        var TSeven = T.CreateChecked(7);

        // Основной цикл расчета осциллятора
        var today = startIdx;          // Текущий индекс обработки
        var outIdx = 0;                // Индекс записи в выходной массив
        var trailingIdx1 = today - optInTimePeriod1 + 1; // Индекс "хвоста" для краткосрочного периода
        var trailingIdx2 = today - optInTimePeriod2 + 1; // Индекс "хвоста" для среднесрочного периода
        var trailingIdx3 = today - optInTimePeriod3 + 1; // Индекс "хвоста" для долгосрочного периода

        while (today <= endIdx)
        {
            // Расчет текущих значений истинного диапазона (TR) и покупательского давления (BP)
            var terms = CalcTerms(inLow, inHigh, inClose, today);
            totals1.aTotal += terms.closeMinusTrueLow; // Накопление суммы BP для краткосрочного периода
            totals2.aTotal += terms.closeMinusTrueLow; // Накопление суммы BP для среднесрочного периода
            totals3.aTotal += terms.closeMinusTrueLow; // Накопление суммы BP для долгосрочного периода
            totals1.bTotal += terms.trueRange;         // Накопление суммы TR для краткосрочного периода
            totals2.bTotal += terms.trueRange;         // Накопление суммы TR для среднесрочного периода
            totals3.bTotal += terms.trueRange;         // Накопление суммы TR для долгосрочного периода

            // Расчет значения осциллятора как взвешенной суммы отношений BP/TR для трех периодов
            var output = T.Zero;

            if (!T.IsZero(totals1.bTotal))
            {
                output += FunctionHelpers.Four<T>() * (totals1.aTotal / totals1.bTotal); // Вес 4 для краткосрочного периода
            }

            if (!T.IsZero(totals2.bTotal))
            {
                output += FunctionHelpers.Two<T>() * (totals2.aTotal / totals2.bTotal);  // Вес 2 для среднесрочного периода
            }

            if (!T.IsZero(totals3.bTotal))
            {
                output += totals3.aTotal / totals3.bTotal;                               // Вес 1 для долгосрочного периода
            }

            // Удаление значений "хвоста" (скользящее окно) для подготовки к следующей итерации
            UpdateTrailingTotals(inLow, inHigh, inClose, trailingIdx1, ref totals1);
            UpdateTrailingTotals(inLow, inHigh, inClose, trailingIdx2, ref totals2);
            UpdateTrailingTotals(inLow, inHigh, inClose, trailingIdx3, ref totals3);

            /* Запись результата в выходной массив.
             * Выполняется ПОСЛЕ обработки "хвостовых" индексов, так как входной и выходной массивы
             * могут ссылаться на одну и ту же область памяти (разрешено вызывающей стороной).
             */
            outReal[outIdx++] = FunctionHelpers.Hundred<T>() * (output / TSeven); // Нормализация в диапазон 0-100
            today++;
            trailingIdx1++;
            trailingIdx2++;
            trailingIdx3++;
        }

        // Установка диапазона валидных выходных значений
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void SortTimePeriods(ref int optInTimePeriod1, ref int optInTimePeriod2, ref int optInTimePeriod3)
    {
        // Флаги использованных периодов для сортировки
        Span<bool> usedFlag = stackalloc bool[3];
        // Исходные периоды
        Span<int> periods = [optInTimePeriod1, optInTimePeriod2, optInTimePeriod3];
        // Отсортированные периоды (по убыванию)
        Span<int> sortedPeriods = stackalloc int[3];

        // Сортировка периодов по убыванию (от наибольшего к наименьшему)
        for (var i = 0; i < 3; ++i)
        {
            var longestPeriod = 0;
            var longestIndex = 0;
            for (var j = 0; j < 3; j++)
            {
                if (!usedFlag[j] && periods[j] > longestPeriod)
                {
                    longestPeriod = periods[j];
                    longestIndex = j;
                }
            }

            usedFlag[longestIndex] = true;
            sortedPeriods[i] = longestPeriod;
        }

        // Присвоение отсортированных значений (в порядке возрастания: period1 < period2 < period3)
        optInTimePeriod1 = sortedPeriods[2];
        optInTimePeriod2 = sortedPeriods[1];
        optInTimePeriod3 = sortedPeriods[0];
    }

    private static void UpdateTrailingTotals<T>(
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> close,
        int trailingIdx,
        ref (T aTotal, T bTotal) totals) where T : IFloatingPointIeee754<T>
    {
        // Расчет значений для "хвостового" индекса (удаляемого из скользящего окна)
        var terms = CalcTerms(low, high, close, trailingIdx);
        totals.aTotal -= terms.closeMinusTrueLow; // Вычитание покупательского давления (BP)
        totals.bTotal -= terms.trueRange;         // Вычитание истинного диапазона (TR)
    }

    private static (T trueRange, T closeMinusTrueLow) CalcTerms<T>(
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> close,
        int day) where T : IFloatingPointIeee754<T>
    {
        // Расчет компонентов для указанного дня:
        var tempLT = low[day];                     // Текущая минимальная цена (Low)
        var tempHT = high[day];                    // Текущая максимальная цена (High)
        var tempCY = close[day - 1];               // Цена закрытия предыдущего дня (Previous Close)
        var trueLow = T.Min(tempLT, tempCY);       // Истинный минимум = min(Low, Previous Close)
        var closeMinusTrueLow = close[day] - trueLow; // Покупательское давление (BP) = Close - True Low
        var trueRange = FunctionHelpers.TrueRange(tempHT, tempLT, tempCY); // Истинный диапазон (TR)

        return (trueRange, closeMinusTrueLow);
    }

    private static (T aTotal, T bTotal) CalcPrimeTotals<T>(
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> close,
        int period,
        int startIdx) where T : IFloatingPointIeee754<T>
    {
        // Инициализация накопительных сумм для периода
        T aTotal = T.Zero, bTotal = T.Zero;
        // Расчет начальных сумм за предыдущие 'period' баров (до первого валидного значения)
        for (var i = startIdx - period + 1; i < startIdx; ++i)
        {
            var tempLT = low[i];
            var tempHT = high[i];
            var tempCY = close[i - 1];
            var trueLow = T.Min(tempLT, tempCY);
            var closeMinusTrueLow = close[i] - trueLow;
            var trueRange = FunctionHelpers.TrueRange(tempHT, tempLT, tempCY);
            var terms = (trueRange, closeMinusTrueLow);
            aTotal += terms.closeMinusTrueLow; // Сумма покупательского давления (BP)
            bTotal += terms.trueRange;         // Сумма истинного диапазона (TR)
        }

        return (aTotal, bTotal);
    }
}
