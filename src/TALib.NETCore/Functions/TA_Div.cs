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
    /// <summary>
    /// Vector Arithmetic Division (Math Operators)
    /// </summary>
    /// <param name="inReal0">The first span of input values.</param>
    /// <param name="inReal1">The second span of input values.</param>
    /// <param name="inRange">The range of indices that determines the portion of data to be calculated within the input spans.</param>
    /// <param name="outReal">A span to store the calculated values.</param>
    /// <param name="outRange">The range of indices representing the valid data within the output spans.</param>
    /// <typeparam name="T">
    /// The numeric data type, typically <see langword="float"/> or <see langword="double"/>,
    /// implementing the <see cref="IFloatingPointIeee754{T}"/> interface.
    /// </typeparam>
    /// <returns>
    /// A <see cref="Core.RetCode"/> value indicating the success or failure of the calculation.
    /// Returns <see cref="Core.RetCode.Success"/> on successful calculation, or an appropriate error code otherwise.
    /// </returns>
    /// <remarks>
    /// The function divides one data series by another element-by-element, useful for creating ratios or normalizing indicators.
    /// <para>
    /// The function is a building block for custom indicators or ratio-based comparisons between instruments.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Div<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        DivImpl(inReal0, inReal1, inRange, outReal, out outRange);

    /// <summary>
    /// Returns the lookback period for <see cref="Div{T}">Div</see>.
    /// </summary>
    /// <returns>Always 0 since no historical data is required for this calculation.</returns>
    [PublicAPI]
    public static int DivLookback() => 0;

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Div<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        DivImpl<T>(inReal0, inReal1, inRange, outReal, out outRange);

    private static Core.RetCode DivImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = inReal0[i] / inReal1[i];
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
