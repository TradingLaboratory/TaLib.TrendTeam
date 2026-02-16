//Название файла: TA_HtDcPeriod.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу трансформации)
//DominantCycle (альтернатива для акцента на доминирующем цикле)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Dominant Cycle Period (Cycle Indicators) — Преобразование Хилберта - Период доминирующего цикла (Индикаторы циклов)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// Обычно используются цены закрытия (Close) или другие ценовые ряды.
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
    /// - Содержит рассчитанные значения периода доминирующего цикла.
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
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Преобразование Хилберта - Период доминирующего цикла определяет доминирующую длину цикла в рыночных данных.
    /// Оно применяет серию преобразований для выявления периодических паттернов и их доминирующей частоты.
    /// <para>
    /// Функция используется в продвинутом техническом анализе для выявления циклов в финансовых временных рядах,
    /// что помогает определить рыночные тренды и время для входа или выхода из позиций.
    /// Функция может быть интегрирована в цикловые подходы для тайминга входов или выходов.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сглаживание входных данных с использованием взвешенного скользящего среднего (WMA) для уменьшения шума при сохранении основного сигнала.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение преобразования Хилберта для вычисления фазовых (I) и квадратурных (Q) компонентов для нечетных и четных ценовых баров.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление арктангенса компонентов Q и I для определения фазовых изменений между последовательными ценовыми барами.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Использование фазовых изменений для оценки мгновенного периода доминирующего цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение сглаживающего фактора для стабилизации рассчитанного периода и уменьшения влияния шума и выбросов.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходные данные представляют собой доминирующий цикл в данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньший период указывает на более быстрые рыночные циклы, тогда как больший период сигнализирует о более медленных циклах.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Функция более эффективна в цикличных или боковых рынках и может давать ненадежные результаты в условиях сильного тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выходные данные чувствительны к шумным данным; сглаживающие техники, такие как WMA, помогают минимизировать это.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtDcPeriod<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtDcPeriodImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="HtDcPeriod{T}">HtDcPeriod</see>.
    /// </summary>
    /// <returns>
    /// Количество периодов, необходимых до расчета первого валидного выходного значения.
    /// <para>
    /// Это означает, что все бары в исходных данных с индексом меньше чем lookback будут пропущены,
    /// чтобы посчитать первое валидное значение индикатора.
    /// </para>
    /// </returns>
    /// <remarks>
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32".
    /// <para>
    /// Значение 32 связано с внутренней структурой преобразования Хилберта и количеством данных,
    /// необходимых для инициализации циркулярного буфера и вычисления стабильных значений.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int HtDcPeriodLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtDcPeriod) + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Этот метод обеспечивает совместимость с версиями API, использующими массивы вместо Span.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtDcPeriod<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtDcPeriodImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode HtDcPeriodImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона - по умолчанию пустой диапазон
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Получение периода обратного просмотра (lookback) для индикатора
        var lookbackTotal = HtDcPeriodLookback();
        // Корректировка начального индекса с учетом lookback - первые lookbackTotal баров пропускаются
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного - данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс начала валидных выходных данных
        var outBegIdx = startIdx;

        // Инициализация взвешенного скользящего среднего (WMA) для сглаживания входных данных
        // periodWMASub и periodWMASum используются для эффективного вычисления WMA
        // trailingWMAValue хранит значение WMA для удаления старых данных из расчета
        // trailingWMAIdx - индекс для отслеживания позиции в скользящем окне
        // 9 - период WMA для сглаживания
        // today - текущий индекс обрабатываемого бара
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        // Индекс для циркулярного буфера преобразования Хилберта
        var hilbertIdx = 0;

        /* Инициализация циркулярного буфера, используемого логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        // Индекс для записи выходных данных
        var outIdx = 0;

        // Переменные для хранения промежуточных значений преобразования Хилберта
        // prevI2, prevQ2 - предыдущие значения I2 и Q2 компонентов
        // re, im - действительная и мнимая части для вычисления периода
        // i1ForOddPrev3, i1ForEvenPrev3 - предыдущие значения I1 для нечетных и четных баров (3 периода назад)
        // i1ForOddPrev2, i1ForEvenPrev2 - предыдущие значения I1 для нечетных и четных баров (2 периода назад)
        // smoothPeriod - сглаженное значение периода доминирующего цикла
        // period - текущее рассчитанное значение периода
        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod;
        var period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        // Основной цикл обработки всех баров от today до endIdx
        while (today <= endIdx)
        {
            // Вычисление скорректированного предыдущего периода для адаптации к текущим рыночным условиям
            // Коэффициент 0.075 обеспечивает плавную адаптацию, 0.54 - базовое смещение
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Вычисление сглаженного значения с использованием WMA
            // inReal[today] - текущее значение цены для обработки
            // smoothedValue - результат сглаживания для уменьшения шума
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Выполнение преобразования Хилберта для вычисления фазовых компонентов
            // q2, i2 - квадратурный и фазовый компоненты текущего бара
            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировка периода для следующего ценового бара
            // Вычисление нового значения периода на основе фазовых компонентов
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            // Сглаживание рассчитанного периода для уменьшения влияния шума и выбросов
            // Коэффициент 0.33 для текущего периода, 0.67 для предыдущего сглаженного значения
            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            // Запись валидного значения в выходной массив, если достигнут начальный индекс
            if (today >= startIdx)
            {
                outReal[outIdx++] = smoothPeriod;
            }

            // Переход к следующему бару
            today++;
        }

        // Установка выходного диапазона - указывает какие индексы входных данных имеют валидные значения
        // outBegIdx - индекс первого бара с валидным значением
        // outBegIdx + outIdx - индекс последнего бара с валидным значением
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static void PerformHilbertTransform<T>(
        int today,
        Span<T> circBuffer,
        T smoothedValue,
        T adjustedPrevPeriod,
        T prevQ2,
        T prevI2,
        ref int hilbertIdx,
        ref T i1ForEvenPrev3,
        ref T i1ForOddPrev3,
        ref T i1ForOddPrev2,
        out T q2,
        out T i2,
        ref T i1ForEvenPrev2) where T : IFloatingPointIeee754<T>
    {
        if (today % 2 == 0)
        {
            // Выполнение преобразования Хилберта для четного ценового бара
            // CalcHilbertEven обрабатывает четные индексы баров с учетом специфики алгоритма
            FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                prevI2,
                out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);
        }
        else
        {
            // Выполнение преобразования Хилберта для нечетного ценового бара
            // CalcHilbertOdd обрабатывает нечетные индексы баров с учетом специфики алгоритма
            FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                prevI2,
                i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);
        }
    }
}
