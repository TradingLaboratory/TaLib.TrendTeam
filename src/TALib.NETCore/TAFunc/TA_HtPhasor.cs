namespace TALib;

public static partial class Functions
{
    public static Core.RetCode HtPhasor(double[] inReal, int startIdx, int endIdx, double[] outInPhase, double[] outQuadrature,
        out int outBegIdx, out int outNbElement)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (inReal == null || outInPhase == null || outQuadrature == null)
        {
            return Core.RetCode.BadParam;
        }

        int lookbackTotal = HtPhasorLookback();
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        const double rad2Deg = 180.0 / Math.PI;

        outBegIdx = startIdx;
        int trailingWMAIdx = startIdx - lookbackTotal;
        int today = trailingWMAIdx;

        double tempReal = inReal[today++];
        double periodWMASub = tempReal;
        double periodWMASum = tempReal;
        tempReal = inReal[today++];
        periodWMASub += tempReal;
        periodWMASum += tempReal * 2.0;
        tempReal = inReal[today++];
        periodWMASub += tempReal;
        periodWMASum += tempReal * 3.0;

        double trailingWMAValue = default;
        var i = 9;
        do
        {
            tempReal = inReal[today++];
            Core.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
        } while (--i != 0);

        int hilbertIdx = default;

        var hilbertVariables = Core.InitHilbertVariables<double>();

        int outIdx = default;

