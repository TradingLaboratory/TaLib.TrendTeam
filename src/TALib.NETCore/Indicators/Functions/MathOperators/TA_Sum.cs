//Название файла TA_Sum.cs

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Суммирование (MathOperators)
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// SUM добавляет точки данных за определенный период, что полезно для накопительных расчетов или методов сглаживания.
    /// <para>
    /// Функция предоставляет скользящее суммирование, которое может использоваться в качестве базового значения для других расчетов или индикаторов.
    /// Его полезность заключается в создании пользовательских индикаторов, где накопленные данные информируют о тренде или импульсе.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Инициализировать сумму значений за первый период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого последующего периода обновлять сумму, добавляя новое значение и удаляя самое старое значение из суммы.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сохранять рассчитанную сумму для каждого периода в выходном массиве.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sum<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        SumImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предварительного просмотра для <see cref="Sum{T}">Sum</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого выходного значения, которое может быть рассчитано.</returns>
    [PublicAPI]
    public static int SumLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sum<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        SumImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode SumImpl<T>(
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

        var lookbackTotal = SumLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var periodTotal = T.Zero;
        var trailingIdx = startIdx - lookbackTotal;
        var i = trailingIdx;
        while (i < startIdx)
        {
            periodTotal += inReal[i++];
        }

        var outIdx = 0;
        do
        {
            periodTotal += inReal[i++];
            var tempReal = periodTotal;
            periodTotal -= inReal[trailingIdx++];
            outReal[outIdx++] = tempReal;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
