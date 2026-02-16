//Название файла: TA_HtPhasor.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу индикатора)
//HilbertTransform (альтернатива для акцента на преобразовании Хилберта)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Phasor Components (Cycle Indicators) — Преобразование Хилберта - Фазовые компоненты (Циклические индикаторы)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены Close, другие индикаторы или другие временные ряды).
    /// Обычно используются цены закрытия (Close) для анализа циклических свойств рынка.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Позволяет ограничить расчет определенной частью исторических данных.
    /// </para>
    /// </param>
    /// <param name="outInPhase">
    /// Массив, содержащий ТОЛЬКО валидные значения фазовой компоненты индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outInPhase[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Фазовая компонента представляет позицию ценовых данных внутри цикла.
    /// </para>
    /// </param>
    /// <param name="outQuadrature">
    /// Массив, содержащий ТОЛЬКО валидные значения квадратурной компоненты индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outQuadrature[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Квадратурная компонента отражает задержку или отставание сигнала относительно цикла.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в выходных массивах.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в выходных массивах.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше lookback периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Преобразование Хилберта - Фазовые компоненты — это технический индикатор, который разлагает ценовые данные на их
    /// фазовые и квадратурные компоненты. Эти компоненты представляют собой действительные и мнимые части сигнала соответственно
    /// и являются важными для анализа циклических свойств ценовых данных.
    /// <para>
    /// Функция обычно используется в анализе, ориентированном на циклы. Интеграция её с традиционными индикаторами тренда или импульса
    /// может подтвердить циклические сигналы.
    /// Функция полезна для идентификации циклов и их фаз в финансовых данных, что позволяет предсказывать развороты цен
    /// или их продолжение.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Сглаживание входных цен с помощью скользящего среднего с взвешиванием (WMA) для минимизации шума и стабилизации основных данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение преобразования Хилберта для определения фазовых (I) и квадратурных (Q) компонентов как для четных, так и для нечетных баров.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление действительных и мнимых частей фазы с использованием тригонометрических вычислений, применяемых к сглаженным данным.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вывод угла фазы из действительных и мнимых частей с корректировкой на малые мнимые значения и любую задержку, вызванную WMA.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выполнение окончательных корректировок угла фазы для обеспечения его соответствия ожидаемому диапазону значений.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>Фазовая компонента (InPhase)</i> представляет собой позицию ценовых данных внутри цикла.
    ///       Значения близкие к максимуму указывают на вершину цикла, значения близкие к минимуму — на дно цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>Квадратурная компонента (Quadrature)</i> отражает задержку или отставание сигнала относительно цикла.
    ///       Используется вместе с фазовой компонентой для определения направления и силы циклического движения.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       При построении этих компонентов на полярном графике можно наблюдать циклическое поведение и идентифицировать фазовые сдвиги,
    ///       которые могут указывать на развороты тренда или переходы.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Ограничения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Функция наиболее эффективна на рынках с циклическим поведением и
    ///       может давать ненадежные результаты в трендовых или высоковолатильных рынках.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Точность компонентов зависит от качества сглаженных входных данных.
    ///       Рекомендуется использовать достаточное количество исторических данных для расчета.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Lookback период составляет 32 + нестабильный период функции. Все бары с индексом меньше lookback
    ///       будут пропущены для получения первого валидного значения индикатора.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode HtPhasor<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outInPhase,
        Span<T> outQuadrature,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtPhasorImpl(inReal, inRange, outInPhase, outQuadrature, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="HtPhasor{T}">HtPhasor</see>.
    /// </summary>
    /// <returns>
    /// Количество периодов (баров), необходимых до вычисления первого валидного выходного значения.
    /// Все бары с индексом меньше этого значения не будут иметь валидных результатов расчета.
    /// </returns>
    /// <remarks>
    /// <para>
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32".
    /// </para>
    /// <para>
    /// Lookback период определяет минимальное количество исторических данных, необходимое для инициализации
    /// внутренних буферов и получения первого валидного значения индикатора.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int HtPhasorLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtPhasor) + 32;

    /// <remarks>
    /// Метод для совместимости с абстрактным API библиотеки TALib.
    /// Использует массивы вместо Span для обратной совместимости.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtPhasor<T>(
        T[] inReal,
        Range inRange,
        T[] outInPhase,
        T[] outQuadrature,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtPhasorImpl<T>(inReal, inRange, outInPhase, outQuadrature, out outRange);

    /// <summary>
    /// Основная реализация расчета индикатора Hilbert Transform - Phasor Components.
    /// </summary>
    /// <param name="inReal">Входные данные (цены Close или другие временные ряды)</param>
    /// <param name="inRange">Диапазон обрабатываемых данных во входном массиве</param>
    /// <param name="outInPhase">Выходной массив для фазовой компоненты</param>
    /// <param name="outQuadrature">Выходной массив для квадратурной компоненты</param>
    /// <param name="outRange">Диапазон индексов с валидными значениями в выходных массивах</param>
    /// <typeparam name="T">Числовой тип данных (float или double)</typeparam>
    /// <returns>Код результата выполнения (RetCode.Success при успехе)</returns>
    private static Core.RetCode HtPhasorImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outInPhase,
        Span<T> outQuadrature,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация outRange пустым диапазоном (будет обновлен после успешного расчета)
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Получение lookback периода - минимальное количество баров для первого валидного значения
        var lookbackTotal = HtPhasorLookback();
        // Корректировка начального индекса с учетом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного - данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс начала валидных выходных данных
        var outBegIdx = startIdx;

        // Инициализация скользящего среднего с взвешиванием (WMA) для сглаживания входных данных
        // Период WMA = 9 баров для фильтрации рыночного шума
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        // Индекс для циклического буфера преобразования Хилберта
        var hilbertIdx = 0;

        /* Инициализация циклического буфера, используемого логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой, необходимых
         * Использование статического циклического буфера позволяет избежать больших динамических выделений памяти для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        // Индекс для записи в выходные массивы (относительно outBegIdx)
        var outIdx = 0;

        // Переменные для хранения предыдущих значений фазовых компонентов
        // prevI2, prevQ2 - предыдущие значения I2 и Q2 для расчета периода
        // re, im - действительная и мнимая части для расчета периода
        // i1ForOddPrev3, i1ForEvenPrev3 - предыдущие значения I1 для нечетных/четных баров (3 периода назад)
        // i1ForOddPrev2, i1ForEvenPrev2 - предыдущие значения I1 для нечетных/четных баров (2 периода назад)
        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2;
        // Инициализация всех переменных нулевым значением
        var period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        // Основной цикл расчета индикатора для каждого бара в диапазоне
        while (today <= endIdx)
        {
            // Расчет адаптированного предыдущего периода для сглаживания
            // Формула: adjustedPrevPeriod = 0.075 * period + 0.54
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Вычисление сглаженного значения с использованием WMA (Weighted Moving Average)
            // WMA помогает минимизировать рыночный шум перед применением преобразования Хилберта
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Выполнение преобразования Хилберта для фазовых компонентов
            // Разделяет сигнал на фазовую (InPhase) и квадратурную (Quadrature) компоненты
            PerformPhasorHilbertTransform(outInPhase, outQuadrature, today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2,
                startIdx, ref hilbertIdx, ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref outIdx,
                ref i1ForEvenPrev2);

            // Корректировка периода для следующего ценового бара
            // Использует текущие значения I2 и Q2 для обновления оценки доминирующего периода цикла
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            // Переход к следующему бару
            today++;
        }

        // Установка диапазона валидных выходных данных
        // outBegIdx - индекс первого валидного значения во входных данных
        // outBegIdx + outIdx - индекс последнего валидного значения + 1
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Выполняет преобразование Хилберта для получения фазовых компонентов.
    /// </summary>
    /// <param name="outInPhase">Выходной массив для фазовой компоненты</param>
    /// <param name="outQuadrature">Выходной массив для квадратурной компоненты</param>
    /// <param name="today">Текущий индекс бара во входных данных</param>
    /// <param name="circBuffer">Циклический буфер для промежуточных вычислений</param>
    /// <param name="smoothedValue">Сглаженное значение цены после WMA</param>
    /// <param name="adjustedPrevPeriod">Адаптированное значение предыдущего периода</param>
    /// <param name="prevQ2">Предыдущее значение квадратурной компоненты Q2</param>
    /// <param name="prevI2">Предыдущее значение фазовой компоненты I2</param>
    /// <param name="startIdx">Начальный индекс для валидных выходных данных</param>
    /// <param name="hilbertIdx">Текущий индекс в циклическом буфере Хилберта</param>
    /// <param name="i1ForEvenPrev3">Предыдущее значение I1 для четных баров (3 периода назад)</param>
    /// <param name="i1ForOddPrev3">Предыдущее значение I1 для нечетных баров (3 периода назад)</param>
    /// <param name="i1ForOddPrev2">Предыдущее значение I1 для нечетных баров (2 периода назад)</param>
    /// <param name="q2">Выходное значение квадратурной компоненты Q2</param>
    /// <param name="i2">Выходное значение фазовой компоненты I2</param>
    /// <param name="outIdx">Текущий индекс записи в выходные массивы</param>
    /// <param name="i1ForEvenPrev2">Предыдущее значение I1 для четных баров (2 периода назад)</param>
    /// <typeparam name="T">Числовой тип данных (float или double)</typeparam>
    /// <remarks>
    /// Метод разделяет обработку на четные и нечетные бары для оптимизации вычислений.
    /// Использует разные формулы для четных и нечетных дней для повышения точности преобразования.
    /// </remarks>
    private static void PerformPhasorHilbertTransform<T>(
        Span<T> outInPhase,
        Span<T> outQuadrature,
        int today,
        Span<T> circBuffer,
        T smoothedValue,
        T adjustedPrevPeriod,
        T prevQ2,
        T prevI2,
        int startIdx,
        ref int hilbertIdx,
        ref T i1ForEvenPrev3,
        ref T i1ForOddPrev3,
        ref T i1ForOddPrev2,
        out T q2,
        out T i2,
        ref int outIdx,
        ref T i1ForEvenPrev2)
        where T : IFloatingPointIeee754<T>
    {
        if (today % 2 == 0)
        {
            // Выполнение преобразования Хилберта для четных ценовых баров
            // Использует специализированную формулу для четных дней
            FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                prevI2, out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);

            // Запись результатов в выходные массивы только если достигнут startIdx (lookback период пройден)
            if (today >= startIdx)
            {
                // Квадратурная компонента берется из циклического буфера (ключ Q1)
                outQuadrature[outIdx] = circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1];
                // Фазовая компонента - предыдущее значение I1 для четных баров (3 периода назад)
                outInPhase[outIdx++] = i1ForEvenPrev3;
            }
        }
        else
        {
            // Выполнение преобразования Хилберта для нечетных ценовых баров
            // Использует специализированную формулу для нечетных дней
            FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                prevI2, i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);

            // Запись результатов в выходные массивы только если достигнут startIdx (lookback период пройден)
            if (today >= startIdx)
            {
                // Квадратурная компонента берется из циклического буфера (ключ Q1)
                outQuadrature[outIdx] = circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1];
                // Фазовая компонента - предыдущее значение I1 для нечетных баров (3 периода назад)
                outInPhase[outIdx++] = i1ForOddPrev3;
            }
        }
    }
}
