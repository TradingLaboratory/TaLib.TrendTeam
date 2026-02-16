// Название файла: TA_StochF.cs
// Рекомендуемое размещение:
// Основная папка: MomentumIndicators
// Подпапка: Oscillators (существующая)
// Альтернативные варианты подпапок (если потребуется детальная группировка):
// - ImpulseOscillators (для осцилляторов, измеряющих импульс)
// - RangeBoundOscillators (для осцилляторов с ограниченным диапазоном 0-100)
// - StochasticFamily (для всех вариаций стохастических осцилляторов)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Stochastic Fast (Momentum Indicators) — Стохастический осциллятор, быстрая версия (Индикаторы импульса)
    /// <para>
    /// Быстрый стохастический осциллятор измеряет положение цены закрытия относительно ценового диапазона за заданный период.
    /// В отличие от медленной версии (Stochastic Slow), использует минимальное сглаживание, что повышает чувствительность к краткосрочным движениям цены.
    /// </para>
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимальные цены баров).</param>
    /// <param name="inLow">Массив входных цен Low (минимальные цены баров).</param>
    /// <param name="inClose">Массив входных цен Close (цены закрытия баров).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outFastK">
    /// Массив, содержащий ТОЛЬКО валидные значения линии %K (быстрой).
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outFastK[i]</c> соответствует <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outFastD">
    /// Массив, содержащий ТОЛЬКО валидные значения линии %D (быстрой) — сглаженной версии %K.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outFastD[i]</c> соответствует <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outFastK"/> и <paramref name="outFastD"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outFastK"/> и <paramref name="outFastD"/>.
    /// - Гарантируется: <c>End == inClose.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inClose"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastKPeriod">Период для расчета быстрой линии %K (определяет длину окна для поиска HighestHigh и LowestLow).</param>
    /// <param name="optInFastDPeriod">Период сглаживания для преобразования линии %K в линию %D (длина скользящей средней).</param>
    /// <param name="optInFastDMAType">Тип скользящей средней для сглаживания линии %K при расчете %D.</param>
    /// <typeparam name="T">Числовой тип данных (float/double).</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>).</returns>
    /// <remarks>
    /// <para>
    /// <b>Особенности индикатора:</b>
    /// <list type="bullet">
    ///   <item><description>Высокая чувствительность к краткосрочным колебаниям цены из-за минимального сглаживания</description></item>
    ///   <item><description>Более частые сигналы по сравнению с медленной версией (Stochastic Slow)</description></item>
    ///   <item><description>Повышенный риск ложных сигналов в условиях высокой волатильности</description></item>
    ///   <item><description>Диапазон значений строго ограничен 0–100</description></item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Этапы расчета:</b>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет сырого значения %K:
    ///       <code>
    ///         %K = 100 * ((Close - LowestLow) / (HighestHigh - LowestLow))
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><description><b>Close</b> — цена закрытия текущего бара</description></item>
    ///         <item><description><b>LowestLow</b> — минимальное значение Low за период <paramref name="optInFastKPeriod"/></description></item>
    ///         <item><description><b>HighestHigh</b> — максимальное значение High за период <paramref name="optInFastKPeriod"/></description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item><description>Сглаживание %K через скользящую среднюю с параметрами <paramref name="optInFastDPeriod"/> и <paramref name="optInFastDMAType"/> для получения линии %D</description></item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <b>Интерпретация сигналов:</b>
    /// <list type="bullet">
    ///   <item><description>Значения выше 80: зона перекупленности (потенциал нисходящей коррекции)</description></item>
    ///   <item><description>Значения ниже 20: зона перепроданности (потенциал восходящего движения)</description></item>
    ///   <item><description>Пересечение линий:
    ///     <list type="bullet">
    ///       <item><description>%K пересекает %D снизу вверх — возможный сигнал на покупку</description></item>
    ///       <item><description>%K пересекает %D сверху вниз — возможный сигнал на продажу</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>Дивергенция между осциллятором и ценой — признак ослабления текущего тренда</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode StochF<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outFastK,
        Span<T> outFastD,
        out Range outRange,
        int optInFastKPeriod = 5,
        int optInFastDPeriod = 3,
        Core.MAType optInFastDMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        StochFImpl(inHigh, inLow, inClose, inRange, outFastK, outFastD, out outRange, optInFastKPeriod, optInFastDPeriod, optInFastDMAType);

    /// <summary>
    /// Возвращает период "просмотра назад" (lookback period) для функции <see cref="StochF{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Значение lookback определяет минимальное количество баров, необходимых для расчета первого валидного значения индикатора.
    /// Все бары с индексом меньше, чем значение lookback, пропускаются при расчете.
    /// </para>
    /// <para>
    /// Формула расчета:
    /// <code>
    /// lookback = (optInFastKPeriod - 1) + MaLookback(optInFastDPeriod, optInFastDMAType)
    /// </code>
    /// где первый член обеспечивает достаточное окно для поиска экстремумов, а второй — для сглаживания %K в %D.
    /// </para>
    /// </remarks>
    /// <param name="optInFastKPeriod">Период для расчета линии %K.</param>
    /// <param name="optInFastDPeriod">Период сглаживания для линии %D.</param>
    /// <param name="optInFastDMAType">Тип скользящей средней для сглаживания.</param>
    /// <returns>Количество периодов, необходимых для первого валидного расчета индикатора.</returns>
    [PublicAPI]
    public static int StochFLookback(int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma) =>
        optInFastKPeriod < 1 || optInFastDPeriod < 1 ? -1 : optInFastKPeriod - 1 + MaLookback(optInFastDPeriod, optInFastDMAType);

    // Реализация метода (логика расчетов не изменена)
    private static Core.RetCode StochFImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outFastK,
        Span<T> outFastD,
        out Range outRange,
        int optInFastKPeriod,
        int optInFastDPeriod,
        Core.MAType optInFastDMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка валидности входного диапазона: убедиться, что все входные массивы имеют достаточную длину
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности входных параметров периодов
        if (optInFastKPeriod < 1 || optInFastDPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периодов "просмотра назад" для разных этапов расчета:
        var lookbackK = optInFastKPeriod - 1; // Период для поиска экстремумов (HighestHigh и LowestLow)
        var lookbackFastD = MaLookback(optInFastDPeriod, optInFastDMAType); // Период для сглаживания %K в %D
        var lookbackTotal = StochFLookback(optInFastKPeriod, optInFastDPeriod, optInFastDMAType); // Общий период просмотра назад

        // Корректировка начального индекса с учетом необходимого периода просмотра назад
        startIdx = Math.Max(startIdx, lookbackTotal);
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var trailingIdx = startIdx - lookbackTotal; // Индекс начала текущего окна поиска экстремумов
        var today = trailingIdx + lookbackK; // Текущий индекс бара для расчета %K

        // Инициализация временного буфера для хранения промежуточных значений %K:
        // Выбор буфера зависит от возможного пересечения с выходными массивами (защита от затирания данных)
        Span<T> tempBuffer;
        if (outFastK == inHigh || outFastK == inLow || outFastK == inClose)
        {
            tempBuffer = outFastK;
        }
        else if (outFastD == inHigh || outFastD == inLow || outFastD == inClose)
        {
            tempBuffer = outFastD;
        }
        else
        {
            tempBuffer = new T[endIdx - today + 1];
        }

        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;

        // Основной цикл расчета сырого значения %K для каждого бара:
        while (today <= endIdx)
        {
            // Поиск минимального значения Low в текущем окне [trailingIdx, today]
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            // Поиск максимального значения High в текущем окне [trailingIdx, today]
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            // Расчет %K с защитой от деления на ноль (когда HighestHigh == LowestLow):
            // %K = 100 * (Close - LowestLow) / (HighestHigh - LowestLow)
            var diff = (highest - lowest) / FunctionHelpers.Hundred<T>();
            tempBuffer[outIdx++] = !T.IsZero(diff) ? (inClose[today] - lowest) / diff : T.Zero;

            trailingIdx++;
            today++;
        }

        // Расчет сглаженной линии %D через скользящую среднюю от значений %K:
        var retCode = MaImpl(tempBuffer, Range.EndAt(outIdx - 1), outFastD, out outRange, optInFastDPeriod, optInFastDMAType);
        if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
        {
            return retCode;
        }

        var nbElement = outRange.End.Value - outRange.Start.Value;

        // Копирование значений %K в выходной массив с учетом периода сглаживания (сдвиг на lookbackFastD):
        tempBuffer.Slice(lookbackFastD, nbElement).CopyTo(outFastK);

        // Установка финального диапазона валидных значений в исходных данных:
        outRange = new Range(startIdx, startIdx + nbElement);

        return Core.RetCode.Success;
    }
}
