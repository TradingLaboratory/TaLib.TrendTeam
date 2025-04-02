//Название файла: TA_Cmo.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendStrength (альтернатива для акцента на силе тренда)
//Oscillators (альтернатива для группировки по типу индикатора)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Chande Momentum Oscillator (Momentum Indicators) — Осциллятор импульса Чанде (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Осциллятор импульса Чанде — это импульсный индикатор, который измеряет разницу между суммой приростов
    /// и убытков за указанный период времени, нормализованную по их общей сумме. Он похож на <see cref="Rsi{T}">RSI</see>,
    /// но CMO учитывает как восходящий, так и нисходящий импульс одновременно в своем расчете.
    /// <para>
    /// Функция может обнаруживать сдвиги импульса. Использование ее вместе с инструментами подтверждения тренда может повысить точность при принятии решений по времени.
    /// Функция особенно полезна для выявления перекупленности и перепроданности и подтверждения трендов при использовании с другими индикаторами.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать приросты и убытки между последовательными значениями цен за указанный период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить сумму приростов и сумму убытков.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать CMO:
    ///       <code>
    ///         CMO = 100 * (Sum of Gains - Sum of Losses) / (Sum of Gains + Sum of Losses)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение указывает на восходящий импульс, что свидетельствует о бычьих условиях.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательное значение указывает на нисходящий импульс, что свидетельствует о медвежьих условиях.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения близкие к нулю указывают на отсутствие значительного импульса в любом направлении.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Cmo<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CmoImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Cmo{T}">Cmo</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int CmoLookback(int optInTimePeriod = 14)
    {
        if (optInTimePeriod < 2)
        {
            return -1;
        }

        var retValue = optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Cmo);
        if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock)
        {
            retValue--;
        }

        return retValue;
    }

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Cmo<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CmoImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode CmoImpl<T>(
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

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Расчет CMO в основном идентичен RSI.
         *
         * Единственное отличие в последнем этапе расчета:
         *
         *   RSI = gain / (gain+loss)
         *   CMO = (gain-loss) / (gain+loss)
         *
         * См. функцию Rsi для получения дополнительной информации об этом алгоритме.
         */

        var lookbackTotal = CmoLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);

        var outIdx = 0;

        // Накопить "Средний прирост" и "Средний убыток" Уайлдера среди начального периода.
        var today = startIdx - lookbackTotal;
        var prevValue = inReal[today];

        // Если есть нестабильный период, нет необходимости рассчитывать, так как это первое значение будет пропущено.
        if (Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Cmo) == 0 &&
            Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock &&
            ProcessCmoMetastockCompatibility(inReal, outReal, ref outRange, optInTimePeriod, endIdx, startIdx, ref prevValue, ref today,
                ref outIdx, out var retCode))
        {
            return retCode;
        }

        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);

        /* Последующие prevLoss и prevGain сглаживаются с использованием предыдущих значений (подход Уайлдера).
         * 1) Умножить предыдущее значение на 'период - 1'.
         * 2) Добавить сегодняшнее значение.
         * 3) Разделить на 'период'.
         */
        prevLoss /= timePeriod;
        prevGain /= timePeriod;

        /* Часто документация представляет расчет RSI следующим образом:
         *    RSI = 100 - (100 / 1 + (prevGain / prevLoss))
         *
         * Следующее эквивалентно:
         *    RSI = 100 * (prevGain / (prevGain + prevLoss))
         *
         * Вторая формула используется здесь для оптимизации скорости.
         */
        if (today > startIdx)
        {
            var tempValue1 = prevGain + prevLoss;
            outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * ((prevGain - prevLoss) / tempValue1) : T.Zero;
        }
        else
        {
            // Пропустить нестабильный период. Выполнить обработку, но не записывать в выходной массив.
            while (today < startIdx)
            {
                ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
            }
        }

        // Нестабильный период пропущен... теперь продолжаем обработку, если необходимо.
        while (today <= endIdx)
        {
            ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
            var tempValue1 = prevGain + prevLoss;
            outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * ((prevGain - prevLoss) / tempValue1) : T.Zero;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static bool ProcessCmoMetastockCompatibility<T>(
        ReadOnlySpan<T> inReal,
        Span<T> outReal,
        ref Range outRange,
        int optInTimePeriod,
        int endIdx,
        int startIdx,
        ref T prevValue,
        ref int today,
        ref int outIdx,
        out Core.RetCode retCode)
        where T : IFloatingPointIeee754<T>
    {
        // Сохранить prevValue, так как оно может быть перезаписано выходным значением.
        // (так как указатель выхода может быть таким же, как указатель входа).
        var savePrevValue = prevValue;

        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);
        WriteInitialCmoValue(prevGain, prevLoss, optInTimePeriod, outReal, ref outIdx);

        if (today > endIdx)
        {
            outRange = new Range(startIdx, startIdx + outIdx);
            retCode = Core.RetCode.Success;

            return true;
        }

        // Начать заново для следующего ценового бара.
        today -= optInTimePeriod;
        prevValue = savePrevValue;
        retCode = Core.RetCode.Success;

        return false;
    }

    private static void WriteInitialCmoValue<T>(
        T prevGain,
        T prevLoss,
        int optInTimePeriod,
        Span<T> outReal,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        var timePeriod = T.CreateChecked(optInTimePeriod);

        var tempValue1 = prevLoss / timePeriod;
        var tempValue2 = prevGain / timePeriod;
        var tempValue3 = tempValue2 - tempValue1;
        var tempValue4 = tempValue1 + tempValue2;

        outReal[outIdx++] = !T.IsZero(tempValue4) ? FunctionHelpers.Hundred<T>() * (tempValue3 / tempValue4) : T.Zero;
    }
}
