//Название файла: TA_Macd.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendFollowing (альтернатива, если требуется группировка по типу индикатора)
//ConvergenceDivergence (альтернатива для акцента на сходимости и расходимости)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Moving Average Convergence/Divergence (Momentum Indicators) — Сходимость/расходимость скользящих средних (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMACD">Массив, содержащий ТОЛЬКО валидные значения линии MACD.</param>
    /// <param name="outMACDSignal">Массив, содержащий ТОЛЬКО валидные значения линии Signal.</param>
    /// <param name="outMACDHist">Массив, содержащий ТОЛЬКО валидные значения гистограммы MACD.</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней.</param>
    /// <param name="optInSignalPeriod">Период для расчета линии Signal.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Сходимость/расходимость скользящих средних (MACD) — это трендовый импульсный индикатор, показывающий отношение
    /// между двумя скользящими средними цены инструмента. Он сравнивает две EMA для выявления смены импульса и потенциальных разворотов тренда.
    /// <para>
    /// Функция широко признана и часто используется с <see cref="Rsi{T}">RSI</see> или <see cref="Bbands{T}">Bollinger Bands</see>
    /// для подтверждения сигналов. Наблюдение за дивергенциями и пересечениями помогает распознавать изменения на рынке.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать быструю экспоненциальную скользящую среднюю (EMA) входных значений за указанный <c>FastPeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать медленную EMA входных значений за указанный <c>SlowPeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить линию MACD как разность между быстрой EMA и медленной EMA:
    ///       <code>
    ///         MACD = EMA(FastPeriod) - EMA(SlowPeriod)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать линию Signal как EMA линии MACD за указанный <c>SignalPeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить гистограмму MACD как разность между линией MACD и линией Signal:
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
    ///       Линия Signal используется для идентификации потенциальных сигналов на покупку или продажу: бычий пересекающийся сигнал возникает, когда линия MACD
    ///       пересекает линию Signal снизу вверх, медвежий — когда линия MACD пересекает линию Signal сверху вниз.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Гистограмма MACD отражает силу импульса: большие столбцы указывают на сильный импульс в направлении линии MACD, уменьшающиеся столбцы сигнализируют о возможном развороте или ослабевающем импульсе.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Macd<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod, optInSignalPeriod);

    /// <summary>
    /// Возвращает период предыстории для <see cref="Macd{T}">Macd</see>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней.</param>
    /// <param name="optInSignalPeriod">Период для расчета линии Signal.</param>
    /// <returns>Количество периодов, необходимых до расчета первого значения.</returns>
    [PublicAPI]
    public static int MacdLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
    {
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return -1;
        }

        if (optInSlowPeriod < optInFastPeriod)
        {
            optInSlowPeriod = optInFastPeriod;
        }

        return EmaLookback(optInSlowPeriod) + EmaLookback(optInSignalPeriod);
    }

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Macd<T>(
        T[] inReal,
        Range inRange,
        T[] outMACD,
        T[] outMACDSignal,
        T[] outMACDHist,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod,
            optInSignalPeriod);

    private static Core.RetCode MacdImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        int optInSignalPeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        return FunctionHelpers.CalcMACD(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outMACD, outMACDSignal,
            outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod, optInSignalPeriod);
    }
}
