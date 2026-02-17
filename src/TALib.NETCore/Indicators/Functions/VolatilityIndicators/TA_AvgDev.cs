//Название файла: TA_AvgDev.cs
//Группы к которым можно отнести индикатор:
//VolatilityIndicators (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по типу функции)
//PriceTransform (альтернатива для акцента на трансформации цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// _Average Deviation (VolatilityIndicators) — Среднее отклонение (Индикаторы волатильности)_
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
    /// <param name="optInTimePeriod">Период времени для расчета среднего отклонения.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Среднее отклонение вычисляет, насколько каждая точка данных отклоняется от среднего значения, служа мерой волатильности.
    /// Оно дает представление о вариативности или волатильности набора данных.
    /// <para>
    /// Функция может помочь оценить стабильность или изменчивость рынка.
    /// Включение ее в стратегии с индикаторами тренда или импульса может уточнить решения, основанные на волатильности.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>Вычислить среднее (среднее значение) значений за указанный период времени:
    ///       <code>
    ///         Mean = Сумма значений / Период времени
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>Вычислить абсолютное отклонение каждого значения от среднего:
    ///       <code>
    ///         Deviation[i] = abs(Value[i] - Mean)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить среднее этих отклонений для получения AvgDev:
    ///       <code>
    ///         AvgDev = Сумма отклонений / Период времени
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокое значение указывает на большую изменчивость.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкое значение указывает на большую стабильность.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode AvgDev<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AvgDevImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="AvgDev{T}">AvgDev</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>
    /// Количество периодов, необходимых до расчета первого выходного значения.
    /// lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// Все бары в исходных данных, индекс которых меньше чем lookback, будут пропущены, чтобы посчитать первое валидное значение индикатора.
    /// </returns>
    [PublicAPI]
    public static int AvgDevLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode AvgDev<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AvgDevImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode AvgDevImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация диапазона вывода пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода времени (должен быть больше 1)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет общего периода обратного просмотра (lookback), необходимого для первого валидного значения
        var lookbackTotal = AvgDevLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом lookback: пропускаем бары, для которых недостаточно данных
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Индекс текущего бара во входных данных
        var today = startIdx;
        // Если начальный индекс уже больше конечного, данные для расчета отсутствуют
        if (today > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Преобразование периода времени в тип данных T для математических операций
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Индекс начала валидных данных в выходном массиве (совпадает с today)
        var outBegIdx = today;

        // Индекс для записи в выходной массив outReal
        var outIdx = 0;
        // Основной цикл расчета по всем барам от startIdx до endIdx
        while (today <= endIdx)
        {
            // Накопительная сумма значений за период для расчета среднего
            var todaySum = T.Zero;
            for (var i = 0; i < optInTimePeriod; i++)
            {
                todaySum += inReal[today - i];
            }

            // Накопительная сумма абсолютных отклонений от среднего за период
            var todayDev = T.Zero;
            for (var i = 0; i < optInTimePeriod; i++)
            {
                // Вычисление абсолютного отклонения текущего значения от среднего значения периода
                todayDev += T.Abs(inReal[today - i] - todaySum / timePeriod);
            }

            // Запись среднего отклонения в выходной массив
            outReal[outIdx++] = todayDev / timePeriod;
            // Переход к следующему бару
            today++;
        }

        // Установка диапазона outRange: указывает индексы во входных данных, для которых есть валидный результат
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
