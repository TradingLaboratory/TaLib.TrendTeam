//Название файла: TA_RocR100.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//PriceTransform (альтернатива для акцента на преобразовании ценовых данных)
//StatisticFunctions (альтернатива для статистических расчётов соотношений)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Rate of change ratio 100 scale: (price/prevPrice)*100 (Momentum Indicators) — Темп изменения в масштабе 100: (Close/предыдущий Close)*100 (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены закрытия <see cref="Close"/>, но могут использоваться и другие временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период расчёта (количество баров для сравнения текущей цены с ценой в прошлом)</param>
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
    /// Индикатор темпа изменения в масштабе 100 (ROCR100) измеряет соотношение между текущей ценой и ценой,
    /// зафиксированной <paramref name="optInTimePeriod"/> периодов назад, умноженное на 100. 
    /// Значения всегда положительны и центрированы вокруг уровня 100, где 100 означает отсутствие изменения цены.
    /// </para>
    /// <para>
    /// Индикатор особенно полезен для анализа пропорциональных изменений в нормализованном формате,
    /// что упрощает сравнение активов с разными ценовыми диапазонами. Значения выше 100 указывают на рост,
    /// ниже 100 — на снижение относительно базового периода.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение значения <paramref name="optInTimePeriod"/> периодов назад:
    ///       <code>
    ///         PreviousValue = inReal[currentIndex - optInTimePeriod]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт ROCR100 как масштабированного соотношения текущего значения к предыдущему:
    ///       <code>
    ///         ROCR100 = (CurrentValue / PreviousValue) * 100
    ///       </code>
    ///       где <c>CurrentValue</c> — значение на текущей позиции, а <c>PreviousValue</c> — 
    ///       значение, зафиксированное <paramref name="optInTimePeriod"/> баров ранее.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение выше 100 указывает на восходящий импульс (текущая цена выше цены <paramref name="optInTimePeriod"/> периодов назад).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение ниже 100 указывает на нисходящий импульс (текущая цена ниже цены <paramref name="optInTimePeriod"/> периодов назад).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение ровно 100 означает отсутствие изменения цены за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Связь с другими индикаторами семейства ROC</b>:
    /// <para>
    ///   ROC и ROCP центрированы вокруг нуля и могут принимать положительные и отрицательные значения:
    ///   <code>
    ///     ROC = ROCP / 100
    ///         = ((price - prevPrice) / prevPrice) / 100
    ///         = ((price / prevPrice) - 1) * 100
    ///   </code>
    /// </para>
    /// <para>
    ///   RocR и RocR100 представляют соотношение, центрированное соответственно вокруг 1 и 100, и всегда имеют положительные значения.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode RocR100<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocR100Impl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback period) для <see cref="RocR100{T}">RocR100</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта индикатора.</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int RocR100Lookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode RocR100<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocR100Impl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode RocR100Impl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода расчёта (должен быть >= 1)
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Roc и RocP центрированы вокруг нуля и могут иметь положительные и отрицательные значения. Эквивалентные преобразования:
         *   ROC = ROCP/100
         *       = ((price - prevPrice) / prevPrice) / 100
         *       = ((price / prevPrice) - 1) * 100
         *
         * RocR и RocR100 представляют соотношение, центрированное соответственно вокруг 1 и 100, и всегда имеют положительные значения.
         */

        // Расчёт минимального количества баров для получения первого валидного значения
        var lookbackTotal = RocR100Lookback(optInTimePeriod);
        // Корректировка начального индекса с учётом периода задержки
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — валидных данных нет
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;                  // Индекс для записи результатов в выходной массив
        var inIdx = startIdx;            // Текущий индекс во входных данных
        var trailingIdx = startIdx - lookbackTotal; // Индекс для доступа к значению периода задержки назад

        // Основной цикл расчёта индикатора
        while (inIdx <= endIdx)
        {
            // Получение значения из прошлого (на optInTimePeriod баров назад)
            var tempReal = inReal[trailingIdx++];
            // Расчёт ROCR100: (текущее значение / значение периода назад) * 100
            // Проверка деления на ноль для предотвращения ошибок
            outReal[outIdx++] = !T.IsZero(tempReal) ? inReal[inIdx] / tempReal * FunctionHelpers.Hundred<T>() : T.Zero;
            inIdx++;
        }

        // Формирование диапазона валидных значений в выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
