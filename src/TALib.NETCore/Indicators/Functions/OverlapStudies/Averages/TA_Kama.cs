//Название файла: TA_Kama.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//MomentumIndicators (альтернатива, если требуется группировка по типу индикатора)
//AdaptiveIndicators (альтернатива для акцента на адаптивности индикатора)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Kaufman Adaptive Moving Average (Overlap Studies) — Адаптивное скользящее среднее Кауфмана (Перекрывающиеся исследования)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены Close, другие индикаторы или другие временные ряды).
    /// Обычно используются цены закрытия (Close) для расчета KAMA.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет subset данных для вычисления индикатора.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Содержит рассчитанные значения KAMA для валидного диапазона.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <param name="optInTimePeriod">
    /// Период времени для расчета коэффициента эффективности (Time Period).
    /// <para>
    /// - Рекомендуемое значение по умолчанию: 30.
    /// - Минимальное допустимое значение: 2.
    /// - Определяет количество баров для расчета изменения цены и волатильности.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Адаптивное скользящее среднее Кауфмана (KAMA) разработано для адаптации к волатильности рынка.
    /// <para>
    /// Оно корректирует свой сглаживающий коэффициент на основе коэффициента эффективности (Efficiency Ratio - ER),
    /// который рассчитывается как отношение направления цены к волатильности цены за указанный период.
    /// Это позволяет KAMA быть более чувствительным во время трендов и менее чувствительным во время консолидаций.
    /// </para>
    /// <para>
    /// Функция может уменьшить шум и ложные сигналы. Благодаря своей адаптивной природе, KAMA может уменьшить запаздывание
    /// по сравнению с традиционными скользящими средними, что делает его полезным для выявления трендов и избегания ложных сигналов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет коэффициента эффективности (Efficiency Ratio - ER):
    ///       <code>
    ///         ER = Abs(PriceChange) / Sum(Abs(PriceChange over TimePeriod))
    ///       </code>
    ///       где PriceChange — разница между текущей ценой (Close) и ценой `TimePeriod` назад.
    ///       <para>
    ///       - ER близок к 1 при сильном тренде (цена движется в одном направлении).
    ///       - ER близок к 0 при боковом движении (высокая волатильность без направления).
    ///       </para>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет сглаживающей константы (Smoothing Constant - SC):
    ///       <code>
    ///         SC = [ER * (FastestSC - SlowestSC) + SlowestSC]^2
    ///       </code>
    ///       где FastestSC и SlowestSC — константы, обычно выводимые из короткого и длинного сглаживающих периодов соответственно.
    ///       <para>
    ///       - FastestSC = 2/(2+1) ≈ 0.6667 (быстрое сглаживание).
    ///       - SlowestSC = 2/(30+1) ≈ 0.0645 (медленное сглаживание).
    ///       </para>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение SC для расчета KAMA:
    ///       <code>
    ///         KAMA = PreviousKAMA + SC * (Price - PreviousKAMA)
    ///       </code>
    ///       <para>
    ///       - Формула аналогична EMA, но с адаптивным коэффициентом сглаживания.
    ///       - PreviousKAMA — предыдущее значение KAMA (для первого бара используется цена).
    ///       </para>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост KAMA указывает на восходящий тренд, особенно когда он быстро реагирует на увеличение цен (Close).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Плоский или снижающийся KAMA указывает на консолидацию или нисходящий тренд.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       KAMA выше цены (Close) может сигнализировать о нисходящем тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       KAMA ниже цены (Close) может сигнализировать о восходящем тренде.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Преимущества KAMA</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Адаптивность: автоматически подстраивается под рыночные условия.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Снижение ложных сигналов: меньше реагирует на боковое движение рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Уменьшенное запаздывание: быстрее реагирует на начало тренда по сравнению с SMA.
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
    /// Возвращает период обратного просмотра (Lookback Period) для <see cref="Kama{T}">Kama</see>.
    /// </summary>
    /// <param name="optInTimePeriod">
    /// Период времени (Time Period) для расчета индикатора.
    /// <para>
    /// - Минимальное значение: 2.
    /// - Значение по умолчанию: 30.
    /// </para>
    /// </param>
    /// <returns>
    /// Количество периодов (баров), необходимых до расчета первого валидного выходного значения.
    /// <para>
    /// - Возвращает -1 если optInTimePeriod &lt; 2 (некорректный параметр).
    /// - Включает нестабильный период из настроек Core.UnstablePeriodSettings.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно получить валидное значение KAMA.
    /// Все бары с индексом меньше lookback будут пропущены при расчете.
    /// </remarks>
    [PublicAPI]
    public static int KamaLookback(int optInTimePeriod = 30) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Kama);

    /// <remarks>
    /// Для совместимости с абстрактным API TALib.
    /// <para>
    /// Этот метод обеспечивает совместимость с массивами вместо Span&lt;T&gt;.
    /// </para>
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
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального периода (должен быть >= 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет общего lookback периода для определения первого валидного бара
        var lookbackTotal = KamaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если startIdx > endIdx, нет данных для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // sumROC1: сумма абсолютных изменений цены за период (волатильность)
        var sumROC1 = T.Zero;
        // today: текущий индекс обрабатываемого бара во входных данных
        var today = startIdx - lookbackTotal;
        // trailingIdx: индекс отстающего бара для скользящего окна
        var trailingIdx = today;
        // Инициализация суммы ROC (Rate of Change) за период
        InitSumROC(inReal, ref sumROC1, ref today, optInTimePeriod);

        // На этом этапе sumROC1 представляет суммирование однодневной разницы цен за (optInTimePeriod - 1) периодов

        // Расчет первого значения KAMA

        // Цена вчера используется здесь как предыдущее значение KAMA (PreviousKAMA).
        // Это начальное значение для рекурсивной формулы KAMA
        var prevKAMA = inReal[today - 1];
        var tempReal = inReal[trailingIdx++];
        // periodROC: изменение цены за период (PriceChange)
        var periodROC = inReal[today] - tempReal;

        // Сохранение отстающего значения (trailingValue).
        // Делается это потому, что входные и выходные данные могут указывать на один и тот же буфер памяти.
        var trailingValue = tempReal;

        // Расчет коэффициента эффективности (Efficiency Ratio)
        var efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
        // Расчет сглаживающей константы (Smoothing Constant)
        var smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

        // Расчет KAMA как EMA, используя сглаживающую константу как адаптивный фактор.
        // Формула: KAMA = PreviousKAMA + SC * (Price - PreviousKAMA)
        prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);

        // 'today' отслеживает текущую позицию обработки во входных данных.
        // Этот цикл обрабатывает бары до startIdx (нестабильный период)
        while (today <= startIdx)
        {
            // Обновление суммы ROC с учетом нового и отстающего значения
            UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);
            // Расчет изменения цены за период
            periodROC = inReal[today] - inReal[trailingIdx - 1];
            // Пересчет коэффициента эффективности
            efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
            // Пересчет сглаживающей константы
            smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

            // Расчет KAMA как EMA, используя сглаживающую константу как адаптивный фактор.
            prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);
        }

        // Запись первого валидного значения KAMA в выходной массив.
        outReal[0] = prevKAMA;
        var outIdx = 1;
        // outBegIdx: индекс первого бара с валидным значением KAMA во входных данных
        var outBegIdx = today - 1;

        // Пропуск нестабильного периода. Выполняется вся необходимая обработка для KAMA, но не записывается в выходные данные.
        // Этот цикл обрабатывает бары от startIdx до endIdx (валидный диапазон)
        while (today <= endIdx)
        {
            // Обновление суммы ROC с учетом нового и отстающего значения
            UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);
            // Расчет изменения цены за период
            periodROC = inReal[today] - inReal[trailingIdx - 1];
            // Пересчет коэффициента эффективности
            efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);
            // Пересчет сглаживающей константы
            smoothingConstant = CalcSmoothingConstant(efficiencyRatio);

            // Расчет KAMA как EMA, используя сглаживающую константу как адаптивный фактор.
            prevKAMA = CalcKAMA(inReal[today++], prevKAMA, smoothingConstant);

            // Запись рассчитанного значения KAMA в выходной массив
            outReal[outIdx++] = prevKAMA;
        }

        // Установка выходного диапазона (outRange)
        // Start: индекс первого бара с валидным KAMA
        // End: индекс последнего бара с валидным KAMA
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Инициализирует сумму абсолютных изменений цены (sumROC1) за период.
    /// </summary>
    /// <param name="inReal">Входные данные (цены Close).</param>
    /// <param name="sumROC1">Ссылка на переменную суммы ROC для обновления.</param>
    /// <param name="today">Ссылка на текущий индекс, обновляется в процессе.</param>
    /// <param name="optInTimePeriod">Период времени (Time Period) для расчета.</param>
    /// <typeparam name="T">Числовой тип данных.</typeparam>
    /// <remarks>
    /// Эта метода рассчитывает начальное значение суммы абсолютных изменений цены за optInTimePeriod баров.
    /// Используется для расчета коэффициента эффективности (Efficiency Ratio).
    /// </remarks>
    private static void InitSumROC<T>(
        ReadOnlySpan<T> inReal,
        ref T sumROC1,
        ref int today,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Цикл по периоду для накопления суммы абсолютных изменений цены
        for (var i = optInTimePeriod; i > 0; i--)
        {
            var tempReal = inReal[today++];
            tempReal -= inReal[today];
            // Добавление абсолютного значения изменения цены к сумме
            sumROC1 += T.Abs(tempReal);
        }
    }

    /// <summary>
    /// Рассчитывает коэффициент эффективности (Efficiency Ratio - ER).
    /// </summary>
    /// <param name="sumROC1">Сумма абсолютных изменений цены за период (волатильность).</param>
    /// <param name="periodROC">Изменение цены за период (направление).</param>
    /// <typeparam name="T">Числовой тип данных.</typeparam>
    /// <returns>
    /// Коэффициент эффективности от 0 до 1.
    /// <para>
    /// - 1: сильный тренд (цена движется в одном направлении).
    /// - 0: боковое движение (высокая волатильность без направления).
    /// </para>
    /// </returns>
    private static T CalcEfficiencyRatio<T>(T sumROC1, T periodROC) where T : IFloatingPointIeee754<T> =>
        sumROC1 <= periodROC || T.IsZero(sumROC1) ? T.One : T.Abs(periodROC / sumROC1);

    /// <summary>
    /// Рассчитывает сглаживающую константу (Smoothing Constant - SC) на основе коэффициента эффективности.
    /// </summary>
    /// <param name="efficiencyRatio">Коэффициент эффективности (Efficiency Ratio).</param>
    /// <typeparam name="T">Числовой тип данных.</typeparam>
    /// <returns>
    /// Сглаживающая константа для использования в формуле KAMA.
    /// <para>
    /// - Диапазон значений: от SlowestSC² до FastestSC².
    /// - Возводится в квадрат для усиления адаптивности.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Формула: SC = [ER * (FastestSC - SlowestSC) + SlowestSC]²
    /// <para>
    /// - FastestSC = 2/(2+1) ≈ 0.6667 (быстрое сглаживание).
    /// - SlowestSC = 2/(30+1) ≈ 0.0645 (медленное сглаживание).
    /// </para>
    /// </remarks>
    private static T CalcSmoothingConstant<T>(T efficiencyRatio) where T : IFloatingPointIeee754<T>
    {
        // constMax: максимальная сглаживающая константа (SlowestSC)
        var constMax = FunctionHelpers.Two<T>() / (T.CreateChecked(30) + T.One);
        // constDiff: разница между быстрой и медленной константами
        var constDiff = FunctionHelpers.Two<T>() / (FunctionHelpers.Two<T>() + T.One) - constMax;
        // Временная переменная для расчета SC до возведения в квадрат
        var tempReal = efficiencyRatio * constDiff + constMax;

        // Возведение в квадрат для усиления адаптивности
        return tempReal * tempReal;
    }

    /// <summary>
    /// Рассчитывает значение KAMA по формуле адаптивного скользящего среднего.
    /// </summary>
    /// <param name="todayValue">Текущая цена (Close) для текущего бара.</param>
    /// <param name="prevKAMA">Предыдущее значение KAMA (PreviousKAMA).</param>
    /// <param name="smoothingConstant">Сглаживающая константа (Smoothing Constant - SC).</param>
    /// <typeparam name="T">Числовой тип данных.</typeparam>
    /// <returns>Новое значение KAMA.</returns>
    /// <remarks>
    /// Формула: KAMA = PreviousKAMA + SC * (Price - PreviousKAMA)
    /// <para>
    /// Аналогична формуле EMA, но с адаптивным коэффициентом сглаживания.
    /// </para>
    /// </remarks>
    private static T CalcKAMA<T>(T todayValue, T prevKAMA, T smoothingConstant) where T : IFloatingPointIeee754<T> =>
        (todayValue - prevKAMA) * smoothingConstant + prevKAMA;

    /// <summary>
    /// Обновляет сумму абсолютных изменений цены (sumROC1) для скользящего окна.
    /// </summary>
    /// <param name="inReal">Входные данные (цены Close).</param>
    /// <param name="sumROC1">Ссылка на переменную суммы ROC для обновления.</param>
    /// <param name="today">Ссылка на текущий индекс.</param>
    /// <param name="trailingIdx">Ссылка на индекс отстающего бара.</param>
    /// <param name="trailingValue">Ссылка на отстающее значение цены.</param>
    /// <typeparam name="T">Числовой тип данных.</typeparam>
    /// <remarks>
    /// Метод обновляет sumROC1 путем:
    /// <para>
    /// 1. Удаления вклада отстающего бара (trailingValue).
    /// 2. Добавления вклада нового бара (today).
    /// </para>
    /// Это обеспечивает эффективный расчет скользящей суммы без пересчета всего периода.
    /// </remarks>
    private static void UpdateSumROC<T>(
        ReadOnlySpan<T> inReal,
        ref T sumROC1,
        ref int today,
        ref int trailingIdx,
        ref T trailingValue) where T : IFloatingPointIeee754<T>
    {
        // Текущая цена
        var tempReal = inReal[today];
        // Цена отстающего бара
        var tempReal2 = inReal[trailingIdx++];

        /* Корректировка sumROC1:
         *  - Удаление отстающего ROC1 (вклад старого бара)
         *  - Добавление нового ROC1 (вклад нового бара)
         */
        sumROC1 -= T.Abs(trailingValue - tempReal2);
        sumROC1 += T.Abs(tempReal - inReal[today - 1]);

        // Сохранение отстающего значения (trailingValue).
        // Делается это потому, что входные и выходные данные могут указывать на один и тот же буфер памяти.
        trailingValue = tempReal2;
    }
}
