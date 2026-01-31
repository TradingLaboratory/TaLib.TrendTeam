// TA_Roc.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// RateIndicators (альтернатива для группировки по типу расчёта скорости изменения)
// PriceMomentum (альтернатива с акцентом на импульс цены)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Rate of Change (Momentum Indicators) — Скорость изменения (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта индикатора (цены закрытия Close, другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End.Value - outRange.Start.Value</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start.Value + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End.Value == inReal.Length</c> при успешном расчёте.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, 0)</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (количество баров для сравнения текущего значения с прошлым)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчёта.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор Rate of Change (ROC) рассчитывает процентное изменение значения временного ряда за указанный период.
    /// Широко применяется как индикатор импульса для оценки скорости изменения цен во времени.
    /// </para>
    /// <para>
    /// Функция схожа с <see cref="Ppo{T}">PPO</see>, но в отличие от PPO сравнивает исходные значения (raw values), а не скользящие средние.
    /// Используется для выявления состояний перекупленности или перепроданности на финансовых рынках.
    /// Подтверждение сигналов ROC с помощью индикаторов тренда или волатильности снижает вероятность ложных сигналов от краткосрочных колебаний.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение значения, которое было <paramref name="optInTimePeriod"/> периодов назад:
    ///       <code>
    ///         PreviousValue = data[currentIndex - optInTimePeriod]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт ROC как процентного изменения от предыдущего значения к текущему:
    ///       <code>
    ///         ROC = ((CurrentValue / PreviousValue) - 1) * 100
    ///       </code>
    ///       где <c>CurrentValue</c> — текущее значение ряда, а <c>PreviousValue</c> — значение,
    ///       отстоящее на <paramref name="optInTimePeriod"/> периодов назад.
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
    ///       Значение около нуля означает отсутствие значимого изменения цены за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Roc<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback period) для индикатора <see cref="Roc{T}">Roc</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>
    /// Количество периодов, необходимых до первого валидного значения индикатора.
    /// Первое валидное значение будет доступно только начиная с индекса, равного этому периоду.
    /// </returns>
    [PublicAPI]
    public static int RocLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Roc<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode RocImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов относительно длины входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода: должен быть положительным целым числом
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* ROC и ROCP центрированы относительно нуля и могут принимать положительные и отрицательные значения.
         * Эквивалентные формулы:
         *   ROC = ROCP / 100
         *       = ((price - prevPrice) / prevPrice) / 100
         *       = ((price / prevPrice) - 1) * 100
         *
         * RocR и RocR100 — это отношения, центрированные соответственно относительно 1 и 100,
         * и всегда принимают положительные значения.
         */

        // Расчёт минимального индекса входных данных, с которого можно получить первое валидное значение индикатора
        var lookbackTotal = RocLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback периода нет данных для расчёта — возвращаем успех с пустым результатом
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;
        // Текущий индекс во входных данных
        var inIdx = startIdx;
        // Индекс предыдущего значения (отстоящего на optInTimePeriod периодов назад)
        var trailingIdx = startIdx - lookbackTotal;
        while (inIdx <= endIdx)
        {
            // Значение из прошлого периода (база для расчёта изменения)
            var tempReal = inReal[trailingIdx++];
            // Расчёт ROC: процентное изменение с защитой от деления на ноль
            // Если предыдущее значение равно нулю — результат устанавливается в ноль
            outReal[outIdx++] = !T.IsZero(tempReal) ? (inReal[inIdx] / tempReal - T.One) * FunctionHelpers.Hundred<T>() : T.Zero;
            inIdx++;
        }

        // Формирование выходного диапазона: от первого валидного индекса до последнего обработанного
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
