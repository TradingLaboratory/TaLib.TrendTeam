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
    public static Core.RetCode Dx<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        DxImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    [PublicAPI]
    public static int DxLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Dx);

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Dx<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        DxImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode DxImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* The DM1 (one period) is based on the largest part of today's range that is outside of yesterday's range.
         *
         * The following 7 cases explain how the +DM and -DM are calculated on one period:
         *
         * Case 1:                       Case 2:
         *    C│                        A│
         *     │                         │ C│
         *     │ +DM1 = (C-A)           B│  │ +DM1 = 0
         *     │ -DM1 = 0                   │ -DM1 = (B-D)
         * A│  │                           D│
         *  │ D│
         * B│
         *
         * Case 3:                       Case 4:
         *    C│                           C│
         *     │                        A│  │
         *     │ +DM1 = (C-A)            │  │ +DM1 = 0
         *     │ -DM1 = 0               B│  │ -DM1 = (B-D)
         * A│  │                            │
         *  │  │                           D│
         * B│  │
         *    D│
         *
         * Case 5:                      Case 6:
         * A│                           A│ C│
         *  │ C│ +DM1 = 0                │  │  +DM1 = 0
         *  │  │ -DM1 = 0                │  │  -DM1 = 0
         *  │ D│                         │  │
         * B│                           B│ D│
         *
         *
         * Case 7:
         *
         *    C│
         * A│  │
         *  │  │ +DM=0
         * B│  │ -DM=0
         *    D│
         *
         * In case 3 and 4, the rule is that the smallest delta between (C-A) and (B-D) determine which of +DM or -DM is zero.
         *
         * In case 7, (C-A) and (B-D) are equal, so both +DM and -DM are zero.
         *
         * The rules remain the same when A=B and C=D (when the highs equal the lows).
         *
         * When calculating the DM over a period > 1, the one-period DM for the desired period are initially summed.
         * In other words, for a -DM14, sum the -DM1 for the first 14 days
         * (that's 13 values because there is no DM for the first day!)
         * Subsequent DM are calculated using Wilder's smoothing approach:
         *
         *                                     Previous -DM14
         *   Today's -DM14 = Previous -DM14 -  ────────────── + Today's -DM1
         *                                           14
         *
         * Calculation of a -DI14 is as follow:
         *
         *             -DM14
         *   -DI14 =  ────────
         *              TR14
         *
         * Calculation of the TR14 is:
         *
         *                                  Previous TR14
         *   Today's TR14 = Previous TR14 - ───────────── + Today's TR1
         *                                        14
         *
         *   The first TR14 is the summation of the first 14 TR1. See the TRange function on how to calculate the true range.
         *
         * Calculation of the DX14 is:
         *
         *   diffDI = ABS((-DI14) - (+DI14))
         *   sumDI  = (-DI14) + (+DI14)
         *
         *   DX14 = 100 * (diffDI / sumDI)
         *
         * Reference:
         *    New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        var lookbackTotal = DxLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);
        T prevMinusDM, prevPlusDM;
        var prevTR = prevMinusDM = prevPlusDM = T.Zero;
        var today = startIdx - lookbackTotal;

        FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
            ref prevPlusDM, ref prevMinusDM, ref prevTR);

        // Skip the unstable period. This loop must be executed at least ONCE to calculate the first DI.
        SkipDxUnstablePeriod(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM, ref prevMinusDM,
            ref prevTR, timePeriod);

        if (!T.IsZero(prevTR))
        {
            var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
            T tempReal = minusDI + plusDI;
            outReal[0] = !T.IsZero(tempReal) ? FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal) : T.Zero;
        }
        else
        {
            outReal[0] = T.Zero;
        }

        var outIdx = 1;

        CalcAndOutputDX(inHigh, inLow, inClose, outReal, ref today, endIdx, ref prevHigh, ref prevLow, ref prevClose,
            ref prevPlusDM, ref prevMinusDM, ref prevTR, timePeriod, ref outIdx);

        outRange = Range.EndAt(outIdx);

        return Core.RetCode.Success;
    }

    private static void SkipDxUnstablePeriod<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ref int today,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod) where T : IFloatingPointIeee754<T>
    {
        for (var i = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Dx) + 1; i > 0; i--)
        {
            today++;
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);
        }
    }

    private static void CalcAndOutputDX<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Span<T> outReal,
        ref int today,
        int endIdx,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        while (today < endIdx)
        {
            today++;

            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);

            var tempReal = FunctionHelpers.TrueRange(prevHigh, prevLow, prevClose);
            prevTR = prevTR - prevTR / timePeriod + tempReal;
            prevClose = inClose[today];

            if (!T.IsZero(prevTR))
            {
                var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                tempReal = minusDI + plusDI;
                outReal[outIdx] = !T.IsZero(tempReal)
                    ? FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal)
                    : outReal[outIdx - 1];
            }
            else
            {
                outReal[outIdx] = outReal[outIdx - 1];
            }

            outIdx++;
        }
    }
}
