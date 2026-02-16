// Файл: TA_Rsi.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (основная категория - 100%)
// Oscillators (подкатегория - 100%)
// TrendStrength (альтернатива ~60% - индикатор отражает силу движения цены)

namespace TALib;

/// <summary>
/// Содержит реализацию технических индикаторов библиотеки TA-Lib.
/// </summary>
public static partial class Functions
{
    /// <summary>
    /// Relative Strength Index (RSI) (Momentum) — Индекс относительной силы (RSI) (Импульсные индикаторы)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены закрытия <c>Close</c>, но могут использоваться и другие временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период расчета (по умолчанию 14). Минимальное значение: 2.</param>
    /// <typeparam name="T">Числовой тип данных (float/double с поддержкой IEEE 754).</typeparam>
    /// <returns>Код результата выполнения из перечисления <see cref="Core.RetCode"/>.</returns>
    /// <remarks>
    /// <para>
    /// RSI измеряет скорость и изменение ценовых движений в диапазоне 0-100.
    /// Используется для определения зон перекупленности/перепроданности, дивергенций и силы текущего тренда.
    /// </para>
    /// 
    /// <para>
    /// <b>Методика расчета (сглаживание по Уайлдеру)</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление приростов (<c>Gain</c>) и убытков (<c>Loss</c>) между последовательными барами:
    ///       <code>
    /// Gain = Max(CurrentClose - PreviousClose, 0)
    /// Loss = Max(PreviousClose - CurrentClose, 0)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет экспоненциального среднего для приростов и убытков (метод Уайлдера):
    ///       <code>
    /// AvgGain = ((PreviousAvgGain * (TimePeriod - 1)) + CurrentGain) / TimePeriod
    /// AvgLoss = ((PreviousAvgLoss * (TimePeriod - 1)) + CurrentLoss) / TimePeriod
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет относительной силы (<c>RS</c>):
    ///       <code>RS = AvgGain / AvgLoss</code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Финальная формула RSI:
    ///       <code>RSI = 100 - (100 / (1 + RS))</code>
    ///       Эквивалентная оптимизированная форма:
    ///       <code>RSI = 100 * (AvgGain / (AvgGain + AvgLoss))</code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item><c>RSI &gt; 70</c> — зона перекупленности (overbought), возможен откат вниз</item>
    ///   <item><c>RSI &lt; 30</c> — зона перепроданности (oversold), возможен отскок вверх</item>
    ///   <item><c>RSI ≈ 50</c> — нейтральная зона, отсутствие явного импульса</item>
    ///   <item>Дивергенции между ценой и RSI часто предвещают разворот тренда</item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Особенности реализации</b>:
    /// <list type="bullet">
    ///   <item>Первые <c>optInTimePeriod</c> баров используют простое усреднение</item>
    ///   <item>Последующие значения рассчитываются с экспоненциальным сглаживанием (метод Уайлдера)</item>
    ///   <item>Поддерживается режим совместимости с Metastock через <see cref="Core.CompatibilitySettings"/></item>
    ///   <item>Нестабильный период может быть настроен через <see cref="Core.UnstablePeriodSettings"/></item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Rsi<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        RsiImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает количество баров, необходимых для расчета первого валидного значения RSI.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета индикатора.</param>
    /// <returns>
    /// Количество баров (lookback period), которые необходимо пропустить перед получением первого валидного значения.
    /// Возвращает -1, если <paramref name="optInTimePeriod"/> меньше минимально допустимого значения (2).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Lookback period включает:
    /// <list type="bullet">
    ///   <item>Базовый период <c>optInTimePeriod</c> для инициализации средних значений</item>
    ///   <item>Дополнительный нестабильный период из <see cref="Core.UnstablePeriodSettings"/></item>
    ///   <item>Коррекция -1 при режиме совместимости с Metastock</item>
    /// </list>
    /// </para>
    /// <para>
    /// Пример: при <c>optInTimePeriod = 14</c> и отсутствии нестабильного периода:
    /// <list type="bullet">
    ///   <item>Стандартный режим: lookback = 14</item>
    ///   <item>Режим Metastock: lookback = 13</item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int RsiLookback(int optInTimePeriod = 14)
    {
        if (optInTimePeriod < 2)
            return -1;

        var retValue = optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Rsi);

