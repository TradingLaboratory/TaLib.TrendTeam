//Название файла: TA_Cmo.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//Oscillators (альтернатива для группировки по типу осцилляторов)
//MomentumOscillators (альтернатива для акцента на осцилляторах импульса)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Chande Momentum Oscillator (Momentum Indicators) — Осциллятор импульса Чанде (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены Close, другие индикаторы или временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период времени для расчета осциллятора (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Осциллятор импульса Чанде (CMO) — это импульсный осциллятор, измеряющий разницу между суммой приростов (Gains)
    /// и суммой убытков (Losses) за указанный период, нормализованную по их общей сумме. В отличие от <see cref="Rsi{T}">RSI</see>,
    /// CMO учитывает как восходящий, так и нисходящий импульс одновременно в одном расчете.
    /// </para>
    /// <para>
    /// Функция эффективна для выявления состояний перекупленности и перепроданности, а также для подтверждения силы тренда
    /// при использовании в комбинации с другими индикаторами. Значения CMO колеблются в диапазоне от -100 до +100.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать изменения цен между последовательными барами: <c>Delta = Close[today] - Close[yesterday]</c>.
    ///       Положительные изменения считаются приростами (Gains), отрицательные — убытками (Losses, берутся по модулю).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить сумму приростов (Sum of Gains) и сумму убытков (Sum of Losses) за период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать значение CMO по формуле:
    ///       <code>
    ///         CMO = 100 * (Sum of Gains - Sum of Losses) / (Sum of Gains + Sum of Losses)
    ///       </code>
    ///       При использовании сглаживания Уайлдера применяется рекурсивный расчет средних значений.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения выше +50 указывают на сильный восходящий импульс (бычьи условия, возможная перекупленность).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения ниже -50 указывают на сильный нисходящий импульс (медвежьи условия, возможная перепроданность).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение нулевой линии снизу вверх сигнализирует о смене медвежьего импульса на бычий.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение нулевой линии сверху вниз сигнализирует о смене бычьего импульса на медвежий.
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
    /// Возвращает период обратного просмотра (lookback period) для <see cref="Cmo{T}">CMO</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета индикатора.</param>
    /// <returns>
    /// Количество баров, необходимых до первого валидного значения индикатора.
    /// Для периода 14 возвращает 14 (или 13 в режиме совместимости с MetaStock).
    /// </returns>
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
    /// Для совместимости с абстрактным API (массивная версия)
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
         * Единственное отличие в финальной формуле:
         *
         *   RSI  = 100 * (Gain / (Gain + Loss))
         *   CMO  = 100 * ((Gain - Loss) / (Gain + Loss))
         *
         * См. функцию Rsi для получения дополнительной информации об алгоритме сглаживания Уайлдера.
         */

        var lookbackTotal = CmoLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);

        var outIdx = 0;

        // Накопить приросты (Gains) и убытки (Losses) за начальный период для последующего сглаживания по Уайлдеру
        var today = startIdx - lookbackTotal;
        var prevValue = inReal[today];

        // Если включен режим совместимости с MetaStock и отсутствует нестабильный период — обработать специальный случай
        if (Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Cmo) == 0 &&
            Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Metastock &&
            ProcessCmoMetastockCompatibility(inReal, outReal, ref outRange, optInTimePeriod, endIdx, startIdx, ref prevValue, ref today,
                ref outIdx, out var retCode))
        {
            return retCode;
        }

        // Инициализировать суммы приростов и убытков за начальный период
        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);

        /* Сгладить накопленные значения по методу Уайлдера:
         * 1) Разделить сумму на период для получения среднего значения
         * 2) Последующие значения будут обновляться рекурсивно: 
         *    NewAvg = (PrevAvg * (Period - 1) + TodayValue) / Period
         */
        prevLoss /= timePeriod;
        prevGain /= timePeriod;

        /* Оптимизированная формула расчета CMO:
         *    CMO = 100 * ((Gain - Loss) / (Gain + Loss))
         *
         * Альтернативная форма (менее эффективная):
         *    CMO = 100 - (200 / (1 + (Gain / Loss)))
         */
        if (today > startIdx)
        {
            var tempValue1 = prevGain + prevLoss; // Сумма средних приростов и убытков
            outReal[outIdx++] = !T.IsZero(tempValue1) ? FunctionHelpers.Hundred<T>() * ((prevGain - prevLoss) / tempValue1) : T.Zero;
        }
        else
        {
            // Пропустить нестабильный период: выполнить расчеты, но не записывать результаты в выходной массив
            while (today < startIdx)
            {
                ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
            }
        }

        // Основной цикл расчета: обработать оставшиеся бары и записать валидные значения в выходной массив
        while (today <= endIdx)
        {
            ProcessToday(inReal, ref today, ref prevValue, ref prevGain, ref prevLoss, timePeriod);
            var tempValue1 = prevGain + prevLoss; // Сумма текущих сглаженных приростов и убытков
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
        // Сохранить предыдущее значение цены, так как оно может быть перезаписано при совпадении входного и выходного массивов
        var savePrevValue = prevValue;

        // Рассчитать начальные суммы приростов и убытков за период
        InitGainsAndLosses(inReal, ref today, ref prevValue, optInTimePeriod, out T prevGain, out T prevLoss);
        // Записать первое значение CMO без сглаживания (специфика MetaStock)
        WriteInitialCmoValue(prevGain, prevLoss, optInTimePeriod, outReal, ref outIdx);

        if (today > endIdx)
        {
            outRange = new Range(startIdx, startIdx + outIdx);
            retCode = Core.RetCode.Success;

            return true;
        }

        // Сбросить позицию для продолжения расчета со следующего бара
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

        var tempValue1 = prevLoss / timePeriod;  // Среднее значение убытков (Losses)
        var tempValue2 = prevGain / timePeriod;  // Среднее значение приростов (Gains)
        var tempValue3 = tempValue2 - tempValue1; // Разница между средними приростами и убытками
        var tempValue4 = tempValue1 + tempValue2; // Сумма средних приростов и убытков

        outReal[outIdx++] = !T.IsZero(tempValue4) ? FunctionHelpers.Hundred<T>() * (tempValue3 / tempValue4) : T.Zero;
    }
}
