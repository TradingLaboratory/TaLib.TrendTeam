//Название файла: TA_Ad.cs
//Группы к которым можно отнести индикатор:
//VolumeIndicators (существующая папка - идеальное соответствие категории)
//AccumulationDistribution (альтернатива для акцента на типе индикатора)
//MoneyFlowIndicators (альтернатива для группировки по методу расчета)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// _Chaikin A/D Line (Volume Indicators) — Линия накопления/распределения Чайкина (Индикаторы объема)_
    /// </summary>
    /// <param name="inHigh">
    /// Массив входных максимальных цен (High) для каждого бара.
    /// </param>
    /// <param name="inLow">
    /// Массив входных минимальных цен (Low) для каждого бара.
    /// </param>
    /// <param name="inClose">
    /// Массив входных цен закрытия (Close) для каждого бара.
    /// </param>
    /// <param name="inVolume">
    /// Массив входных объемов торгов (Volume) для каждого бара.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатываются все доступные данные во входных массивах.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует данным входных массивов по индексу <c>outRange.Start + i</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения индикатора:  
    /// - <b>Start</b>: индекс первого элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == входной массив.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>), 
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>:  
    /// - <see cref="Core.RetCode.Success"/> при успешном расчете.  
    /// - Код ошибки при проблемах с входными данными.
    /// </returns>
    /// <remarks>
    /// Линия накопления/распределения Чайкина (Chaikin A/D Line) измеряет кумулятивный денежный поток в актив или из него 
    /// через анализ цены и объема, отражая баланс спроса и предложения.
    /// <para>
    /// Может подтверждать ценовые тренды и выявлять их возможные развороты. 
    /// Часто используется с другими объемными или импульсными индикаторами.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет множителя денежного потока (MFM - Money Flow Multiplier) для каждого периода:
    ///       <code>
    ///         MFM = ((Close - Low) - (High - Close)) / (High - Low)
    ///       </code>
    ///       Где:  
    ///       - Close: цена закрытия (Close)  
    ///       - High: максимальная цена (High)  
    ///       - Low: минимальная цена (Low)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет объема денежного потока (MFV - Money Flow Volume):
    ///       <code>
    ///         MFV = MFM * Volume
    ///       </code>
    ///       Где:  
    ///       - Volume: объем торгов (Volume)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Кумулятивное суммирование MFV для формирования линии A/D:
    ///       <code>
    ///         A/D Line = Накопленная сумма(MFV)
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост линии A/D указывает на давление покупок (спрос превышает предложение).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Падение линии A/D сигнализирует о давлении продаж (предложение превышает спрос).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Дивергенции между линией A/D и ценой могут предвещать развороты тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Ad<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AdImpl(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает lookback-период для <see cref="Ad{T}"/>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для расчета не требуется исторических данных. 
    /// Валидное значение индикатора может быть рассчитано начиная с первого бара (индекс 0).
    /// </returns>
    [PublicAPI]
    public static int AdLookback() => 0;

    /// <remarks>
    /// Реализация для совместимости с абстрактным API (работа с массивами вместо Span).
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Ad<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        T[] inVolume,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AdImpl<T>(inHigh, inLow, inClose, inVolume, inRange, outReal, out outRange);

    private static Core.RetCode AdImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением (конец на 0)
        outRange = Range.EndAt(0);

        // Проверка валидности входных диапазонов и длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Начальный и конечный индексы для обработки данных
        var (startIdx, endIdx) = rangeIndices;
        // Количество обрабатываемых баров в заданном диапазоне
        var nbBar = endIdx - startIdx + 1;
        // Установка диапазона выходных данных (валидные значения начинаются с startIdx)
        outRange = new Range(startIdx, startIdx + nbBar);

        // Текущий индекс обрабатываемого бара во входных данных
        var currentBar = startIdx;
        // Индекс для записи результатов в выходной массив outReal
        var outIdx = 0;
        // Текущее накопленное значение линии накопления/распределения (A/D)
        var ad = T.Zero;

        // Цикл обработки каждого бара в заданном диапазоне
        while (nbBar != 0)
        {
            // Расчет накопленного значения A/D для текущего бара
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref currentBar, ad);
            // Запись рассчитанного значения в выходной массив
            outReal[outIdx++] = ad;
            // Уменьшение счетчика оставшихся баров
            nbBar--;
        }

        // Возврат кода успешного завершения операции
        return Core.RetCode.Success;
    }
}
