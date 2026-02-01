//Название файла: TA_RocR.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//PriceTransform (альтернатива для акцента на преобразовании ценовых данных)
//RelativeIndicators (альтернатива для группировки относительных показателей)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Rate of Change Ratio (ROCR) (Momentum Indicators) — Коэффициент скорости изменения (Индикаторы импульса)
    /// <para>
    /// Измеряет отношение текущего значения к значению за указанное количество периодов назад.
    /// Результат всегда положительный, центрирован относительно единицы (1.0).
    /// </para>
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
    /// <param name="optInTimePeriod">Период расчета (количество баров для сравнения с текущим значением)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Коэффициент скорости изменения (ROCR) — это индикатор импульса, измеряющий отношение текущего значения
    /// к значению за указанное количество периодов в прошлом. Показатель предоставляет информацию об относительных
    /// изменениях и силе тренда, где значения обычно сосредоточены вокруг 1.0, что означает отсутствие изменения.
    /// </para>
    /// <para>
    /// Индикатор всегда положительный и особенно полезен для выявления пропорциональных соотношений между точками данных
    /// во времени. Широко применяется в финансовом анализе для оценки относительной динамики цен и обнаружения трендов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
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
    ///       Расчет ROCR как отношения текущего значения к предыдущему:
    ///       <code>
    ///         ROCR = CurrentValue / PreviousValue
    ///       </code>
    ///       где <c>CurrentValue</c> — значение в текущей позиции, а <c>PreviousValue</c>
    ///       — значение за <paramref name="optInTimePeriod"/> шагов ранее.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение больше 1.0 указывает на восходящий импульс (текущее значение выше предыдущего).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение меньше 1.0 указывает на нисходящий импульс (текущее значение ниже предыдущего).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение ровно 1.0 означает отсутствие изменения за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Сравнение с другими вариантами индикатора скорости изменения</b>:
    /// <para>
    /// ROC и ROCP центрированы относительно нуля и могут принимать положительные и отрицательные значения:
    /// <c>ROC = ROCP / 100 = ((price - prevPrice) / prevPrice) / 100 = ((price / prevPrice) - 1) * 100</c>
    /// </para>
    /// <para>
    /// RocR и RocR100 представляют собой коэффициенты, центрированные относительно 1 и 100 соответственно,
    /// и всегда принимают положительные значения.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode RocR<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocRImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период запаздывания (lookback) для индикатора <see cref="RocR{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета индикатора.</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int RocRLookback(int optInTimePeriod = 10) => optInTimePeriod < 1 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode RocR<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 10) where T : IFloatingPointIeee754<T> =>
        RocRImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode RocRImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода расчета (должен быть >= 1)
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* ROC и ROCP центрированы относительно нуля и могут принимать положительные и отрицательные значения. Эквивалентности:
         *   ROC = ROCP / 100
         *       = ((price - prevPrice) / prevPrice) / 100
         *       = ((price / prevPrice) - 1) * 100
         *
         * RocR и RocR100 — это коэффициенты, центрированные относительно 1 и 100 соответственно,
         * и всегда принимают положительные значения.
         */

        // Расчет минимального индекса для первого валидного значения (период запаздывания)
        var lookbackTotal = RocRLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учета периода запаздывания нет данных для расчета — возврат успеха без значений
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;          // Индекс для записи в выходной массив
        var inIdx = startIdx;    // Текущий индекс во входных данных
        var trailingIdx = startIdx - lookbackTotal; // Индекс предыдущего значения (за optInTimePeriod периодов назад)

        // Цикл расчета индикатора для каждого бара в диапазоне
        while (inIdx <= endIdx)
        {
            // Получение предыдущего значения (базы для расчета отношения)
            var tempReal = inReal[trailingIdx++];

            // Расчет отношения текущего значения к предыдущему
            // Защита от деления на ноль: если предыдущее значение равно нулю — результат 0
            outReal[outIdx++] = !T.IsZero(tempReal) ? inReal[inIdx] / tempReal : T.Zero;

            inIdx++;
        }

        // Установка диапазона валидных выходных значений
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
