//Название файла TA_HtTrendline.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//OverlapStudies (альтернатива, если требуется группировка по типу индикатора)
//TrendIndicators (альтернатива для акцента на трендовых индикаторах)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Instantaneous Trendline (Overlap Studies) — Трансформация Гильберта - Мгновенная Трендовая Линия (Наложение)
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
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Трансформация Гильберта - Мгновенная Трендовая Линия — это циклический индикатор, предназначенный для вычисления сглаженной трендовой линии с использованием
    /// трансформации Гильберта. Он устраняет шум и обеспечивает мгновенное представление тренда, комбинируя недавние сглаженные точки данных и
    /// экстраполируя на основе доминирующих циклов.
    /// <para>
    /// Функция может быть комбинирована с <see cref="Adx{T}">ADX</see> или <see cref="Macd{T}">MACD</see>, чтобы убедиться, что изменения
    /// в трендовой линии соответствуют более широким условиям, снижая риск действий на ложные сигналы.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сглаживание входных цен с использованием взвешенного скользящего среднего (WMA) для уменьшения шума и обеспечения более плавных переходов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение трансформации Гильберта для извлечения фазовой (I) и квадратурной (Q) компонент, которые используются для определения свойств цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление доминирующего периода цикла с использованием компонент I и Q для динамической оценки длины цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление трендовой линии с использованием взвешенного скользящего среднего сглаженных значений за доминирующий период цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сохранение вычисленных значений трендовой линии в выходном массиве для визуализации или дальнейшего анализа.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>Трендовая линия</i> предоставляет сглаженное представление рыночного тренда, фильтруя краткосрочную волатильность.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Функция наиболее эффективна в трендовых рынках и может быть менее эффективна в высоко цикличных или волатильных условиях.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Алгоритм предполагает наличие доминирующего цикла, поэтому рынки, лишенные циклического поведения, могут привести к вводящим в заблуждение трендам.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtTrendline<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtTrendlineImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для <see cref="HtTrendline{T}">HtTrendline</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых перед первым вычисленным значением.</returns>
    /// <remarks>
    /// Пропускается 31 входных данных для совместимости с Tradestation.
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32".
    /// </remarks>
    [PublicAPI]
    public static int HtTrendlineLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtTrendline) + 31 + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtTrendline<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtTrendlineImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode HtTrendlineImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var lookbackTotal = HtTrendlineLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        const int smoothPriceSize = 50;
        Span<T> smoothPrice = new T[smoothPriceSize];

        T iTrend2, iTrend1;
        var iTrend3 = iTrend2 = iTrend1 = T.Zero;

        var outBegIdx = startIdx;

        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 34, out var today);

        var hilbertIdx = 0;
        var smoothPriceIdx = 0;

        /* Инициализация циклического буфера, используемого логикой трансформации Гильберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения расчетов.
         * Использование статического циклического буфера позволяет избежать больших динамических выделений памяти для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        var outIdx = 0;

        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod;
        var period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Сохранение сглаженного значения в циклический буфер smoothPrice.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировка периода для следующего бара цен
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            var trendLineValue = ComputeTrendLine(inReal, ref today, smoothPeriod, ref iTrend1, ref iTrend2, ref iTrend3);

            if (today >= startIdx)
            {
                outReal[outIdx++] = trendLineValue;
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

    private static T ComputeTrendLine<T>(
        ReadOnlySpan<T> real,
        ref int today,
        T smoothPeriod,
        ref T iTrend1,
        ref T iTrend2,
        ref T iTrend3) where T : IFloatingPointIeee754<T>
    {
        var idx = today;
        var tempReal = T.Zero;
        var dcPeriod = Int32.CreateTruncating(smoothPeriod + T.CreateChecked(0.5));
        for (var i = 0; i < dcPeriod; i++)
        {
            tempReal += real[idx--];
        }

        if (dcPeriod > 0)
        {
            tempReal /= T.CreateChecked(dcPeriod);
        }

        var trendLine =
            (FunctionHelpers.Four<T>() * tempReal + FunctionHelpers.Three<T>() * iTrend1 + FunctionHelpers.Two<T>() * iTrend2 + iTrend3) /
            T.CreateChecked(10);

        iTrend3 = iTrend2;
        iTrend2 = iTrend1;
        iTrend1 = tempReal;

        return trendLine;
    }
}
