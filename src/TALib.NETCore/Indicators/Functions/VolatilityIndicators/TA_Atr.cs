// файл TA_Atr.cs
// Группы к которым можно отнести индикатор:
// VolatilityIndicators (существующая папка - идеальное соответствие категории)
// MomentumIndicators (альтернатива, если требуется группировка по типу индикатора)
// PriceRangeIndicators (альтернатива для акцента на диапазоне цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Средний Истинный Диапазон (Индикаторы Волатильности)
    /// </summary>
    /// <param name="inHigh">Входные данные для максимальных цен.</param>
    /// <param name="inLow">Входные данные для минимальных цен.</param>
    /// <param name="inClose">Входные данные для цен закрытия.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
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
    /// Средний Истинный Диапазон (ATR) — это индикатор волатильности, который измеряет степень изменения цены за указанный период времени.
    /// <para>
    /// Функция широко используется для установки уровней стоп-лосс, управления размерами позиций и оценки условий волатильности.
    /// Она может быть интегрирована с индикаторами тренда или импульса для улучшения управления рисками.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить Истинный Диапазон (TR), который является наибольшим из следующих значений:
    ///       <code>
    ///         TR = max[(High - Low), abs(High - Previous Close), abs(Low - Previous Close)]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для первого значения ATR вычислить простое среднее значений TR за указанный период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для последующих значений ATR использовать метод сглаживания Уайлдера:
    ///       <code>
    ///         ATR = [(Previous ATR * (Time Period - 1)) + Current TR] / Time Period
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокое значение указывает на высокую волатильность рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкое значение свидетельствует о снижении волатильности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       ATR не указывает направление движения цены, но отражает степень волатильности за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Atr<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AtrImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предыстории для <see cref="Atr{T}">Atr</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до вычисления первого выходного значения.</returns>
    [PublicAPI]
    public static int AtrLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 1 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Atr);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Atr<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AtrImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode AtrImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Средний Истинный Диапазон — это наибольшее из следующих значений:
         *
         *   val1 = расстояние от сегодняшнего максимума до сегодняшнего минимума.
         *   val2 = расстояние от вчерашнего закрытия до сегодняшнего максимума.
         *   val3 = расстояние от вчерашнего закрытия до сегодняшнего минимума.
         *
         * Эти значения усредняются за указанный период с использованием метода Уайлдера.
         * Метод имеет нестабильный период, сравнимый с экспоненциальным скользящим средним (EMA).
         */

        var lookbackTotal = AtrLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Учитываем случай, когда сглаживание не требуется.
        if (optInTimePeriod == 1)
        {
            return TRange(inHigh, inLow, inClose, inRange, outReal, out outRange);
        }

        Span<T> tempBuffer = new T[lookbackTotal + (endIdx - startIdx) + 1];
        var retCode = TRangeImpl(inHigh, inLow, inClose, new Range(startIdx - lookbackTotal + 1, endIdx), tempBuffer, out _);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Первое значение ATR — это простое среднее значений TRange за указанный период.
        Span<T> prevATRTemp = new T[1];
        retCode = FunctionHelpers.CalcSimpleMA(tempBuffer, new Range(optInTimePeriod - 1, optInTimePeriod - 1), prevATRTemp, out _,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);

        var prevATR = prevATRTemp[0];

        /* Последующие значения сглаживаются с использованием предыдущего значения ATR (метод Уайлдера).
         *   1) Умножить предыдущее значение ATR на 'период - 1'.
         *   2) Добавить сегодняшнее значение TR.
         *   3) Разделить на 'период'.
         */
        var today = optInTimePeriod;
        var outIdx = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Atr);
        // Пропускаем нестабильный период.
        while (outIdx != 0)
        {
            prevATR *= timePeriod - T.One;
            prevATR += tempBuffer[today++];
            prevATR /= timePeriod;
            outIdx--;
        }

        outIdx = 1;
        outReal[0] = prevATR;

        // Выполняем количество запрошенных ATR.
        var nbATR = endIdx - startIdx + 1;

        while (--nbATR != 0)
        {
            prevATR *= timePeriod - T.One;
            prevATR += tempBuffer[today++];
            prevATR /= timePeriod;
            outReal[outIdx++] = prevATR;
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return retCode;
    }
}
