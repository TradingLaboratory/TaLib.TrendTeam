//Название файла: TA_Sar.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//TrendFollowing (альтернатива, если требуется группировка по типу индикатора)
//ReversalIndicators (альтернатива для акцента на разворотах тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Parabolic SAR (Overlap Studies) — Параболический SAR (Индикаторы наложения)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High).</param>
    /// <param name="inLow">Массив минимальных цен (Low).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInAcceleration">
    /// Коэффициент ускорения, который контролирует чувствительность SAR:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения делают SAR более чувствительным к изменениям цен, но могут увеличить риск ложных сигналов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшие значения снижают чувствительность, обеспечивая более плавные значения.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.01..0.2</c>.
    /// </para>
    /// </param>
    /// <param name="optInMaximum">
    /// Максимальное значение, до которого может увеличиваться коэффициент ускорения:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения позволяют SAR быстрее ускоряться во время сильных трендов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшие значения ограничивают ускорение, поддерживая более плавные тренды.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.01..0.5</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает <see cref="Core.RetCode"/>, указывающий успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Параболический Stop and Reverse (SAR) — это трендовый индикатор, предназначенный для определения потенциальных точек разворота на рынке.
    /// Он отображает серию точек выше или ниже ценовых баров, чтобы указать направление тренда.
    /// По мере развития тренда точки SAR приближаются к ценам, предоставляя динамические уровни стоп-лосс, адаптирующиеся к изменяющимся условиям рынка.
    /// <para>
    /// Функция особенно полезна для определения направления тренда, установки трейлинговых стопов и выявления потенциальных разворотов.
    /// Использование SAR вместе с трендовыми или волатильными индикаторами, такими как <see cref="Adx{T}">ADX</see> или
    /// <see cref="Atr{T}">ATR</see>, усиливает его применение.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определите начальное направление тренда на основе направленного движения (DM) первых двух баров:
    ///       <code>
    ///         Direction = Long, если +DM > -DM; иначе Short.
    ///       </code>
    ///       В случае равенства, тренд по умолчанию считается Long.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Установите начальные значения SAR (Stop and Reverse) и Extreme Point (EP):
    /// <code>
    /// SAR = Lowest Low (для Long) или Highest High (для Short) первого ценового бара.
    /// EP = Highest High (для Long) или Lowest Low (для Short) второго ценового бара.
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого последующего ценового бара:
    ///       <list type="bullet">
    ///         <item>
    ///           <description>
    ///             Рассчитайте SAR по формуле:
    ///             <code>
    ///               SAR = Previous SAR + Acceleration Factor * (EP - Previous SAR)
    ///             </code>
    ///             Где Acceleration Factor (AF) начинается с начального значения и увеличивается постепенно с новыми максимумами/минимумами до максимального значения.
    ///             EP обновляется до нового максимума (для Long) или минимума (для Short).
    ///             </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             Убедитесь, что SAR не проникает в диапазон двух предыдущих ценовых баров.
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             Если SAR пересекает текущую цену, направление тренда меняется, и SAR сбрасывается до EP.
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Точки SAR ниже цены указывают на восходящий тренд, предоставляя потенциальные уровни стоп-лосс для длинных позиций.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Точки SAR выше цены указывают на нисходящий тренд, предоставляя потенциальные уровни стоп-лосс для коротких позиций.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Когда SAR переходит снизу вверх (или наоборот), это сигнализирует о потенциальном развороте тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sar<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        double optInAcceleration = 0.02,
        double optInMaximum = 0.2) where T : IFloatingPointIeee754<T> =>
        SarImpl(inHigh, inLow, inRange, outReal, out outRange, optInAcceleration, optInMaximum);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Sar{T}">Sar</see>.
    /// </summary>
    /// <returns>Всегда 1, так как для этого расчета требуется только один ценовой бар.</returns>
    [PublicAPI]
    public static int SarLookback() => 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sar<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        double optInAcceleration = 0.02,
        double optInMaximum = 0.2) where T : IFloatingPointIeee754<T> =>
        SarImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInAcceleration, optInMaximum);

    private static Core.RetCode SarImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        double optInAcceleration,
        double optInMaximum) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInAcceleration < 0.0 || optInMaximum < 0.0)
        {
            return Core.RetCode.BadParam;
        }

        /* Реализация SAR была немного интерпретирована, так как Уайлдер (оригинальный автор)
         * не определил точный алгоритм для запуска алгоритма.
         * Возьмите любое существующее программное обеспечение, и вы увидите небольшие вариации в адаптации алгоритма.
         *
         * Каково начальное направление торговли? Длинное или короткое?
         * ───────────────────────────────────────────────────
         * Интерпретация начальных значений SAR открыта для обсуждения,
         * особенно потому, что вызывающий метод не указывает начальное направление торговли.
         *
         * Следующая логика используется:
         *   - Рассчитайте +DM и -DM между первым и вторым баром.
         *     Наибольшее направленное значение укажет предполагаемое направление торговли для второго ценового бара.
         *   - В случае равенства между +DM и -DM, направление по умолчанию — LONG.
         *
         * Какова начальная "экстремальная точка" и, следовательно, SAR?
         * ─────────────────────────────────────────────────
         * Ниже показано, как разные люди использовали разные подходы:
         *   - Metastock использует максимум/минимум первого ценового бара в зависимости от направления.
         *     SAR для первого ценового бара не рассчитывается.
         *   - Tradestation использует цену закрытия второго бара.
         *     SAR для первого ценового бара не рассчитывается.
         *   - Уайлдер (оригинальный автор) использует SIP с предыдущей торговли
         *     (не может быть реализовано здесь, так как направление и длина предыдущей торговли неизвестны).
         *   - Журнал TASC, похоже, следует подходу Уайлдера, что здесь непрактично.
         *
         * Библиотека "потребляет" первый ценовой бар и использует его максимум/минимум в качестве начального SAR второго ценового бара.
         * Было обнаружено, что такой подход наиболее близок к идее Уайлдера о том, что
         * первый день входа использует предыдущую экстремальную точку, за исключением того, что здесь экстремальная точка
         * определяется только по первому ценовому бару. Было обнаружено, что Metastock использует тот же подход.
         */

        var lookbackTotal = SarLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Убедитесь, что ускорение и максимум согласованы. Если нет, скорректируйте ускорение.
        optInAcceleration = Math.Min(optInAcceleration, optInMaximum);
        var af = optInAcceleration;

        // Определите, является ли начальное направление длинным или коротким.
        // (ep используется здесь как временный буфер, название параметра не значимо).
        Span<T> epTemp = new T[1];
        var retCode = MinusDMImpl(inHigh, inLow, new Range(startIdx, startIdx), epTemp, out _, 1);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var outBegIdx = startIdx;
        var outIdx = 0;

        var todayIdx = startIdx;

        var newHigh = inHigh[todayIdx - 1];
        var newLow = inLow[todayIdx - 1];

        var isLong = epTemp[0] <= T.Zero;

        var sar = InitializeSar(inHigh, inLow, isLong, todayIdx, newLow, newHigh, out var ep);

        // Используем обновленные значения newLow и newHigh для первой итерации.
        newLow = inLow[todayIdx];
        newHigh = inHigh[todayIdx];

        while (todayIdx <= endIdx)
        {
            var prevLow = newLow;
            var prevHigh = newHigh;
            newLow = inLow[todayIdx];
            newHigh = inHigh[todayIdx];
            todayIdx++;

            if (isLong)
            {
                // Переключиться на короткую позицию, если минимальная цена пробивает значение SAR.
                if (newLow <= sar)
                {
                    // Переключение и переопределение SAR на ep
                    isLong = false;
                    sar = SwitchToShort(ref ep, prevHigh, newLow, newHigh, out af, optInAcceleration, T.NegativeOne, ref outIdx, outReal);
                }
                else
                {
                    // Без переключения
                    // Вывод SAR (рассчитан в предыдущей итерации)
                    outReal[outIdx++] = sar;

                    sar = ProcessLongPosition(ref ep, prevLow, newLow, newHigh, ref af, optInAcceleration, optInMaximum, sar);
                }
            }
            /* Переключиться на длинную позицию, если максимальная цена пробивает значение SAR. */
            else if (newHigh >= sar)
            {
                /* Переключение и переопределение SAR на ep */
                isLong = true;
                sar = SwitchToLong(ref ep, prevLow, newLow, newHigh, out af, optInAcceleration, T.NegativeOne, ref outIdx, outReal);
            }
            else
            {
                // Без переключения
                // Вывод SAR (рассчитан в предыдущей итерации)
                outReal[outIdx++] = sar;

                sar = ProcessShortPosition(ref ep, prevHigh, newLow, newHigh, ref af, optInAcceleration, optInMaximum, sar);
            }
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T InitializeSar<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        bool isLong,
        int todayIdx,
        T newLow,
        T newHigh,
        out T ep) where T : IFloatingPointIeee754<T>
    {
        T sar;
        if (isLong)
        {
            ep = inHigh[todayIdx];
            sar = newLow;
        }
        else
        {
            ep = inLow[todayIdx];
            sar = newHigh;
        }

        return sar;
    }

    private static T SwitchToShort<T>(
        ref T ep,
        T prevHigh,
        T newLow,
        T newHigh,
        out double af,
        double optInAcceleration,
        T optInOffsetOnReverse,
        ref int outIdx,
        Span<T> outReal) where T : IFloatingPointIeee754<T>
    {
        var sar = ep;

        // Убедитесь, что переопределенный SAR находится в диапазоне вчерашнего и сегодняшнего значений.
        sar = T.Max(sar, prevHigh);
        sar = T.Max(sar, newHigh);

        if (optInOffsetOnReverse > T.Zero)
        {
            sar += sar * optInOffsetOnReverse;
        }

        // Вывод переопределенного SAR
        outReal[outIdx++] = sar * (optInOffsetOnReverse < T.Zero ? T.One : T.NegativeOne);

        // Корректировка af и ep
        af = optInAcceleration;
        ep = newLow;

        sar += T.CreateChecked(af) * (ep - sar);

        // Убедитесь, что новый SAR находится в диапазоне вчерашнего и сегодняшнего значений.
        sar = T.Max(sar, prevHigh);
        sar = T.Max(sar, newHigh);

        return sar;
    }

    private static T ProcessLongPosition<T>(
        ref T ep,
        T prevLow,
        T newLow,
        T newHigh,
        ref double af,
        double optInAcceleration,
        double optInMaximum,
        T sar) where T : IFloatingPointIeee754<T>
    {
        // Корректировка af и ep.
        if (newHigh > ep)
        {
            ep = newHigh;
            af += optInAcceleration;
            af = Math.Min(af, optInMaximum);
        }

        // Рассчитать новый SAR
        sar += T.CreateChecked(af) * (ep - sar);

        // Убедитесь, что новый SAR находится в диапазоне вчерашнего и сегодняшнего значений.
        sar = T.Min(sar, prevLow);
        sar = T.Min(sar, newLow);

        return sar;
    }

    private static T SwitchToLong<T>(
        ref T ep,
        T prevLow,
        T newLow,
        T newHigh,
        out double af,
        double optInAcceleration,
        T optInOffsetOnReverse,
        ref int outIdx,
        Span<T> outReal) where T : IFloatingPointIeee754<T>
    {
        var sar = ep;

        // Убедитесь, что переопределенный SAR находится в диапазоне вчерашнего и сегодняшнего значений.
        sar = T.Min(sar, prevLow);
        sar = T.Min(sar, newLow);

        if (optInOffsetOnReverse > T.Zero)
        {
            sar -= sar * optInOffsetOnReverse;
        }

        // Вывод переопределенного SAR
        outReal[outIdx++] = sar;

        /* Корректировка af и ep */
        af = optInAcceleration;
        ep = newHigh;

        sar += T.CreateChecked(af) * (ep - sar);

        // Убедитесь, что новый SAR находится в диапазоне вчерашнего и сегодняшнего значений.
        sar = T.Min(sar, prevLow);
        sar = T.Min(sar, newLow);

        return sar;
    }

    private static T ProcessShortPosition<T>(
        ref T ep,
        T prevHigh,
        T newLow,
        T newHigh,
        ref double af,
        double optInAcceleration,
        double optInMaximum,
        T sar) where T : IFloatingPointIeee754<T>
    {
        // Корректировка af и ep.
        if (newLow < ep)
        {
            ep = newLow;
            af += optInAcceleration;
            af = Math.Min(af, optInMaximum);
        }

        // Рассчитать новый SAR
        sar += T.CreateChecked(af) * (ep - sar);

        // Убедитесь, что новый SAR находится в диапазоне вчерашнего и сегодняшнего значений.
        sar = T.Max(sar, prevHigh);
        sar = T.Max(sar, newHigh);

        return sar;
    }
}
