//Название файла: TA_Aroon.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//DirectionalIndicators (альтернатива для акцента на направлении тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Aroon (Momentum Indicators) — Арун (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входные данные для расчета индикатора (максимальные цены).</param>
    /// <param name="inLow">Входные данные для расчета индикатора (минимальные цены).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outAroonDown">Массив, содержащий ТОЛЬКО валидные значения индикатора Aroon Down.</param>
    /// <param name="outAroonUp">Массив, содержащий ТОЛЬКО валидные значения индикатора Aroon Up.</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outAroonDown"/> и <paramref name="outAroonUp"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outAroonDown"/> и <paramref name="outAroonUp"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> или <paramref name="inLow"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
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
    /// Индикатор Aroon является импульсным индикатором, который измеряет время между максимумами и минимумами
    /// за указанный период времени для определения силы и направления тренда. Он состоит из двух компонентов:
    /// Aroon Up и Aroon Down, которые вычисляются отдельно.
    /// <para>
    /// Функция особенно полезна для выявления разворотов тренда и оценки его силы.
    /// Она также может использоваться в сочетании с другими техническими индикаторами для подтверждения торговых сигналов или фильтрации шума.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить максимальный максимум и минимальный минимум в указанном периоде времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить Aroon Up:
    ///       <code>
    ///         Aroon Up = 100 * (Период времени - Дни с момента максимального максимума) / Период времени
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить Aroon Down:
    ///       <code>
    ///         Aroon Down = 100 * (Период времени - Дни с момента минимального минимума) / Период времени
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения близкие к 100 указывают на сильный восходящий тренд, тогда как значения близкие к 0 указывают на отсутствие недавних максимумов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения близкие к 100 указывают на сильный нисходящий тренд, тогда как значения близкие к 0 указывают на отсутствие недавних минимумов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Когда Aroon Up пересекает Aroon Down снизу вверх, это может сигнализировать о начале восходящего тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Когда Aroon Down пересекает Aroon Up снизу вверх, это может сигнализировать о начале нисходящего тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Aroon<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outAroonDown,
        Span<T> outAroonUp,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AroonImpl(inHigh, inLow, inRange, outAroonDown, outAroonUp, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период lookback для <see cref="Aroon{T}">Aroon</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до вычисления первого допустимого значения.</returns>
    [PublicAPI]
    public static int AroonLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Aroon<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outAroonDown,
        T[] outAroonUp,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AroonImpl<T>(inHigh, inLow, inRange, outAroonDown, outAroonUp, out outRange, optInTimePeriod);

    private static Core.RetCode AroonImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outAroonDown,
        Span<T> outAroonUp,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Эта функция использует оптимизированный по скорости алгоритм для логики min/max.
        // Возможно, сначала нужно посмотреть, как работает Min/Max, и эта функция станет понятнее.

        var lookbackTotal = AroonLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжаем вычисление для запрашиваемого диапазона.
        // Алгоритм позволяет использовать один и тот же буфер для входных и выходных данных.
        var outIdx = 0;
        var today = startIdx;
        var trailingIdx = startIdx - lookbackTotal;

        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;
        var factor = FunctionHelpers.Hundred<T>() / T.CreateChecked(optInTimePeriod);
        while (today <= endIdx)
        {
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            outAroonUp[outIdx] = factor * T.CreateChecked(optInTimePeriod - (today - highestIdx));
            outAroonDown[outIdx] = factor * T.CreateChecked(optInTimePeriod - (today - lowestIdx));

            outIdx++;
            trailingIdx++;
            today++;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
