//Название файла: TA_Apo.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//PriceOscillators (альтернатива для акцента на осцилляторах цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Absolute Price Oscillator (Momentum Indicators) — Абсолютный ценовой осциллятор (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
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
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней.</param>
    /// <param name="optInMAType">Тип скользящей средней.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Абсолютный ценовой осциллятор — это индикатор импульса, который рассчитывает разницу между двумя скользящими средними входного ряда,
    /// обычно цены. Он используется для выявления трендов, импульса и потенциальных разворотов в ценовых движениях.
    /// <para>
    /// Функция может помочь выявить ранние признаки разворота тренда.
    /// Её можно комбинировать с объемными индикаторами или мерами волатильности для дополнительного подтверждения.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать быструю скользящую среднюю входного ряда с использованием указанного быстрого периода и типа скользящей средней.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать медленную скользящую среднюю входного ряда с использованием указанного медленного периода и типа скользящей средней.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычесть медленную скользящую среднюю из быстрой скользящей средней для вычисления APO:
    ///       <code>
    ///         APO = Fast MA - Slow MA
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение указывает, что быстрая скользящая средняя выше медленной скользящей средней, что свидетельствует о восходящем импульсе.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательное значение указывает, что быстрая скользящая средняя ниже медленной скользящей средней, что свидетельствует о нисходящем импульсе.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечения APO выше или ниже нулевой линии могут сигнализировать о потенциальных возможностях для покупки или продажи соответственно.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Apo<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        ApoImpl(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Apo{T}">Apo</see>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней.</param>
    /// <param name="optInMAType">Тип скользящей средней.</param>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    [PublicAPI]
    public static int ApoLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : MaLookback(Math.Max(optInSlowPeriod, optInFastPeriod), optInMAType);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Apo<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        ApoImpl<T>(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

    private static Core.RetCode ApoImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInFastPeriod < 2 || optInSlowPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Временный буфер для хранения промежуточных значений
        Span<T> tempBuffer = new T[endIdx - startIdx + 1];

        return FunctionHelpers.CalcPriceOscillator(inReal, new Range(startIdx, endIdx), outReal, out outRange, optInFastPeriod,
            optInSlowPeriod, optInMAType, tempBuffer, false);
    }
}
