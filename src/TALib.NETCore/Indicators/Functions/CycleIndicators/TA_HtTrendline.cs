//Название файла TA_HtTrendline.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//OverlapStudies (альтернатива, если требуется группировка по типу индикатора)
//TrendIndicators (альтернатива для акцента на трендовых индикаторах)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Instantaneous Trendline (Overlap Studies) — Трансформация Гильберта - Мгновенная Трендовая Линия (Наложение)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// Обычно используются цены закрытия (Close) для расчета трендовой линии.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Позволяет ограничить расчет определенной частью входных данных.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Содержит рассчитанные значения мгновенной трендовой линии.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Трансформация Гильберта - Мгновенная Трендовая Линия — это циклический индикатор, предназначенный для вычисления сглаженной трендовой линии с использованием
    /// трансформации Гильберта. Он устраняет шум и обеспечивает мгновенное представление тренда, комбинируя недавние сглаженные точки данных и
    /// экстраполируя на основе доминирующих циклов.
    /// <para>
    /// Функция может быть комбинирована с <see cref="Adx{T}">ADX</see> или <see cref="Macd{T}">MACD</see>, чтобы убедиться, что изменения
    /// в трендовой линии соответствуют более широким условиям, снижая риск действий на ложные сигналы.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сглаживание входных цен с использованием взвешенного скользящего среднего (WMA) для уменьшения шума и обеспечения более плавных переходов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение трансформации Гильберта для извлечения фазовой (I) и квадратурной (Q) компонент, которые используются для определения свойств цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление доминирующего периода цикла с использованием компонент I и Q для динамической оценки длины цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление трендовой линии с использованием взвешенного скользящего среднего сглаженных значений за доминирующий период цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сохранение вычисленных значений трендовой линии в выходном массиве для визуализации или дальнейшего анализа.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>Трендовая линия</i> предоставляет сглаженное представление рыночного тренда, фильтруя краткосрочную волатильность.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Цена выше трендовой линии указывает на восходящий тренд (бычий рынок).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Цена ниже трендовой линии указывает на нисходящий тренд (медвежий рынок).
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Функция наиболее эффективна в трендовых рынках и может быть менее эффективна в высоко цикличных или волатильных условиях.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Алгоритм предполагает наличие доминирующего цикла, поэтому рынки, лишенные циклического поведения, могут привести к вводящим в заблуждение трендам.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Требуется достаточное количество исторических данных (lookback период) для получения первого валидного значения.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtTrendline<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtTrendlineImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для <see cref="HtTrendline{T}">HtTrendline</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых перед первым вычисленным значением.</returns>
    /// <remarks>
    /// Пропускается 31 входных данных для совместимости с Tradestation.
    /// <para>
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32".
    /// </para>
    /// <para>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// Все бары в исходных данных с индексом меньше чем lookback будут пропущены, чтобы посчитать первое валидное значение индикатора.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int HtTrendlineLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtTrendline) + 31 + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtTrendline<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtTrendlineImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode HtTrendlineImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация outRange с начальным значением (пустой диапазон)
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        // Если диапазон некорректен, возвращаем код ошибки OutOfRangeParam
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Получение общего lookback периода для индикатора
        // Это количество баров, необходимых для получения первого валидного значения
        var lookbackTotal = HtTrendlineLookback();
        // Корректировка начального индекса с учетом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Размер буфера для сглаженных цен
        const int smoothPriceSize = 50;
        // Буфер для хранения сглаженных значений цен
        Span<T> smoothPrice = new T[smoothPriceSize];

        // Переменные для хранения предыдущих значений тренда (для сглаживания)
        T iTrend2, iTrend1;
        // Инициализация переменных тренда нулевыми значениями
        var iTrend3 = iTrend2 = iTrend1 = T.Zero;

        // Индекс начала вывода валидных значений
        var outBegIdx = startIdx;

        // Инициализация вспомогательных переменных для взвешенного скользящего среднего (WMA)
        // periodWMASub - вычитаемое значение для обновления суммы WMA
        // periodWMASum - текущая сумма значений для WMA
        // trailingWMAValue - значение, которое будет удалено из скользящего окна
        // trailingWMAIdx - индекс для отслеживания удаляемого значения
        // today - текущий индекс обрабатываемого бара
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 34, out var today);

        // Индекс для циклического буфера трансформации Гильберта
        var hilbertIdx = 0;
        // Индекс для буфера сглаженных цен
        var smoothPriceIdx = 0;

        /* Инициализация циклического буфера, используемого логикой трансформации Гильберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения расчетов.
         * Использование статического циклического буфера позволяет избежать больших динамических выделений памяти для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        // Индекс для записи в выходной массив
        var outIdx = 0;

        // Переменные для трансформации Гильберта:
        // prevI2, prevQ2 - предыдущие значения I2 и Q2 компонент
        // re, im - действительная и мнимая части для расчета периода
        // i1ForOddPrev3, i1ForEvenPrev3 - предыдущие значения I1 для нечетных/четных итераций (3 периода назад)
        // i1ForOddPrev2, i1ForEvenPrev2 - предыдущие значения I1 для нечетных/четных итераций (2 периода назад)
        // smoothPeriod - сглаженное значение периода цикла
        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod;
        // Инициализация всех переменных нулевыми значениями
        var period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        // Основной цикл расчета индикатора
        while (today <= endIdx)
        {
            // Расчет корректировки предыдущего периода для сглаживания
            // Формула: adjustedPrevPeriod = 0.075 * period + 0.54
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Вычисление взвешенного скользящего среднего (WMA) для сглаживания цены
            // smoothedValue - сглаженное значение цены для текущего бара
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Сохранение сглаженного значения в циклический буфер smoothPrice.
            smoothPrice[smoothPriceIdx] = smoothedValue;

            // Выполнение трансформации Гильберта для извлечения фазовой и квадратурной компонент
            // q2, i2 - квадратурная и фазовая компоненты для текущего бара
            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировка периода для следующего бара цен
            // Обновление переменных re, im, period на основе компонент I2 и Q2
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            // Сглаживание периода цикла с использованием экспоненциального сглаживания
            // Формула: smoothPeriod = 0.33 * period + 0.67 * smoothPeriod
            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            // Вычисление значения трендовой линии на основе сглаженного периода
            // trendLineValue - рассчитанное значение мгновенной трендовой линии для текущего бара
            var trendLineValue = ComputeTrendLine(inReal, ref today, smoothPeriod, ref iTrend1, ref iTrend2, ref iTrend3);

            // Запись значения в выходной массив только если текущий индекс >= начального индекса вывода
            // Это обеспечивает, что в outReal попадают только валидные значения индикатора
            if (today >= startIdx)
            {
                outReal[outIdx++] = trendLineValue;
            }

            // Обновление индекса буфера сглаженных цен (циклический буфер)
            if (++smoothPriceIdx > smoothPriceSize - 1)
            {
                smoothPriceIdx = 0;
            }

            // Переход к следующему бару
            today++;
        }

        // Установка outRange - диапазона индексов входных данных, для которых рассчитаны валидные значения
        // outRange.Start - индекс первого бара с валидным значением
        // outRange.End - индекс последнего бара с валидным значением
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Вычисляет значение трендовой линии на основе сглаженного периода цикла.
    /// </summary>
    /// <param name="real">Входные данные (цены) для расчета трендовой линии.</param>
    /// <param name="today">Ссылка на текущий индекс обрабатываемого бара.</param>
    /// <param name="smoothPeriod">Сглаженное значение периода цикла.</param>
    /// <param name="iTrend1">Ссылка на предыдущее значение тренда (1 период назад).</param>
    /// <param name="iTrend2">Ссылка на предыдущее значение тренда (2 периода назад).</param>
    /// <param name="iTrend3">Ссылка на предыдущее значение тренда (3 периода назад).</param>
    /// <typeparam name="T">Числовой тип данных, реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <returns>Рассчитанное значение трендовой линии.</returns>
    /// <remarks>
    /// Использует взвешенное среднее текущих и предыдущих значений тренда для сглаживания.
    /// Формула: trendLine = (4 * tempReal + 3 * iTrend1 + 2 * iTrend2 + iTrend3) / 10
    /// </remarks>
    private static T ComputeTrendLine<T>(
        ReadOnlySpan<T> real,
        ref int today,
        T smoothPeriod,
        ref T iTrend1,
        ref T iTrend2,
        ref T iTrend3) where T : IFloatingPointIeee754<T>
    {
        // Текущий индекс для чтения из массива цен
        var idx = today;
        // Временная переменная для накопления суммы цен
        var tempReal = T.Zero;
        // Округление сглаженного периода до ближайшего целого числа
        // dcPeriod - доминирующий период цикла (в барах)
        var dcPeriod = Int32.CreateTruncating(smoothPeriod + T.CreateChecked(0.5));

        // Суммирование цен за период dcPeriod
        for (var i = 0; i < dcPeriod; i++)
        {
            tempReal += real[idx--];
        }

        // Вычисление среднего значения цен за период
        if (dcPeriod > 0)
        {
            tempReal /= T.CreateChecked(dcPeriod);
        }

        // Расчет трендовой линии с использованием взвешенного среднего
        // Веса: 4 для текущего среднего, 3 для iTrend1, 2 для iTrend2, 1 для iTrend3
        // Общая сумма весов = 10
        var trendLine =
            (FunctionHelpers.Four<T>() * tempReal + FunctionHelpers.Three<T>() * iTrend1 + FunctionHelpers.Two<T>() * iTrend2 + iTrend3) /
            T.CreateChecked(10);

        // Обновление предыдущих значений тренда для следующей итерации
        iTrend3 = iTrend2;
        iTrend2 = iTrend1;
        iTrend1 = tempReal;

        return trendLine;
    }
}
