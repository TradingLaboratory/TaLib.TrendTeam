// Momentum.cs
// Группы к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// PriceTransform (альтернатива при акценте на преобразование ценовых данных)
// RateOfChange (альтернатива при группировке по принципу расчёта изменения)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Momentum (Momentum Indicators) — Моментум (Индикаторы момента)
    /// <para>
    /// Индикатор Моментум измеряет скорость изменения цены за указанный период, показывая силу движения цены в заданном направлении.
    /// Это простой, но эффективный индикатор импульса, вычисляющий абсолютную разницу между текущей ценой и ценой <paramref name="optInTimePeriod"/> периодов назад.
    /// </para>
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта индикатора (обычно цены закрытия <c>Close</c>, но могут быть и другие временные ряды)</param>
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
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчёт успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (количество баров для сравнения текущей цены с ценой в прошлом)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор Моментум вычисляет скорость изменения цены за указанный период, подчёркивая темп движения цены в заданном направлении.
    /// Это простой, но эффективный индикатор импульса.
    /// </para>
    /// <para>
    /// Индикатор Моментум не нормализован, то есть его значения представляют абсолютные разницы, а не процентные изменения.
    /// Из-за отсутствия нормализации его не следует использовать для сравнения различных временных рядов с разными уровнями цен.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение значения за указанное количество периодов назад:
    ///       <code>
    ///         previousValue = data[today - optInTimePeriod]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление разницы между текущим значением и значением <paramref name="optInTimePeriod"/> периодов назад:
    ///       <code>
    ///         Momentum = data[today] - previousValue
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение указывает на восходящий импульс, что означает рост цен.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательное значение указывает на нисходящий импульс, что свидетельствует о падении цен.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение, близкое к нулю, указывает на слабое или отсутствующее ценовое движение за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Mom<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        MomImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период отката (lookback period) для индикатора <see cref="Mom{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>Количество периодов, необходимых до первого валидного значения индикатора.</returns>
    /// <remarks>
    /// Период отката равен значению <paramref name="optInTimePeriod"/>, так как для расчёта первого значения
    /// требуется доступ к данным, отстоящим на <paramref name="optInTimePeriod"/> периодов назад.
    /// </remarks>
    [PublicAPI]
    public static int MomLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Mom<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        MomImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MomImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода расчёта (должен быть >= 1)
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Индикатор Моментум является единственным ненормализованным индикатором в библиотеке,
        // поэтому его не следует использовать для сравнения различных временных рядов с разными уровнями цен.

        // Расчёт общего периода отката (равен указанному периоду расчёта)
        var lookbackTotal = MomLookback(optInTimePeriod);
        // Корректировка начального индекса с учётом периода отката
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — валидных данных нет
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Расчёт Моментума: вычитание значения, отстоящего на 'optInTimePeriod' периодов назад, из текущего значения
        var outIdx = 0;           // Индекс в выходном массиве для записи результатов
        var inIdx = startIdx;     // Текущий индекс во входном массиве
        var trailingIdx = startIdx - lookbackTotal; // Индекс значения, отстоящего на 'optInTimePeriod' периодов назад
        while (inIdx <= endIdx)
        {
            // Momentum[t] = Close[t] - Close[t - optInTimePeriod]
            outReal[outIdx++] = inReal[inIdx++] - inReal[trailingIdx++];
        }

        // Установка диапазона валидных значений в выходном массиве
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
