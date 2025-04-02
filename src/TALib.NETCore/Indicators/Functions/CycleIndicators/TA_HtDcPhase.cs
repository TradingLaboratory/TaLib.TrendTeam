//Название файла: TA_HtDcPhase.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу индикатора)
//PhaseAnalysis (альтернатива для акцента на анализе фаз)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Dominant Cycle Phase (Cycle Indicators) — Преобразование Гильберта - Фаза доминирующего цикла (Индикаторы циклов)
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Преобразование Гильберта - Фаза доминирующего цикла определяет фазу доминирующего ценового цикла,
    /// определяя текущую позицию внутри цикла.
    /// Этот угол фазы дает представление о позиции внутри цикла, что может быть использовано для анализа времени и тренда.
    /// <para>
    /// Функция полезна для идентификации циклов и их фаз в финансовых данных.
    /// Она помогает выявлять перекупленность и перепроданность, а также потенциальные развороты цен.
    /// Функция может улучшить тайминг при использовании вместе с <see cref="HtDcPeriod{T}">HT DC Period</see> или другими инструментами циклов.
    /// Добавление трендовых и импульсных мер может уточнить решения.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сглаживание входных цен с помощью взвешенного скользящего среднего (WMA) для удаления шума и стабилизации данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Использование преобразования Гильберта для вычисления согласованных (I) и квадратурных (Q) компонентов для четных и нечетных ценовых баров.
    ///       Эти компоненты служат основой для расчета фазы.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление действительной и мнимой частей фазы доминирующего цикла с помощью тригонометрических операций над сглаженными ценами.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вывод угла фазы из действительной и мнимой частей. Корректировка угла фазы для малых мнимых компонентов
    ///       и учет однобаровых задержек, введенных WMA.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выполнение окончательных корректировок фазы для обеспечения результата в ожидаемом диапазоне.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение указывает текущую позицию внутри рыночного цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рост значения может указывать на начало бычьего тренда, в то время как падение фазы может сигнализировать о медвежьих трендах.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Падение значения может сигнализировать о медвежьих трендах.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Функция более эффективна в циклических или боковых рынках и может давать ненадежные результаты в условиях сильного тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выход чувствителен к шумным данным; методы сглаживания, такие как WMA, помогают смягчить это.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtDcPhase<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtDcPhaseImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="HtDcPhase{T}">HtDcPhase</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    /// <remarks>
    /// 31 входных значений пропускается для совместимости с Tradestation.
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения "32"
    /// </remarks>
    [PublicAPI]
    public static int HtDcPhaseLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtDcPhase) + 31 + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtDcPhase<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtDcPhaseImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode HtDcPhaseImpl<T>(
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

        var lookbackTotal = HtDcPhaseLookback();
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

        /* Инициализация кольцевого буфера, используемого логикой преобразования Гильберта.
         * Один буфер используется для нечетных дней, другой - для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения промежуточных вычислений.
         * Использование статического кольцевого буфера позволяет избежать больших динамических выделений памяти для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        var outIdx = 0;

        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
        var period = prevI2 = prevQ2 =
            re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = T.Zero;

        // Код оптимизирован по скорости и, вероятно, будет сложен для понимания, если вы не знакомы с оригинальным алгоритмом.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Запоминание сглаженного значения в кольцевой буфер smoothPrice.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировка периода для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            dcPhase = ComputeDcPhase(smoothPrice, smoothPeriod, smoothPriceIdx, dcPhase);

            if (today >= startIdx)
            {
                outReal[outIdx++] = dcPhase;
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

    private static T ComputeDcPhase<T>(
        Span<T> smoothPrice,
        T smoothPeriod,
        int smoothPriceIdx,
        T dcPhase) where T : IFloatingPointIeee754<T>
    {
        var dcPeriod = smoothPeriod + T.CreateChecked(0.5);
        var dcPeriodInt = Int32.CreateTruncating(dcPeriod);
        var realPart = T.Zero;
        var imagPart = T.Zero;

        var idx = smoothPriceIdx;
        for (var i = 0; i < dcPeriodInt; i++)
        {
            var tempReal = T.CreateChecked(i) * FunctionHelpers.Two<T>() * T.Pi / T.CreateChecked(dcPeriodInt);
            var tempReal2 = smoothPrice[idx];
            realPart += T.Sin(tempReal) * tempReal2;
            imagPart += T.Cos(tempReal) * tempReal2;
            idx = idx == 0 ? smoothPrice.Length - 1 : idx - 1;
        }

        dcPhase = CalcDcPhase(realPart, imagPart, dcPhase, smoothPeriod);

        return dcPhase;
    }

    private static T CalcDcPhase<T>(
        T realPart,
        T imagPart,
        T dcPhase,
        T smoothPeriod) where T : IFloatingPointIeee754<T>
    {
        var tempReal = T.Abs(imagPart);
        T dcPhaseValue = T.Zero;
        if (tempReal > T.Zero)
        {
            dcPhaseValue = T.RadiansToDegrees(T.Atan(realPart / imagPart));
        }
        else if (tempReal <= T.CreateChecked(0.01))
        {
            dcPhaseValue = AdjustPhaseForSmallImaginaryPart(realPart, dcPhase);
        }

        dcPhase = FinalPhaseAdjustments(imagPart, dcPhaseValue, smoothPeriod);

        return dcPhase;
    }

    private static T AdjustPhaseForSmallImaginaryPart<T>(T realPart, T dcPhase) where T : IFloatingPointIeee754<T>
    {
        if (realPart < T.Zero)
        {
            dcPhase -= FunctionHelpers.Ninety<T>();
        }
        else if (realPart > T.Zero)
        {
            dcPhase += FunctionHelpers.Ninety<T>();
        }

        return dcPhase;
    }

    private static T FinalPhaseAdjustments<T>(T imagPart, T dcPhase, T smoothPeriod) where T : IFloatingPointIeee754<T>
    {
        dcPhase += FunctionHelpers.Ninety<T>();
        // Компенсация однобаровой задержки скользящего среднего
        dcPhase += FunctionHelpers.Ninety<T>() * FunctionHelpers.Four<T>() / smoothPeriod;

        if (imagPart < T.Zero)
        {
            dcPhase += FunctionHelpers.Ninety<T>() * FunctionHelpers.Two<T>();
        }

        if (dcPhase > FunctionHelpers.Ninety<T>() * T.CreateChecked(3.5))
        {
            dcPhase -= FunctionHelpers.Ninety<T>() * FunctionHelpers.Four<T>();
        }

        return dcPhase;
    }
}
