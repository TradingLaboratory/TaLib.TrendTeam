//Название файла: TA_Adx.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendStrength (альтернатива для акцента на силе тренда)
//DirectionalMovement (альтернатива для акцента на направленном движении)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Индекс среднего направленного движения (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Массив входных максимальных цен.</param>
    /// <param name="inLow">Массив входных минимальных цен.</param>
    /// <param name="inClose">Массив входных цен закрытия.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Индекс среднего направленного движения (ADX) — это индикатор импульса, который измеряет силу тренда
    /// без указания его направления. Он выводится из индикаторов направленного движения (+DI и -DI) и часто используется
    /// вместе с ними для оценки интенсивности рыночных трендов.
    /// <para>
    /// Функция может направлять выбор стратегий следования за трендом или основанных на диапазоне. Часто комбинируется с
    /// скользящими средними, осцилляторами или анализом уровней поддержки/сопротивления, чтобы избежать ложных сигналов в условиях
    /// низкой силы тренда.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать значения Истинного диапазона (TR) и Направленного движения (DM):
    /// <code>
    /// TR = max(High - Low, abs(High - Previous Close), abs(Low - Previous Close))
    /// +DM = High - Previous High (если положительное и больше, чем Low - Previous Low, иначе 0)
    /// -DM = Previous Low - Low (если положительное и больше, чем High - Previous High, иначе 0)
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сгладить значения TR, +DM и -DM за указанный период времени с использованием метода сглаживания Уайлдера.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать индикаторы направленного движения (+DI и -DI):
    /// <code>
    /// +DI = 100 * (+DM / TR)
    /// -DI = 100 * (-DM / TR)
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать индекс направленного движения (DX):
    ///       <code>
    ///         DX = 100 * abs(+DI - -DI) / (+DI + -DI)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить ADX как сглаженное среднее значений DX за указанный период времени.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения выше 25 указывают на сильный тренд, тогда как значения ниже 20 свидетельствуют о слабом или отсутствующем тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Растущее значение указывает на усиление тренда, тогда как падающий ADX свидетельствует об ослабевающем тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       ADX не указывает направление тренда; он измеряет только его силу.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Adx<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AdxImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Adx{T}">Adx</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    [PublicAPI]
    public static int AdxLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod * 2 + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Adx) - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Adx<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AdxImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode AdxImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inClose.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* DM1 (один период) основан на самой большой части сегодняшнего диапазона, которая находится за пределами вчерашнего диапазона.
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
         *  │  │ +DM=0
         * B│  │ -DM=0
         *    D│
         *
         * В случаях 3 и 4 правило заключается в том, что наименьшая разница между (C-A) и (B-D) определяет,
         * какое из значений +DM или -DM будет равно нулю.
         *
         * В случае 7 (C-A) и (B-D) равны, поэтому оба значения +DM и -DM равны нулю.
         *
         * Правила остаются теми же, когда A=B и C=D (когда максимумы равны минимумам).
         *
         * При расчете DM за период > 1, однопериодные DM для желаемого периода сначала суммируются.
         * Другими словами, для -DM14 суммируются -DM1 за первые 14 дней
         * (это 13 значений, так как для первого дня нет DM!)
         * Последующие DM рассчитываются с использованием метода сглаживания Уайлдера:
         *
         *                                    Previous -DM14
         *   Сегодняшний -DM14 = Previous -DM14 -  ────────────── + Сегодняшний -DM1
         *                                          14
         *
         * (То же самое для +DM14)
         *
         * Расчет -DI14 выполняется следующим образом:
         *
         *             -DM14
         *   -DI14 =  ────────
         *              TR14
         *
         * (То же самое для +DI14)
         *
         * Расчет TR14 выполняется следующим образом:
         *
         *                                  Previous TR14
         *   Сегодняшний TR14 = Previous TR14 - ───────────── + Сегодняшний TR1
         *                                        14
         *
         *   Первый TR14 — это сумма первых 14 TR1. См. функцию TRange для расчета истинного диапазона.
         *
         * Расчет DX14 выполняется следующим образом:
         *
         *   diffDI = ABS((-DI14) - (+DI14))
         *   sumDI  = (-DI14) + (+DI14)
         *
         *   DX14 = 100 * (diffDI / sumDI)
         *
         * Расчет первого ADX:
         *
         *   ADX14 = СУММА первых 14 DX
         *
         * Расчет последующих ADX:
         *
         *           ((Previous ADX14) * (14 - 1)) + Сегодняшний DX
         *   ADX14 = ──────────────────────────────────────────
         *                            14
         *
         * Источник:
         *   New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        var lookbackTotal = AdxLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);
        T prevMinusDM, prevPlusDM;
        var prevTR = prevMinusDM = prevPlusDM = T.Zero;
        var today = startIdx;
        var outBegIdx = today;
        today = startIdx - lookbackTotal;

        // Инициализация значений DM и TR
        FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
            ref prevPlusDM, ref prevMinusDM, ref prevTR);

        // Суммирование всех начальных значений DX
        var sumDX = AddAllInitialDX(inHigh, inLow, inClose, timePeriod, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
            ref prevMinusDM, ref prevTR);

        // Расчет первого ADX
        var prevADX = sumDX / timePeriod;

        // Пропуск нестабильного периода ADX
        SkipAdxUnstablePeriod(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM, ref prevMinusDM,
            ref prevTR, timePeriod, ref prevADX);

        // Вывод первого ADX
        outReal[0] = prevADX;
        var outIdx = 1;

        // Расчет и вывод последующих значений ADX
        CalcAndOutputSubsequentADX(inHigh, inLow, inClose, outReal, ref today, endIdx, ref prevHigh, ref prevLow, ref prevClose,
            ref prevPlusDM, ref prevMinusDM, ref prevTR, timePeriod, ref prevADX, ref outIdx);

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T AddAllInitialDX<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        T timePeriod,
        ref int today,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR) where T : IFloatingPointIeee754<T>
    {
        var sumDX = T.Zero;
        for (var i = Int32.CreateTruncating(timePeriod); i > 0; i--)
        {
            today++;
            // Обновление значений DM и TR
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);

            if (T.IsZero(prevTR))
            {
                continue;
            }

            var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
            var tempReal = minusDI + plusDI;
            if (!T.IsZero(tempReal))
            {
                sumDX += FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal);
            }
        }

        return sumDX;
    }

    private static void SkipAdxUnstablePeriod<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> icClose,
        ref int today,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod,
        ref T prevADX) where T : IFloatingPointIeee754<T>
    {
        for (var i = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Adx); i > 0; i--)
        {
            today++;
            // Обновление значений DM и TR
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, icClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);

            if (T.IsZero(prevTR))
            {
                continue;
            }

            var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
            var tempReal = minusDI + plusDI;
            if (T.IsZero(tempReal))
            {
                continue;
            }

            tempReal = FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal);
            prevADX = (prevADX * (timePeriod - T.One) + tempReal) / timePeriod;
        }
    }

    private static void CalcAndOutputSubsequentADX<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Span<T> outReal,
        ref int today,
        int endIdx,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod,
        ref T prevADX,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        while (today < endIdx)
        {
            today++;
            // Обновление значений DM и TR
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);

            if (!T.IsZero(prevTR))
            {
                var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                var tempReal = minusDI + plusDI;
                if (!T.IsZero(tempReal))
                {
                    tempReal = FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal);
                    prevADX = (prevADX * (timePeriod - T.One) + tempReal) / timePeriod;
                }
            }

            outReal[outIdx++] = prevADX;
        }
    }
}
