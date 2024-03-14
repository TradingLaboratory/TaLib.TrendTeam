namespace TALib;

public static partial class Functions
{
    public static Core.RetCode Bbands(double[] inReal, int startIdx, int endIdx, double[] outRealUpperBand, double[] outRealMiddleBand,
        double[] outRealLowerBand, out int outBegIdx, out int outNbElement, int optInTimePeriod = 5, double optInNbDevUp = 2.0,
        double optInNbDevDn = 2.0, Core.MAType optInMAType = Core.MAType.Sma)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (inReal == null || outRealUpperBand == null || outRealMiddleBand == null || outRealLowerBand == null ||
            optInTimePeriod < 2 || optInTimePeriod > 100000)
        {
            return Core.RetCode.BadParam;
        }

        double[] tempBuffer1;
        double[] tempBuffer2;
        if (inReal == outRealUpperBand)
        {
            tempBuffer1 = outRealMiddleBand;
            tempBuffer2 = outRealLowerBand;
        }
        else if (inReal == outRealLowerBand)
        {
            tempBuffer1 = outRealMiddleBand;
            tempBuffer2 = outRealUpperBand;
        }
        else if (inReal == outRealMiddleBand)
        {
            tempBuffer1 = outRealLowerBand;
            tempBuffer2 = outRealUpperBand;
        }
        else
        {
            tempBuffer1 = outRealMiddleBand;
            tempBuffer2 = outRealUpperBand;
        }

        if (tempBuffer1 == inReal || tempBuffer2 == inReal)
        {
            return Core.RetCode.BadParam;
        }

        Core.RetCode retCode = Ma(inReal, startIdx, endIdx, tempBuffer1, out outBegIdx, out outNbElement, optInTimePeriod, optInMAType);
        if (retCode != Core.RetCode.Success || outNbElement == 0)
        {
            return retCode;
        }

        if (optInMAType == Core.MAType.Sma)
        {
            Core.TA_INT_StdDevUsingPrecalcMA(inReal, tempBuffer1, outBegIdx, outNbElement, tempBuffer2, optInTimePeriod);
        }
        else
        {
            retCode = StdDev(inReal, outBegIdx, endIdx, tempBuffer2, out outBegIdx, out outNbElement, optInTimePeriod);
            if (retCode != Core.RetCode.Success)
            {
                outNbElement = 0;

                return retCode;
            }
        }

        if (tempBuffer1 != outRealMiddleBand)
        {
            Array.Copy(tempBuffer1, 0, outRealMiddleBand, 0, outNbElement);
        }

