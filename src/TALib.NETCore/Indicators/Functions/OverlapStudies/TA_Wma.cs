// Wma.cs
// Группы к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// TrendIndicators (альтернатива для группировки по определению тренда)
// SmoothingIndicators (альтернатива для акцента на сглаживании данных)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Weighted Moving Average (Overlap Studies) — Взвешенная скользящая средняя (Индикаторы наложения)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены Close, другие индикаторы или временные ряды)</param>
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
    /// - Гарантируется: <c>End.Value == inReal.Length</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, 0]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчета индикатора (количество баров для усреднения).</param>
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
    /// Взвешенная скользящая средняя (WMA) — это разновидность скользящей средней, которая присваивает больший вес более
    ///_recentным_ данным. Такая взвешенность делает индикатор более чувствительным к недавним изменениям цен по сравнению
    /// с простой скользящей средней (SMA), которая обрабатывает все точки данных одинаково.
    /// </para>
    /// <para>
    /// Индикатор эффективно отражает краткосрочные тренды. Комбинация WMA с осцилляторами или другими индикаторами
    /// импульса улучшает точность входов и повышает отзывчивость в условиях меняющегося рынка.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого периода вычисляется взвешенная сумма входных значений:
    ///       <code>
    ///         Weighted Sum = Σ(Value * Weight), где веса последовательно равны 1, 2, ..., n для n периодов.
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Взвешенная сумма делится на сумму весов для получения WMA:
    ///       <code>
    ///         WMA = Weighted Sum / Σ(Weights), где Σ(Weights) = n * (n + 1) / 2 для n периодов.
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Wma<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        WmaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период запаздывания (lookback period) для метода <see cref="Wma{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета индикатора.</param>
    /// <returns>
    /// Количество периодов, необходимых до появления первого валидного значения индикатора.
    /// Для WMA равен <c>optInTimePeriod - 1</c>.
    /// </returns>
    [PublicAPI]
    public static int WmaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Wma<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        WmaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode WmaImpl<T>(
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

        // Проверка минимально допустимого периода (должен быть >= 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Алгоритм использует базовое свойство умножения/сложения: (x * 2) = x + x
         *
         * Пример для 3-периодной WMA, которую можно интерпретировать двумя способами:
         *   (x1 * 1) + (x2 * 2) + (x3 * 3)
         *     ИЛИ
         *   x1 + x2 + x2 + x3 + x3 + x3 (это periodSum)
         *
         * При переходе к следующему бару periodSum быстро корректируется путем вычитания:
         *   x1 + x2 + x3 (это periodSub)
         * В результате periodSum становится равным:
         *   x2 + x3 + x3
         *
         * Затем добавляется новое значение x4 + x4 + x4, получая:
         *   x2 + x3 + x3 + x4 + x4 + x4
         *
         * На этом завершается одна итерация, и алгоритм возвращается к шагу 1.
         *
         * Такой подход минимизирует количество обращений к памяти и операций с плавающей точкой.
         */

        // Период запаздывания (количество баров, необходимых для первого валидного значения)
        var lookbackTotal = WmaLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом периода запаздывания
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — расчет не требуется
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;
        // Индекс самого старого бара в текущем окне расчета
        var trailingIdx = startIdx - lookbackTotal;

        // Сумма значений в текущем окне (используется для коррекции при сдвиге окна)
        var periodSub = T.Zero;
        // Взвешенная сумма значений в текущем окне
        var periodSum = periodSub;
        // Текущий индекс обработки входных данных
        var inIdx = trailingIdx;
        // Текущий вес для взвешивания значений при инициализации
        var i = 1;
        // Инициализация сумм для первого окна расчета
        while (inIdx < startIdx)
        {
            var tempReal = inReal[inIdx++];
            periodSub += tempReal;                    // Накопление простой суммы
            periodSum += tempReal * T.CreateChecked(i); // Накопление взвешенной суммы с весом = i
            i++;
        }

        // Значение самого старого бара в окне (будет вычитаться при сдвиге окна)
        var trailingValue = T.Zero;

        // Делитель для нормализации взвешенной суммы (всегда целое число)
        // По формуле арифметической прогрессии: 1 + 2 + 3 + ... + n = n * (n + 1) / 2
        var divider = T.CreateChecked((optInTimePeriod * (optInTimePeriod + 1)) >> 1);
        // Вес для нового значения при добавлении в окно (равен периоду)
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Основной цикл расчета WMA для запрошенного диапазона
        while (inIdx <= endIdx)
        {
            // Добавление нового значения в окно расчета
            var tempReal = inReal[inIdx++];
            periodSub += tempReal;        // Добавление нового значения в простую сумму
            periodSub -= trailingValue;   // Вычитание самого старого значения из простой суммы
            periodSum += tempReal * timePeriod; // Добавление нового значения с максимальным весом в взвешенную сумму

            // Сохранение значения для вычитания на следующей итерации
            // Выполняется до записи результата, так как входной и выходной буферы могут совпадать
            trailingValue = inReal[trailingIdx++];

            // Расчет и запись значения WMA для текущего бара
            outReal[outIdx++] = periodSum / divider;

            // Коррекция взвешенной суммы для следующей итерации (вычитание простой суммы)
            periodSum -= periodSub;
        }

        // Установка диапазона валидных значений в выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
