//Название файла: TA_Accbands.cs
//Группы к которым можно отнести:
//Overlap Studies (существующая папка - идеальное соответствие категории)
//VolatilityIndicators (альтернатива, если требуется группировка по типу индикатора)
//TrendStrength (альтернатива для акцента на силе тренда)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Acceleration Bands (Overlap Studies) - Полосы ускорения
    /// </summary>
    /// <param name="inHigh">Массив входных максимальных цен.</param>
    /// <param name="inLow">Массив входных минимальных цен.</param>
    /// <param name="inClose">Массив входных цен закрытия.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outRealUpperBand">Массив для хранения рассчитанных значений верхней полосы.</param>
    /// <param name="outRealMiddleBand">Массив для хранения рассчитанных значений средней полосы.</param>
    /// <param name="outRealLowerBand">Массив для хранения рассчитанных значений нижней полосы.</param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:  
    /// - Start: индекс первого элемента с валидным значением.  
    /// - End: индекс последнего элемента с валидным значением.
    /// </param>
    /// <param name="optInTimePeriod">Период расчета (по умолчанию 20).</param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>), 
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата из <see cref="Core.RetCode"/>.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете.
    /// </returns>
    /// <remarks>
    /// Полосы ускорения - индикатор волатильности для определения точек прорыва и силы тренда.  
    /// Полосы динамически адаптируются к движению цены.
    /// <para>
    /// Может использоваться совместно с индикаторами импульса или тренда, такими как 
    /// <see cref="Adx{T}">ADX</see> или <see cref="Rsi{T}">RSI</see> для подтверждения сигналов.
    /// </para>
    ///
    /// <b>Шаги расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет верхней и нижней полос для каждого периода:
    /// <code>
    /// Upper Band = High * (1 + 4 * (High - Low) / (High + Low))  
    /// Lower Band = Low * (1 - 4 * (High - Low) / (High + Low))
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет скользящей средней цены закрытия для средней полосы:
    ///       <code>
    ///         Middle Band = SMA(Close, Period)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение SMA для сглаживания верхней и нижней полос.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Прорыв цены выше верхней полосы указывает на сильный восходящий импульс.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Прорыв цены ниже нижней полосы указывает на сильный нисходящий импульс.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Средняя полоса служит базовой линией для определения тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Accbands<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outRealUpperBand,
        Span<T> outRealMiddleBand,
        Span<T> outRealLowerBand,
        out Range outRange,
        int optInTimePeriod = 20) where T : IFloatingPointIeee754<T> =>
        AccbandsImpl(inHigh, inLow, inClose, inRange, outRealUpperBand, outRealMiddleBand, outRealLowerBand, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период lookback для <see cref="Accbands{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета.</param>
    /// <returns>
    /// Количество периодов, необходимых для расчета первого валидного значения.  
    /// Возвращает -1 при некорректном периоде.
    /// </returns>
    [PublicAPI]
    public static int AccbandsLookback(int optInTimePeriod = 20) => optInTimePeriod < 2 ? -1 : SmaLookback(optInTimePeriod);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Accbands<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outRealUpperBand,
        T[] outRealMiddleBand,
        T[] outRealLowerBand,
        out Range outRange,
        int optInTimePeriod = 20) where T : IFloatingPointIeee754<T> =>
        AccbandsImpl<T>(inHigh, inLow, inClose, inRange, outRealUpperBand, outRealMiddleBand, outRealLowerBand, out outRange,
            optInTimePeriod);

    private static Core.RetCode AccbandsImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outRealUpperBand,
        Span<T> outRealMiddleBand,
        Span<T> outRealLowerBand,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация диапазона выходных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam; // Неверный диапазон входных данных
        }
        var (startIdx, endIdx) = rangeIndices; // Индексы начала и конца обработки

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam; // Некорректный период
        }

        var lookbackTotal = AccbandsLookback(optInTimePeriod); // Расчет периода lookback
        startIdx = Math.Max(startIdx, lookbackTotal); // Корректировка начального индекса

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success; // Нет данных для обработки
        }

        // Буферы для хранения промежуточных значений полос
        var outputSize = endIdx - startIdx + 1;
        var bufferSize = outputSize + lookbackTotal;
        Span<T> tempBuffer1 = new T[bufferSize]; // Верхняя полоса до сглаживания
        Span<T> tempBuffer2 = new T[bufferSize]; // Нижняя полоса до сглаживания

        // Расчет исходных значений полос
        for (int j = 0, i = startIdx - lookbackTotal; i <= endIdx; i++, j++)
        {
            var high = inHigh[i];
            var low = inLow[i];
            var tempReal = high + low;

            if (!T.IsZero(tempReal))
            {
                // Расчет коэффициента волатильности
                tempReal = FunctionHelpers.Four<T>() * (high - low) / tempReal;
                tempBuffer1[j] = high * (T.One + tempReal); // Верхняя полоса
                tempBuffer2[j] = low * (T.One - tempReal);  // Нижняя полоса
            }
            else
            {
                // Если сумма High+Low = 0, используем исходные значения
                tempBuffer1[j] = high;
                tempBuffer2[j] = low;
            }
        }

        // Расчет средней полосы (SMA цены закрытия)
        var retCode = SmaImpl(inClose, new Range(startIdx, endIdx), outRealMiddleBand, out var dummyRange, optInTimePeriod);
        if (retCode != Core.RetCode.Success || dummyRange.End.Value - dummyRange.Start.Value != outputSize)
        {
            return retCode; // Ошибка при расчете SMA
        }

        // Применение SMA к верхней полосе
        retCode = SmaImpl(tempBuffer1, Range.EndAt(bufferSize - 1), outRealUpperBand, out dummyRange, optInTimePeriod);
        if (retCode != Core.RetCode.Success || dummyRange.End.Value - dummyRange.Start.Value != outputSize)
        {
            return retCode; // Ошибка при сглаживании верхней полосы
        }

        // Применение SMA к нижней полосе
        retCode = SmaImpl(tempBuffer2, Range.EndAt(bufferSize - 1), outRealLowerBand, out dummyRange, optInTimePeriod);
        if (retCode != Core.RetCode.Success || dummyRange.End.Value - dummyRange.Start.Value != outputSize)
        {
            return retCode; // Ошибка при сглаживании нижней полосы
        }

        outRange = new Range(startIdx, startIdx + outputSize); // Установка диапазона валидных данных
        return Core.RetCode.Success;
    }
}
