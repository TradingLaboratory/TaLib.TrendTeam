// Название файла: TA_Bop.cs
// Рекомендуемые категории размещения:
// MomentumIndicators (основная категория - индикатор измеряет импульс/силу движения цены)
// PriceTransform (альтернатива - трансформирует ценовые данные в нормализованное значение)
// BalanceIndicators (альтернатива для группировки по концепции баланса покупателей/продавцов)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Balance of Power (Momentum Indicators) — Баланс Сил (Индикаторы Импульса)
    /// </summary>
    /// <param name="inOpen">Входные данные для цен открытия (Open)</param>
    /// <param name="inHigh">Входные данные для максимальных цен (High)</param>
    /// <param name="inLow">Входные данные для минимальных цен (Low)</param>
    /// <param name="inClose">Входные данные для цен закрытия (Close)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inOpen[outRange.Start + i]</c>, <c>inHigh[outRange.Start + i]</c>, <c>inLow[outRange.Start + i]</c> и <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inOpen"/>, <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inOpen.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inOpen"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Баланс Сил (BOP) — импульсный индикатор, измеряющий относительную силу покупателей против продавцов
    /// путём сравнения позиции цены закрытия (Close) внутри дневного торгового диапазона (High–Low).
    /// </para>
    /// <para>
    /// Индикатор нормализует разницу между ценой закрытия и открытия относительно полного торгового диапазона бара,
    /// что позволяет сравнивать силу движения независимо от абсолютной волатильности инструмента.
    /// </para>
    ///
    /// <b>Формула расчета</b>:
    /// <code>
    ///   BOP = (Close - Open) / (High - Low)
    /// </code>
    /// где:
    /// - <c>Close - Open</c> — разница между ценой закрытия и открытия (направление движения внутри бара)
    /// - <c>High - Low</c> — полный торговый диапазон бара (нормализующий множитель)
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение BOP (> 0): доминирование покупателей, цена закрытия выше цены открытия.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательное значение BOP (< 0): доминирование продавцов, цена закрытия ниже цены открытия.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение близкое к нулю: равновесие сил покупателей и продавцов внутри бара.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения в диапазоне [-1.0, +1.0]: теоретические границы индикатора (достигаются при экстремальных сценариях).
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Особенности</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Период задержки (lookback) = 0: для расчёта каждого значения требуется только один бар.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       При нулевом торговом диапазоне (High == Low) значение индикатора принимается равным 0 для избежания деления на ноль.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Bop<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        BopImpl(inOpen, inHigh, inLow, inClose, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период задержки (lookback) для индикатора <see cref="Bop{T}"/>.
    /// </summary>
    /// <returns>
    /// Всегда возвращает 0, так как для расчёта каждого значения BOP требуется только один бар
    /// (не нужны исторические данные предыдущих периодов).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Период задержки определяет минимальное количество баров, необходимых для расчёта первого валидного значения индикатора.
    /// </para>
    /// <para>
    /// Для BOP lookback = 0 означает, что первое значение индикатора может быть рассчитано для самого первого бара во входных данных.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int BopLookback() => 0;

    /// <summary>
    /// Внутренняя реализация индикатора Balance of Power для совместимости с устаревшим API.
    /// </summary>
    /// <remarks>
    /// Приватный метод-обёртка для поддержки массивов вместо Span. Перенаправляет вызов в основную реализацию <see cref="BopImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Bop<T>(
        T[] inOpen,
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        BopImpl<T>(inOpen, inHigh, inLow, inClose, inRange, outReal, out outRange);

    private static Core.RetCode BopImpl<T>(
        ReadOnlySpan<T> inOpen,
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона значением [0, 0) — пустой диапазон (пока нет валидных значений)
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона и длины массивов
        // Валидация гарантирует, что все входные массивы имеют достаточную длину для запрошенного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inOpen.Length, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Вычисление торгового диапазона бара: разница между максимальной и минимальной ценой (High - Low)
            var tempReal = inHigh[i] - inLow[i];

            // Расчёт значения BOP с защитой от деления на ноль:
            // - Если торговый диапазон > 0: BOP = (Close - Open) / (High - Low)
            // - Если торговый диапазон = 0 (High == Low): возвращаем 0 (нейтральное значение)
            outReal[outIdx++] = tempReal > T.Zero ? (inClose[i] - inOpen[i]) / tempReal : T.Zero;
        }

        // Формирование выходного диапазона: все обработанные бары имеют валидные значения (lookback = 0)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
