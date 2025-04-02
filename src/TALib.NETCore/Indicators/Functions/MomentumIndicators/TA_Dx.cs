//Название файла: TA_Dx.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendStrength (альтернатива для акцента на силе тренда)
//DirectionalMovement (альтернатива для акцента на направленном движении)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Directional Movement Index (Momentum Indicators) — Индекс направленного движения (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены (High).</param>
    /// <param name="inLow">Входные минимальные цены (Low).</param>
    /// <param name="inClose">Входные цены закрытия (Close).</param>
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Индекс направленного движения (DX) — это технический индикатор, используемый для измерения силы тренда на рынке.
    /// <para>
    /// Он является частью расчета Индекса среднего направленного движения (<see cref="Adx{T}">ADX</see>) и предоставляет основу для понимания
    /// положительного (<see cref="PlusDI{T}">PlusDI</see>) и отрицательного (<see cref="MinusDI{T}">MinusDI</see>) направленного движения.
    /// Использование его вместе с индикаторами тренда или объема может улучшить идентификацию сильных направленных движений.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить Истинный диапазон (TR), Положительное направленное движение (+DM) и Отрицательное направленное движение (-DM).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать сглаженные средние значения для +DM, -DM и TR за указанный период времени с использованием метода сглаживания Уайлдера.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вывести +DI и -DI, используя сглаженные значения:
    /// <code>
    /// +DI = (+DM / TR) * 100
    /// -DI = (-DM / TR) * 100
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать DX:
    ///       <code>
    ///         DX = ((|+DI - -DI|) / (+DI + -DI)) * 100
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Высокое значение (например, выше 25) обычно сигнализирует о сильном тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Низкое значение (например, ниже 20) указывает на слабый или отсутствующий тренд.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       DX сам по себе не указывает направление тренда; вместо этого +DI и -DI предоставляют информацию о направлении.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Dx<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        DxImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Dx{T}">Dx</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    [PublicAPI]
    public static int DxLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Dx);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Dx<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        DxImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode DxImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
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
         * В случаях 3 и 4 правило заключается в том, что наименьшая дельта между (C-A) и (B-D) определяет, какое из значений +DM или -DM будет равно нулю.
         *
         * В случае 7 (C-A) и (B-D) равны, поэтому оба значения +DM и -DM равны нулю.
         *
         * Правила остаются теми же, когда A=B и C=D (когда максимумы равны минимумам).
         *
         * При расчете DM за период > 1, сначала суммируются однопериодные DM за нужный период.
         * Другими словами, для -DM14 суммируются -DM1 за первые 14 дней
         * (это 13 значений, так как для первого дня нет DM!)
         * Последующие DM рассчитываются с использованием метода сглаживания Уайлдера:
         *
         *                                     Прежний -DM14
         *   Сегодняшний -DM14 = Прежний -DM14 -  ────────────── + Сегодняшний -DM1
         *                                           14
         *
         * Расчет -DI14 выполняется следующим образом:
         *
         *             -DM14
         *   -DI14 =  ────────
         *              TR14
         *
         * Расчет TR14 выполняется следующим образом:
         *
         *                                  Прежний TR14
         *   Сегодняшний TR14 = Прежний TR14 - ───────────── + Сегодняшний TR1
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
         * Источник:
         *    New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        var lookbackTotal = DxLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);
        T prevMinusDM, prevPlusDM;
        var prevTR = prevMinusDM = prevPlusDM = T.Zero;
        var today = startIdx - lookbackTotal;

        // Инициализация DM и TR
        FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
            ref prevPlusDM, ref prevMinusDM, ref prevTR);

        // Пропуск нестабильного периода. Этот цикл должен быть выполнен хотя бы ОДИН раз для расчета первого DI.
        SkipDxUnstablePeriod(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM, ref prevMinusDM,
            ref prevTR, timePeriod);

        if (!T.IsZero(prevTR))
        {
            var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
            T tempReal = minusDI + plusDI;
            outReal[0] = !T.IsZero(tempReal) ? FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal) : T.Zero;
        }
        else
        {
            outReal[0] = T.Zero;
        }

        var outIdx = 1;

        CalcAndOutputDX(inHigh, inLow, inClose, outReal, ref today, endIdx, ref prevHigh, ref prevLow, ref prevClose,
            ref prevPlusDM, ref prevMinusDM, ref prevTR, timePeriod, ref outIdx);

        outRange = Range.EndAt(outIdx);

        return Core.RetCode.Success;
    }

    private static void SkipDxUnstablePeriod<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ref int today,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod) where T : IFloatingPointIeee754<T>
    {
        for (var i = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Dx) + 1; i > 0; i--)
        {
            today++;
            // Обновление DM и TR
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);
        }
    }

    private static void CalcAndOutputDX<T>(
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
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        while (today < endIdx)
        {
            today++;

            // Обновление DM и TR
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);

            var tempReal = FunctionHelpers.TrueRange(prevHigh, prevLow, prevClose);
            prevTR = prevTR - prevTR / timePeriod + tempReal;
            prevClose = inClose[today];

            if (!T.IsZero(prevTR))
            {
                var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                tempReal = minusDI + plusDI;
                outReal[outIdx] = !T.IsZero(tempReal)
                    ? FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal)
                    : outReal[outIdx - 1];
            }
            else
            {
                outReal[outIdx] = outReal[outIdx - 1];
            }

            outIdx++;
        }
    }
}
