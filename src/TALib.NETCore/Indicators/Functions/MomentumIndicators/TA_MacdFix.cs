//Название файла: TA_MacdFix.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//SignalIndicators (альтернатива для акцента на сигнальных линиях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Moving Average Convergence/Divergence Fix 12/26 (Momentum Indicators) — Сходимость/расходимость скользящих средних Fix 12/26 (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMACD">Массив, содержащий ТОЛЬКО валидные значения линии MACD.</param>
    /// <param name="outMACDSignal">Массив, содержащий ТОЛЬКО валидные значения сигнальной линии MACD.</param>
    /// <param name="outMACDHist">Массив, содержащий ТОЛЬКО валидные значения гистограммы MACD.</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Сходимость/расходимость скользящих средних Fix — это упрощенная версия индикатора MACD
    /// с фиксированными быстрым и медленным периодами (12 и 26 соответственно). Эта версия фокусируется на расчете сигнальной линии
    /// с настраиваемым периодом для гибкости в определении смены импульса.
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить быструю экспоненциальную скользящую среднюю (EMA) входных значений с периодом 12.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить медленную EMA входных значений с периодом 26.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать линию MACD как разницу между быстрой EMA и медленной EMA:
    ///       <code>
    ///         MACD = EMA(FastPeriod) - EMA(SlowPeriod)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить сигнальную линию как EMA линии MACD с использованием указанного <paramref name="optInSignalPeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать гистограмму MACD как разницу между линией MACD и сигнальной линией:
    ///       <code>
    ///         MACDHist = MACD - Signal
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение линии MACD указывает на восходящий импульс, отрицательное — на нисходящий.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сигнальная линия используется для определения потенциальных сигналов на покупку или продажу: бычий перекрест происходит, когда линия MACD
    ///       пересекает сигнальную линию снизу вверх, медвежий перекрест — когда линия MACD пересекает сигнальную линию сверху вниз.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Гистограмма MACD отражает силу импульса: большие столбцы указывают на сильный импульс в направлении линии MACD, уменьшающиеся столбцы могут сигнализировать о возможном развороте или ослаблении импульса.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MacdFix<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdFixImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInSignalPeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="MacdFix{T}">MacdFix</see>.
    /// </summary>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии.</param>
    /// <returns>Количество периодов, необходимых до расчета первого допустимого значения.</returns>
    [PublicAPI]
    public static int MacdFixLookback(int optInSignalPeriod = 9) => EmaLookback(26) + EmaLookback(optInSignalPeriod);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MacdFix<T>(
        T[] inReal,
        Range inRange,
        T[] outMACD,
        T[] outMACDSignal,
        T[] outMACDHist,
        out Range outRange,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdFixImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInSignalPeriod);

    private static Core.RetCode MacdFixImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInSignalPeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        if (optInSignalPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        return FunctionHelpers.CalcMACD(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outMACD, outMACDSignal,
            outMACDHist, out outRange,
            0, /* 0 указывает на фиксированный период 12 == 0.15 для optInFastPeriod */
            0, /* 0 указывает на фиксированный период 26 == 0.075 для optInSlowPeriod */
            optInSignalPeriod);
    }
}
