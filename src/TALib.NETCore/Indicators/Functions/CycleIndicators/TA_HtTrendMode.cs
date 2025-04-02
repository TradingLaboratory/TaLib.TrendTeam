//Название файла: TA_HtTrendMode.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//TrendModeIndicators (альтернатива, если требуется группировка по типу индикатора)
//MarketModeIndicators (альтернатива для акцента на режиме рынка)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Trend vs Cycle Mode (Cycle Indicators) — Преобразование Хилберта - Режим тренда против цикла (Индикаторы циклов)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outInteger">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outInteger[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Преобразование Хилберта - Режим тренда против цикла — это индикатор цикла, который определяет, находится ли рынок в трендовом состоянии или в циклическом.
    /// Это достигается путем анализа доминирующих циклов в данных и сравнения их свойств с заранее определенными порогами.
    /// <para>
    /// Функция может помочь выбрать подходящие индикаторы для текущего режима рынка.
    /// В трендовом режиме могут быть предпочтительны инструменты следования за трендом; в циклическом режиме осцилляторы могут давать лучшие результаты.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сгладить входные цены с помощью взвешенного скользящего среднего (WMA), чтобы уменьшить шум и выделить ключевые тренды.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применить преобразование Хилберта для извлечения синфазной (I) и квадратурной (Q) компонент, которые используются для вычисления свойств доминирующего цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить фазу доминирующего цикла (DC Phase) и сгладить фазу по последовательным итерациям.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сгенерировать значения синуса и ведущего синуса с использованием сглаженной фазы DC для обнаружения циклического режима.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Проанализировать пересечения и отклонения компонент синусоидальной волны, чтобы определить, находится ли ряд в трендовом или циклическом режиме.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>Трендовый режим (1)</i> указывает на устойчивое направленное движение рынка,
    ///       при котором циклы играют второстепенную роль.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>Циклический режим (0)</i> указывает на колебания рынка в пределах четко определенного диапазона,
    ///       при котором можно наблюдать доминирующие циклы.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Эта функция менее эффективна на шумных или волатильных рынках, где трудно обнаружить доминирующие циклы.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Ложные срабатывания могут возникать на рынках, переходящих между трендовым и циклическим поведением.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtTrendMode<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<int> outInteger,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtTrendModeImpl(inReal, inRange, outInteger, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="HtTrendMode{T}">HtTrendMode</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    /// <remarks>
    /// Пропускается 31 входных данных для совместимости с Tradestation.
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения "32"
    /// </remarks>
    [PublicAPI]
    public static int HtTrendModeLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtTrendMode) + 31 + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtTrendMode<T>(
        T[] inReal,
        Range inRange,
        int[] outInteger,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtTrendModeImpl<T>(inReal, inRange, outInteger, out outRange);

    private static Core.RetCode HtTrendModeImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<int> outInteger,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        const int smoothPriceSize = 50;
        Span<T> smoothPrice = new T[smoothPriceSize];

        var iTrend3 = T.Zero;
        var iTrend2 = iTrend3;
        var iTrend1 = iTrend2;
        var daysInTrend = 0;
        var sine = T.Zero;
        var leadSine = T.Zero;

        var lookbackTotal = HtTrendModeLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx;

        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 34, out var today);

        var hilbertIdx = 0;
        var smoothPriceIdx = 0;

        /* Инициализировать циклический буфер, используемый логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        var outIdx = 0;

        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
        var period = prevI2 = prevQ2 =
            re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Запомнить smoothedValue в циклическом буфере smoothPrice.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировать период для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            var prevDCPhase = dcPhase;

            /* Вычислить фазу доминирующего цикла */
            dcPhase = ComputeDcPhase(smoothPrice, smoothPeriod, smoothPriceIdx, dcPhase);

            var prevSine = sine;
            var prevLeadSine = leadSine;

            sine = T.Sin(T.DegreesToRadians(dcPhase));
            leadSine = T.Sin(T.DegreesToRadians(dcPhase + FunctionHelpers.Ninety<T>() / FunctionHelpers.Two<T>()));

            // idx используется для итерации по последним 50 значениям smoothPrice.
            var trendLineValue = ComputeTrendLine(inReal, ref today, smoothPeriod, ref iTrend1, ref iTrend2, ref iTrend3);

            var trend = DetermineTrend(sine, leadSine, prevSine, prevLeadSine, smoothPeriod, dcPhase, prevDCPhase, smoothPrice,
                smoothPriceIdx, trendLineValue, ref daysInTrend);

            if (today >= startIdx)
            {
                outInteger[outIdx++] = trend;
            }

            if (++smoothPriceIdx > smoothPriceSize - 1)
            {
                smoothPriceIdx = 0;
            }

            today++;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static int DetermineTrend<T>(
        T sine,
        T leadSine,
        T prevSine,
        T prevLeadSine,
        T smoothPeriod,
        T dcPhase,
        T prevDCPhase,
        Span<T> smoothPrice,
        int smoothPriceIdx,
        T trendLineValue,
        ref int daysInTrend)
        where T : IFloatingPointIeee754<T>
    {
        // Вычислить режим тренда и предположить тренд по умолчанию
        var trend = 1;

        // Измерить дни в тренде с последнего пересечения линий индикатора SineWave
        if (sine > leadSine && prevSine <= prevLeadSine || sine < leadSine && prevSine >= prevLeadSine)
        {
            daysInTrend = 0;
            trend = 0;
        }

        if (T.CreateChecked(++daysInTrend) < T.CreateChecked(0.5) * smoothPeriod)
        {
            trend = 0;
        }

        var tempReal = dcPhase - prevDCPhase;
        if (!T.IsZero(smoothPeriod) &&
            tempReal > T.CreateChecked(0.67) * FunctionHelpers.Ninety<T>() * FunctionHelpers.Four<T>() / smoothPeriod &&
            tempReal < T.CreateChecked(1.5) * FunctionHelpers.Ninety<T>() * FunctionHelpers.Four<T>() / smoothPeriod)
        {
            trend = 0;
        }

        tempReal = smoothPrice[smoothPriceIdx];
        if (!T.IsZero(trendLineValue) && T.Abs((tempReal - trendLineValue) / trendLineValue) >= T.CreateChecked(0.015))
        {
            trend = 1;
        }

        return trend;
    }
}
