/*
 * Technical Analysis Library for .NET
 * Copyright (c) 2020-2024 Anatolii Siryi
 *
 * This file is part of Technical Analysis Library for .NET.
 *
 * Technical Analysis Library for .NET is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Technical Analysis Library for .NET is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Technical Analysis Library for .NET. If not, see <https://www.gnu.org/licenses/>.
 */

namespace TALib;

public static partial class Functions
{
    [PublicAPI]
    public static Core.RetCode LinearRegIntercept<T>(
        ReadOnlySpan<T> inReal,
        int startIdx,
        int endIdx,
        Span<T> outReal,
        out int outBegIdx,
        out int outNbElement,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegInterceptImpl(inReal, startIdx, endIdx, outReal, out outBegIdx, out outNbElement, optInTimePeriod);

    [PublicAPI]
    public static int LinearRegInterceptLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode LinearRegIntercept<T>(
        T[] inReal,
        int startIdx,
        int endIdx,
        T[] outReal,
        out int outBegIdx,
        out int outNbElement,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegInterceptImpl<T>(inReal, startIdx, endIdx, outReal, out outBegIdx, out outNbElement, optInTimePeriod);

    private static Core.RetCode LinearRegInterceptImpl<T>(
        ReadOnlySpan<T> inReal,
        int startIdx,
        int endIdx,
        Span<T> outReal,
        out int outBegIdx,
        out int outNbElement,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx || endIdx >= inReal.Length)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Linear Regression is a concept also known as the "least squares method" or "best fit."
         * Linear Regression attempts to fit a straight line between several data points in such a way that
         * distance between each data point and the line is minimized.
         *
         * For each point, a straight line over the specified previous bar period is determined in terms of y = b + m * x:
         *
         * Returns 'b'
         */

        var lookbackTotal = LinearRegInterceptLookback(optInTimePeriod);
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        int outIdx = default;
        var today = startIdx;

        var timePeriod = T.CreateChecked(optInTimePeriod);
        var sumX = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * 0.5);
        var sumXSqr = T.CreateChecked(optInTimePeriod * (optInTimePeriod - 1) * (optInTimePeriod * 2 - 1) / 6.0);
        var divisor = sumX * sumX - timePeriod * sumXSqr;
        while (today <= endIdx)
        {
            var sumXY = T.Zero;
            var sumY = T.Zero;
            for (var i = optInTimePeriod; i-- != 0;)
            {
                var tempValue1 = inReal[today - i];
                sumY += tempValue1;
                sumXY += T.CreateChecked(i) * tempValue1;
            }

            var m = (timePeriod * sumXY - sumX * sumY) / divisor;
            outReal[outIdx++] = (sumY - m * sumX) / timePeriod;
            today++;
        }

        outBegIdx = startIdx;
        outNbElement = outIdx;

        return Core.RetCode.Success;
    }
}