        double prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2;
        double period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = default;
        while (today <= endIdx)
        {
            double i2;
            double q2;

            double adjustedPrevPeriod = 0.075 * period + 0.54;

            Core.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);
            if (today % 2 == 0)
            {
                Core.DoHilbertEven(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertEven(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                if (today >= startIdx)
                {
                    outQuadrature[outIdx] = hilbertVariables["q1"];
                    outInPhase[outIdx++] = i1ForEvenPrev3;
                }

                Core.DoHilbertEven(hilbertVariables, "jI", i1ForEvenPrev3, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertEven(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);
                if (++hilbertIdx == 3)
                {
                    hilbertIdx = 0;
                }

                q2 = 0.2 * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8 * prevQ2;
                i2 = 0.2 * (i1ForEvenPrev3 - hilbertVariables["jQ"]) + 0.8 * prevI2;
                i1ForOddPrev3 = i1ForOddPrev2;
                i1ForOddPrev2 = hilbertVariables["detrender"];
            }
            else
            {
                Core.DoHilbertOdd(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertOdd(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                if (today >= startIdx)
                {
                    outQuadrature[outIdx] = hilbertVariables["q1"];
                    outInPhase[outIdx++] = i1ForOddPrev3;
                }

                Core.DoHilbertOdd(hilbertVariables, "jI", i1ForOddPrev3, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertOdd(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                q2 = 0.2 * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8 * prevQ2;
                i2 = 0.2 * (i1ForOddPrev3 - hilbertVariables["jQ"]) + 0.8 * prevI2;
                i1ForEvenPrev3 = i1ForEvenPrev2;
                i1ForEvenPrev2 = hilbertVariables["detrender"];
            }

            re = 0.2 * (i2 * prevI2 + q2 * prevQ2) + 0.8 * re;
            im = 0.2 * (i2 * prevQ2 - q2 * prevI2) + 0.8 * im;
            prevQ2 = q2;
            prevI2 = i2;
            tempReal = period;
            if (!im.Equals(0.0) && !re.Equals(0.0))
            {
                period = 360.0 / (Math.Atan(im / re) * rad2Deg);
            }

            double tempReal2 = 1.5 * tempReal;
            if (period > tempReal2)
            {
                period = tempReal2;
            }

            tempReal2 = 0.67 * tempReal;
            if (period < tempReal2)
            {
                period = tempReal2;
            }

            if (period < 6.0)
            {
                period = 6.0;
            }
            else if (period > 50.0)
            {
                period = 50.0;
            }

            period = 0.2 * period + 0.8 * tempReal;
            today++;
        }

        outNbElement = outIdx;

        return Core.RetCode.Success;
    }

    public static Core.RetCode HtPhasor(decimal[] inReal, int startIdx, int endIdx, decimal[] outInPhase, decimal[] outQuadrature,
        out int outBegIdx, out int outNbElement)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (inReal == null || outInPhase == null || outQuadrature == null)
        {
            return Core.RetCode.BadParam;
        }

        int lookbackTotal = HtPhasorLookback();
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        const decimal rad2Deg = 180m / DecimalMath.PI;

        outBegIdx = startIdx;
        int trailingWMAIdx = startIdx - lookbackTotal;
        int today = trailingWMAIdx;

        decimal tempReal = inReal[today++];
        decimal periodWMASub = tempReal;
        decimal periodWMASum = tempReal;
        tempReal = inReal[today++];
        periodWMASub += tempReal;
        periodWMASum += tempReal * 2m;
        tempReal = inReal[today++];
        periodWMASub += tempReal;
        periodWMASum += tempReal * 3m;

        decimal trailingWMAValue = default;
        var i = 9;
        do
        {
            tempReal = inReal[today++];
            Core.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
        } while (--i != 0);

        int hilbertIdx = default;

        var hilbertVariables = Core.InitHilbertVariables<decimal>();

        int outIdx = default;

        decimal prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2;
        decimal period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = default;
        while (today <= endIdx)
        {
            decimal i2;
            decimal q2;

            decimal adjustedPrevPeriod = 0.075m * period + 0.54m;

            Core.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);
            if (today % 2 == 0)
            {
                Core.DoHilbertEven(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertEven(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                if (today >= startIdx)
                {
                    outQuadrature[outIdx] = hilbertVariables["q1"];
                    outInPhase[outIdx++] = i1ForEvenPrev3;
                }

                Core.DoHilbertEven(hilbertVariables, "jI", i1ForEvenPrev3, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertEven(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);
                if (++hilbertIdx == 3)
                {
                    hilbertIdx = 0;
                }

                q2 = 0.2m * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8m * prevQ2;
                i2 = 0.2m * (i1ForEvenPrev3 - hilbertVariables["jQ"]) + 0.8m * prevI2;
                i1ForOddPrev3 = i1ForOddPrev2;
                i1ForOddPrev2 = hilbertVariables["detrender"];
            }
            else
            {
                Core.DoHilbertOdd(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertOdd(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                if (today >= startIdx)
                {
                    outQuadrature[outIdx] = hilbertVariables["q1"];
                    outInPhase[outIdx++] = i1ForOddPrev3;
                }

                Core.DoHilbertOdd(hilbertVariables, "jI", i1ForOddPrev3, hilbertIdx, adjustedPrevPeriod);
                Core.DoHilbertOdd(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                q2 = 0.2m * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8m * prevQ2;
                i2 = 0.2m * (i1ForOddPrev3 - hilbertVariables["jQ"]) + 0.8m * prevI2;
                i1ForEvenPrev3 = i1ForEvenPrev2;
                i1ForEvenPrev2 = hilbertVariables["detrender"];
            }

            re = 0.2m * (i2 * prevI2 + q2 * prevQ2) + 0.8m * re;
            im = 0.2m * (i2 * prevQ2 - q2 * prevI2) + 0.8m * im;
            prevQ2 = q2;
            prevI2 = i2;
            tempReal = period;
            if (im != Decimal.Zero && re != Decimal.Zero)
            {
                period = 360m / (DecimalMath.Atan(im / re) * rad2Deg);
            }

            decimal tempReal2 = 1.5m * tempReal;
            if (period > tempReal2)
            {
                period = tempReal2;
            }

            tempReal2 = 0.67m * tempReal;
            if (period < tempReal2)
            {
                period = tempReal2;
            }

            if (period < 6m)
            {
                period = 6m;
            }
            else if (period > 50m)
            {
                period = 50m;
            }

            period = 0.2m * period + 0.8m * tempReal;
            today++;
        }

        outNbElement = outIdx;

        return Core.RetCode.Success;
    }

    public static int HtPhasorLookback() => Core.UnstablePeriodSettings.Get(Core.FuncUnstId.HtPhasor) + 32;
}
