// Название файла: TA_Accbands.cs
// Рекомендуемое размещение:
// Основная папка: OverlapStudies
// Подпапка: BandsChannels (существующая подпапка для индикаторов-каналов и полос)
// Альтернативные категории: VolatilityIndicators (70%), TrendStrength (60%)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Acceleration Bands (Overlap Studies) — Полосы ускорения (Индикаторы перекрытия)
    /// </summary>
    /// <param name="inHigh">Массив входных максимальных цен (High).</param>
    /// <param name="inLow">Массив входных минимальных цен (Low).</param>
    /// <param name="inClose">Массив входных цен закрытия (Close).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outRealUpperBand">Массив для хранения рассчитанных значений верхней полосы индикатора.</param>
    /// <param name="outRealMiddleBand">Массив для хранения рассчитанных значений средней полосы (скользящей средней цены закрытия).</param>
    /// <param name="outRealLowerBand">Массив для хранения рассчитанных значений нижней полосы индикатора.</param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в выходных массивах.  
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение (гарантированно равен последнему индексу входных данных при успешном расчете).  
    /// - Если данных недостаточно для расчета (длина входных данных меньше периода + 1), возвращается диапазон <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчета скользящей средней для сглаживания полос (по умолчанию 20). Минимальное значение: 2.</param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>), 
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код результата из <see cref="Core.RetCode"/>.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете индикатора.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Полосы ускорения (Acceleration Bands) — индикатор волатильности, разработанный Пэтом Дорси, 
    /// предназначенный для определения точек прорыва и оценки силы тренда. Полосы динамически расширяются 
    /// при увеличении волатильности и сужаются при её снижении.
    /// </para>
    /// <para>
    /// Может использоваться совместно с индикаторами импульса или тренда, такими как 
    /// <see cref="Adx{T}">ADX</see> или <see cref="Rsi{T}">RSI</see>, для подтверждения сигналов прорыва.
    /// </para>
    ///
    /// <b>Шаги расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет исходных значений верхней и нижней полос для каждого бара:
    ///       <code>
    ///         Upper Band Raw = High * (1 + 4 * (High - Low) / (High + Low))
    ///         Lower Band Raw = Low * (1 - 4 * (High - Low) / (High + Low))
    ///       </code>
    ///       где коэффициент 4 определяет чувствительность полос к изменению волатильности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет средней полосы как простой скользящей средней (SMA) от цены закрытия:
    ///       <code>
    ///         Middle Band = SMA(Close, Period)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение простой скользящей средней (SMA) с тем же периодом к исходным значениям 
    ///       верхней и нижней полос для получения сглаженных финальных значений.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация сигналов</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Прорыв цены выше верхней полосы — сигнал сильного восходящего импульса и потенциального продолжения тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Прорыв цены ниже нижней полосы — сигнал сильного нисходящего импульса.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Цена, торгующаяся между полосами, указывает на боковое движение или низкую волатильность.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сужение полос (схождение) часто предшествует всплеску волатильности и возможному прорыву.
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
    /// Возвращает период lookback (запаздывания) для индикатора <see cref="Accbands{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета скользящей средней.</param>
    /// <returns>
    /// Количество баров, необходимых для расчета первого валидного значения индикатора.  
    /// Возвращает -1 при некорректном значении периода (меньше 2).
    /// </returns>
    /// <remarks>
    /// Период lookback определяется как период скользящей средней, так как именно он задает 
    /// минимальное количество исторических данных, необходимых для получения первого валидного значения.
    /// </remarks>
    [PublicAPI]
    public static int AccbandsLookback(int optInTimePeriod = 20) => optInTimePeriod < 2 ? -1 : SmaLookback(optInTimePeriod);

    /// <remarks>
    /// Для совместимости с абстрактным API (устаревшая версия для массивов)
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
        outRange = Range.EndAt(0); // Инициализация диапазона выходных данных значением [0, 0)

        // Валидация входного диапазона и проверка согласованности длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam; // Ошибка: некорректный диапазон входных данных
        }
        var (startIdx, endIdx) = rangeIndices; // Индексы начала и конца обработки во входных данных

        // Проверка корректности периода (минимальное значение = 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam; // Ошибка: период меньше минимально допустимого
        }

        // Расчет периода lookback (минимальное количество баров для первого валидного значения)
        var lookbackTotal = AccbandsLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом периода lookback
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Проверка наличия данных для обработки после учета lookback
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success; // Успешное завершение: недостаточно данных для расчета
        }

        // Расчет размеров буферов для промежуточных вычислений
        var outputSize = endIdx - startIdx + 1; // Количество валидных выходных значений
        var bufferSize = outputSize + lookbackTotal; // Размер буфера с учетом периода сглаживания

        // Буферы для хранения несглаженных значений верхней и нижней полос
        Span<T> tempBuffer1 = new T[bufferSize]; // Верхняя полоса до применения SMA
        Span<T> tempBuffer2 = new T[bufferSize]; // Нижняя полоса до применения SMA

        // Расчет исходных (несглаженных) значений полос для всего диапазона, включая период lookback
        for (int j = 0, i = startIdx - lookbackTotal; i <= endIdx; i++, j++)
        {
            var high = inHigh[i]; // Текущее значение High
            var low = inLow[i];   // Текущее значение Low
            var tempReal = high + low; // Сумма High + Low для нормализации

            if (!T.IsZero(tempReal))
            {
                // Расчет коэффициента волатильности: 4 * (High - Low) / (High + Low)
                tempReal = FunctionHelpers.Four<T>() * (high - low) / tempReal;
                // Расчет верхней полосы: High * (1 + коэффициент)
                tempBuffer1[j] = high * (T.One + tempReal);
                // Расчет нижней полосы: Low * (1 - коэффициент)
                tempBuffer2[j] = low * (T.One - tempReal);
            }
            else
            {
                // Обработка вырожденного случая (High + Low = 0)
                tempBuffer1[j] = high;
                tempBuffer2[j] = low;
            }
        }

        // Расчет средней полосы как простой скользящей средней (SMA) от цены закрытия
        var retCode = SmaImpl(inClose, new Range(startIdx, endIdx), outRealMiddleBand, out var dummyRange, optInTimePeriod);
        if (retCode != Core.RetCode.Success || dummyRange.End.Value - dummyRange.Start.Value != outputSize)
        {
            return retCode; // Ошибка при расчете средней полосы
        }

        // Применение SMA к верхней полосе для сглаживания
        retCode = SmaImpl(tempBuffer1, Range.EndAt(bufferSize - 1), outRealUpperBand, out dummyRange, optInTimePeriod);
        if (retCode != Core.RetCode.Success || dummyRange.End.Value - dummyRange.Start.Value != outputSize)
        {
            return retCode; // Ошибка при сглаживании верхней полосы
        }

        // Применение SMA к нижней полосе для сглаживания
        retCode = SmaImpl(tempBuffer2, Range.EndAt(bufferSize - 1), outRealLowerBand, out dummyRange, optInTimePeriod);
        if (retCode != Core.RetCode.Success || dummyRange.End.Value - dummyRange.Start.Value != outputSize)
        {
            return retCode; // Ошибка при сглаживании нижней полосы
        }

        // Установка диапазона валидных выходных данных
        outRange = new Range(startIdx, startIdx + outputSize);
        return Core.RetCode.Success;
    }
}
