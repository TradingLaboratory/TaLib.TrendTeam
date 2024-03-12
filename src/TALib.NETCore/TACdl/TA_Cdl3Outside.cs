namespace TALib;

public static partial class Core
{
    public static RetCode Cdl3Outside(double[] inOpen, double[] inHigh, double[] inLow, double[] inClose, int startIdx, int endIdx,
        int[] outInteger, out int outBegIdx, out int outNbElement)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return RetCode.OutOfRangeStartIndex;
        }

        if (inOpen == null || inHigh == null || inLow == null || inClose == null || outInteger == null)
        {
            return RetCode.BadParam;
        }

        int lookbackTotal = Cdl3OutsideLookback();
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return RetCode.Success;
        }

        int i = startIdx;
        int outIdx = default;
        do
        {
            if (TA_CandleColor(inClose, inOpen, i - 1) && !TA_CandleColor(inClose, inOpen, i - 2) &&
                inClose[i - 1] > inOpen[i - 2] && inOpen[i - 1] < inClose[i - 2] &&
                inClose[i] > inClose[i - 1]
                ||
                !TA_CandleColor(inClose, inOpen, i - 1) && TA_CandleColor(inClose, inOpen, i - 2) &&
                inOpen[i - 1] > inClose[i - 2] && inClose[i - 1] < inOpen[i - 2] &&
                inClose[i] < inClose[i - 1])
            {
                outInteger[outIdx++] = Convert.ToInt32(TA_CandleColor(inClose, inOpen, i - 1)) * 100;
            }
            else
            {
                outInteger[outIdx++] = 0;
            }

            i++;
        } while (i <= endIdx);

        outBegIdx = startIdx;
        outNbElement = outIdx;

        return RetCode.Success;
    }

    public static RetCode Cdl3Outside(decimal[] inOpen, decimal[] inHigh, decimal[] inLow, decimal[] inClose, int startIdx, int endIdx,
        int[] outInteger, out int outBegIdx, out int outNbElement)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return RetCode.OutOfRangeStartIndex;
        }

        if (inOpen == null || inHigh == null || inLow == null || inClose == null || outInteger == null)
        {
            return RetCode.BadParam;
        }

        int lookbackTotal = Cdl3OutsideLookback();
        if (startIdx < lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return RetCode.Success;
        }

        int i = startIdx;
        int outIdx = default;
        do
        {
            if (TA_CandleColor(inClose, inOpen, i - 1) && !TA_CandleColor(inClose, inOpen, i - 2) &&
                inClose[i - 1] > inOpen[i - 2] && inOpen[i - 1] < inClose[i - 2] &&
                inClose[i] > inClose[i - 1]
                ||
                !TA_CandleColor(inClose, inOpen, i - 1) && TA_CandleColor(inClose, inOpen, i - 2) &&
                inOpen[i - 1] > inClose[i - 2] && inClose[i - 1] < inOpen[i - 2] &&
                inClose[i] < inClose[i - 1])
            {
                outInteger[outIdx++] = Convert.ToInt32(TA_CandleColor(inClose, inOpen, i - 1)) * 100;
            }
            else
            {
                outInteger[outIdx++] = 0;
            }

            i++;
        } while (i <= endIdx);

        outBegIdx = startIdx;
        outNbElement = outIdx;

        return RetCode.Success;
    }

    public static int Cdl3OutsideLookback() => 3;
}
