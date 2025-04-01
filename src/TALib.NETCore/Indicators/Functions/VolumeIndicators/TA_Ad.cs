//Название файла: TA_Ad.cs
//Группы к которым можно отнести индикатор:
//VolumeIndicators (существующая папка - идеальное соответствие категории)
//AccumulationDistribution (альтернатива для акцента на типе индикатора)
//MoneyFlowIndicators (альтернатива для группировки по методу расчета)

namespace TALib;
public static partial class Functions
{
    /// <summary>
    /// Chaikin A/D Line (Volume Indicators) | Линия накопления/распределения Чайкина
    /// </summary>
    /// <param name="inHigh">Массив входных максимальных цен.</param>
    /// <param name="inLow">Массив входных минимальных цен.</param>
    /// <param name="inClose">Массив входных цен закрытия.</param>
    /// <param name="inVolume">Массив входных объемов торгов.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатываются все доступные данные.
    /// </param>
    /// <param name="outReal">Массив для сохранения рассчитанных значений индикатора.</param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых получены валидные значения:  
    /// - <b>Start</b>: индекс первого обработанного элемента.  
    /// - <b>End</b>: индекс последнего обработанного элемента.
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
    /// Линия накопления/распределения Чайкина измеряет кумулятивный денежный поток в/из актива 
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
    ///       Расчет множителя денежного потока (MFM) для каждого периода:
    ///       <code>
    ///         MFM = ((Close - Low) - (High - Close)) / (High - Low)
    ///       </code>
    ///       Где:  
    ///       - Close: цена закрытия  
    ///       - High: максимальная цена  
    ///       - Low: минимальная цена
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет объема денежного потока (MFV):
    ///       <code>
    ///         MFV = MFM * Volume
    ///       </code>
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
    /// <returns>Всегда 0, так как для расчета не требуется исторических данных.</returns>
    [PublicAPI]
    public static int AdLookback() => 0;

    /// <remarks>
    /// Реализация для совместимости с абстрактным API
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
        outRange = Range.EndAt(0);

        // Проверка валидности входных диапазонов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length, inVolume.Length) is not
            { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices; // Начальный и конечный индексы для обработки
        var nbBar = endIdx - startIdx + 1; // Количество обрабатываемых баров
        outRange = new Range(startIdx, startIdx + nbBar); // Диапазон выходных данных

        var currentBar = startIdx; // Текущий индекс обрабатываемого бара
        var outIdx = 0; // Индекс для записи результатов в outReal
        var ad = T.Zero; // Текущее значение линии накопления/распределения

        while (nbBar != 0)
        {
            // Расчет накопленного значения A/D
            ad = FunctionHelpers.CalcAccumulationDistribution(inHigh, inLow, inClose, inVolume, ref currentBar, ad);
            outReal[outIdx++] = ad;
            nbBar--;
        }

        return Core.RetCode.Success;
    }
}
