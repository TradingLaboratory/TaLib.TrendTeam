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
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены Close, другие индикаторы или другие временные ряды).
    /// Обычно используются цены закрытия (Close) для анализа рыночного режима.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Позволяет ограничить вычисления конкретной частью входных данных.
    /// </para>
    /// </param>
    /// <param name="outInteger">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outInteger[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Значения: 1 = Трендовый режим (Trend Mode), 0 = Циклический режим (Cycle Mode).
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Преобразование Хилберта - Режим тренда против цикла — это индикатор цикла, который определяет, 
    /// находится ли рынок в трендовом состоянии или в циклическом.
    /// <para>
    /// Это достигается путем анализа доминирующих циклов в данных и сравнения их свойств 
    /// с заранее определенными порогами. Индикатор использует преобразование Хилберта для выделения 
    /// синфазной (I) и квадратурной (Q) компонент сигнала.
    /// </para>
    /// <para>
    /// Функция может помочь выбрать подходящие индикаторы для текущего режима рынка.
    /// В трендовом режиме могут быть предпочтительны инструменты следования за трендом; 
    /// в циклическом режиме осцилляторы могут давать лучшие результаты.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сгладить входные цены с помощью взвешенного скользящего среднего (WMA), 
    ///       чтобы уменьшить шум и выделить ключевые тренды.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применить преобразование Хилберта для извлечения синфазной (I) и квадратурной (Q) компонент, 
    ///       которые используются для вычисления свойств доминирующего цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить фазу доминирующего цикла (DC Phase) и сгладить фазу по последовательным итерациям.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сгенерировать значения синуса (Sine) и ведущего синуса (Lead Sine) с использованием 
    ///       сглаженной фазы DC для обнаружения циклического режима.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Проанализировать пересечения и отклонения компонент синусоидальной волны, 
    ///       чтобы определить, находится ли ряд в трендовом или циклическом режиме.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>Трендовый режим (1)</i> указывает на устойчивое направленное движение рынка,
    ///       при котором циклы играют второстепенную роль. Рекомендуется использовать трендовые индикаторы.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>Циклический режим (0)</i> указывает на колебания рынка в пределах четко определенного диапазона,
    ///       при котором можно наблюдать доминирующие циклы. Рекомендуется использовать осцилляторы.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Эта функция менее эффективна на шумных или волатильных рынках, 
    ///       где трудно обнаружить доминирующие циклы.
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
    /// Возвращает период обратного просмотра (Lookback Period) для <see cref="HtTrendMode{T}">HtTrendMode</see>.
    /// </summary>
    /// <returns>
    /// Количество периодов, необходимых до первого вычисленного валидного значения индикатора.
    /// Все бары с индексом меньше этого значения будут пропущены при расчете.
    /// </returns>
    /// <remarks>
    /// Пропускается 31 входных данных для совместимости с Tradestation.
    /// <para>
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32".
    /// Формула: UnstablePeriod + 31 + 32
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int HtTrendModeLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtTrendMode) + 31 + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API библиотеки TALib.
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
        // Инициализируем outRange как пустой диапазон (начало = 0, конец = 0)
        outRange = Range.EndAt(0);

        // Проверяем корректность входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлекаем начальный и конечный индексы для обработки данных
        var (startIdx, endIdx) = rangeIndices;

        // Размер циклического буфера для сглаженных цен (50 периодов)
        const int smoothPriceSize = 50;
        // Буфер для хранения сглаженных значений цен
        Span<T> smoothPrice = new T[smoothPriceSize];

        // Переменные для хранения предыдущих значений тренда (для вычисления трендовой линии)
        var iTrend3 = T.Zero;      // Значение тренда 3 периода назад
        var iTrend2 = iTrend3;     // Значение тренда 2 периода назад
        var iTrend1 = iTrend2;     // Значение тренда 1 период назад
        // Счетчик дней в текущем трендовом режиме
        var daysInTrend = 0;
        // Значения синусоидальных компонент преобразования Хилберта
        var sine = T.Zero;         // Текущее значение Sine
        var leadSine = T.Zero;     // Текущее значение Lead Sine (опережающий синус)

        // Вычисляем общий период обратного просмотра (lookback) для индикатора
        var lookbackTotal = HtTrendModeLookback();
        // Корректируем начальный индекс с учетом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Сохраняем начальный индекс для вывода (первый валидный результат)
        var outBegIdx = startIdx;

        // Инициализируем вспомогательные переменные для взвешенного скользящего среднего (WMA)
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub,
            out var periodWMASum, out var trailingWMAValue, out var trailingWMAIdx, 34, out var today);

        // Индекс для циклического буфера преобразования Хилберта
        var hilbertIdx = 0;
        // Индекс для буфера сглаженных цен
        var smoothPriceIdx = 0;

        /* Инициализировать циклический буфер, используемый логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Переменные для вычислений преобразования Хилберта
        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
        // Инициализируем все переменные нулевыми значениями
        var period = prevI2 = prevQ2 =
            re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        // Основной цикл обработки всех баров от startIdx до endIdx
        while (today <= endIdx)
        {
            // Вычисляем скорректированный предыдущий период для сглаживания
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Вычисляем сглаженное значение цены с помощью WMA
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum,
                ref trailingWMAValue, inReal[today], out var smoothedValue);

            // Запомнить smoothedValue в циклическом буфере smoothPrice.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            // Выполняем преобразование Хилберта для получения I и Q компонент
            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2,
                ref hilbertIdx, ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2,
                out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировать период для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            // Сглаживаем вычисленный период (экспоненциальное сглаживание с коэффициентом 0.33)
            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            // Сохраняем предыдущее значение фазы доминирующего цикла
            var prevDCPhase = dcPhase;

            /* Вычислить фазу доминирующего цикла (DC Phase) */
            dcPhase = ComputeDcPhase(smoothPrice, smoothPeriod, smoothPriceIdx, dcPhase);

            // Сохраняем предыдущие значения синусоидальных компонент
            var prevSine = sine;
            var prevLeadSine = leadSine;

            // Вычисляем текущие значения Sine и Lead Sine из фазы DC
            sine = T.Sin(T.DegreesToRadians(dcPhase));
            leadSine = T.Sin(T.DegreesToRadians(dcPhase + FunctionHelpers.Ninety<T>() / FunctionHelpers.Two<T>()));

            // idx используется для итерации по последним 50 значениям smoothPrice.
            // Вычисляем значение трендовой линии
            var trendLineValue = ComputeTrendLine(inReal, ref today, smoothPeriod,
                ref iTrend1, ref iTrend2, ref iTrend3);

            // Определяем текущий режим рынка (тренд или цикл)
            var trend = DetermineTrend(sine, leadSine, prevSine, prevLeadSine, smoothPeriod, dcPhase,
                prevDCPhase, smoothPrice, smoothPriceIdx, trendLineValue, ref daysInTrend);

            // Записываем результат в выходной массив, если достигли startIdx
            if (today >= startIdx)
            {
                outInteger[outIdx++] = trend;
            }

            // Инкрементируем индекс буфера сглаженных цен с циклическим переходом
            if (++smoothPriceIdx > smoothPriceSize - 1)
            {
                smoothPriceIdx = 0;
            }

            // Переходим к следующему бару
            today++;
        }

        // Устанавливаем диапазон валидных выходных данных
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Определяет текущий режим рынка на основе анализа синусоидальных компонент и трендовой линии.
    /// </summary>
    /// <param name="sine">Текущее значение Sine компоненты преобразования Хилберта.</param>
    /// <param name="leadSine">Текущее значение Lead Sine (опережающей) компоненты.</param>
    /// <param name="prevSine">Предыдущее значение Sine компоненты.</param>
    /// <param name="prevLeadSine">Предыдущее значение Lead Sine компоненты.</param>
    /// <param name="smoothPeriod">Сглаженное значение периода доминирующего цикла.</param>
    /// <param name="dcPhase">Текущая фаза доминирующего цикла (DC Phase).</param>
    /// <param name="prevDCPhase">Предыдущая фаза доминирующего цикла.</param>
    /// <param name="smoothPrice">Буфер сглаженных цен (размер 50).</param>
    /// <param name="smoothPriceIdx">Текущий индекс в буфере сглаженных цен.</param>
    /// <param name="trendLineValue">Вычисленное значение трендовой линии.</param>
    /// <param name="daysInTrend">Счетчик дней в текущем трендовом режиме (передаётся по ссылке).</param>
    /// <typeparam name="T">Числовой тип данных, реализующий <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <returns>
    /// Режим рынка: 1 = Трендовый режим (Trend Mode), 0 = Циклический режим (Cycle Mode).
    /// </returns>
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
        // Вычислить режим тренда и предположить тренд по умолчанию (1 = Trend Mode)
        var trend = 1;

        // Измерить дни в тренде с последнего пересечения линий индикатора SineWave
        // Если Sine пересекает Lead Sine - сбрасываем счетчик и устанавливаем циклический режим
        if (sine > leadSine && prevSine <= prevLeadSine || sine < leadSine && prevSine >= prevLeadSine)
        {
            daysInTrend = 0;
            trend = 0;
        }

        // Если дней в тренде меньше половины сглаженного периода - считаем режим циклическим
        if (T.CreateChecked(++daysInTrend) < T.CreateChecked(0.5) * smoothPeriod)
        {
            trend = 0;
        }

        // Проверяем изменение фазы DC для определения циклического режима
        var tempReal = dcPhase - prevDCPhase;
        if (!T.IsZero(smoothPeriod) &&
            tempReal > T.CreateChecked(0.67) * FunctionHelpers.Ninety<T>() * FunctionHelpers.Four<T>() / smoothPeriod &&
            tempReal < T.CreateChecked(1.5) * FunctionHelpers.Ninety<T>() * FunctionHelpers.Four<T>() / smoothPeriod)
        {
            trend = 0;
        }

        // Проверяем отклонение цены от трендовой линии
        // Если отклонение больше 1.5% - устанавливаем трендовый режим
        tempReal = smoothPrice[smoothPriceIdx];
        if (!T.IsZero(trendLineValue) && T.Abs((tempReal - trendLineValue) / trendLineValue) >= T.CreateChecked(0.015))
        {
            trend = 1;
        }

        return trend;
    }
}
