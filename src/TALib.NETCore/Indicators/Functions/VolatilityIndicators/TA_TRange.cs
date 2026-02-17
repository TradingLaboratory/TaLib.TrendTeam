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
    /// <param name="inHigh">
    /// Входные максимальные цены (High) для расчета индикатора.
    /// </param>
    /// <param name="inLow">
    /// Входные минимальные цены (Low) для расчета индикатора.
    /// </param>
    /// <param name="inClose">
    /// Входные цены закрытия (Close) для расчета индикатора.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// </para>
    /// <para>
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>, <c>inLow[outRange.Start + i]</c> и <c>inClose[outRange.Start + i]</c>.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// </para>
    /// <para>
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// </para>
    /// <para>
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// </para>
    /// <para>
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </para>
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
    /// Возвращает период обратного просмотра (lookback period) для <see cref="TRange{T}">TRange</see>.
    /// <para>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// Все бары в исходных данных с индексом меньше чем lookback будут пропущены, чтобы посчитать первое валидное значение индикатора.
    /// </para>
    /// </summary>
    /// <returns>Всегда 1, так как для этого расчета требуется только один ценовой бар (предыдущий Close для сравнения с текущим High/Low).</returns>
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
        // Инициализация outRange - диапазон для которых рассчитаны валидные значения индикатора
        outRange = Range.EndAt(0);

        // Проверка корректности входных диапазонов данных
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

        // Получение периода обратного просмотра (lookback) - количество баров необходимых для первого валидного значения
        var lookbackTotal = TRangeLookback();
        // Корректировка начального индекса с учётом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного - нет данных для обработки
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;  // Индекс для записи в выходной массив
        var today = startIdx;  // Текущий индекс бара для обработки
        while (today <= endIdx)
        {
            var tempHT = inHigh[today];      // Временная переменная для хранения сегодняшней максимальной цены (High)
            var tempLT = inLow[today];       // Временная переменная для хранения сегодняшней минимальной цены (Low)
            var tempCY = inClose[today - 1]; // Временная переменная для хранения вчерашней цены закрытия (Close)

            // Расчет Истинного диапазона и запись результата в выходной массив
            outReal[outIdx++] = FunctionHelpers.TrueRange(tempHT, tempLT, tempCY);
            today++;
        }

        // Установка диапазона выходных данных (outRange) - индексы первой и последней ячейки с валидными значениями
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
