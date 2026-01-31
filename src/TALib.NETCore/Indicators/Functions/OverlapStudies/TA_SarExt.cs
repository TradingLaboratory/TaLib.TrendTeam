// SarExt.cs
// Группы к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// TrendIndicators (альтернатива для акцента на определении тренда)
// StopAndReverse (альтернатива для группировки по типу сигнала)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Parabolic SAR - Extended (Overlap Studies) — Параболический SAR - Расширенный (Индикаторы наложения)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High) входных баров.</param>
    /// <param name="inLow">Массив минимальных цен (Low) входных баров.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">
    /// Массив для хранения рассчитанных значений индикатора:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения — позиция Long (SAR ниже цены).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения — позиция Short (SAR выше цены).
    ///     </description>
    ///   </item>
    /// </list>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения индикатора:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>Start</b> — индекс первого бара во входных данных с валидным значением.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>End</b> — индекс последнего бара во входных данных с валидным значением.
    ///     </description>
    ///   </item>
    /// </list>
    /// </param>
    /// <param name="optInStartValue">
    /// Начальное значение SAR для инициализации расчёта:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения — начальная позиция Long.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения — начальная позиция Short.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Нулевое значение — направление определяется автоматически по первым двум барам.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>-100.0..100.0</c>.
    /// </para>
    /// </param>
    /// <param name="optInOffsetOnReverse">
    /// Смещение (offset), применяемое к значению SAR при смене позиции (развороте):
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения увеличивают расстояние между SAR и текущей ценой после разворота.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения уменьшают расстояние.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>-0.5..0.5</c>.
    /// </para>
    /// </param>
    /// <param name="optInAccelerationInitLong">
    /// Начальный фактор ускорения (AF) для длинных позиций (Long):
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения делают SAR более чувствительным в начале тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения сглаживают начальное движение SAR.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.01..0.1</c>.
    /// </para>
    /// </param>
    /// <param name="optInAccelerationLong">
    /// Инкрементный фактор ускорения для длинных позиций — величина увеличения AF при каждом новом экстремуме:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения повышают чувствительность SAR к новым экстремумам.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения ограничивают скорость нарастания ускорения.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.01..0.05</c>.
    /// </para>
    /// </param>
    /// <param name="optInAccelerationMaxLong">
    /// Максимальный фактор ускорения для длинных позиций:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения позволяют быстрее следовать за трендом.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения ограничивают движение SAR, повышая стабильность.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.1..0.5</c>.
    /// </para>
    /// </param>
    /// <param name="optInAccelerationInitShort">
    /// Начальный фактор ускорения для коротких позиций (Short), аналогичный <paramref name="optInAccelerationInitLong"/>.
    /// Типичный диапазон: <c>0.01..0.1</c>.
    /// </param>
    /// <param name="optInAccelerationShort">
    /// Инкрементный фактор ускорения для коротких позиций, аналогичный <paramref name="optInAccelerationLong"/>.
    /// Типичный диапазон: <c>0.01..0.05</c>.
    /// </param>
    /// <param name="optInAccelerationMaxShort">
    /// Максимальный фактор ускорения для коротких позиций, аналогичный <paramref name="optInAccelerationMaxLong"/>.
    /// Типичный диапазон: <c>0.1..0.5</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчёта:
    /// <see cref="Core.RetCode.Success"/> при успешном выполнении или соответствующий код ошибки.
    /// </returns>
    /// <remarks>
    /// Расширенный параболический индикатор Stop and Reverse (SAR-Ext) — улучшенная версия классического <see cref="Sar{T}">SAR</see>.
    /// Позволяет детально настраивать расчёт отдельно для длинных (Long) и коротких (Short) позиций,
    /// а также задавать смещение при развороте тренда. Особенно эффективен в волатильных и трендовых рынках.
    /// <para>
    /// Благодаря настройке факторов ускорения, смещений и начальных значений можно точно адаптировать
    /// чувствительность индикатора к изменению цен.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение начального направления тренда на основе направленного движения (DM) первых двух баров
    ///       или явного задания через параметр <paramref name="optInStartValue"/>:
    ///       <code>
    ///         Направление = Long, если +DM > -DM; иначе Short.
    ///       </code>
    ///       Если <paramref name="optInStartValue"/> > 0 — направление Long, если < 0 — направление Short.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Инициализация SAR и экстремальной точки (EP — Extreme Point):
    ///       <code>
    ///         SAR = Lowest Low (для Long) или Highest High (для Short) первого бара.
    ///         EP = Highest High (для Long) или Lowest Low (для Short) второго бара.
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого последующего бара:
    ///       <list type="bullet">
    ///         <item>
    ///           <description>
    ///             Расчёт SAR с использованием отдельных факторов ускорения для позиций Long/Short:
    ///             <code>
    ///               SAR = Предыдущий SAR + AF * (EP - Предыдущий SAR)
    ///             </code>
    ///             Фактор ускорения (AF) начинается с начального значения и увеличивается при каждом новом экстремуме
    ///             до достижения максимального предела. EP обновляется до нового Highest High (Long) или Lowest Low (Short).
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             Проверка, чтобы SAR не проникал в диапазон цен предыдущих двух баров.
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             При пересечении SAR текущей цены — разворот позиции, сброс SAR к значению EP
    ///             и применение смещения <paramref name="optInOffsetOnReverse"/> при необходимости.
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Точки SAR ниже цены — восходящий тренд (Long), уровни стоп-лосса для длинных позиций.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Точки SAR выше цены — нисходящий тренд (Short), уровни стоп-лосса для коротких позиций.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение SAR цены (снизу вверх или сверху вниз) — сигнал потенциального разворота тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode SarExt<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        double optInStartValue = 0.0,
        double optInOffsetOnReverse = 0.0,
        double optInAccelerationInitLong = 0.02,
        double optInAccelerationLong = 0.02,
        double optInAccelerationMaxLong = 0.2,
        double optInAccelerationInitShort = 0.02,
        double optInAccelerationShort = 0.02,
        double optInAccelerationMaxShort = 0.2) where T : IFloatingPointIeee754<T> =>
        SarExtImpl(inHigh, inLow, inRange, outReal, out outRange, optInStartValue, optInOffsetOnReverse, optInAccelerationInitLong,
            optInAccelerationLong, optInAccelerationMaxLong, optInAccelerationInitShort, optInAccelerationShort, optInAccelerationMaxShort);

    /// <summary>
    /// Возвращает период ожидания (lookback) для индикатора <see cref="SarExt{T}">SarExt</see>.
    /// </summary>
    /// <returns>Всегда 1, так как для расчёта требуется всего один предыдущий ценовой бар.</returns>
    [PublicAPI]
    public static int SarExtLookback() => 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode SarExt<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        double optInStartValue = 0.0,
        double optInOffsetOnReverse = 0.0,
        double optInAccelerationInitLong = 0.02,
        double optInAccelerationLong = 0.02,
        double optInAccelerationMaxLong = 0.2,
        double optInAccelerationInitShort = 0.02,
        double optInAccelerationShort = 0.02,
        double optInAccelerationMaxShort = 0.2) where T : IFloatingPointIeee754<T> =>
        SarExtImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInStartValue, optInOffsetOnReverse, optInAccelerationInitLong,
            optInAccelerationLong, optInAccelerationMaxLong, optInAccelerationInitShort, optInAccelerationShort, optInAccelerationMaxShort);

    private static Core.RetCode SarExtImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        double optInStartValue,
        double optInOffsetOnReverse,
        double optInAccelerationInitLong,
        double optInAccelerationLong,
        double optInAccelerationMaxLong,
        double optInAccelerationInitShort,
        double optInAccelerationShort,
        double optInAccelerationMaxShort) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка неотрицательности критических параметров
        if (optInOffsetOnReverse < 0.0 || optInAccelerationInitLong < 0.0 || optInAccelerationLong < 0.0 ||
            optInAccelerationMaxLong < 0.0 || optInAccelerationInitShort < 0.0 || optInAccelerationShort < 0.0 ||
            optInAccelerationMaxShort < 0.0)
        {
            return Core.RetCode.BadParam;
        }

        // Период ожидания (минимальное количество баров для расчёта первого валидного значения)
        var lookbackTotal = SarExtLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback нет данных для расчёта — выход с успехом (пустой результат)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Корректировка факторов ускорения: проверка согласованности начального значения и максимума
        var afLong = AdjustAcceleration(ref optInAccelerationInitLong, ref optInAccelerationLong, optInAccelerationMaxLong);
        var afShort = AdjustAcceleration(ref optInAccelerationInitShort, ref optInAccelerationShort, optInAccelerationMaxShort);

        // Определение начального направления тренда (Long/Short)
        var (isLong, retCode) = DetermineInitialDirection(inHigh, inLow, optInStartValue, startIdx);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Индекс первого бара во входных данных, для которого будет рассчитано значение
        var outBegIdx = startIdx;
        // Текущий индекс в выходном массиве
        var outIdx = 0;

        // Текущий индекс обрабатываемого бара
        var todayIdx = startIdx;

        // Цены предыдущего бара (для инициализации)
        var newLow = inLow[todayIdx - 1];
        var newHigh = inHigh[todayIdx - 1];
        // Инициализация SAR и экстремальной точки (EP)
        var sar = InitializeSar(inHigh, inLow, optInStartValue, isLong, todayIdx, newLow, newHigh, out var ep);

        // Подготовка цен текущего бара для первой итерации цикла
        newLow = inLow[todayIdx];
        newHigh = inHigh[todayIdx];

        while (todayIdx <= endIdx)
        {
            // Сохранение цен предыдущего бара
            var prevLow = newLow;
            var prevHigh = newHigh;
            // Чтение цен текущего бара
            newLow = inLow[todayIdx];
            newHigh = inHigh[todayIdx++];
            if (isLong)
            {
                // Проверка условия разворота: цена пробила SAR снизу
                if (newLow <= sar)
                {
                    isLong = false;
                    // Смена на короткую позицию с применением смещения
                    sar = SwitchToShort(ref ep, prevHigh, newLow, newHigh, out afShort, optInAccelerationInitShort,
                        T.CreateChecked(optInOffsetOnReverse), ref outIdx, outReal);
                }
                else
                {
                    // Сохранение значения SAR для длинной позиции (положительное значение)
                    outReal[outIdx++] = sar;
                    // Обновление SAR и EP для продолжения длинной позиции
                    sar = ProcessLongPosition(ref ep, prevLow, newLow, newHigh, ref afLong, optInAccelerationLong, optInAccelerationMaxLong,
                        sar);
                }
            }
            else if (newHigh >= sar)
            {
                // Проверка условия разворота: цена пробила SAR сверху
                isLong = true;
                // Смена на длинную позицию с применением смещения
                sar = SwitchToLong(ref ep, prevLow, newLow, newHigh, out afLong, optInAccelerationInitLong,
                    T.CreateChecked(optInOffsetOnReverse), ref outIdx, outReal);
            }
            else
            {
                // Сохранение значения SAR для короткой позиции (отрицательное значение)
                outReal[outIdx++] = -sar;

                // Обновление SAR и EP для продолжения короткой позиции
                sar = ProcessShortPosition(ref ep, prevHigh, newLow, newHigh, ref afShort, optInAccelerationShort,
                    optInAccelerationMaxShort, sar);
            }
        }

        // Формирование выходного диапазона с валидными значениями
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Корректирует фактор ускорения для обеспечения согласованности параметров:
    /// начальное значение не должно превышать максимум, инкремент ограничен максимумом.
    /// </summary>
    private static double AdjustAcceleration(ref double optInAccelerationInit, ref double optInAcceleration, double optInAccelerationMax)
    {
        var af = optInAccelerationInit;
        if (af > optInAccelerationMax)
        {
            optInAccelerationInit = optInAccelerationMax;
            af = optInAccelerationInit;
        }

        optInAcceleration = Math.Min(optInAcceleration, optInAccelerationMax);

        return af;
    }

    /// <summary>
    /// Определяет начальное направление тренда (Long/Short) на основе:
    /// 1) Явно заданного значения optInStartValue, или
    /// 2) Автоматического анализа направленного движения (DM) первых двух баров.
    /// </summary>
    private static (bool, Core.RetCode) DetermineInitialDirection<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        double optInStartValue,
        int startIdx) where T : IFloatingPointIeee754<T>
    {
        // Если задано явное начальное значение — используем его знак для определения направления
        if (!optInStartValue.Equals(0.0))
        {
            return (optInStartValue > 0.0, Core.RetCode.Success);
        }

        // Автоматическое определение направления через анализ направленного движения (-DM)
        Span<T> epTemp = new T[1];
        var retCode = MinusDMImpl(inHigh, inLow, new Range(startIdx, startIdx), epTemp, out _, 1);

        // Если +DM <= -DM — тренд нисходящий (Short), иначе восходящий (Long)
        return retCode == Core.RetCode.Success ? (epTemp[0] <= T.Zero, Core.RetCode.Success) : (default, retCode);
    }

    /// <summary>
    /// Инициализирует начальное значение SAR и экстремальной точки (EP) в зависимости от:
    /// - явно заданного значения optInStartValue, или
    /// - автоматического определения направления тренда (isLong).
    /// </summary>
    private static T InitializeSar<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        double optInStartValue,
        bool isLong,
        int todayIdx,
        T newLow,
        T newHigh,
        out T ep) where T : IFloatingPointIeee754<T>
    {
        T sar;
        switch (optInStartValue)
        {
            case 0.0 when isLong:
                // Автоматическая инициализация для длинной позиции
                ep = inHigh[todayIdx];      // EP = Highest High текущего бара
                sar = newLow;               // SAR = Lowest Low предыдущего бара
                break;
            case 0.0:
                // Автоматическая инициализация для короткой позиции
                ep = inLow[todayIdx];       // EP = Lowest Low текущего бара
                sar = newHigh;              // SAR = Highest High предыдущего бара
                break;
            case > 0.0:
                // Явная инициализация для длинной позиции
                ep = inHigh[todayIdx];
                sar = T.CreateChecked(optInStartValue);
                break;
            default:
                // Явная инициализация для короткой позиции (отрицательное значение)
                ep = inLow[todayIdx];
                sar = T.CreateChecked(Math.Abs(optInStartValue));
                break;
        }

        return sar;
    }
}
