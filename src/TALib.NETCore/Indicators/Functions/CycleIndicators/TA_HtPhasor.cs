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
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outInPhase">Массив, содержащий ТОЛЬКО валидные значения индикатора.</param>
    /// <param name="outQuadrature">Массив, содержащий ТОЛЬКО валидные значения индикатора.</param>
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
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
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
    ///       <i>Фазовая компонента</i> представляет собой позицию ценовых данных внутри цикла.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>Квадратурная компонента</i> отражает задержку или отставание сигнала относительно цикла.
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
    ///       Функция наиболее эффективна в рынках с циклическим поведением и
    ///       может давать ненадежные результаты в трендовых или высоковолатильных рынках.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Точность компонентов зависит от качества сглаженных входных данных.
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
    /// Возвращает период обратного просмотра для <see cref="HtPhasor{T}">HtPhasor</see>.
    /// </summary>
    /// <returns>Количество периодов, необходимых до вычисления первого выходного значения.</returns>
    /// <remarks>
    /// См. <see cref="MamaLookback">MamaLookback</see> для объяснения значения "32"
    /// </remarks>
    [PublicAPI]
    public static int HtPhasorLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.HtPhasor) + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode HtPhasor<T>(
        T[] inReal,
        Range inRange,
        T[] outInPhase,
        T[] outQuadrature,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        HtPhasorImpl<T>(inReal, inRange, outInPhase, outQuadrature, out outRange);

    private static Core.RetCode HtPhasorImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outInPhase,
        Span<T> outQuadrature,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        var lookbackTotal = HtPhasorLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx;

        // Инициализация скользящего среднего с взвешиванием (WMA)
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        var hilbertIdx = 0;

        /* Инициализация циклического буфера, используемого логикой преобразования Хилберта.
         * Один буфер используется для нечетных дней, другой — для четных.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой, необходимых
         * Использование статического циклического буфера позволяет избежать больших динамических выделений памяти для хранения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        var outIdx = 0;

        T prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2;
        var period = prevI2 = prevQ2 = re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        while (today <= endIdx)
        {
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Вычисление сглаженного значения с использованием WMA
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                out var smoothedValue);

            // Выполнение преобразования Хилберта для фазовых компонентов
            PerformPhasorHilbertTransform(outInPhase, outQuadrature, today, circBuffer, smoothedValue, adjustedPrevPeriod, prevQ2, prevI2,
                startIdx, ref hilbertIdx, ref i1ForEvenPrev3, ref i1ForOddPrev3, ref i1ForOddPrev2, out var q2, out var i2, ref outIdx,
                ref i1ForEvenPrev2);

            // Корректировка периода для следующего ценового бара
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            today++;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

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
            FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                prevI2, out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);

            if (today >= startIdx)
            {
                outQuadrature[outIdx] = circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1];
                outInPhase[outIdx++] = i1ForEvenPrev3;
            }
        }
        else
        {
            // Выполнение преобразования Хилберта для нечетных ценовых баров
            FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                prevI2, i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);

            if (today >= startIdx)
            {
                outQuadrature[outIdx] = circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1];
                outInPhase[outIdx++] = i1ForOddPrev3;
            }
        }
    }
}