        double tempReal;
        double tempReal2;
        if (optInNbDevUp.Equals(optInNbDevDn))
        {
            if (optInNbDevUp.Equals(1.0))
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i];
                    tempReal2 = outRealMiddleBand[i];
                    outRealUpperBand[i] = tempReal2 + tempReal;
                    outRealLowerBand[i] = tempReal2 - tempReal;
                }
            }
            else
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i] * optInNbDevUp;
                    tempReal2 = outRealMiddleBand[i];
                    outRealUpperBand[i] = tempReal2 + tempReal;
                    outRealLowerBand[i] = tempReal2 - tempReal;
                }
            }
        }
        else if (optInNbDevUp.Equals(1.0))
        {
            for (var i = 0; i < outNbElement; i++)
            {
                tempReal = tempBuffer2[i];
                tempReal2 = outRealMiddleBand[i];
                outRealUpperBand[i] = tempReal2 + tempReal;
                outRealLowerBand[i] = tempReal2 - tempReal * optInNbDevDn;
            }
        }
        else if (optInNbDevDn.Equals(1.0))
        {
            for (var i = 0; i < outNbElement; i++)
            {
                tempReal = tempBuffer2[i];
                tempReal2 = outRealMiddleBand[i];
                outRealLowerBand[i] = tempReal2 - tempReal;
                outRealUpperBand[i] = tempReal2 + tempReal * optInNbDevUp;
            }
        }
        else
        {
            for (var i = 0; i < outNbElement; i++)
            {
                tempReal = tempBuffer2[i];
                tempReal2 = outRealMiddleBand[i];
                outRealUpperBand[i] = tempReal2 + tempReal * optInNbDevUp;
                outRealLowerBand[i] = tempReal2 - tempReal * optInNbDevDn;
            }
        }

        return Core.RetCode.Success;
    }

    public static Core.RetCode Bbands(decimal[] inReal, int startIdx, int endIdx, decimal[] outRealUpperBand, decimal[] outRealMiddleBand,
        decimal[] outRealLowerBand, out int outBegIdx, out int outNbElement, int optInTimePeriod = 5, decimal optInNbDevUp = 2m,
        decimal optInNbDevDn = 2m, Core.MAType optInMAType = Core.MAType.Sma)
    {
        outBegIdx = outNbElement = 0;

        if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
        {
            return Core.RetCode.OutOfRangeStartIndex;
        }

        if (inReal == null || outRealUpperBand == null || outRealMiddleBand == null || outRealLowerBand == null ||
            optInTimePeriod < 2 || optInTimePeriod > 100000)
        {
            return Core.RetCode.BadParam;
        }

        decimal[] tempBuffer1;
        decimal[] tempBuffer2;
        if (inReal == outRealUpperBand)
        {
            tempBuffer1 = outRealMiddleBand;
            tempBuffer2 = outRealLowerBand;
        }
        else if (inReal == outRealLowerBand)
        {
            tempBuffer1 = outRealMiddleBand;
            tempBuffer2 = outRealUpperBand;
        }
        else if (inReal == outRealMiddleBand)
        {
            tempBuffer1 = outRealLowerBand;
            tempBuffer2 = outRealUpperBand;
        }
        else
        {
            tempBuffer1 = outRealMiddleBand;
            tempBuffer2 = outRealUpperBand;
        }

        if (tempBuffer1 == inReal || tempBuffer2 == inReal)
        {
            return Core.RetCode.BadParam;
        }

        Core.RetCode retCode = Ma(inReal, startIdx, endIdx, tempBuffer1, out outBegIdx, out outNbElement, optInTimePeriod, optInMAType);
        if (retCode != Core.RetCode.Success || outNbElement == 0)
        {
            return retCode;
        }

        if (optInMAType == Core.MAType.Sma)
        {
            Core.TA_INT_StdDevUsingPrecalcMA(inReal, tempBuffer1, outBegIdx, outNbElement, tempBuffer2, optInTimePeriod);
        }
        else
        {
            retCode = StdDev(inReal, outBegIdx, endIdx, tempBuffer2, out outBegIdx, out outNbElement, optInTimePeriod);
            if (retCode != Core.RetCode.Success)
            {
                outNbElement = 0;

                return retCode;
            }
        }

        if (tempBuffer1 != outRealMiddleBand)
        {
            Array.Copy(tempBuffer1, 0, outRealMiddleBand, 0, outNbElement);
        }

        decimal tempReal;
        decimal tempReal2;
        if (optInNbDevUp == optInNbDevDn)
        {
            if (optInNbDevUp == Decimal.One)
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i];
                    tempReal2 = outRealMiddleBand[i];
                    outRealUpperBand[i] = tempReal2 + tempReal;
                    outRealLowerBand[i] = tempReal2 - tempReal;
                }
            }
            else
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i] * optInNbDevUp;
                    tempReal2 = outRealMiddleBand[i];
                    outRealUpperBand[i] = tempReal2 + tempReal;
                    outRealLowerBand[i] = tempReal2 - tempReal;
                }
            }
        }
        else if (optInNbDevUp == Decimal.One)
        {
            for (var i = 0; i < outNbElement; i++)
            {
                tempReal = tempBuffer2[i];
                tempReal2 = outRealMiddleBand[i];
                outRealUpperBand[i] = tempReal2 + tempReal;
                outRealLowerBand[i] = tempReal2 - tempReal * optInNbDevDn;
            }
        }
        else if (optInNbDevDn == Decimal.One)
        {
            for (var i = 0; i < outNbElement; i++)
            {
                tempReal = tempBuffer2[i];
                tempReal2 = outRealMiddleBand[i];
                outRealLowerBand[i] = tempReal2 - tempReal;
                outRealUpperBand[i] = tempReal2 + tempReal * optInNbDevUp;
            }
        }
        else
        {
            for (var i = 0; i < outNbElement; i++)
            {
                tempReal = tempBuffer2[i];
                tempReal2 = outRealMiddleBand[i];
                outRealUpperBand[i] = tempReal2 + tempReal * optInNbDevUp;
                outRealLowerBand[i] = tempReal2 - tempReal * optInNbDevDn;
            }
        }

        return Core.RetCode.Success;
    }

    public static int BbandsLookback(int optInTimePeriod = 5, Core.MAType optInMAType = Core.MAType.Sma)
    {
        if (optInTimePeriod is < 2 or > 100000)
        {
            return -1;
        }

        return MaLookback(optInTimePeriod, optInMAType);
    }
}
