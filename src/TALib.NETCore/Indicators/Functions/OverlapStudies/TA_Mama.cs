//Название файла: TA_Mama.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//AdaptiveIndicators (альтернатива, если требуется группировка по типу индикатора)
//TrendFollowing (альтернатива для акцента на следовании тренду)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MESA Adaptive Moving Average (Overlap Studies) — Адаптивное скользящее среднее MESA (Индикаторы наложения)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMAMA">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора MAMA.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMAMA[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outFAMA">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора FAMA.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outFAMA[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMAMA"/> и <paramref name="outFAMA"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMAMA"/> и <paramref name="outFAMA"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastLimit">
    /// Верхняя граница для адаптивного фактора:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения увеличивают чувствительность к изменениям цен, делая MAMA более восприимчивым к рыночным трендам.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения снижают чувствительность, сглаживая выходные данные MAMA.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Допустимый диапазон: <c>0.01..0.99</c>. Значения ближе к 0.99 максимально повышают чувствительность, тогда как значения ближе к 0.01 приоритизируют сглаживание.
    /// </para>
    /// </param>
    /// <param name="optInSlowLimit">
    /// Нижняя граница для адаптивного фактора:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения снижают минимальную чувствительность, добавляя стабильности MAMA во время консолидации рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения увеличивают минимальную чувствительность, позволяя быстрее реагировать на изменения рынка.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Допустимый диапазон: <c>0.01..0.99</c>. Значения ближе к 0.99 снижают шум, тогда как значения ближе к 0.01 позволяют большую гибкость.
    /// </para>
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
    /// Адаптивное скользящее среднее MESA динамически настраивает свою чувствительность на основе доминирующего цикла на рынке.
    /// Оно использует комбинацию преобразования Хилберта и альфа-фактора для адаптации к изменяющимся рыночным условиям, производя
    /// два выхода: MAMA и FAMA (Следующее адаптивное скользящее среднее).
    /// <para>
    /// Адаптивность функции позволяет ей быстро реагировать на тренды, минимизируя ложные сигналы в фазах консолидации.
    /// Комбинирование с <see cref="Adx{T}">ADX</see>, <see cref="Rsi{T}">RSI</see>,
    /// или мерами волатильности, такими как <see cref="Atr{T}">ATR</see>, может уточнить разработку стратегии.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Применяется взвешенное скользящее среднее (WMA) для сглаживания входных цен и уменьшения шума.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выполняется преобразование Хилберта на сглаженных данных для извлечения согласованных (I) и квадратурных (Q) компонентов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисляется доминирующий период цикла на основе фазовых различий между последовательными согласованными и квадратурными значениями.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитывается альфа-фактор с использованием быстрых и медленных ограничений, который определяет уровень чувствительности:
    ///       <code>
    ///         Alpha = FastLimit / DeltaPhase
    ///       </code>
    ///       Корректировки проводятся для обеспечения того, чтобы альфа оставалась в пределах, определенных медленными и быстрыми ограничениями.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Обновляется MAMA с использованием текущей цены и альфа, и рассчитывается FAMA как сглаженная версия MAMA.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>MAMA</i> отслеживает доминирующий тренд с минимальной задержкой.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>FAMA</i> обеспечивает дополнительное сглаживание, выступая в роли сигнальной линии для идентификации изменений тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечения между MAMA и FAMA могут указывать на потенциальные сигналы покупки или продажи: бычье пересечение происходит, когда MAMA пересекает FAMA снизу вверх, а медвежье пересечение — когда MAMA пересекает FAMA сверху вниз.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Mama<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMAMA,
        Span<T> outFAMA,
        out Range outRange,
        double optInFastLimit = 0.5,
        double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
        MamaImpl(inReal, inRange, outMAMA, outFAMA, out outRange, optInFastLimit, optInSlowLimit);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Mama{T}">Mama</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    /// <remarks>
    /// Фиксированный период обратного просмотра составляет 32 и устанавливается следующим образом:
    /// <list type="bullet">
    /// <item><description>12 ценовых баров для совместимости с реализацией TradeStation, описанной в книге Джона Элерса.</description></item>
    /// <item><description>6 ценовых баров для <c>Detrender</c></description></item>
    /// <item><description>6 ценовых баров для <c>Q1</c></description></item>
    /// <item><description>3 ценовых бара для <c>JI</c></description></item>
    /// <item><description>3 ценовых бара для <c>JQ</c></description></item>
    /// <item><description>1 ценовой бар для <c>Re</c>/<c>Im</c></description></item>
    /// <item><description>1 ценовой бар для <c>Delta Phase</c></description></item>
    /// <item><description>————————</description></item>
    /// <item><description>32 всего</description></item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static int MamaLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Mama) + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Mama<T>(
        T[] inReal,
        Range inRange,
        T[] outMAMA,
        T[] outFAMA,
        out Range outRange,
        double optInFastLimit = 0.5,
        double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
        MamaImpl<T>(inReal, inRange, outMAMA, outFAMA, out outRange, optInFastLimit, optInSlowLimit);

    private static Core.RetCode MamaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMAMA,
        Span<T> outFAMA,
        out Range outRange,
        double optInFastLimit,
        double optInSlowLimit) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInFastLimit < 0.01 || optInFastLimit > 0.99 || optInSlowLimit < 0.01 || optInSlowLimit > 0.99)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = MamaLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx;

        // Инициализация взвешенного скользящего среднего (WMA)
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        var hilbertIdx = 0;

        /* Инициализация циркулярного буфера, используемого логикой преобразования Хилберта.
         * Буфер используется для нечетных и четных дней.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        var outIdx = 0;

        // Переменные для хранения промежуточных значений
        T prevI2, prevQ2, re, im, mama, fama, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, prevPhase;
        var period = prevI2 = prevQ2
            = re = im = mama = fama = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = prevPhase = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            var todayValue = inReal[today];
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, todayValue,
                out var smoothedValue);

            var tempReal2 = PerformMAMAHilbertTransform(today, circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod,
                ref i1ForOddPrev3, ref i1ForEvenPrev3, ref i1ForOddPrev2, ref i1ForEvenPrev2, prevQ2, prevI2, out var i2, out var q2);

            // Разница фаз помещается в tempReal
            var tempReal = prevPhase - tempReal2;
            prevPhase = tempReal2;
            if (tempReal < T.One)
            {
                tempReal = T.One;
            }

            // Альфа помещается в tempReal
            if (tempReal > T.One)
            {
                tempReal = T.CreateChecked(optInFastLimit) / tempReal;
                if (tempReal < T.CreateChecked(optInSlowLimit))
                {
                    tempReal = T.CreateChecked(optInSlowLimit);
                }
            }
            else
            {
                tempReal = T.CreateChecked(optInFastLimit);
            }

            // Расчет MAMA и FAMA
            mama = tempReal * todayValue + (T.One - tempReal) * mama;
            tempReal *= T.CreateChecked(0.5);
            fama = tempReal * mama + (T.One - tempReal) * fama;
            if (today >= startIdx)
            {
                outMAMA[outIdx] = mama;
                outFAMA[outIdx++] = fama;
            }

            // Корректировка периода для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            today++;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T PerformMAMAHilbertTransform<T>(
        int today,
        Span<T> circBuffer,
        T smoothedValue,
        ref int hilbertIdx,
        T adjustedPrevPeriod,
        ref T i1ForOddPrev3,
        ref T i1ForEvenPrev3,
        ref T i1ForOddPrev2,
        ref T i1ForEvenPrev2,
        T prevQ2,
        T prevI2,
        out T i2,
        out T q2) where T : IFloatingPointIeee754<T>
    {
        T tempReal2;
        if (today % 2 == 0)
        {
            FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                prevI2,
                out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);

            tempReal2 = !T.IsZero(i1ForEvenPrev3)
                ? T.RadiansToDegrees(T.Atan(circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1] / i1ForEvenPrev3))
                : T.Zero;
        }
        else
        {
            FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                prevI2,
                i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);

            tempReal2 = !T.IsZero(i1ForOddPrev3)
                ? T.RadiansToDegrees(T.Atan(circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1] / i1ForOddPrev3))
                : T.Zero;
        }

        return tempReal2;
    }
}
