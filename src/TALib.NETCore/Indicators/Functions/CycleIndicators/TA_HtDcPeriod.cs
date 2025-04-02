//Название файла: TA_HtDcPeriod.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу трансформации)
//DominantCycle (альтернатива для акцента на доминирующем цикле)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Hilbert Transform - Dominant Cycle Period (Cycle Indicators) — Преобразование Хилберта - Доминирующий цикл (Индикаторы циклов)
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
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Преобразование Хилберта - Доминирующий цикл определяет доминирующую длину цикла в рыночных данных.
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
    /// Возвращает период обратного просмотра для <see cref="HtDcPeriod{T}">HtDcPeriod</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых до расчета первого выходного значения.</returns>
    /// <remarks>См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32"</remarks>
    [PublicAPI]
    public static int HtDcPeriodLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtDcPeriod) + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
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
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var lookbackTotal = HtDcPeriodLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx;

        // Инициализация взвешенного скользящего среднего (WMA)
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        var hilbertIdx = 0;

        /* Инициализация циркулярного буфера, используемого логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        var outIdx = 0;

        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod;
        var period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Вычисление сглаженного значения с использованием WMA
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Выполнение преобразования Хилберта
            PerformHilbertTransform(today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2, ref hilbertIdx,
                ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref i1ForEvenPrev2);

            // Корректировка периода для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            smoothPeriod = T.CreateChecked(0.33) * period + T.CreateChecked(0.67) * smoothPeriod;

            if (today >= startIdx)
            {
                outReal[outIdx++] = smoothPeriod;
            }

            today++;
        }

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
            FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                prevI2,
                out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);
        }
        else
        {
            // Выполнение преобразования Хилберта для нечетного ценового бара
            FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                prevI2,
                i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);
        }
    }
}
