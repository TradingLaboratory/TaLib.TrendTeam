//Название файла: TA_Cci.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//CommodityChannelIndex (альтернатива для акцента на специфике индикатора)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Commodity Channel Index (Momentum Indicators) — Индекс Товарного Канала (Индикаторы Импульса)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены (High).</param>
    /// <param name="inLow">Входные минимальные цены (Low).</param>
    /// <param name="inClose">Входные цены закрытия (Close).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>, <c>inLow[outRange.Start + i]</c> и <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Индекс Товарного Канала (CCI) — это импульсный индикатор, который измеряет отклонение типичной цены
    /// (среднее значение максимальной, минимальной и закрывающей цены) от её скользящего среднего за указанный период времени.
    /// <para>
    /// Он часто используется для определения перекупленности и перепроданности, а также потенциальных разворотов тренда.
    /// Комбинирование его с трендовыми индикаторами может помочь избежать реакции против сильных существующих трендов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить Типичную Цену (TP) для каждого периода:
    ///       <code>
    ///         TP = (High + Low + Close) / 3
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать скользящее среднее (MA) Типичной Цены за указанный период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить Среднее Отклонение Типичной Цены от скользящего среднего.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать CCI:
    ///       <code>
    ///         CCI = (TP - MA) / (0.015 * Среднее Отклонение)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение указывает на то, что цена выше скользящего среднего, что свидетельствует о бычьем импульсе.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательное значение указывает на то, что цена ниже скользящего среднего, что свидетельствует о медвежьем импульсе.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения выше +100 или ниже -100 обычно используются как пороги для перекупленности или перепроданности.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Cci<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CciImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Cci{T}">Cci</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int CciLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Cci<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CciImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode CciImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = CciLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделяем циклический буфер, равный запрашиваемому периоду.
        Span<T> circBuffer = new T[optInTimePeriod];
        var circBufferIdx = 0;
        var maxIdxCircBuffer = optInTimePeriod - 1;

        // Выполняем расчет скользящего среднего с использованием оптимизированных циклов.

        // Суммируем начальный период, за исключением последнего значения. Заполняем циклический буфер одновременно.
        var i = startIdx - lookbackTotal;
        while (i < startIdx)
        {
            circBuffer[circBufferIdx++] = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
            i++;
            if (circBufferIdx > maxIdxCircBuffer)
            {
                circBufferIdx = 0;
            }
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);
        var tPointZeroOneFive = T.CreateChecked(0.015);

        // Продолжаем расчет для запрашиваемого диапазона.
        // Алгоритм позволяет использовать один и тот же буфер для входных и выходных данных.
        var outIdx = 0;
        do
        {
            var lastValue = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
            circBuffer[circBufferIdx++] = lastValue;

            // Рассчитываем среднее значение для всего периода.
            var theAverage = CalcAverage(circBuffer, timePeriod);

            // Суммируем абсолютные отклонения типичной цены от среднего значения для всего периода.
            var tempReal2 = CalcSummation(circBuffer, theAverage);

            var tempReal = lastValue - theAverage;
            var denominator = tPointZeroOneFive * (tempReal2 / timePeriod);
            outReal[outIdx++] = !T.IsZero(tempReal) && !T.IsZero(denominator) ? tempReal / denominator : T.Zero;

            // Перемещаем индексы циклического буфера.
            if (circBufferIdx > maxIdxCircBuffer)
            {
                circBufferIdx = 0;
            }

            i++;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T CalcAverage<T>(Span<T> circBuffer, T timePeriod) where T : IFloatingPointIeee754<T>
    {
        var theAverage = T.Zero;
        foreach (var t in circBuffer)
        {
            theAverage += t;
        }

        theAverage /= timePeriod;
        return theAverage;
    }

    private static T CalcSummation<T>(Span<T> circBuffer, T theAverage) where T : IFloatingPointIeee754<T>
    {
        var tempReal2 = T.Zero;
        foreach (var t in circBuffer)
        {
            tempReal2 += T.Abs(t - theAverage);
        }

        return tempReal2;
    }
}
