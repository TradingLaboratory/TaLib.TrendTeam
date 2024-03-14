namespace TALib;

public static partial class Functions
{
    public static Core.RetCode Correl(double[] inReal0, double[] inReal1, int startIdx, int endIdx, double[] outReal, out int outBegIdx,
        out int outNbElement, int optInTimePeriod = 30)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (inReal0 == null || inReal1 == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
        {
            return Core.RetCode.BadParam;
        }

        int lookbackTotal = CorrelLookback(optInTimePeriod);
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        outBegIdx = startIdx;
        int trailingIdx = startIdx - lookbackTotal;

        double sumX, sumY, sumX2, sumY2;
        double sumXY = sumX = sumY = sumX2 = sumY2 = default;
        int today;
        for (today = trailingIdx; today <= startIdx; today++)
        {
            double x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            double y = inReal1[today];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;
        }

        double trailingX = inReal0[trailingIdx];
        double trailingY = inReal1[trailingIdx++];
        double tempReal = (sumX2 - sumX * sumX / optInTimePeriod) * (sumY2 - sumY * sumY / optInTimePeriod);
        outReal[0] = !TA_IsZeroOrNeg(tempReal) ? (sumXY - sumX * sumY / optInTimePeriod) / Math.Sqrt(tempReal) : 0.0;

        int outIdx = 1;
        while (today <= endIdx)
        {
            sumX -= trailingX;
            sumX2 -= trailingX * trailingX;

            sumXY -= trailingX * trailingY;
            sumY -= trailingY;
            sumY2 -= trailingY * trailingY;

            double x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            double y = inReal1[today++];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;

            trailingX = inReal0[trailingIdx];
            trailingY = inReal1[trailingIdx++];
            tempReal = (sumX2 - sumX * sumX / optInTimePeriod) * (sumY2 - sumY * sumY / optInTimePeriod);
            outReal[outIdx++] = !TA_IsZeroOrNeg(tempReal) ? (sumXY - sumX * sumY / optInTimePeriod) / Math.Sqrt(tempReal) : 0.0;
        }

        outNbElement = outIdx;

        return Core.RetCode.Success;
    }

    public static Core.RetCode Correl(decimal[] inReal0, decimal[] inReal1, int startIdx, int endIdx, decimal[] outReal, out int outBegIdx,
        out int outNbElement, int optInTimePeriod = 30)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (inReal0 == null || inReal1 == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
        {
            return Core.RetCode.BadParam;
        }

        int lookbackTotal = CorrelLookback(optInTimePeriod);
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        outBegIdx = startIdx;
        int trailingIdx = startIdx - lookbackTotal;

        decimal sumX, sumY, sumX2, sumY2;
        decimal sumXY = sumX = sumY = sumX2 = sumY2 = default;
        int today;
        for (today = trailingIdx; today <= startIdx; today++)
        {
            decimal x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            decimal y = inReal1[today];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;
        }

        decimal trailingX = inReal0[trailingIdx];
        decimal trailingY = inReal1[trailingIdx++];
        decimal tempReal = (sumX2 - sumX * sumX / optInTimePeriod) * (sumY2 - sumY * sumY / optInTimePeriod);
        outReal[0] = !TA_IsZeroOrNeg(tempReal) ? (sumXY - sumX * sumY / optInTimePeriod) / DecimalMath.Sqrt(tempReal) : Decimal.Zero;

        int outIdx = 1;
        while (today <= endIdx)
        {
            sumX -= trailingX;
            sumX2 -= trailingX * trailingX;

            sumXY -= trailingX * trailingY;
            sumY -= trailingY;
            sumY2 -= trailingY * trailingY;

            decimal x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            decimal y = inReal1[today++];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;

            trailingX = inReal0[trailingIdx];
            trailingY = inReal1[trailingIdx++];
            tempReal = (sumX2 - sumX * sumX / optInTimePeriod) * (sumY2 - sumY * sumY / optInTimePeriod);
            outReal[outIdx++] = !TA_IsZeroOrNeg(tempReal)
                ? (sumXY - sumX * sumY / optInTimePeriod) / DecimalMath.Sqrt(tempReal)
                : Decimal.Zero;
        }

        outNbElement = outIdx;

        return Core.RetCode.Success;
    }

    public static int CorrelLookback(int optInTimePeriod = 30)
    {
        if (optInTimePeriod is < 1 or > 100000)
        {
            return -1;
        }

        return optInTimePeriod - 1;
    }
}
