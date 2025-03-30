// файл TA_Roc.cs

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Rate of change : ((price/prevPrice)-1)*100 (Momentum Indicators)
    /// </summary>
    /// <param name="inReal">A span of input values.</param>
    /// <param name="inRange">The range of indices that determines the portion of data to be calculated within the input spans.</param>
    /// <param name="outReal">A span to store the calculated values.</param>
    /// <param name="outRange">The range of indices representing the valid data within the output spans.</param>
    /// <param name="optInTimePeriod">The time period.</param>
    /// <typeparam name="T">
    /// The numeric data type, typically <see langword="float"/> or <see langword="double"/>,
    /// implementing the <see cref="IFloatingPointIeee754{T}"/> interface.
    /// </typeparam>
    /// <returns>
    /// A <see cref="Core.RetCode"/> value indicating the success or failure of the calculation.
    /// Returns <see cref="Core.RetCode.Success"/> on successful calculation, or an appropriate error code otherwise.
    /// </returns>
    /// <remarks>
    /// Rate of Change function calculates the percentage change of a time series value over a specified period.
    /// It is commonly used as a momentum indicator to evaluate the rate at which price values are changing over time.
    /// <para>
    /// The function is similar to <see cref="Ppo{T}">PPO</see>, but the PPO compares moving averages rather than raw values.
    /// Used to identify overbought or oversold conditions in financial markets.
    /// Verifying ROC findings with trend or volatility tools can reduce the likelihood of short-lived fluctuations.
    /// </para>
    ///
    /// <b>Calculation steps</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Identify the value from <paramref name="optInTimePeriod"/> periods ago:
    ///       <code>
    ///         PreviousValue = data[currentIndex - optInTimePeriod]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Calculate the ROC as the percentage change from the previous value to the current value:
    ///       <code>
    ///         ROC = ((CurrentValue / PreviousValue) - 1) * 100
    ///       </code>
    ///       where <c>CurrentValue</c> is the value at the current position, and <c>PreviousValue</c>
    ///       is the value <paramref name="optInTimePeriod"/> steps earlier.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Value interpretation</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Positive values indicate upward momentum, suggesting prices are rising compared to the past.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Negative values indicate downward momentum, suggesting prices are falling compared to the past.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       A value close to zero indicates little or no change in value over the specified period.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Roc<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Returns the lookback period for <see cref="Roc{T}">Roc</see>.
    /// </summary>
    /// <param name="optInTimePeriod">The time period.</param>
    /// <returns>The number of periods required before the first output value can be calculated.</returns>
    [PublicAPI]
    public static int RocLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Roc<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode RocImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Roc and RocP are centered at zero and can have positive and negative value. Here are some equivalence:
         *   ROC = ROCP/100
         *       = ((price - prevPrice) / prevPrice) / 100
         *       = ((price / prevPrice) - 1) * 100
         *
         * RocR and RocR100 are ratio respectively centered at 1 and 100 and are always positive values.
         */

        var lookbackTotal = RocLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var inIdx = startIdx;
        var trailingIdx = startIdx - lookbackTotal;
        while (inIdx <= endIdx)
        {
            var tempReal = inReal[trailingIdx++];
            outReal[outIdx++] = !T.IsZero(tempReal) ? (inReal[inIdx] / tempReal - T.One) * FunctionHelpers.Hundred<T>() : T.Zero;
            inIdx++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
