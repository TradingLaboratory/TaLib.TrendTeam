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
    public static Core.RetCode HtDcPhase<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtDcPhaseImpl(inReal, inRange, outReal, out outRange);

    [PublicAPI]
    public static int HtDcPhaseLookback() =>
        // See MamaLookback for an explanation of the "32"
        Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtDcPhase) + 63;

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtDcPhase<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtDcPhaseImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode HtDcPhaseImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var lookbackTotal = HtDcPhaseLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        const int smoothPriceSize = 50;
        Span<T> smoothPrice = new T[smoothPriceSize];

        var outBegIdx = startIdx;

        HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum, out var trailingWMAValue,
            out var trailingWMAIdx, 34, out var today);

        int hilbertIdx = default;
        int smoothPriceIdx = default;

        /* Initialize the circular buffer used by the hilbert transform logic.
         * A buffer is used for odd day and another for even days.
         * This minimizes the number of memory access and floating point operations needed
         * By using static circular buffer, no large dynamic memory allocation is needed for storing intermediate calculation.
         */
        Span<T> circBuffer = HTHelper.BufferFactory<T>();

        int outIdx = default;

        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
        var period = prevI2 = prevQ2 =
            re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = T.Zero;

        // The code is speed optimized and is most likely very hard to follow if you do not already know well the original algorithm.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Remember the smoothedValue into the smoothPrice circular buffer.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Adjust the period for next price bar
            HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            dcPhase = ComputeDcPhase(smoothPrice, smoothPeriod, smoothPriceIdx, dcPhase);

            if (today >= startIdx)
            {
                outReal[outIdx++] = dcPhase;
            }

            if (++smoothPriceIdx > smoothPriceSize - 1)
            {
                smoothPriceIdx = 0;
            }

            today++;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T ComputeDcPhase<T>(
        Span<T> smoothPrice,
        T smoothPeriod,
        int smoothPriceIdx,
        T dcPhase) where T : IFloatingPointIeee754<T>
    {
        var dcPeriod = smoothPeriod + T.CreateChecked(0.5);
        var dcPeriodInt = Int32.CreateTruncating(dcPeriod);
        var realPart = T.Zero;
        var imagPart = T.Zero;

        var idx = smoothPriceIdx;
        for (var i = 0; i < dcPeriodInt; i++)
        {
            var tempReal = T.CreateChecked(i) * Two<T>() * T.Pi / T.CreateChecked(dcPeriodInt);
            var tempReal2 = smoothPrice[idx];
            realPart += T.Sin(tempReal) * tempReal2;
            imagPart += T.Cos(tempReal) * tempReal2;
            idx = (idx == 0) ? smoothPrice.Length - 1 : idx - 1;
        }

        dcPhase = CalcDcPhase(realPart, imagPart, dcPhase, smoothPeriod);

        return dcPhase;
    }

    private static T CalcDcPhase<T>(
        T realPart,
        T imagPart,
        T dcPhase,
        T smoothPeriod) where T : IFloatingPointIeee754<T>
    {
        var tempReal = T.Abs(imagPart);
        if (tempReal > T.Zero)
        {
            dcPhase = T.RadiansToDegrees(T.Atan(realPart / imagPart));
        }
        else if (tempReal <= T.CreateChecked(0.01))
        {
            dcPhase = AdjustPhaseForSmallImaginaryPart(realPart, dcPhase);
        }

        dcPhase = FinalPhaseAdjustments(imagPart, dcPhase, smoothPeriod);

        return dcPhase;
    }

    private static T AdjustPhaseForSmallImaginaryPart<T>(T realPart, T dcPhase) where T : IFloatingPointIeee754<T>
    {
        if (realPart < T.Zero)
        {
            dcPhase -= Ninety<T>();
        }
        else if (realPart > T.Zero)
        {
            dcPhase += Ninety<T>();
        }

        return dcPhase;
    }

    private static T FinalPhaseAdjustments<T>(T imagPart, T dcPhase, T smoothPeriod) where T : IFloatingPointIeee754<T>
    {
        dcPhase += Ninety<T>();
        // Compensate for one bar lag of the weighted moving average
        dcPhase += Ninety<T>() * Four<T>() / smoothPeriod;

        if (imagPart < T.Zero)
        {
            dcPhase += Ninety<T>() * Two<T>();
        }

        if (dcPhase > Ninety<T>() * T.CreateChecked(3.5))
        {
            dcPhase -= Ninety<T>() * Four<T>();
        }

        return dcPhase;
    }
}