        // Коррекция для совместимости с Metastock: уменьшение на 1 период
        if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock)
            retValue--;

        return retValue;
    }

    // Реализация основного алгоритма расчета RSI с экспоненциальным сглаживанием по методу Уайлдера
    private static Core.RetCode RsiImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);
        var rangeIndices = FunctionHelpers.ValidateInputRange(inRange, inReal.Length);
        if (rangeIndices == null)
            return Core.RetCode.OutOfRangeParam;

        var (startIdx, endIdx) = rangeIndices.Value;
        if (optInTimePeriod < 2)
            return Core.RetCode.BadParam;

        // Расчет количества баров, которые необходимо пропустить для получения первого валидного значения
        var lookbackTotal = RsiLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);
        if (startIdx > endIdx)
            return Core.RetCode.Success;

        var timePeriod = T.CreateChecked(optInTimePeriod);
        var outIdx = 0;
        var today = startIdx - lookbackTotal;
        var prevValue = inReal[today];

        // Обработка специфических требований совместимости с платформой Metastock
        if (Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Rsi) == 0 &&
            Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock &&
            ProcessRsiMetastockCompatibility(inReal, outReal, ref outRange, optInTimePeriod, endIdx, startIdx, ref prevValue, ref today, ref outIdx, out var retCode))
        {
            return retCode;
        }

        // Инициализация накопленных приростов (Gain) и убытков (Loss) за начальный период
        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);

        // Нормализация накопленных значений к средним за период (простое усреднение для инициализации)
        prevLoss /= timePeriod;
        prevGain /= timePeriod;

        // Пропуск нестабильного периода (если он задан в настройках)
        if (today > startIdx)
        {
            var total = prevGain + prevLoss;
            outReal[outIdx++] = !T.IsZero(total) ? FunctionHelpers.Hundred<T>() * (prevGain / total) : T.Zero;
        }
        else
        {
            // Досчет до начала запрошенного диапазона с применением сглаживания Уайлдера
            while (today < startIdx)
                ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
        }

        // Основной цикл расчета RSI для запрошенного диапазона данных
        while (today <= endIdx)
        {
            ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
            var total = prevGain + prevLoss;
            outReal[outIdx++] = !T.IsZero(total) ? FunctionHelpers.Hundred<T>() * (prevGain / total) : T.Zero;
        }

        outRange = new Range(startIdx, startIdx + outIdx);
        return Core.RetCode.Success;
    }

    // Обработка специфических требований совместимости с платформой Metastock
    private static bool ProcessRsiMetastockCompatibility<T>(
        ReadOnlySpan<T> inReal,
        Span<T> outReal,
        ref Range outRange,
        int optInTimePeriod,
        int endIdx,
        int startIdx,
        ref T prevValue,
        ref int today,
        ref int outIdx,
        out Core.RetCode retCode) where T : IFloatingPointIeee754<T>
    {
        var savePrevValue = prevValue;
        // Инициализация приростов/убытков за начальный период
        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);
        // Запись первого значения RSI (режим совместимости требует особой обработки)
        WriteInitialRsiValue(prevGain, prevLoss, optInTimePeriod, outReal, ref outIdx);

        if (today > endIdx)
        {
            outRange = new Range(startIdx, startIdx + outIdx);
            retCode = Core.RetCode.Success;
            return true;
        }

        // Возврат к предыдущему значению для продолжения расчета в основном цикле
        today -= optInTimePeriod;
        prevValue = savePrevValue;
        retCode = Core.RetCode.Success;
        return false;
    }

    // Инициализация накопленных приростов (Gain) и убытков (Loss) за начальный период
    private static void InitGainsAndLosses<T>(
        ReadOnlySpan<T> real,
        ref int today,
        ref T prevValue,
        int optInTimePeriod,
        out T prevGain,
        out T prevLoss) where T : IFloatingPointIeee754<T>
    {
        prevGain = T.Zero;  // Накопленная сумма положительных изменений (приростов)
        prevLoss = T.Zero;  // Накопленная сумма отрицательных изменений (убытков)
        today++;            // Переход к следующему бару для начала расчета изменений

        // Цикл накопления изменений за начальный период (без сглаживания)
        for (var i = optInTimePeriod; i > 0; i--)
        {
            var currentValue = real[today++];
            var change = currentValue - prevValue;  // Изменение цены относительно предыдущего бара
            prevValue = currentValue;

            if (change < T.Zero)
                prevLoss -= change;  // Накопление абсолютного значения убытка (отрицательное изменение)
            else
                prevGain += change;  // Накопление прироста (положительное изменение)
        }
    }

    // Запись начального значения RSI после инициализации средних приростов/убытков
    private static void WriteInitialRsiValue<T>(
        T prevGain,
        T prevLoss,
        int optInTimePeriod,
        Span<T> outReal,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        var timePeriod = T.CreateChecked(optInTimePeriod);
        var avgLoss = prevLoss / timePeriod;   // Средний убыток за начальный период
        var avgGain = prevGain / timePeriod;   // Средний прирост за начальный период
        var total = avgGain + avgLoss;         // Сумма средних прироста и убытка

        // Расчет и запись RSI: 100 * (AvgGain / (AvgGain + AvgLoss))
        outReal[outIdx++] = !T.IsZero(total)
            ? FunctionHelpers.Hundred<T>() * (avgGain / total)
            : T.Zero;
    }

    // Обработка текущего бара с применением экспоненциального сглаживания по методу Уайлдера
    private static void ProcessToday<T>(
        ReadOnlySpan<T> real,
        ref int today,
        ref T prevValue,
        ref T prevGain,
        ref T prevLoss,
        T timePeriod) where T : IFloatingPointIeee754<T>
    {
        var currentValue = real[today++];      // Текущая цена закрытия
        var change = currentValue - prevValue; // Изменение цены относительно предыдущего бара
        prevValue = currentValue;

        // Применение сглаживания Уайлдера: умножение предыдущего среднего на (период - 1)
        prevLoss *= timePeriod - T.One;
        prevGain *= timePeriod - T.One;

        // Добавление текущего изменения к накопленным значениям
        if (change < T.Zero)
            prevLoss -= change;  // Добавление абсолютного значения убытка
        else
            prevGain += change;  // Добавление прироста

        // Деление на полный период для получения нового среднего значения
        prevLoss /= timePeriod;
        prevGain /= timePeriod;
    }
}
