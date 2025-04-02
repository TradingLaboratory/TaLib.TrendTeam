//Название файла: TA_HtSine.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу индикатора)
//HilbertTransform (альтернатива для акцента на преобразовании Хилберта)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - SineWave (Cycle Indicators) — Преобразование Хилберта - Синусоида (Индикаторы циклов)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outSine">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outSine[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outLeadSine">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outLeadSine[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outSine"/> и <paramref name="outLeadSine"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outSine"/> и <paramref name="outLeadSine"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Преобразование Хилберта - Синусоида идентифицирует и визуализирует циклическое поведение временных рядов данных, вычисляя
    /// синусоидальные и ведущие синусоидальные компоненты доминирующего цикла. Эти компоненты особенно полезны для анализа рыночных трендов
    /// и выявления потенциальных разворотов.
    /// <para>
    /// Эта функция может помочь в определении времени входа или выхода в циклических условиях. Подтверждение выявленных точек разворота с помощью трендового анализа,
    /// осцилляторов, таких как <see cref="Rsi{T}">RSI</see>, или инструментов на основе циклов, таких как <see cref="HtDcPeriod{T}">HT DC Period</see>, может повысить надежность.
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
    ///       Применение преобразования Хилберта для извлечения в фазе (I) и квадратурных (Q) компонентов для четных и нечетных баров, захватывающих свойства цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление доминирующей циклической фазы (DCPhase) с использованием компонентов I и Q. Это дает текущую позицию в цикле.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление синуса DCPhase (outSine) и ведущего синуса (outLeadSine), который является фазой, сдвинутой на 45 градусов.
    ///       Эти значения дают представление о циклических движениях и потенциальных точках разворота.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>Синусоида</i> представляет текущую фазу доминирующего цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>Ведущая синусоида</i> — это синусоида, сдвинутая вперед на 45 градусов, что помогает выявить ранние фазовые переходы.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Когда синусоида и ведущая синусоида пересекаются, это может указывать на потенциальный пик или впадину цикла,
    ///       сигнализируя о возможных разворотах рынка.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Функция наиболее эффективна в циклических рынках и может давать ненадежные сигналы в трендовых или высоковолатильных рынках.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Она чувствительна к шуму во входных данных; поэтому правильное сглаживание критически важно для точных результатов.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtSine<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outSine,
        Span<T> outLeadSine,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtSineImpl(inReal, inRange, outSine, outLeadSine, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="HtSine{T}">HtSine</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    /// <remarks>
    /// Пропускается 31 входных данных для совместимости с Tradestation.
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения "32"
    /// </remarks>
    [PublicAPI]
    public static int HtSineLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtSine) + 31 + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtSine<T>(
        T[] inReal,
        Range inRange,
        T[] outSine,
        T[] outLeadSine,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtSineImpl<T>(inReal, inRange, outSine, outLeadSine, out outRange);

    private static Core.RetCode HtSineImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outSine,
        Span<T> outLeadSine,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var lookbackTotal = HtSineLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        const int smoothPriceSize = 50;
        Span<T> smoothPrice = new T[smoothPriceSize];

        var outBegIdx = startIdx;

        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 34, out var today);

        var hilbertIdx = 0;
        var smoothPriceIdx = 0;

        /* Инициализация циклического буфера, используемого логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения вычислений.
         * Использование статического циклического буфера позволяет избежать большой динамической аллокации памяти для хранения промежуточных вычислений.
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

            // Запоминаем сглаженное значение в циклический буфер smoothPrice.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировка периода для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            dcPhase = ComputeDcPhase(smoothPrice, smoothPeriod, smoothPriceIdx, dcPhase);

            if (today >= startIdx)
            {
                outSine[outIdx] = T.Sin(T.DegreesToRadians(dcPhase));
                outLeadSine[outIdx++] = T.Sin(T.DegreesToRadians(dcPhase + T.CreateChecked(45)));
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
}
