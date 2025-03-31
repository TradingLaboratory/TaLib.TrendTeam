// Файл: TA_Rsi.cs
namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Индекс относительной силы (RSI) - осциллятор импульса
    /// </summary>
    /// <param name="inReal">Массив входных значений для расчета.</param>
    /// <param name="inRange">Диапазон индексов, определяющий часть данных для расчета во входном массиве.</param>
    /// <param name="outReal">Массив для сохранения рассчитанных значений RSI.</param>
    /// <param name="outRange">Диапазон индексов с валидными данными в выходном массиве.</param>
    /// <param name="optInTimePeriod">Период расчета (по умолчанию 14).</param>
    /// <typeparam name="T">Числовой тип данных (float/double с поддержкой IEEE 754).</typeparam>
    /// <returns>Код результата выполнения из перечисления Core.RetCode</returns>
    /// <remarks>
    /// RSI измеряет скорость и изменение ценовых движений в диапазоне 0-100. 
    /// Используется для определения перекупленности/перепроданности, точек разворота и силы тренда.
    /// 
    /// <b>Методика расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление приростов и убытков:
    /// <code>
    /// Gain = Max(CurrentValue - PreviousValue, 0)
    /// Loss = Max(PreviousValue - CurrentValue, 0)
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сглаживание Уайлдера для средних приростов/убытков:
    /// <code>
    /// AvgGain = ((PreviousAvgGain * (TimePeriod - 1)) + CurrentGain) / TimePeriod
    /// AvgLoss = ((PreviousAvgLoss * (TimePeriod - 1)) + CurrentLoss) / TimePeriod
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет относительной силы (RS):
    ///       <code>RS = AvgGain / AvgLoss</code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Формула RSI:
    ///       <code>RSI = 100 - (100 / (1 + RS))</code>
    ///       Оптимизированный вариант:
    ///       <code>RSI = 100 * (AvgGain / (AvgGain + AvgLoss))</code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>>70 - перекупленность (возможен откат)</item>
    ///   <item><30 - перепроданность (возможен рост)</item>
    ///   <item>~50 - нейтральная зона</item>
    /// </list>
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
    /// Возвращает количество периодов для расчета начального значения RSI
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета</param>
    /// <returns>Количество периодов для полного расчета</returns>
    [PublicAPI]
    public static int RsiLookback(int optInTimePeriod = 14)
    {
        if (optInTimePeriod < 2)
            return -1;

        var retValue = optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Rsi);

        // Коррекция для совместимости с Metastock
        if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock)
            retValue--;

        return retValue;
    }

    // Реализация основного алгоритма RSI
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

        // Обработка начального периода для сглаживания
        var lookbackTotal = RsiLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);
        if (startIdx > endIdx)
            return Core.RetCode.Success;

        var timePeriod = T.CreateChecked(optInTimePeriod);
        var outIdx = 0;
        var today = startIdx - lookbackTotal;
        var prevValue = inReal[today];

        // Обработка специфики Metastock
        if (Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Rsi) == 0 &&
            Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock &&
            ProcessRsiMetastockCompatibility(inReal, outReal, ref outRange, optInTimePeriod, endIdx, startIdx, ref prevValue, ref today,
                ref outIdx, out var retCode))
        {
            return retCode;
        }

        // Инициализация начальных приростов и убытков
        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);

        // Нормализация по периоду
        prevLoss /= timePeriod;
        prevGain /= timePeriod;

        // Пропуск нестабильного периода
        if (today > startIdx)
        {
            var total = prevGain + prevLoss;
            outReal[outIdx++] = !T.IsZero(total) ? FunctionHelpers.Hundred<T>() * (prevGain / total) : T.Zero;
        }
        else
        {
            while (today < startIdx)
                ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
        }

        // Основной цикл расчета
        while (today <= endIdx)
        {
            ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
            var total = prevGain + prevLoss;
            outReal[outIdx++] = !T.IsZero(total) ? FunctionHelpers.Hundred<T>() * (prevGain / total) : T.Zero;
        }

        outRange = new Range(startIdx, startIdx + outIdx);
        return Core.RetCode.Success;
    }

    // Обработка специфических требований Metastock
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
        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);
        WriteInitialRsiValue(prevGain, prevLoss, optInTimePeriod, outReal, ref outIdx);

        if (today > endIdx)
        {
            outRange = new Range(startIdx, startIdx + outIdx);
            retCode = Core.RetCode.Success;
            return true;
        }

        today -= optInTimePeriod;
        prevValue = savePrevValue;
        retCode = Core.RetCode.Success;
        return false;
    }

    // Инициализация начальных значений приростов/убытков
    private static void InitGainsAndLosses<T>(
        ReadOnlySpan<T> real,
        ref int today,
        ref T prevValue,
        int optInTimePeriod,
        out T prevGain,
        out T prevLoss) where T : IFloatingPointIeee754<T>
    {
        prevGain = T.Zero;
        prevLoss = T.Zero;
        today++;

        for (var i = optInTimePeriod; i > 0; i--)
        {
            var currentValue = real[today++];
            var change = currentValue - prevValue;
            prevValue = currentValue;

            if (change < T.Zero)
                prevLoss -= change;
            else
                prevGain += change;
        }
    }

    // Запись начального значения RSI
    private static void WriteInitialRsiValue<T>(
        T prevGain,
        T prevLoss,
        int optInTimePeriod,
        Span<T> outReal,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        var timePeriod = T.CreateChecked(optInTimePeriod);
        var avgLoss = prevLoss / timePeriod;
        var avgGain = prevGain / timePeriod;
        var total = avgGain + avgLoss;

        outReal[outIdx++] = !T.IsZero(total)
            ? FunctionHelpers.Hundred<T>() * (avgGain / total)
            : T.Zero;
    }

    // Обработка текущего периода
    private static void ProcessToday<T>(
        ReadOnlySpan<T> real,
        ref int today,
        ref T prevValue,
        ref T prevGain,
        ref T prevLoss,
        T timePeriod) where T : IFloatingPointIeee754<T>
    {
        var currentValue = real[today++];
        var change = currentValue - prevValue;
        prevValue = currentValue;

        // Сглаживание по методу Уайлдера
        prevLoss *= timePeriod - T.One;
        prevGain *= timePeriod - T.One;

        if (change < T.Zero)
            prevLoss -= change;
        else
            prevGain += change;

        prevLoss /= timePeriod;
        prevGain /= timePeriod;
    }
}
