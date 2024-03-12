namespace TALib;

public static partial class Core
{
    public static RetCode Cosh(double[] inReal, int startIdx, int endIdx, double[] outReal, out int outBegIdx, out int outNbElement)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return RetCode.OutOfRangeStartIndex;
        }

        if (inReal == null || outReal == null)
        {
            return RetCode.BadParam;
        }

        int outIdx = default;
        for (int i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = Math.Cosh(inReal[i]);
        }

        outBegIdx = startIdx;
        outNbElement = outIdx;

        return RetCode.Success;
    }

    public static RetCode Cosh(decimal[] inReal, int startIdx, int endIdx, decimal[] outReal, out int outBegIdx, out int outNbElement)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return RetCode.OutOfRangeStartIndex;
        }

        if (inReal == null || outReal == null)
        {
            return RetCode.BadParam;
        }

        int outIdx = default;
        for (int i = startIdx; i <= endIdx; i++)
        {
            outReal[outIdx++] = DecimalMath.Cosh(inReal[i]);
        }

        outBegIdx = startIdx;
        outNbElement = outIdx;

        return RetCode.Success;
    }

    public static int CoshLookback() => 0;
}
