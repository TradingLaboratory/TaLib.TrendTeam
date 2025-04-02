//Название файла: TA_Kama.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//AdaptiveIndicators (альтернатива для акцента на адаптивности индикатора)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Kaufman Adaptive Moving Average (Overlap Studies) — Адаптивное скользящее среднее Кауфмана (Перекрывающиеся исследования)
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
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Адаптивное скользящее среднее Кауфмана (KAMA) разработано для адаптации к волатильности рынка.
    /// Оно корректирует свой сглаживающий коэффициент на основе коэффициента эффективности, который рассчитывается как отношение направления цены
    /// к волатильности цены за указанный период. Это позволяет KAMA быть более чувствительным во время трендов и менее чувствительным во время консолидаций.
    /// <para>
    /// Функция может уменьшить шум и ложные сигналы. Благодаря своей адаптивной природе, KAMA может уменьшить запаздывание по сравнению с традиционными скользящими средними,
    /// что делает его полезным для выявления трендов и избегания ложных сигналов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет коэффициента эффективности (ER):
    ///       <code>
    ///         ER = Abs(PriceChange) / Sum(Abs(PriceChange over TimePeriod))
    ///       </code>
    ///       где PriceChange — разница между текущей ценой и ценой `TimePeriod` назад.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет сглаживающей константы (SC):
    ///       <code>
    ///         SC = [ER * (FastestSC - SlowestSC) + SlowestSC]^2
    ///       </code>
    ///       где FastestSC и SlowestSC — константы, обычно выводимые из короткого и длинного сглаживающих периодов соответственно.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение SC для расчета KAMA:
    ///       <code>
    ///         KAMA = PreviousKAMA + SC * (Price - PreviousKAMA)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост KAMA указывает на восходящий тренд, особенно когда он быстро реагирует на увеличение цен.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Плоский или снижающийся KAMA указывает на консолидацию или нисходящий тренд.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Kama<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        KamaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Kama{T}">Kama</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    [PublicAPI]
    public static int KamaLookback(int optInTimePeriod = 30) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Kama);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Kama<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        KamaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode KamaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = KamaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var sumROC1 = T.Zero;
        var today = startIdx - lookbackTotal;
        var trailingIdx = today;
        InitSumROC(inReal, ref sumROC1, ref today, optInTimePeriod);

        // На этом этапе sumROC1 представляет суммирование однодневной разницы цен за (optInTimePeriod - 1)

        // Расчет первого KAMA

        // Цена вчера используется здесь как предыдущее значение KAMA.
        var prevKAMA = inReal[today - 1];
        var tempReal = inReal[trailingIdx++];
        var periodROC = inReal[today] - tempReal;

        // Сохранение отстающего значения. Делается это потому, что входные и выходные данные могут указывать на один и тот же буфер.
        var trailingValue = tempReal;

        var efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
        var smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

        // Расчет KAMA как EMA, используя сглаживающую константу как адаптивный фактор.
        prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);

        // 'today' отслеживает текущую позицию обработки во входных данных.
        while (today <= startIdx)
        {
            UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);
            periodROC = inReal[today] - inReal[trailingIdx - 1];
            efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
            smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

            // Расчет KAMA как EMA, используя сглаживающую константу как адаптивный фактор.
            prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);
        }

        // Запись первого значения.
        outReal[0] = prevKAMA;
        var outIdx = 1;
        var outBegIdx = today - 1;

        // Пропуск нестабильного периода. Выполняется вся необходимая обработка для KAMA, но не записывается в выходные данные.
        while (today <= endIdx)
        {
            UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);
            periodROC = inReal[today] - inReal[trailingIdx - 1];
            efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
            smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

            // Расчет KAMA как EMA, используя сглаживающую константу как адаптивный фактор.
            prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);

            outReal[outIdx++] = prevKAMA;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void InitSumROC<T>(
        ReadOnlySpan<T> inReal,
        ref T sumROC1,
        ref int today,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        for (var i = optInTimePeriod; i > 0; i--)
        {
            var tempReal = inReal[today++];
            tempReal -= inReal[today];
            sumROC1 += T.Abs(tempReal);
        }
    }

    private static T CalcEfficiencyRatio<T>(T sumROC1, T periodROC) where T : IFloatingPointIeee754<T> =>
        sumROC1 <= periodROC || T.IsZero(sumROC1) ? T.One : T.Abs(periodROC / sumROC1);

    private static T CalcSmoothingConstant<T>(T efficiencyRatio) where T : IFloatingPointIeee754<T>
    {
        var constMax = FunctionHelpers.Two<T>() / (T.CreateChecked(30) + T.One);
        var constDiff = FunctionHelpers.Two<T>() / (FunctionHelpers.Two<T>() + T.One) - constMax;
        var tempReal = efficiencyRatio * constDiff + constMax;

        return tempReal * tempReal;
    }

    private static T CalcKAMA<T>(T todayValue, T prevKAMA, T smoothingConstant) where T : IFloatingPointIeee754<T> =>
        (todayValue - prevKAMA) * smoothingConstant + prevKAMA;

    private static void UpdateSumROC<T>(
        ReadOnlySpan<T> inReal,
        ref T sumROC1,
        ref int today,
        ref int trailingIdx,
        ref T trailingValue) where T : IFloatingPointIeee754<T>
    {
        var tempReal = inReal[today];
        var tempReal2 = inReal[trailingIdx++];

        /* Корректировка sumROC1:
         *  - Удаление отстающего ROC1
         *  - Добавление нового ROC1
         */
        sumROC1 -= T.Abs(trailingValue - tempReal2);
        sumROC1 += T.Abs(tempReal - inReal[today - 1]);

        // Сохранение отстающего значения. Делается это потому, что входные и выходные данные могут указывать на один и тот же буфер.
        trailingValue = tempReal2;
    }
}
