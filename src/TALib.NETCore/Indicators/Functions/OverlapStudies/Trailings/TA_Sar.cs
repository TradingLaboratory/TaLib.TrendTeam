// Название файла: TA_Sar.cs
// Рекомендуемое размещение:
// Основная папка: OverlapStudies
// Подпапка: Trailings (идеальное соответствие — индикатор используется для трейлинговых стопов)
// Альтернативные категории: TrendFollowing, ReversalIndicators

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Parabolic SAR (Overlap Studies) — Параболический SAR (Индикаторы наложения)
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High).</param>
    /// <param name="inLow">Массив минимальных цен (Low).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInAcceleration">
    /// Коэффициент ускорения (Acceleration Factor, AF), контролирующий чувствительность SAR:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения делают SAR более чувствительным к изменениям цен, но могут увеличить риск ложных сигналов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшие значения снижают чувствительность, обеспечивая более плавные значения.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.01..0.2</c>.
    /// </para>
    /// </param>
    /// <param name="optInMaximum">
    /// Максимальное значение коэффициента ускорения (Maximum AF):
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения позволяют SAR быстрее ускоряться во время сильных трендов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Меньшие значения ограничивают ускорение, поддерживая более плавные тренды.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Типичный диапазон: <c>0.01..0.5</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает <see cref="Core.RetCode"/>, указывающий успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Параболический Stop and Reverse (SAR) — это трендовый индикатор наложения, разработанный Уэллсом Уайлдером (Welles Wilder).
    /// Индикатор отображает серию точек выше или ниже ценовых баров для определения направления тренда и потенциальных точек разворота.
    /// </para>
    /// <para>
    /// Основные характеристики:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Точки SAR ниже цены (Low) указывают на восходящий тренд (позиция Long).</description>
    ///   </item>
    ///   <item>
    ///     <description>Точки SAR выше цены (High) указывают на нисходящий тренд (позиция Short).</description>
    ///   </item>
    ///   <item>
    ///     <description>Пересечение цены и SAR сигнализирует о потенциальном развороте тренда.</description>
    ///   </item>
    ///   <item>
    ///     <description>Используется как динамический трейлинг-стоп для управления рисками.</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// Формула расчета:
    /// <code>
    /// SAR<sub>n</sub> = SAR<sub>n-1</sub> + AF × (EP - SAR<sub>n-1</sub>)
    /// </code>
    /// Где:
    /// <list type="bullet">
    ///   <item><description>SAR<sub>n</sub> — текущее значение индикатора</description></item>
    ///   <item><description>SAR<sub>n-1</sub> — предыдущее значение индикатора</description></item>
    ///   <item><description>AF (Acceleration Factor) — коэффициент ускорения, начинающийся с optInAcceleration и увеличивающийся до optInMaximum</description></item>
    ///   <item><description>EP (Extreme Point) — экстремальная точка (максимум для Long, минимум для Short)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Особенности реализации:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Начальное направление определяется через сравнение направленного движения (+DM и -DM) между первым и вторым барами.
    ///       При равенстве направление по умолчанию — Long.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Начальное значение SAR устанавливается как:
    ///       <list type="bullet">
    ///         <item>Для Long: Lowest Low первого бара</item>
    ///         <item>Для Short: Highest High первого бара</item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       После переключения позиции SAR сбрасывается на значение EP предыдущего тренда, затем корректируется с учетом ценовых экстремумов двух последних баров.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       SAR никогда не должен проникать в ценовой диапазон двух предыдущих баров (проверка через max/min с предыдущими High/Low).
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// Рекомендации по использованию:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Наилучшие результаты достигаются на трендовых рынках; в боковиках возможны ложные сигналы.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для фильтрации сигналов рекомендуется комбинировать с <see cref="Adx{T}">ADX</see> (подтверждение силы тренда)
    ///       или <see cref="Atr{T}">ATR</see> (учет волатильности).
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sar<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        double optInAcceleration = 0.02,
        double optInMaximum = 0.2) where T : IFloatingPointIeee754<T> =>
        SarImpl(inHigh, inLow, inRange, outReal, out outRange, optInAcceleration, optInMaximum);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="Sar{T}">Sar</see>.
    /// </summary>
    /// <returns>Всегда 1, так как для расчета требуется только один предыдущий ценовой бар.</returns>
    [PublicAPI]
    public static int SarLookback() => 1;

    /// <remarks>
    /// Для совместимости с абстрактным API (массивная версия)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sar<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        double optInAcceleration = 0.02,
        double optInMaximum = 0.2) where T : IFloatingPointIeee754<T> =>
        SarImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInAcceleration, optInMaximum);

    private static Core.RetCode SarImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        double optInAcceleration,
        double optInMaximum) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона (должен быть валидным для обоих массивов)
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности параметров ускорения (не могут быть отрицательными)
        if (optInAcceleration < 0.0 || optInMaximum < 0.0)
        {
            return Core.RetCode.BadParam;
        }

        /* Реализация алгоритма SAR содержит интерпретационные особенности, так как Уэллс Уайлдер
         * (оригинальный автор) не определил точный алгоритм инициализации.
         * Разные торговые платформы используют различные подходы к запуску алгоритма.
         *
         * Определение начального направления тренда:
         * ───────────────────────────────────────────────────
         * Направление определяется через сравнение направленного движения (+DM и -DM)
         * между первым и вторым ценовыми барами:
         *   - Если +DM > -DM → начальное направление Long
         *   - Если -DM > +DM → начальное направление Short
         *   - При равенстве → направление по умолчанию Long
         *
         * Определение начальной экстремальной точки (EP) и SAR:
         * ───────────────────────────────────────────────────
         * Сравнение подходов различных платформ:
         *   - Metastock: использует максимум/минимум первого бара в зависимости от направления.
         *                SAR для первого бара не рассчитывается.
         *   - Tradestation: использует цену закрытия (Close) второго бара.
         *   - Уайлдер: использует экстремальную точку предыдущей сделки (неприменимо без контекста истории).
         *
         * Данная реализация:
         *   - "Потребляет" первый ценовой бар (индекс startIdx - 1)
         *   - Использует его минимум (для Long) или максимум (для Short) как начальное значение SAR
         *   - Использует максимум/минимум второго бара как начальную экстремальную точку (EP)
         *   - Подход максимально приближен к логике Уайлдера и совпадает с реализацией Metastock.
         */

        // Минимальный период для расчета (требуется 1 предыдущий бар)
        var lookbackTotal = SarLookback();
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учета lookback нет данных для обработки — возврат успеха с пустым диапазоном
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Коррекция параметров: ускорение не может превышать максимум
        optInAcceleration = Math.Min(optInAcceleration, optInMaximum);
        var af = optInAcceleration; // Текущее значение коэффициента ускорения (Acceleration Factor)

        // Определение начального направления через расчет -DM для первого бара
        // (epTemp используется как временный буфер для хранения значения -DM)
        Span<T> epTemp = new T[1];
        var retCode = MinusDMImpl(inHigh, inLow, new Range(startIdx, startIdx), epTemp, out _, 1);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Индекс первого валидного значения в выходном массиве
        var outBegIdx = startIdx;
        var outIdx = 0;

        // Текущий индекс обрабатываемого бара
        var todayIdx = startIdx;

        // Цены предыдущего бара (индекс startIdx - 1) для инициализации SAR
        var newHigh = inHigh[todayIdx - 1];
        var newLow = inLow[todayIdx - 1];

        // Определение начального направления: Long если -DM <= 0 (т.е. +DM >= -DM)
        var isLong = epTemp[0] <= T.Zero;

        // Инициализация начального значения SAR и экстремальной точки (EP)
        var sar = InitializeSar(inHigh, inLow, isLong, todayIdx, newLow, newHigh, out var ep);

        // Обновление цен для текущего бара (индекс startIdx)
        newLow = inLow[todayIdx];
        newHigh = inHigh[todayIdx];

        // Основной цикл расчета SAR для каждого бара
        while (todayIdx <= endIdx)
        {
            // Сохранение цен предыдущего бара перед обновлением
            var prevLow = newLow;
            var prevHigh = newHigh;
            newLow = inLow[todayIdx];
            newHigh = inHigh[todayIdx];
            todayIdx++;

            if (isLong)
            {
                // Проверка условия разворота: пробитие SAR снизу вверх (цена Low <= SAR)
                if (newLow <= sar)
                {
                    // Переключение на короткую позицию (Short)
                    isLong = false;
                    sar = SwitchToShort(ref ep, prevHigh, newLow, newHigh, out af, optInAcceleration, T.NegativeOne, ref outIdx, outReal);
                }
                else
                {
                    // Сохранение текущего значения SAR в выходной массив (рассчитано в предыдущей итерации)
                    outReal[outIdx++] = sar;

                    // Обработка длинной позиции без разворота
                    sar = ProcessLongPosition(ref ep, prevLow, newLow, newHigh, ref af, optInAcceleration, optInMaximum, sar);
                }
            }
            // Проверка условия разворота для короткой позиции: пробитие SAR сверху вниз (цена High >= SAR)
            else if (newHigh >= sar)
            {
                // Переключение на длинную позицию (Long)
                isLong = true;
                sar = SwitchToLong(ref ep, prevLow, newLow, newHigh, out af, optInAcceleration, T.NegativeOne, ref outIdx, outReal);
            }
            else
            {
                // Сохранение текущего значения SAR в выходной массив (рассчитано в предыдущей итерации)
                outReal[outIdx++] = sar;

                // Обработка короткой позиции без разворота
                sar = ProcessShortPosition(ref ep, prevHigh, newLow, newHigh, ref af, optInAcceleration, optInMaximum, sar);
            }
        }

        // Установка диапазона валидных значений в выходном массиве
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T InitializeSar<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        bool isLong,
        int todayIdx,
        T newLow,
        T newHigh,
        out T ep) where T : IFloatingPointIeee754<T>
    {
        T sar;
        if (isLong)
        {
            // Для длинной позиции: EP = максимум текущего бара, SAR = минимум предыдущего бара
            ep = inHigh[todayIdx];
            sar = newLow;
        }
        else
        {
            // Для короткой позиции: EP = минимум текущего бара, SAR = максимум предыдущего бара
            ep = inLow[todayIdx];
            sar = newHigh;
        }

        return sar;
    }

    private static T SwitchToShort<T>(
        ref T ep,
        T prevHigh,
        T newLow,
        T newHigh,
        out double af,
        double optInAcceleration,
        T optInOffsetOnReverse,
        ref int outIdx,
        Span<T> outReal) where T : IFloatingPointIeee754<T>
    {
        // Сброс SAR на значение предыдущей экстремальной точки (EP)
        var sar = ep;

        // Коррекция SAR: должен находиться выше максимумов двух последних баров (защита от проникновения в ценовой диапазон)
        sar = T.Max(sar, prevHigh);
        sar = T.Max(sar, newHigh);

        // Дополнительное смещение при развороте (не используется в стандартной реализации, т.к. optInOffsetOnReverse = -1)
        if (optInOffsetOnReverse > T.Zero)
        {
            sar += sar * optInOffsetOnReverse;
        }

        // Сохранение скорректированного значения SAR в выходной массив (со знаком минус для визуального разделения направлений)
        outReal[outIdx++] = sar * (optInOffsetOnReverse < T.Zero ? T.One : T.NegativeOne);

        // Сброс коэффициента ускорения и установка новой экстремальной точки (минимум текущего бара)
        af = optInAcceleration;
        ep = newLow;

        // Расчет нового значения SAR по формуле: SAR = SAR + AF * (EP - SAR)
        sar += T.CreateChecked(af) * (ep - sar);

        // Повторная коррекция SAR относительно максимумов двух последних баров
        sar = T.Max(sar, prevHigh);
        sar = T.Max(sar, newHigh);

        return sar;
    }

    private static T ProcessLongPosition<T>(
        ref T ep,
        T prevLow,
        T newLow,
        T newHigh,
        ref double af,
        double optInAcceleration,
        double optInMaximum,
        T sar) where T : IFloatingPointIeee754<T>
    {
        // Обновление экстремальной точки (EP) и коэффициента ускорения (AF) при новом максимуме
        if (newHigh > ep)
        {
            ep = newHigh; // Новый максимум становится экстремальной точкой
            af += optInAcceleration; // Увеличение коэффициента ускорения
            af = Math.Min(af, optInMaximum); // Ограничение максимальным значением
        }

        // Расчет нового значения SAR по формуле: SAR = SAR + AF * (EP - SAR)
        sar += T.CreateChecked(af) * (ep - sar);

        // Коррекция SAR: должен находиться ниже минимумов двух последних баров (защита от проникновения в ценовой диапазон)
        sar = T.Min(sar, prevLow);
        sar = T.Min(sar, newLow);

        return sar;
    }

    private static T SwitchToLong<T>(
        ref T ep,
        T prevLow,
        T newLow,
        T newHigh,
        out double af,
        double optInAcceleration,
        T optInOffsetOnReverse,
        ref int outIdx,
        Span<T> outReal) where T : IFloatingPointIeee754<T>
    {
        // Сброс SAR на значение предыдущей экстремальной точки (EP)
        var sar = ep;

        // Коррекция SAR: должен находиться ниже минимумов двух последних баров (защита от проникновения в ценовой диапазон)
        sar = T.Min(sar, prevLow);
        sar = T.Min(sar, newLow);

        // Дополнительное смещение при развороте (не используется в стандартной реализации)
        if (optInOffsetOnReverse > T.Zero)
        {
            sar -= sar * optInOffsetOnReverse;
        }

        // Сохранение скорректированного значения SAR в выходной массив
        outReal[outIdx++] = sar;

        // Сброс коэффициента ускорения и установка новой экстремальной точки (максимум текущего бара)
        af = optInAcceleration;
        ep = newHigh;

        // Расчет нового значения SAR по формуле: SAR = SAR + AF * (EP - SAR)
        sar += T.CreateChecked(af) * (ep - sar);

        // Повторная коррекция SAR относительно минимумов двух последних баров
        sar = T.Min(sar, prevLow);
        sar = T.Min(sar, newLow);

        return sar;
    }

    private static T ProcessShortPosition<T>(
        ref T ep,
        T prevHigh,
        T newLow,
        T newHigh,
        ref double af,
        double optInAcceleration,
        double optInMaximum,
        T sar) where T : IFloatingPointIeee754<T>
    {
        // Обновление экстремальной точки (EP) и коэффициента ускорения (AF) при новом минимуме
        if (newLow < ep)
        {
            ep = newLow; // Новый минимум становится экстремальной точкой
            af += optInAcceleration; // Увеличение коэффициента ускорения
            af = Math.Min(af, optInMaximum); // Ограничение максимальным значением
        }

        // Расчет нового значения SAR по формуле: SAR = SAR + AF * (EP - SAR)
        sar += T.CreateChecked(af) * (ep - sar);

        // Коррекция SAR: должен находиться выше максимумов двух последних баров (защита от проникновения в ценовой диапазон)
        sar = T.Max(sar, prevHigh);
        sar = T.Max(sar, newHigh);

        return sar;
    }
}
