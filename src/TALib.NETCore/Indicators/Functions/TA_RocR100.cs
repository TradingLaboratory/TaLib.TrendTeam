//Название файла: TA_RocR100

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Rate of change ratio 100 scale: (price/prevPrice)*100 (Momentum Indicators)
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
    /// Rate of Change Ratio (100 Scale) is a momentum indicator that measures the ratio between the current price and a price from
    /// a specified number of periods in the past, scaled to 100. It provides a normalized representation of relative changes
    /// over time and is always positive, centered around 100, which indicates no change.
    /// <para>
    /// The function is particularly useful for analyzing proportional relationships in a format that is easier to interpret,
    /// especially when comparing data across different scales or units. The function facilitates intuitive momentum interpretation.
    /// Pairing it with oscillators or volume measures can refine timing and clarity.
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
    ///       Calculate the ROCR100 as the scaled ratio of the current value to the previous value:
    ///       <code>
    ///         ROCR100 = (CurrentValue / PreviousValue) * 100
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
    ///       A value greater than 100 indicates upward momentum, suggesting the current value is higher than the past value.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       A value less than 100 indicates downward momentum, suggesting the current value is lower than the past value.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       A value of exactly 100 indicates no change in value over the specified time period.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode RocR100<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocR100Impl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Returns the lookback period for <see cref="RocR100{T}">RocR100</see>.
    /// </summary>
    /// <param name="optInTimePeriod">The time period.</param>
    /// <returns>The number of periods required before the first output value can be calculated.</returns>
    [PublicAPI]
    public static int RocR100Lookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode RocR100<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocR100Impl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode RocR100Impl<T>(
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

        var lookbackTotal = RocR100Lookback(optInTimePeriod);
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
            outReal[outIdx++] = !T.IsZero(tempReal) ? inReal[inIdx] / tempReal * FunctionHelpers.Hundred<T>() : T.Zero;
            inIdx++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
