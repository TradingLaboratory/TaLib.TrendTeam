// T3.cs
// Группы, к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// MomentumIndicators (альтернатива для анализа импульса и трендов)
// SmoothedAverages (альтернатива для акцента на сглаживании)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// T3 Moving Average (Overlap Studies) — T3 Скользящая средняя (Индикаторы перекрытия)
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
    /// <param name="optInTimePeriod">Период времени для расчета экспоненциальных скользящих средних (EMA)</param>
    /// <param name="optInVFactor">
    /// Объемный фактор (vFactor) контролирует степень сглаживания и отзывчивости индикатора:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения (ближе к 1) дают более гладкие выходные данные с большим лагом.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения (ближе к 0) делают выходные данные более отзывчивыми к недавним изменениям цены.
    ///     </description>
    ///   </item>
    /// </list>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// T3 Скользящая средняя — это метод сглаживания, улучшающий традиционные экспоненциальные скользящие средние (EMA)
    /// путем уменьшения лага при сохранении отзывчивости к изменениям цены. Достигается это применением множественных слоев EMA
    /// к одним и тем же входным данным с использованием объемного фактора (<paramref name="optInVFactor"/>) для регулировки веса каждого слоя.
    /// </para>
    /// <para>
    /// Значения T3 представляют собой более гладкую скользящую среднюю с меньшим лагом по сравнению с обычными EMA.
    /// Этот индикатор особенно полезен для идентификации трендов и снижения влияния краткосрочной волатильности.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление первой EMA (E1) по входным данным (<paramref name="inReal"/>) с использованием указанного
    ///       <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Последовательное применение дополнительных EMA поверх предыдущего результата до шести слоев:
    /// <code>
    /// E2 = EMA(E1, optInTimePeriod)
    /// E3 = EMA(E2, optInTimePeriod)
    /// ...
    /// E6 = EMA(E5, optInTimePeriod)
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Комбинирование взвешенных вкладов шести EMA с использованием объемного фактора (<paramref name="optInVFactor"/>):
    ///       <code>
    ///         T3 = C1 * E6 + C2 * E5 + C3 * E4 + C4 * E3
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><b>C1</b>, <b>C2</b>, <b>C3</b> и <b>C4</b> — константы, производные от объемного фактора.</item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// <b>Важно:</b> T3 не следует путать с EMA3. Оба индикатора называются "Тройная EMA" в литературе, но имеют различную математику.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode T3<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 5,
        double optInVFactor = 0.7) where T : IFloatingPointIeee754<T> =>
        T3Impl(inReal, inRange, outReal, out outRange, optInTimePeriod, optInVFactor);

    /// <summary>
    /// Возвращает период lookback для <see cref="T3{T}">T3</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета индикатора.</param>
    /// <returns>Количество периодов, необходимых перед расчетом первого валидного значения индикатора.</returns>
    /// <remarks>
    /// Период lookback определяет минимальное количество баров во входных данных, необходимых для получения первого валидного значения.
    /// Все бары с индексом меньше lookback будут пропущены при расчете.
    /// </remarks>
    [PublicAPI]
    public static int T3Lookback(int optInTimePeriod = 5) =>
        optInTimePeriod < 2 ? -1 : (optInTimePeriod - 1) * 6 + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.T3);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode T3<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 5,
        double optInVFactor = 0.7) where T : IFloatingPointIeee754<T> =>
        T3Impl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod, optInVFactor);

    private static Core.RetCode T3Impl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod,
        double optInVFactor) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка допустимых значений параметров
        if (optInTimePeriod < 2 || optInVFactor < 0.0 || optInVFactor > 1.0)
        {
            return Core.RetCode.BadParam;
        }

        /* Объяснение алгоритма можно найти в статьях Тима Тилсона (Tim Tillson):
         *
         * По сути, T3 для временного ряда "t" рассчитывается как:
         *   EMA1(x, Period) = EMA(x, Period)
         *   EMA2(x, Period) = EMA(EMA1(x, Period), Period)
         *   GD(x, Period, vFactor) = (EMA1(x, Period) * (1 + vFactor)) - (EMA2(x, Period) * vFactor)
         *   T3 = GD(GD(GD(t, Period, vFactor), Period, vFactor), Period, vFactor)
         *
         * T3 обеспечивает скользящую среднюю с меньшим лагом по сравнению с традиционной EMA.
         * T3 не следует путать с EMA3. Оба называются "Тройная EMA" в литературе, но имеют разную математику.
         */

        // Расчет общего периода lookback (6 слоев EMA)
        var lookbackTotal = T3Lookback(optInTimePeriod);
        if (startIdx <= lookbackTotal)
        {
            startIdx = lookbackTotal;
        }

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx; // Индекс первого валидного выходного значения
        var today = startIdx - lookbackTotal; // Текущий индекс в исходных данных

        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Коэффициент сглаживания для EMA: k = 2 / (Period + 1)
        var k = FunctionHelpers.Two<T>() / (timePeriod + T.One);
        var oneMinusK = T.One - k; // Дополнение коэффициента до 1

        // Инициализация первого слоя EMA (E1) — простое усреднение первых 'optInTimePeriod' значений
        var tempReal = inReal[today++];
        for (var i = optInTimePeriod - 1; i > 0; i--)
        {
            tempReal += inReal[today++];
        }

        var e1 = tempReal / timePeriod; // E1 — первая EMA

        // Инициализация второго слоя EMA (E2) — усреднение первых 'optInTimePeriod' значений E1
        tempReal = e1;
        for (var i = optInTimePeriod - 1; i > 0; i--)
        {
            e1 = k * inReal[today++] + oneMinusK * e1; // Обновление E1 с использованием формулы EMA
            tempReal += e1;
        }

        var e2 = tempReal / timePeriod; // E2 — вторая EMA

        // Инициализация третьего слоя EMA (E3)
        tempReal = e2;
        for (var i = optInTimePeriod - 1; i > 0; i--)
        {
            e1 = k * inReal[today++] + oneMinusK * e1; // Обновление E1
            e2 = k * e1 + oneMinusK * e2; // Обновление E2 на основе E1
            tempReal += e2;
        }

        var e3 = tempReal / timePeriod; // E3 — третья EMA

        // Инициализация четвертого слоя EMA (E4)
        tempReal = e3;
        for (var i = optInTimePeriod - 1; i > 0; i--)
        {
            e1 = k * inReal[today++] + oneMinusK * e1; // Обновление E1
            e2 = k * e1 + oneMinusK * e2; // Обновление E2
            e3 = k * e2 + oneMinusK * e3; // Обновление E3 на основе E2
            tempReal += e3;
        }

        var e4 = tempReal / timePeriod; // E4 — четвертая EMA

        // Инициализация пятого слоя EMA (E5)
        tempReal = e4;
        for (var i = optInTimePeriod - 1; i > 0; i--)
        {
            e1 = k * inReal[today++] + oneMinusK * e1; // Обновление E1
            e2 = k * e1 + oneMinusK * e2; // Обновление E2
            e3 = k * e2 + oneMinusK * e3; // Обновление E3
            e4 = k * e3 + oneMinusK * e4; // Обновление E4 на основе E3
            tempReal += e4;
        }

        var e5 = tempReal / timePeriod; // E5 — пятая EMA

        // Инициализация шестого слоя EMA (E6)
        tempReal = e5;
        for (var i = optInTimePeriod - 1; i > 0; i--)
        {
            e1 = k * inReal[today++] + oneMinusK * e1; // Обновление E1
            e2 = k * e1 + oneMinusK * e2; // Обновление E2
            e3 = k * e2 + oneMinusK * e3; // Обновление E3
            e4 = k * e3 + oneMinusK * e4; // Обновление E4
            e5 = k * e4 + oneMinusK * e5; // Обновление E5 на основе E4
            tempReal += e5;
        }

        var e6 = tempReal / timePeriod; // E6 — шестая EMA

        // Пропуск нестабильного периода (unstable period)
        while (today <= startIdx)
        {
            e1 = k * inReal[today++] + oneMinusK * e1;
            e2 = k * e1 + oneMinusK * e2;
            e3 = k * e2 + oneMinusK * e3;
            e4 = k * e3 + oneMinusK * e4;
            e5 = k * e4 + oneMinusK * e5;
            e6 = k * e5 + oneMinusK * e6;
        }

        // Расчет констант на основе объемного фактора (vFactor)
        var vFactor = T.CreateChecked(optInVFactor);
        tempReal = vFactor * vFactor; // vFactor²
        var c1 = T.NegativeOne * tempReal * vFactor; // C1 = -vFactor³
        var c2 = FunctionHelpers.Three<T>() * (tempReal - c1); // C2 = 3 * (vFactor² - C1)
        var c3 = T.NegativeOne * FunctionHelpers.Two<T>() * FunctionHelpers.Three<T>() * tempReal - FunctionHelpers.Three<T>() * (vFactor - c1); // C3 = -6 * vFactor² - 3 * (vFactor - C1)
        var c4 = T.One + FunctionHelpers.Three<T>() * vFactor - c1 + FunctionHelpers.Three<T>() * tempReal; // C4 = 1 + 3 * vFactor - C1 + 3 * vFactor²

        // Расчет и запись первого валидного значения T3
        var outIdx = 0;
        outReal[outIdx++] = c1 * e6 + c2 * e5 + c3 * e4 + c4 * e3; // T3 = C1*E6 + C2*E5 + C3*E4 + C4*E3

        // Расчет и запись оставшихся значений в диапазоне
        while (today <= endIdx)
        {
            e1 = k * inReal[today++] + oneMinusK * e1;
            e2 = k * e1 + oneMinusK * e2;
            e3 = k * e2 + oneMinusK * e3;
            e4 = k * e3 + oneMinusK * e4;
            e5 = k * e4 + oneMinusK * e5;
            e6 = k * e5 + oneMinusK * e6;
            outReal[outIdx++] = c1 * e6 + c2 * e5 + c3 * e4 + c4 * e3;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx); // Установка диапазона валидных выходных значений

        return Core.RetCode.Success;
    }
}
