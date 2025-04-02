//Название файла: TA_TRange.cs
//Группы к которым можно отнести индикатор:
//VolatilityIndicators (существующая папка - идеальное соответствие категории)
//PriceTransform (альтернатива, если требуется группировка по типу индикатора)
//RangeIndicators (альтернатива для акцента на диапазоне цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// True Range (Volatility Indicators) — Истинный диапазон (Индикаторы волатильности)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены.</param>
    /// <param name="inLow">Входные минимальные цены.</param>
    /// <param name="inClose">Входные цены закрытия.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>, <c>inLow[outRange.Start + i]</c> и <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// TRANGE рассчитывает максимум из нескольких метрик диапазона, служащих фундаментальной мерой в индикаторах волатильности, таких как ATR.
    /// <para>
    /// Функция сама по себе измеряет волатильность. Использование её в качестве входных данных для <see cref="Atr{T}">ATR</see> или
    /// других стратегий, основанных на волатильности, может улучшить адаптацию к изменяющимся рыночным условиям.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить текущую разницу между максимальной и минимальной ценой: <c>val1 = High[today] - Low[today]</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать разницу между вчерашней ценой закрытия и сегодняшней максимальной ценой: <c>val2 = |High[today] - Close[yesterday]|</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать разницу между вчерашней ценой закрытия и сегодняшней минимальной ценой: <c>val3 = |Low[today] - Close[yesterday]|</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выбрать наибольшее из этих трех значений: <c>TrueRange = Max(val1, val2, val3)</c>.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокое значение указывает на более высокую волатильность рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкое значение указывает на более стабильный или менее волатильный рынок.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode TRange<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TRangeImpl(inHigh, inLow, inClose, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="TRange{T}">TRange</see>.
    /// </summary>
    /// <returns>Всегда 1, так как для этого расчета требуется только один ценовой бар.</returns>
    [PublicAPI]
    public static int TRangeLookback() => 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode TRange<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TRangeImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange);

    private static Core.RetCode TRangeImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        /* Истинный диапазон — это наибольшее из следующих значений:
         *
         *   val1 = расстояние от сегодняшней максимальной цены до сегодняшней минимальной цены.
         *   val2 = расстояние от вчерашней цены закрытия до сегодняшней максимальной цены.
         *   val3 = расстояние от вчерашней цены закрытия до сегодняшней минимальной цены.
         *
         * Некоторые книги и программы делают первое значение TRANGE равным (High - Low) первого бара.
         * Эта функция вместо этого игнорирует первый ценовой бар, и только выходные данные, начиная со второго ценового бара, являются допустимыми.
         * Это сделано для избежания несоответствий.
         */

        var lookbackTotal = TRangeLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var today = startIdx;
        while (today <= endIdx)
        {
            var tempHT = inHigh[today]; // Временная переменная для хранения сегодняшней максимальной цены
            var tempLT = inLow[today]; // Временная переменная для хранения сегодняшней минимальной цены
            var tempCY = inClose[today - 1]; // Временная переменная для хранения вчерашней цены закрытия

            outReal[outIdx++] = FunctionHelpers.TrueRange(tempHT, tempLT, tempCY);
            today++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
