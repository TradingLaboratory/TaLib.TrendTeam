//Название файла: TA_Bop.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//PriceTransform (альтернатива, если требуется группировка по типу индикатора)
//BalanceIndicators (альтернатива для акцента на балансе сил)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Balance of Power (Momentum Indicators) — Баланс Сил (Индикаторы Импульса)
    /// </summary>
    /// <param name="inOpen">Входные данные для цен открытия.</param>
    /// <param name="inHigh">Входные данные для максимальных цен.</param>
    /// <param name="inLow">Входные данные для минимальных цен.</param>
    /// <param name="inClose">Входные данные для цен закрытия.</param>
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
    /// Баланс Сил — это импульсный индикатор, который измеряет силу покупателей по сравнению с продавцами, сравнивая цену закрытия
    /// относительно цены открытия, нормализованной по торговому диапазону.
    /// <para>
    /// Функция может быть интегрирована с объемными или индикаторами ценового действия
    /// для обнаружения тонких изменений в настроениях и усиления сигналов от других инструментов.
    /// </para>
    ///
    /// <b>Формула расчета</b>:
    /// <code>
    ///   BOP = (Close - Open) / (High - Low)
    /// </code>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение указывает на доминирование покупателей, где цена закрытия выше цены открытия.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательное значение указывает на доминирование продавцов, где цена закрытия ниже цены открытия.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение близкое к нулю указывает на равновесие между покупателями и продавцами.
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
    /// Возвращает период задержки для <see cref="Bop{T}">Bop</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого расчета не требуется исторических данных.</returns>
    [PublicAPI]
    public static int BopLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
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
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inOpen.Length, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Вычисление временной переменной для нормализации
            var tempReal = inHigh[i] - inLow[i];
            // Вычисление значения BOP и запись его в выходной массив
            outReal[outIdx++] = tempReal > T.Zero ? (inClose[i] - inOpen[i]) / tempReal : T.Zero;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
