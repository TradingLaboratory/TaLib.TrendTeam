//Файл: TA_Adxr.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendStrength (альтернатива для акцента на силе тренда)
//AverageDirectionalMovement (альтернатива для акцента на направлении движения)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Average Directional Movement Index Rating (Momentum Indicators) — Средний индекс направленного движения (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входные данные максимальных цен.</param>
    /// <param name="inLow">Входные данные минимальных цен.</param>
    /// <param name="inClose">Входные данные закрывающих цен.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Средний индекс направленного движения (ADXR) — это сглаженная версия индикатора <see cref="Adx{T}">ADX</see>,
    /// используемая для измерения силы тренда. Он рассчитывается как среднее значение текущего значения ADX и значения ADX из предыдущего периода.
    /// ADXR дополнительно сглаживает ADX, делая его менее волатильным и более легким для интерпретации.
    /// <para>
    /// Функция часто используется в трендовых стратегиях для подтверждения силы тренда и фильтрации периодов низкой волатильности.
    /// Она менее чувствительна к краткосрочным колебаниям цен, что делает её особенно полезной для выявления долгосрочных трендов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать значения ADX за указанный период времени. Подробности расчета смотрите в функции ADX.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого значения ADX усреднить его с значением ADX из предыдущего периода (смещенного на период времени):
    ///       <code>
    ///         ADXR = (Текущее ADX + ADX из предыдущего периода) / 2
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения выше 25 указывают на сильный тренд, тогда как значения ниже 20 свидетельствуют о слабом или отсутствующем тренде.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рост значения указывает на усиление тренда, а снижение — на его ослабление.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Как и ADX, ADXR не указывает направление тренда, а измеряет его силу.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Adxr<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AdxrImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Adxr{T}">Adxr</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    [PublicAPI]
    public static int AdxrLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + AdxLookback(optInTimePeriod) - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Adxr<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AdxrImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode AdxrImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inClose.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = AdxrLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        Span<T> adx = new T[endIdx - startIdx + optInTimePeriod];

        var retCode = AdxImpl(inHigh, inLow, inClose, new Range(startIdx - (optInTimePeriod - 1), endIdx), adx, out outRange,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var i = optInTimePeriod - 1;
        var j = 0;
        var outIdx = 0;
        var nbElement = endIdx - startIdx + 2;
        while (--nbElement != 0)
        {
            outReal[outIdx++] = (adx[i++] + adx[j++]) / FunctionHelpers.Two<T>();
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
