// файл TA_StochF.cs
namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Быстрый стохастический осциллятор (индикатор импульса)
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимумы).</param>
    /// <param name="inLow">Массив входных цен Low (минимумы).</param>
    /// <param name="inClose">Массив входных цен Close (закрытия).</param>
    /// <param name="inRange">Диапазон индексов для вычислений во входных данных.</param>
    /// <param name="outFastK">Массив для сохранения значений линии %K (быстрой).</param>
    /// <param name="outFastD">Массив для сохранения значений линии %D (быстрой).</param>
    /// <param name="outRange">Диапазон индексов с валидными данными в выходных массивах.</param>
    /// <param name="optInFastKPeriod">Период для расчета быстрой линии %K (определяет длину окна для поиска HighestHigh/LowestLow).</param>
    /// <param name="optInFastDPeriod">Период сглаживания для преобразования Fast %K в Fast %D (определяет длину MA).</param>
    /// <param name="optInFastDMAType">Тип скользящей средней для сглаживания Fast %K.</param>
    /// <typeparam name="T">Числовой тип данных (float/double).</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>).</returns>
    /// <remarks>
    /// Быстрый стохастический осциллятор оценивает положение цены закрытия относительно ценового диапазона за указанный период. 
    /// В отличие от стандартного стохастического осциллятора, здесь используется минимальное сглаживание, что делает индикатор более чувствительным к изменениям цены.
    /// <para>
    /// <b>Особенности</b>:
    /// <list type="bullet">
    ///   <item><description>Более высокая волатильность по сравнению с "медленной" версией</description></item>
    ///   <item><description>Высокая чувствительность к краткосрочным колебаниям цены</description></item>
    ///   <item><description>Потенциальные ложные сигналы в условиях высокой волатильности</description></item>
    /// </list>
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет сырого значения %K:
    ///       <code>
    ///         %K = 100 * ((Close - LowestLow) / (HighestHigh - LowestLow))
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><description><b>Close</b> - цена закрытия текущего периода</description></item>
    ///         <item><description><b>LowestLow</b> - минимальный минимум за период FastKPeriod</description></item>
    ///         <item><description><b>HighestHigh</b> - максимальный максимум за период FastKPeriod</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item><description>Сглаживание %K через скользящую среднюю с параметрами FastDPeriod и FastDMAType для получения %D</description></item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item><description>Значения выше 80: зона перекупленности (риск коррекции вниз)</description></item>
    ///   <item><description>Значения ниже 20: зона перепроданности (потенциал роста)</description></item>
    ///   <item><description>Пересечения %K и %D: 
    ///     <list type="bullet">
    ///       <item><description>%K ↑ выше %D - возможный сигнал на покупку</description></item>
    ///       <item><description>%K ↓ ниже %D - возможный сигнал на продажу</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>Дивергенция между осциллятором и ценой указывает на ослабление тренда</description></item>
    /// </list>
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
    /// Возвращает период "просмотра назад" (firstValidValue) для функции <see cref="StochF{T}"/>.
    /// </summary>
    /// <remarks>
    /// Этот период показывает, сколько свечей требуется "пропустить" перед началом стабильного расчета индикатора.
    /// Первое валидное значение будет доступно начиная с индекса, равного этому значению.
    /// </remarks>
    /// <returns>Количество периодов, необходимых для первого валидного расчета.</returns>
    [PublicAPI]
    public static int StochFLookback(int optInFastKPeriod = 5, int optInFastDPeriod = 3, Core.MAType optInFastDMAType = Core.MAType.Sma) =>
        optInFastKPeriod < 1 || optInFastDPeriod < 1 ? -1 : optInFastKPeriod - 1 + MaLookback(optInFastDPeriod, optInFastDMAType);

    // Реализация метода (без изменений логики)
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

        // Проверка валидности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периодов
        if (optInFastKPeriod < 1 || optInFastDPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периодов "просмотра назад":
        var lookbackK = optInFastKPeriod - 1; // Для расчета HighestHigh/LowestLow
        var lookbackFastD = MaLookback(optInFastDPeriod, optInFastDMAType); // Для сглаживания %K
        var lookbackTotal = StochFLookback(optInFastKPeriod, optInFastDPeriod, optInFastDMAType); // Общий период

        // Корректировка начального индекса для обеспечения достаточного количества данных
        startIdx = Math.Max(startIdx, lookbackTotal);
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var trailingIdx = startIdx - lookbackTotal; // Начало текущего окна расчета
        var today = trailingIdx + lookbackK; // Текущий индекс свечи для расчета

        // Инициализация временного буфера для промежуточных значений %K
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

        // Основной цикл расчета сырого %K
        while (today <= endIdx)
        {
            // Поиск минимума в текущем окне
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            // Поиск максимума в текущем окне
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            // Расчет %K с защитой от деления на ноль
            var diff = (highest - lowest) / FunctionHelpers.Hundred<T>();
            tempBuffer[outIdx++] = !T.IsZero(diff) ? (inClose[today] - lowest) / diff : T.Zero;

            trailingIdx++;
            today++;
        }

        // Расчет сглаженной линии %D через скользящую среднюю
        var retCode = MaImpl(tempBuffer, Range.EndAt(outIdx - 1), outFastD, out outRange, optInFastDPeriod, optInFastDMAType);
        if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
        {
            return retCode;
        }

        var nbElement = outRange.End.Value - outRange.Start.Value;

        // Копирование значений FastK с учетом периода сглаживания
        tempBuffer.Slice(lookbackFastD, nbElement).CopyTo(outFastK);

        // Установка выходного диапазона с первым валидным значением
        outRange = new Range(startIdx, startIdx + nbElement);

        return Core.RetCode.Success;
    }
}
