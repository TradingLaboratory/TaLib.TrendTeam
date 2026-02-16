// Название файла: TA_Mavp.cs
// Группы, к которым можно отнести индикатор:
// OverlapStudies/Averages (основная категория — скользящие средние как индикаторы наложения)
// AdaptiveIndicators (альтернатива — акцент на адаптивной природе индикатора)
// VariablePeriodIndicators (альтернатива — группировка по принципу переменного периода)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Moving Average with Variable Period (Overlap Studies) — Скользящее среднее с переменным периодом (Индикаторы наложения)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или временные ряды)</param>
    /// <param name="inPeriods">Массив периодов, определяющих длину скользящего среднего (MA) для каждой точки данных. Каждое значение усекается до диапазона [<paramref name="optInMinPeriod"/>, <paramref name="optInMaxPeriod"/>].</param>
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
    /// <param name="optInMinPeriod">Минимально допустимый период для расчета скользящего среднего (по умолчанию: 2).</param>
    /// <param name="optInMaxPeriod">Максимально допустимый период для расчета скользящего среднего (по умолчанию: 30).</param>
    /// <param name="optInMAType">Тип скользящего среднего (MA Type): SMA, EMA, WMA и др.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор скользящего среднего с переменным периодом (MAVP) рассчитывает скользящее среднее, где период может динамически изменяться для каждой точки данных.
    /// Это позволяет адаптировать чувствительность индикатора к текущим рыночным условиям (например, повышать реактивность при высокой волатильности).
    /// </para>
    /// <para>
    /// <b>Этапы расчета:</b>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Усечение значений периода из <paramref name="inPeriods"/> до диапазона [<paramref name="optInMinPeriod"/>, <paramref name="optInMaxPeriod"/>].
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого уникального периода выполняется однократный расчет скользящего среднего типа <paramref name="optInMAType"/>.
    ///       Результаты кэшируются и повторно используются для всех точек с одинаковым периодом.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Заполнение выходного массива <paramref name="outReal"/> рассчитанными значениями.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Интерпретация результатов:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более короткие периоды создают более реактивное скользящее среднее, чувствительное к краткосрочным колебаниям цены.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более длинные периоды создают сглаженное скользящее среднее, фильтрующее шум и отражающее долгосрочную тенденцию.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Динамическая смена периода позволяет индикатору адаптироваться к изменяющимся рыночным условиям без перерасчета всей истории.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Mavp<T>(
        ReadOnlySpan<T> inReal,
        ReadOnlySpan<T> inPeriods,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInMinPeriod = 2,
        int optInMaxPeriod = 30,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MavpImpl(inReal, inPeriods, inRange, outReal, out outRange, optInMinPeriod, optInMaxPeriod, optInMAType);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для индикатора <see cref="Mavp{T}"/>.
    /// </summary>
    /// <param name="optInMaxPeriod">Максимально допустимый период для расчета скользящего среднего.</param>
    /// <param name="optInMAType">Тип скользящего среднего (MA Type).</param>
    /// <returns>Количество баров, необходимых до первого валидного значения индикатора.</returns>
    /// <remarks>
    /// Период обратного просмотра определяется максимальным периодом скользящего среднего,
    /// так как для расчета значения с периодом N требуется N предыдущих баров.
    /// </remarks>
    [PublicAPI]
    public static int MavpLookback(int optInMaxPeriod = 30, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInMaxPeriod < 2 ? -1 : MaLookback(optInMaxPeriod, optInMAType);

    /// <remarks>
    /// Для совместимости с абстрактным API (массивы вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Mavp<T>(
        T[] inReal,
        T[] inPeriods,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInMinPeriod = 2,
        int optInMaxPeriod = 30,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MavpImpl<T>(inReal, inPeriods, inRange, outReal, out outRange, optInMinPeriod, optInMaxPeriod, optInMAType);

    /// <summary>
    /// Реализация расчета скользящего среднего с переменным периодом.
    /// </summary>
    private static Core.RetCode MavpImpl<T>(
        ReadOnlySpan<T> inReal,
        ReadOnlySpan<T> inPeriods,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInMinPeriod,
        int optInMaxPeriod,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона и длины массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length, inPeriods.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального и максимального периодов (должны быть >= 2)
        if (optInMinPeriod < 2 || optInMaxPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периода обратного просмотра на основе максимального периода
        var lookbackTotal = MavpLookback(optInMaxPeriod, optInMAType);
        if (inPeriods.Length < lookbackTotal)
        {
            return Core.RetCode.BadParam;
        }

        // Смещение начального индекса с учетом периода обратного просмотра
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var tempInt = lookbackTotal > startIdx ? lookbackTotal : startIdx;
        if (tempInt > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Определение размера выходного массива
        var outputSize = endIdx - tempInt + 1;

        // Выделение промежуточного локального буфера для хранения результатов
        Span<T> localOutputArray = new T[outputSize];
        // Выделение буфера для хранения усеченных периодов
        Span<int> localPeriodArray = new int[outputSize];

        // Копирование массива периодов в локальный буфер с одновременным усечением до диапазона [min, max]
        for (var i = 0; i < outputSize; i++)
        {
            var period = Int32.CreateTruncating(inPeriods[startIdx + i]);
            localPeriodArray[i] = Math.Clamp(period, optInMinPeriod, optInMaxPeriod);
        }

        // Определение промежуточного буфера вывода (защита от перезаписи входных данных)
        var intermediateOutput = outReal == inReal ? new T[outputSize] : outReal;

        /* Обработка каждого элемента входных данных.
         * Для каждого уникального значения периода MA рассчитывается только один раз.
         * Затем outReal заполняется для всех элементов с одинаковым периодом.
         * Устанавливается локальный флаг (значение 0) в localPeriodArray, чтобы избежать повторного вычисления.
         */
        var retCode = CalcMovingAverages(inReal, localPeriodArray, localOutputArray, new Range(startIdx, endIdx), outputSize, optInMAType,
            intermediateOutput);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Копирование промежуточного буфера в выходной буфер, если использовался временный массив
        if (intermediateOutput != outReal)
        {
            intermediateOutput[..outputSize].CopyTo(outReal);
        }

        // Формирование диапазона валидных выходных значений
        outRange = new Range(startIdx, startIdx + outputSize);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Вспомогательный метод для расчета скользящих средних с кэшированием результатов для одинаковых периодов.
    /// </summary>
    /// <param name="real">Входные данные для расчета.</param>
    /// <param name="periodArray">Массив усеченных периодов (значение 0 означает, что расчет уже выполнен).</param>
    /// <param name="outputArray">Промежуточный буфер для хранения результатов расчета.</param>
    /// <param name="range">Диапазон входных данных для обработки.</param>
    /// <param name="outputSize">Размер выходного массива.</param>
    /// <param name="maType">Тип скользящего среднего (SMA, EMA, WMA и др.).</param>
    /// <param name="intermediateOutput">Промежуточный буфер вывода для накопления результатов.</param>
    private static Core.RetCode CalcMovingAverages<T>(
        ReadOnlySpan<T> real,
        Span<int> periodArray,
        Span<T> outputArray,
        Range range,
        int outputSize,
        Core.MAType maType,
        Span<T> intermediateOutput) where T : IFloatingPointIeee754<T>
    {
        for (var i = 0; i < outputSize; i++)
        {
            var curPeriod = periodArray[i];
            if (curPeriod == 0)
            {
                continue; // Пропуск уже обработанных периодов
            }

            // Вычисление скользящего среднего для текущего периода
            var retCode = MaImpl(real, range, outputArray, out _, curPeriod, maType);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            // Сохранение результата для текущей позиции
            intermediateOutput[i] = outputArray[i];
            // Кэширование результата для всех последующих позиций с тем же периодом
            for (var j = i + 1; j < outputSize; j++)
            {
                if (periodArray[j] == curPeriod)
                {
                    periodArray[j] = 0; // Флаг для избежания повторного вычисления
                    intermediateOutput[j] = outputArray[j];
                }
            }
        }

        return Core.RetCode.Success;
    }
}
