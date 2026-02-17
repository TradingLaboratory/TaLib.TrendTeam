// Название файла: TA_AdOsc.cs
// Группы к которым можно отнести индикатор:
// VolumeIndicators (существующая папка - идеальное соответствие категории)
// Oscillators (альтернатива для группировки по типу индикатора)
// MomentumIndicators (альтернатива для акцента на импульсе)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Chaikin A/D Oscillator (Volume Indicators) — Осциллятор Chaikin Накопления/Распределения (Индикаторы Объёма)
    /// </summary>
    /// <param name="inHigh">
    /// Массив входных цен High (максимальные цены баров).  
    /// Используется для расчета линии Накопления/Распределения (A/D Line).
    /// </param>
    /// <param name="inLow">
    /// Массив входных цен Low (минимальные цены баров).  
    /// Используется для расчета линии Накопления/Распределения (A/D Line).
    /// </param>
    /// <param name="inClose">
    /// Массив входных цен Close (цены закрытия баров).  
    /// Используется для расчета линии Накопления/Распределения (A/D Line).
    /// </param>
    /// <param name="inVolume">
    /// Массив входных данных Volume (объёмы торгов).  
    /// Используется для взвешивания денежного потока в линии A/D.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив входных данных.  
    /// - Позволяет ограничить расчет определенной частью исторических данных.
    /// </param>
    /// <param name="outReal">
    /// Массив для сохранения рассчитанных значений осциллятора.  
    /// - Содержит ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c>.  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c> и другим входным массивам.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных массивах, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина входных массивов меньше lookback периода), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastPeriod">
    /// Период для расчета быстрой EMA (Exponential Moving Average).  
    /// - По умолчанию: 3  
    /// - Должен быть >= 2 для корректного расчета.
    /// </param>
    /// <param name="optInSlowPeriod">
    /// Период для расчета медленной EMA (Exponential Moving Average).  
    /// - По умолчанию: 10  
    /// - Должен быть >= 2 для корректного расчета.
    /// </param>
    /// <typeparam name="T">Числовой тип данных (должен реализовывать <see cref="IFloatingPointIeee754{T}"/>)</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>)</returns>
    /// <remarks>
    /// Осциллятор Chaikin A/D (Accumulation/Distribution Oscillator) измеряет импульс линии Накопления/Распределения, 
    /// сравнивая краткосрочные и долгосрочные интервалы денежного потока (Money Flow).
    /// <para>
    /// <b>Назначение индикатора</b>:  
    /// - Выявление изменений баланса покупок/продаж до их отражения в ценах.  
    /// - Подтверждение трендов или предсказание пробоев уровней.  
    /// - Обнаружение дивергенций между ценой и объёмом.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет линии Накопления/Распределения (A/D Line):
    ///       <code>
    ///         Money Flow Multiplier = ((Close - Low) - (High - Close)) / (High - Low)
    ///         A/D Line = Money Flow Multiplier * Volume
    ///       </code>
    ///       Где:  
    ///       - <c>Close</c>, <c>Low</c>, <c>High</c> — цены закрытия, минимума и максимума  
    ///       - <c>Volume</c> — объём торгов
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет экспоненциальных скользящих средних (EMA) для A/D Line:
    ///       <code>
    ///         EMA(Fast) = α × A/D Line + (1 - α) × EMA(Fast-1)
    ///         EMA(Slow) = β × A/D Line + (1 - β) × EMA(Slow-1)
    ///       </code>
    ///       Где:  
    ///       - α = 2 / (FastPeriod + 1) — коэффициент сглаживания для быстрой EMA  
    ///       - β = 2 / (SlowPeriod + 1) — коэффициент сглаживания для медленной EMA
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Осциллятор как разница между EMA:
    ///       <code>
    ///         AdOsc = EMA(Fast) - EMA(Slow)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация сигналов</b>:
    /// <list type="bullet">
    ///   <item><description>Рост значений ⇒ доминирование покупателей (накопление)</description></item>
    ///   <item><description>Падение значений ⇒ давление продавцов (распределение)</description></item>
    ///   <item><description>Пересечение нулевой линии ⇒ смена импульса денежного потока</description></item>
    ///   <item><description>Дивергенция с ценой ⇒ возможный разворот тренда</description></item>
    /// </list>
    ///
    /// <b>Важные замечания</b>:
    /// <list type="bullet">
    ///   <item><description>Lookback период определяет индекс первого бара, для которого доступно валидное значение индикатора.</description></item>
    ///   <item><description>Все бары с индексом меньше lookback будут пропущены при расчете.</description></item>
    ///   <item><description>Для корректной работы требуется минимум (lookback + 1) баров во входных данных.</description></item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode AdOsc<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod = 3,
        int optInSlowPeriod = 10) where T : IFloatingPointIeee754<T> =>
        AdOscImpl(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod);

    /// <summary>
    /// Возвращает период "просмотра назад" (lookback) для осциллятора Chaikin A/D.
    /// </summary>
    /// <param name="optInFastPeriod">Период быстрой EMA (должен быть >= 2)</param>
    /// <param name="optInSlowPeriod">Период медленной EMA (должен быть >= 2)</param>
    /// <returns>
    /// Минимальное количество баров, необходимое для расчета первого валидного значения индикатора.  
    /// - Возвращает -1, если периоды некорректны (< 2).  
    /// - Все бары с индексом меньше этого значения не будут иметь валидных результатов.
    /// </returns>
    /// <remarks>
    /// Lookback период определяет, сколько исторических данных требуется для инициализации индикатора.  
    /// Это значение используется для определения <paramref name="outRange.Start"/> в результатах расчета.
    /// </remarks>
    [PublicAPI]
    public static int AdOscLookback(int optInFastPeriod = 3, int optInSlowPeriod = 10)
    {
        // Выбираем наибольший период между быстрым и медленным для определения lookback
        var slowestPeriod = optInFastPeriod < optInSlowPeriod ? optInSlowPeriod : optInFastPeriod;
        // Возвращаем -1 если периоды некорректны, иначе рассчитываем lookback для EMA
        return optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : EmaLookback(slowestPeriod);
    }

    /// <summary>
    /// Вариант метода для массивов (совместимость с абстрактным API).
    /// </summary>
    /// <remarks>
    /// Этот перегруженный метод обеспечивает совместимость с кодом, использующим массивы вместо Span<T>.  
    /// Логика расчета идентична основной реализации <see cref="AdOsc{T}(ReadOnlySpan{T}, ReadOnlySpan{T}, ReadOnlySpan{T}, ReadOnlySpan{T}, Range, Span{T}, out Range, int, int)"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode AdOsc<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        T[] inVolume,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInFastPeriod = 3,
        int optInSlowPeriod = 10) where T : IFloatingPointIeee754<T> =>
        AdOscImpl<T>(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod);

    /// <summary>
    /// Основная реализация расчета осциллятора Chaikin A/D.
    /// </summary>
    /// <remarks>
    /// Метод выполняет полный цикл расчета индикатора:  
    /// 1. Валидация входных параметров и диапазонов  
    /// 2. Инициализация коэффициентов сглаживания EMA  
    /// 3. Расчет линии Накопления/Распределения (A/D Line)  
    /// 4. Расчет быстрой и медленной EMA  
    /// 5. Вычисление осциллятора как разницы EMA
    /// </remarks>
    private static Core.RetCode AdOscImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализируем outRange как пустой диапазон (будет обновлен при успешном расчете)
        outRange = Range.EndAt(0);

        // Проверка корректности входных диапазонов
        // ValidateInputRange возвращает кортеж (startIdx, endIdx) или null при ошибке
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (startIdx, endIdx) = rangeIndices;

        // Проверка валидности периодов EMA (должны быть >= 2)
        if (optInFastPeriod < 2 || optInSlowPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Определение периода "просмотра назад" (lookback)
        // lookbackTotal определяет минимальное количество баров для первого валидного значения
        var lookbackTotal = AdOscLookback(optInFastPeriod, optInSlowPeriod);
        // Корректируем startIdx с учетом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);
        // Если startIdx > endIdx, данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация индексов для выходных данных
        var outBegIdx = startIdx;  // Индекс первого валидного значения в выходном массиве
        var today = startIdx - lookbackTotal;  // Текущий индекс во входных данных (начинаем с lookback)

        // Расчет коэффициентов сглаживания для EMA (Exponential Moving Average)
        // fastK = α = 2 / (FastPeriod + 1) - коэффициент для быстрой EMA
        var fastK = FunctionHelpers.Two<T>() / (T.CreateChecked(optInFastPeriod) + T.One);
        var oneMinusFastK = T.One - fastK;  // (1 - α) для быстрой EMA

        // slowK = β = 2 / (SlowPeriod + 1) - коэффициент для медленной EMA
        var slowK = FunctionHelpers.Two<T>() / (T.CreateChecked(optInSlowPeriod) + T.One);
        var oneMinusSlowK = T.One - slowK;  // (1 - β) для медленной EMA

        // Инициализация линии Накопления/Распределения (Accumulation/Distribution Line)
        // A/D Line измеряет кумулятивный денежный поток
        var ad = T.Zero;
        // Рассчитываем начальное значение A/D Line
        ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);

        // Начальные значения EMA (Exponential Moving Average)
        // Инициализируем обе EMA первым значением A/D Line
        var fastEMA = ad;  // Начальное значение быстрой EMA
        var slowEMA = ad;  // Начальное значение медленной EMA

        // Пропуск нестабильного периода (до startIdx)
        // В этом периоде рассчитываем EMA, но не сохраняем результаты в outReal
        while (today < startIdx)
        {
            // Обновляем линию Накопления/Распределения для текущего бара
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);
            // Обновляем быструю EMA: EMA = α × AD + (1 - α) × EMA_prev
            fastEMA = fastK * ad + oneMinusFastK * fastEMA;
            // Обновляем медленную EMA: EMA = β × AD + (1 - β) × EMA_prev
            slowEMA = slowK * ad + oneMinusSlowK * slowEMA;
        }

        // Основной цикл расчета осциллятора
        // В этом цикле рассчитываем и сохраняем валидные значения индикатора
        var outIdx = 0;  // Индекс для записи в выходной массив outReal
        while (today <= endIdx)
        {
            // Обновляем линию Накопления/Распределения для текущего бара
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref today, ad);
            // Обновляем быструю EMA
            fastEMA = fastK * ad + oneMinusFastK * fastEMA;
            // Обновляем медленную EMA
            slowEMA = slowK * ad + oneMinusSlowK * slowEMA;
            // Рассчитываем осциллятор как разницу между быстрой и медленной EMA
            outReal[outIdx++] = fastEMA - slowEMA;
        }

        // Устанавливаем диапазон валидных выходных данных
        // outRange.Start = outBegIdx (первый индекс с валидным значением)
        // outRange.End = outBegIdx + outIdx (последний индекс с валидным значением)
        outRange = new Range(outBegIdx, outBegIdx + outIdx);
        return Core.RetCode.Success;
    }
}
