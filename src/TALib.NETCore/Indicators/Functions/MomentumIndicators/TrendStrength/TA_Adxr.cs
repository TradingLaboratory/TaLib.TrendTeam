//Файл: TA_Adxr.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//DirectionalMovement (альтернатива для группировки индикаторов направленного движения: ADX, ADXR, DI+, DI-)
//TrendStrength (альтернатива для акцента на измерении силы тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Average Directional Movement Index Rating (Momentum Indicators) — Индекс рейтинга среднего направленного движения (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High) для расчета индикатора.</param>
    /// <param name="inLow">Массив минимальных цен (Low) для расчета индикатора.</param>
    /// <param name="inClose">Массив цен закрытия (Close) для расчета индикатора.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора ADXR.  
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
    /// <param name="optInTimePeriod">Период времени для расчета ADX и последующего ADXR (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индекс рейтинга среднего направленного движения (ADXR) — это сглаженная версия индикатора <see cref="Adx{T}">ADX</see>,
    /// используемая для измерения силы тренда. Он рассчитывается как среднее арифметическое текущего значения ADX и значения ADX,
    /// рассчитанного <paramref name="optInTimePeriod"/> периодов назад:
    /// <code>
    /// ADXR[t] = (ADX[t] + ADX[t - optInTimePeriod]) / 2
    /// </code>
    /// </para>
    /// <para>
    /// ADXR дополнительно сглаживает ADX, делая его менее чувствительным к краткосрочным колебаниям и более подходящим для анализа долгосрочных трендов.
    /// Как и ADX, индикатор не указывает направление тренда (вверх/вниз), а измеряет только его силу.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать значения базового индикатора ADX за указанный период <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого бара усреднить текущее значение ADX со значением ADX, рассчитанным <paramref name="optInTimePeriod"/> периодов ранее.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения выше 25 указывают на сильный тренд (восходящий или нисходящий).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения ниже 20 свидетельствуют о слабом тренде или боковом движении (флэте).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рост значения ADXR подтверждает усиление тренда, снижение — его ослабление.
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
    /// Возвращает период обратного просмотра (lookback period) для индикатора <see cref="Adxr{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета ADX/ADXR.</param>
    /// <returns>
    /// Количество баров, необходимых до появления первого валидного значения ADXR.
    /// Рассчитывается как: <c>optInTimePeriod + AdxLookback(optInTimePeriod) - 1</c>.
    /// Возвращает -1, если <paramref name="optInTimePeriod"/> меньше 2.
    /// </returns>
    [PublicAPI]
    public static int AdxrLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + AdxLookback(optInTimePeriod) - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span)
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

        // Проверка корректности входного диапазона и длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inClose.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимально допустимого периода (должен быть >= 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет общего периода обратного просмотра для ADXR
        var lookbackTotal = AdxrLookback(optInTimePeriod);
        // Смещение начального индекса с учетом необходимого количества исторических данных
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если недостаточно данных для расчета хотя бы одного значения
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Временный массив для хранения промежуточных значений ADX
        // Размер массива включает дополнительные optInTimePeriod элементов для расчета сдвига
        Span<T> adx = new T[endIdx - startIdx + optInTimePeriod];

        // Расчет базового индикатора ADX с расширенным диапазоном (включая данные для сдвига)
        var retCode = AdxImpl(inHigh, inLow, inClose, new Range(startIdx - (optInTimePeriod - 1), endIdx), adx, out outRange,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Индекс в массиве adx для текущего значения ADX
        var i = optInTimePeriod - 1;
        // Индекс в массиве adx для значения ADX из предыдущего периода (сдвинутого на optInTimePeriod)
        var j = 0;
        // Индекс записи результата в выходной массив outReal
        var outIdx = 0;
        // Количество элементов для обработки (включая текущий бар)
        var nbElement = endIdx - startIdx + 2;
        // Расчет ADXR как среднего между текущим ADX и ADX из предыдущего периода
        while (--nbElement != 0)
        {
            outReal[outIdx++] = (adx[i++] + adx[j++]) / FunctionHelpers.Two<T>();
        }

        // Установка диапазона валидных выходных значений
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
