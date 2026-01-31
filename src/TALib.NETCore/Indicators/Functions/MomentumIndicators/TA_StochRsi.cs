// StochRsi.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// OscillatorIndicators (альтернатива для группировки осцилляторов)
// RsiBasedIndicators (альтернатива для индикаторов, основанных на RSI)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Stochastic Relative Strength Index (Momentum Indicators) — Стохастический индекс относительной силы (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены закрытия <c>Close</c>)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outFastK">Массив для хранения рассчитанных значений линии быстрого %K (не сглаженная версия)</param>
    /// <param name="outFastD">Массив для хранения рассчитанных значений линии быстрого %D (сглаженная линия %K)</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в выходных массивах.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в выходных массивах.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период для расчета базового индекса относительной силы (RSI)</param>
    /// <param name="optInFastKPeriod">Период для расчета линии быстрого %K (поиск экстремумов RSI)</param>
    /// <param name="optInFastDPeriod">Период сглаживания линии %K для получения линии %D</param>
    /// <param name="optInFastDMAType">Тип скользящей средней, используемой для сглаживания линии %D</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успешность расчета.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Стохастический индекс относительной силы (StochRSI) — это осциллятор импульса, объединяющий 
    /// стохастический осциллятор (<see cref="Stoch{T}">Stoch</see>) и индекс относительной силы 
    /// (<see cref="Rsi{T}">RSI</see>) для определения условий перекупленности и перепроданности актива.
    /// В отличие от стандартного RSI, StochRSI обеспечивает большую чувствительность и детализацию 
    /// за счет применения формулы стохастического осциллятора к значениям RSI вместо исходных цен.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет значений RSI за указанный период <paramref name="optInTimePeriod"/>:
    ///       <code>
    ///         RSI = 100 - (100 / (1 + RS))
    ///       </code>
    ///       где <c>RS</c> — отношение среднего прироста к среднему убытку за период <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление линии быстрого %K с использованием формулы стохастического осциллятора, примененной к значениям RSI:
    ///       <code>
    ///         %K = 100 * ((RSI - LowestRSI) / (HighestRSI - LowestRSI))
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item>
    ///           <description>
    ///             <b>LowestRSI</b> — минимальное значение RSI за период <paramref name="optInFastKPeriod"/>.
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             <b>HighestRSI</b> — максимальное значение RSI за период <paramref name="optInFastKPeriod"/>.
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сглаживание линии быстрого %K за период <paramref name="optInFastDPeriod"/> с использованием 
    ///       указанного типа скользящей средней <paramref name="optInFastDMAType"/> для получения линии %D.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения около 0 указывают, что RSI находится на минимальных уровнях за период поиска экстремумов, 
    ///       что сигнализирует об условиях перепроданности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения около 100 указывают, что RSI находится на максимальных уровнях за период поиска экстремумов, 
    ///       что сигнализирует об условиях перекупленности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечения линий %K и %D используются как потенциальные торговые сигналы:
    ///       <list type="bullet">
    ///         <item>
    ///           <description>
    ///             Пересечение линии %K сверху вниз через линию %D может сигнализировать о возможности продажи.
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             Пересечение линии %K снизу вверх через линию %D может сигнализировать о возможности покупки.
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       StochRSI обеспечивает большую чувствительность по сравнению с RSI, но может генерировать 
    ///       больше ложных сигналов из-за повышенной волатильности.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode StochRsi<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outFastK,
        Span<T> outFastD,
        out Range outRange,
        int optInTimePeriod = 14,
        int optInFastKPeriod = 5,
        int optInFastDPeriod = 3,
        Core.MAType optInFastDMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        StochRsiImpl(inReal, inRange, outFastK, outFastD, out outRange, optInTimePeriod, optInFastKPeriod, optInFastDPeriod,
            optInFastDMAType);

    /// <summary>
    /// Возвращает период задержки (lookback) для индикатора <see cref="StochRsi{T}">StochRsi</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период для расчета базового индекса относительной силы (RSI)</param>
    /// <param name="optInFastKPeriod">Период для расчета линии быстрого %K</param>
    /// <param name="optInFastDPeriod">Период сглаживания линии %K для получения линии %D</param>
    /// <param name="optInFastDMAType">Тип скользящей средней, используемой для сглаживания линии %D</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного выходного значения.</returns>
    [PublicAPI]
    public static int StochRsiLookback(
        int optInTimePeriod = 14,
        int optInFastKPeriod = 5,
        int optInFastDPeriod = 3,
        Core.MAType optInFastDMAType = Core.MAType.Sma) =>
        optInTimePeriod < 2 || optInFastKPeriod < 1 || optInFastDPeriod < 1
            ? -1
            : RsiLookback(optInTimePeriod) + StochFLookback(optInFastKPeriod, optInFastDPeriod, optInFastDMAType);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode StochRsi<T>(
        T[] inReal,
        Range inRange,
        T[] outFastK,
        T[] outFastD,
        out Range outRange,
        int optInTimePeriod = 14,
        int optInFastKPeriod = 5,
        int optInFastDPeriod = 3,
        Core.MAType optInFastDMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        StochRsiImpl<T>(inReal, inRange, outFastK, outFastD, out outRange, optInTimePeriod, optInFastKPeriod, optInFastDPeriod,
            optInFastDMAType);

    private static Core.RetCode StochRsiImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outFastK,
        Span<T> outFastD,
        out Range outRange,
        int optInTimePeriod,
        int optInFastKPeriod,
        int optInFastDPeriod,
        Core.MAType optInFastDMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности входных параметров периода
        if (optInTimePeriod < 2 || optInFastKPeriod < 1 || optInFastDPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Ссылка: "Stochastic RSI and Dynamic Momentum Index"
         *         Tushar Chande и Stanley Kroll
         *         Stock&Commodities V.11:5 (189-199)
         *
         * Реализация предоставляет гибкость, выходящую за рамки описания в статье.
         *
         * Для расчета "несглаженного стохастического RSI" с симметрией, как описано в статье,
         * установите равными параметры optInTimePeriod и optInFastKPeriod. Пример:
         *
         *   несглаженный стохастический RSI 14: optInTimePeriod   = 14
         *                                        optInFastKPeriod = 14
         *                                        optInFastDPeriod = 'x'
         *
         * Массив outFastK будет содержать несглаженный стохастический RSI, описанный в статье.
         *
         * Параметр optInFastDPeriod можно использовать для сглаживания стохастического RSI.
         * Сглаженная версия будет находиться в outFastD, при этом outFastK по-прежнему
         * будет содержать несглаженный стохастический RSI.
         * Если сглаживание не требуется, установите optInFastDPeriod = 1 и игнорируйте outFastD.
         */

        // Период задержки для расчета стохастического осциллятора (%K и %D)
        var lookbackStochF = StochFLookback(optInFastKPeriod, optInFastDPeriod, optInFastDMAType);

        // Общий период задержки (сумма периодов для RSI и стохастического осциллятора)
        var lookbackTotal = StochRsiLookback(optInTimePeriod, optInFastKPeriod, optInFastDPeriod, optInFastDMAType);

        // Корректировка начального индекса с учетом периода задержки
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — валидных данных нет
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс первого валидного значения в выходных данных
        var outBegIdx = startIdx;

        // Размер временного буфера для хранения промежуточных значений RSI
        var tempArraySize = endIdx - startIdx + 1 + lookbackStochF;
        Span<T> tempRsiBuffer = new T[tempArraySize];

        // Расчет базового индекса относительной силы (RSI) для входных данных
        var retCode = RsiImpl(inReal, new Range(startIdx - lookbackStochF, endIdx), tempRsiBuffer, out var outRange1, optInTimePeriod);
        if (retCode != Core.RetCode.Success || outRange1.End.Value == 0)
        {
            return retCode;
        }

        // Применение стохастического осциллятора к значениям RSI для получения %K и %D
        retCode = StochFImpl(tempRsiBuffer, tempRsiBuffer, tempRsiBuffer, Range.EndAt(tempArraySize - 1), outFastK, outFastD, out outRange,
            optInFastKPeriod, optInFastDPeriod, optInFastDMAType);
        if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
        {
            return retCode;
        }

        // Корректировка выходного диапазона для соответствия исходным входным данным
        outRange = new Range(outBegIdx, outBegIdx + outRange.End.Value - outRange.Start.Value);

        return Core.RetCode.Success;
    }
}
