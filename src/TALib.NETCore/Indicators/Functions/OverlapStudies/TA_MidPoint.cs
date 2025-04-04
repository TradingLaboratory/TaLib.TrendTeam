//Название файла: TA_MidPoint.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//PriceTransform (альтернатива, если требуется группировка по типу индикатора)
//TrendIndicators (альтернатива для акцента на трендовых индикаторах)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MidPoint over period (Overlap Studies) — Средняя точка за период (Накладывающиеся исследования)
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Индикатор MidPoint вычисляет среднее значение между максимальными и минимальными значениями за указанный период,
    /// предоставляя центральное значение временного ряда.
    /// Он может использоваться для идентификации трендов или подтверждения уровней поддержки или сопротивления в ценовых движениях.
    /// <para>
    /// Интеграция с <see cref="Bbands{T}">Bollinger Bands</see> или осцилляторами может выявить
    /// возможности, когда цена колеблется вокруг равновесия. См. <see cref="MidPrice{T}">MidPrice</see> для связанной функции
    /// при работе с временными рядами максимальных и минимальных цен.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить максимальное значение за указанный период:
    ///       <code>
    ///         Highest Value = Max(inReal[i] for i in range(trailingIdx, today))
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определить минимальное значение за указанный период:
    ///       <code>
    ///         Lowest Value = Min(inReal[i] for i in range(trailingIdx, today))
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить среднюю точку по формуле:
    ///       <code>
    ///         MidPoint = (Highest Value + Lowest Value) / 2
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходные данные представляют собой сглаженную линию, которая фильтрует краткосрочные колебания и выделяет
    ///       центральную тенденцию временного ряда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MidPoint<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MidPointImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="MidPoint{T}">MidPoint</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до вычисления первого выходного значения.</returns>
    [PublicAPI]
    public static int MidPointLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MidPoint<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MidPointImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MidPointImpl<T>(
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

        /* Найти максимальное и минимальное значение временного ряда за период.
         *   MidPoint = (Highest Value + Lowest Value) / 2
         *
         * См. MidPrice, если входные данные представляют собой цену с максимальными и минимальными значениями.
         */

        var lookbackTotal = MidPointLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжить вычисление для запрашиваемого диапазона.
        // Алгоритм позволяет использовать один и тот же буфер для входных и выходных данных.
        var outIdx = 0;
        var today = startIdx;
        var trailingIdx = startIdx - lookbackTotal;
        while (today <= endIdx)
        {
            var lowest = inReal[trailingIdx++];
            var highest = lowest;
            for (var i = trailingIdx; i <= today; i++)
            {
                var tmp = inReal[i];
                if (tmp < lowest)
                {
                    lowest = tmp;
                }
                else if (tmp > highest)
                {
                    highest = tmp;
                }
            }

            outReal[outIdx++] = (highest + lowest) / FunctionHelpers.Two<T>();
            today++;
        }

        // Сохранить outBegIdx относительно входных данных перед возвратом.
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
