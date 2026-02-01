//Название файла: TA_RocP.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//PriceTransform (альтернатива для акцента на преобразовании ценовых данных)
//RateIndicators (альтернатива для группировки по типу расчёта темпов изменения)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Rate of Change Percentage — Процентное изменение (Momentum Indicators — Импульсные индикаторы)
    /// Рассчитывает процентное изменение цены относительно значения за <paramref name="optInTimePeriod"/> периодов назад: (Close - Close[n]) / Close[n]
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта индикатора (цены закрытия <see cref="Close"/>, другие индикаторы или временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период расчёта (количество баров для сравнения текущего значения с предыдущим)</param>
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
    /// ROCP (Rate of Change Percentage) — импульсный индикатор, измеряющий процентное изменение цены
    /// за указанный период относительно предыдущего значения. Помогает оценить скорость и силу ценовых изменений,
    /// выявляя потенциальные развороты или продолжения тренда.
    /// </para>
    /// <para>
    /// В отличие от стандартного <see cref="Roc{T}">ROC</see>, который масштабируется на 100, ROCP напрямую выражает изменение
    /// как долю от предыдущего значения (дробное представление). Индикатор может интегрироваться с подтверждением тренда
    /// или осцилляторами для улучшения качества торговых решений и снижения ложных сигналов.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение значения за <paramref name="optInTimePeriod"/> периодов назад:
    ///       <code>
    ///         PreviousValue = data[currentIndex - optInTimePeriod]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление ROCP как дробного изменения от предыдущего значения к текущему:
    ///       <code>
    ///         ROCP = (CurrentValue - PreviousValue) / PreviousValue
    ///       </code>
    ///       где <c>CurrentValue</c> — значение на текущей позиции, а <c>PreviousValue</c> —
    ///       значение <paramref name="optInTimePeriod"/> шагов ранее.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения указывают на восходящий импульс — цены растут относительно прошлого периода.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения указывают на нисходящий импульс — цены падают относительно прошлого периода.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение ноль означает отсутствие изменения цены за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode RocP<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocPImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback period) для <see cref="RocP{T}">RocP</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int RocPLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode RocP<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocPImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode RocPImpl<T>(
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

        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Roc и RocP центрированы относительно нуля и могут принимать положительные и отрицательные значения. Эквивалентные преобразования:
         *   ROC = ROCP/100
         *       = ((Close - Close[n]) / Close[n]) / 100
         *       = ((Close / Close[n]) - 1) * 100
         *
         * RocR и RocR100 представляют собой отношения, центрированные соответственно относительно 1 и 100,
         * и всегда принимают положительные значения.
         */

        var lookbackTotal = RocPLookback(optInTimePeriod); // Период задержки: минимальное количество баров для первого валидного значения
        startIdx = Math.Max(startIdx, lookbackTotal);     // Сдвиг начального индекса с учётом периода задержки

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;                                    // Индекс для записи результатов в outReal
        var inIdx = startIdx;                              // Текущий индекс во входных данных
        var trailingIdx = startIdx - lookbackTotal;        // Индекс предыдущего значения (optInTimePeriod периодов назад)
        while (inIdx <= endIdx)
        {
            var tempReal = inReal[trailingIdx++];          // Значение Close[n] — цена за optInTimePeriod периодов назад
            // Расчёт ROCP: (Close - Close[n]) / Close[n]. Проверка деления на ноль
            outReal[outIdx++] = !T.IsZero(tempReal) ? (inReal[inIdx] - tempReal) / tempReal : T.Zero;
            inIdx++;
        }

        outRange = new Range(startIdx, startIdx + outIdx); // Установка диапазона валидных значений в выходных данных

        return Core.RetCode.Success;
    }
}
