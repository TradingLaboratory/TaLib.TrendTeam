//Название файла: TA_Dx.cs
//Рекомендуемая подпапка: MomentumIndicators/TrendStrength
//Альтернативные подпапки: MomentumIndicators/DirectionalMovement, MomentumIndicators/WilderIndicators

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Directional Movement Index (Momentum Indicators) — Индекс направленного движения DX (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входные данные: максимальные цены (High).</param>
    /// <param name="inLow">Входные данные: минимальные цены (Low).</param>
    /// <param name="inClose">Входные данные: цены закрытия (Close).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени для сглаживания (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индекс направленного движения (DX) — технический индикатор, разработанный Уэллсом Уайлдером (J. Welles Wilder Jr.) 
    /// для измерения силы тренда на рынке. DX не указывает направление тренда, а лишь его интенсивность.
    /// </para>
    /// <para>
    /// DX является ключевым компонентом для расчета Индекса среднего направленного движения (<see cref="Adx{T}">ADX</see>) 
    /// и основывается на значениях Положительного (<see cref="PlusDI{T}">+DI</see>) и Отрицательного (<see cref="MinusDI{T}">-DI</see>) 
    /// индикаторов направленного движения.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление Истинного диапазона (TR), Положительного направленного движения (+DM) и Отрицательного направленного движения (-DM) для каждого периода.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сглаживание значений +DM, -DM и TR за указанный период с использованием метода сглаживания Уайлдера (Wilder's Smoothing):
    ///       <code>
    ///         Today's Smoothed = Previous Smoothed - (Previous Smoothed / Period) + Today's Value
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет направленных индикаторов:
    ///       <code>
    ///         +DI = (+DM / TR) * 100
    ///         -DI = (-DM / TR) * 100
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет DX:
    ///       <code>
    ///         DX = (|+DI - -DI| / (+DI + -DI)) * 100
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значения DX выше 25 обычно указывают на сильный тренд (высокую силу направленного движения).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения DX ниже 20 сигнализируют о слабом или отсутствующем тренде (боковое движение).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для определения направления тренда необходимо анализировать соотношение +DI и -DI:
    ///       когда +DI > -DI — восходящий тренд, когда -DI > +DI — нисходящий тренд.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// <b>Источник</b>:
    /// <para>
    ///   New Concepts In Technical Trading Systems, J. Welles Wilder Jr.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Dx<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        DxImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для индикатора <see cref="Dx{T}">DX</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для сглаживания (по умолчанию 14).</param>
    /// <returns>
    /// Количество баров, необходимых до расчета первого валидного значения индикатора.
    /// Включает базовый период + нестабильный период сглаживания Уайлдера.
    /// </returns>
    [PublicAPI]
    public static int DxLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Dx);

    /// <remarks>
    /// Вспомогательная перегрузка для совместимости с абстрактным API (работа с массивами вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Dx<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        DxImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode DxImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона и длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимально допустимого периода (не менее 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Расчет направленного движения (DM) за один период (+DM1 и -DM1):
         *
         * +DM1 определяется как разница между сегодняшним максимумом (High) и вчерашним максимумом (Previous High),
         * если эта разница положительна И больше, чем разница между вчерашним минимумом и сегодняшним минимумом.
         * 
         * -DM1 определяется как разница между вчерашним минимумом (Previous Low) и сегодняшним минимумом (Low),
         * если эта разница положительна И больше, чем разница между сегодняшним максимумом и вчерашним максимумом.
         *
         * Существует 7 основных случаев взаимного расположения ценовых баров:
         *
         * Случай 1:                       Случай 2:
         *    C│                        A│
         *     │                         │ C│
         *     │ +DM1 = (C-A)           B│  │ +DM1 = 0
         *     │ -DM1 = 0                   │ -DM1 = (B-D)
         * A│  │                           D│
         *  │ D│
         * B│
         *
         * Случай 3:                       Случай 4:
         *    C│                           C│
         *     │                        A│  │
         *     │ +DM1 = (C-A)            │  │ +DM1 = 0
         *     │ -DM1 = 0               B│  │ -DM1 = (B-D)
         * A│  │                            │
         *  │  │                           D│
         * B│  │
         *    D│
         *
         * Случай 5:                      Случай 6:
         * A│                           A│ C│
         *  │ C│ +DM1 = 0                │  │  +DM1 = 0
         *  │  │ -DM1 = 0                │  │  -DM1 = 0
         *  │ D│                         │  │
         * B│                           B│ D│
         *
         *
         * Случай 7 (нейтральный):
         *
         *    C│
         * A│  │
         *  │  │ +DM1 = 0
         * B│  │ -DM1 = 0
         *    D│
         *
         * Правила:
         * - В случаях 3 и 4: наименьшая из разниц (C-A) и (B-D) определяет, какое значение (+DM1 или -DM1) будет равно нулю.
         * - В случае 7: когда (C-A) = (B-D), оба значения +DM1 и -DM1 равны нулю.
         * - При равенстве максимумов и минимумов (A=B и C=D) правила остаются теми же.
         *
         * Для периодов > 1:
         * - Первое значение сглаженного DM (например, +DM14) рассчитывается как сумма первых 14 значений +DM1 (13 значений, так как для первого бара нет предыдущего).
         * - Последующие значения рассчитываются по методу сглаживания Уайлдера:
         *       Сегодняшний +DM14 = Предыдущий +DM14 - (Предыдущий +DM14 / 14) + Сегодняшний +DM1
         *
         * Расчет +DI14 и -DI14:
         *       +DI14 = (+DM14 / TR14) * 100
         *       -DI14 = (-DM14 / TR14) * 100
         *
         * Расчет TR14 (Истинный диапазон):
         *       Первый TR14 = сумма первых 14 значений TR1
         *       Последующие: Сегодняшний TR14 = Предыдущий TR14 - (Предыдущий TR14 / 14) + Сегодняшний TR1
         *
         * Расчет DX14:
         *       diffDI = |(-DI14) - (+DI14)|
         *       sumDI  = (-DI14) + (+DI14)
         *       DX14 = 100 * (diffDI / sumDI)
         *
         * Источник: New Concepts In Technical Trading Systems, J. Welles Wilder Jr
         */

        // Расчет общего периода обратного просмотра (включая нестабильный период сглаживания)
        var lookbackTotal = DxLookback(optInTimePeriod);
        // Сдвиг начального индекса с учетом периода обратного просмотра
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после сдвига начальный индекс превышает конечный — нет данных для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Преобразование целочисленного периода в числовой тип T
        var timePeriod = T.CreateChecked(optInTimePeriod);
        // Инициализация переменных для хранения предыдущих значений
        T prevMinusDM, prevPlusDM;
        var prevTR = prevMinusDM = prevPlusDM = T.Zero;
        // Установка индекса на начало периода для инициализации (до первого валидного значения)
        var today = startIdx - lookbackTotal;

        // Инициализация значений направленного движения (+DM, -DM) и истинного диапазона (TR)
        // за период, необходимый для первого валидного расчета
        FunctionHelpers.InitDMAndTR(inHigh, inLow, inClose, out var prevHigh, ref today, out var prevLow, out var prevClose, timePeriod,
            ref prevPlusDM, ref prevMinusDM, ref prevTR);

        // Пропуск нестабильного периода сглаживания (для достижения стабильных значений)
        // Цикл выполняется минимум один раз для получения первого валидного значения DI
        SkipDxUnstablePeriod(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM, ref prevMinusDM,
            ref prevTR, timePeriod);

        // Расчет первого значения DX на основе сглаженных +DI и -DI
        if (!T.IsZero(prevTR))
        {
            // Расчет направленных индикаторов: +DI и -DI
            var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
            // Сумма абсолютных значений направленных индикаторов
            T tempReal = minusDI + plusDI;
            // Расчет DX: отношение разницы к сумме, умноженное на 100
            outReal[0] = !T.IsZero(tempReal) ? FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal) : T.Zero;
        }
        else
        {
            // При нулевом истинном диапазоне — значение DX равно нулю
            outReal[0] = T.Zero;
        }

        // Индекс для записи результатов в выходной массив (начинаем со второго элемента)
        var outIdx = 1;

        // Основной цикл расчета и записи значений DX для оставшихся баров
        CalcAndOutputDX(inHigh, inLow, inClose, outReal, ref today, endIdx, ref prevHigh, ref prevLow, ref prevClose,
            ref prevPlusDM, ref prevMinusDM, ref prevTR, timePeriod, ref outIdx);

        // Установка диапазона валидных значений в выходных данных
        outRange = Range.EndAt(outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Пропускает нестабильный период сглаживания для достижения стабильных значений индикатора.
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High).</param>
    /// <param name="inLow">Массив минимальных цен (Low).</param>
    /// <param name="inClose">Массив цен закрытия (Close).</param>
    /// <param name="today">Текущий индекс обработки (изменяется по ссылке).</param>
    /// <param name="prevHigh">Предыдущее значение максимума (изменяется по ссылке).</param>
    /// <param name="prevLow">Предыдущее значение минимума (изменяется по ссылке).</param>
    /// <param name="prevClose">Предыдущее значение цены закрытия (изменяется по ссылке).</param>
    /// <param name="prevPlusDM">Предыдущее сглаженное значение +DM (изменяется по ссылке).</param>
    /// <param name="prevMinusDM">Предыдущее сглаженное значение -DM (изменяется по ссылке).</param>
    /// <param name="prevTR">Предыдущее сглаженное значение истинного диапазона TR (изменяется по ссылке).</param>
    /// <param name="timePeriod">Период сглаживания в числовом типе T.</param>
    /// <typeparam name="T">Числовой тип данных с плавающей точкой.</typeparam>
    private static void SkipDxUnstablePeriod<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        ref int today,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod) where T : IFloatingPointIeee754<T>
    {
        // Количество итераций = нестабильный период + 1 (гарантирует минимум одну итерацию)
        for (var i = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Dx) + 1; i > 0; i--)
        {
            today++;
            // Обновление значений направленного движения и истинного диапазона с применением сглаживания Уайлдера
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);
        }
    }

    /// <summary>
    /// Выполняет основной расчет и запись значений индикатора DX в выходной массив.
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High).</param>
    /// <param name="inLow">Массив минимальных цен (Low).</param>
    /// <param name="inClose">Массив цен закрытия (Close).</param>
    /// <param name="outReal">Выходной массив для записи значений DX.</param>
    /// <param name="today">Текущий индекс обработки (изменяется по ссылке).</param>
    /// <param name="endIdx">Конечный индекс входных данных.</param>
    /// <param name="prevHigh">Предыдущее значение максимума (изменяется по ссылке).</param>
    /// <param name="prevLow">Предыдущее значение минимума (изменяется по ссылке).</param>
    /// <param name="prevClose">Предыдущее значение цены закрытия (изменяется по ссылке).</param>
    /// <param name="prevPlusDM">Предыдущее сглаженное значение +DM (изменяется по ссылке).</param>
    /// <param name="prevMinusDM">Предыдущее сглаженное значение -DM (изменяется по ссылке).</param>
    /// <param name="prevTR">Предыдущее сглаженное значение истинного диапазона TR (изменяется по ссылке).</param>
    /// <param name="timePeriod">Период сглаживания в числовом типе T.</param>
    /// <param name="outIdx">Индекс записи в выходной массив (изменяется по ссылке).</param>
    /// <typeparam name="T">Числовой тип данных с плавающей точкой.</typeparam>
    private static void CalcAndOutputDX<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Span<T> outReal,
        ref int today,
        int endIdx,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod,
        ref int outIdx) where T : IFloatingPointIeee754<T>
    {
        // Цикл обработки оставшихся баров до конца диапазона
        while (today < endIdx)
        {
            today++;

            // Обновление значений направленного движения (+DM, -DM) и истинного диапазона (TR)
            // с применением сглаживания Уайлдера
            FunctionHelpers.UpdateDMAndTR(inHigh, inLow, inClose, ref today, ref prevHigh, ref prevLow, ref prevClose, ref prevPlusDM,
                ref prevMinusDM, ref prevTR, timePeriod);

            // Расчет текущего истинного диапазона (TR1) на основе цен текущего и предыдущего бара
            var tempReal = FunctionHelpers.TrueRange(prevHigh, prevLow, prevClose);
            // Применение сглаживания Уайлдера к истинному диапазону
            prevTR = prevTR - prevTR / timePeriod + tempReal;
            // Обновление цены закрытия предыдущего бара
            prevClose = inClose[today];

            // Расчет DX только при ненулевом истинном диапазоне
            if (!T.IsZero(prevTR))
            {
                // Расчет направленных индикаторов на основе сглаженных значений
                var (minusDI, plusDI) = FunctionHelpers.CalcDI(prevMinusDM, prevPlusDM, prevTR);
                // Сумма направленных индикаторов для нормализации
                tempReal = minusDI + plusDI;
                // Расчет DX: отношение абсолютной разницы к сумме, умноженное на 100
                // При нулевой сумме — сохранение предыдущего значения для избежания разрыва
                outReal[outIdx] = !T.IsZero(tempReal)
                    ? FunctionHelpers.Hundred<T>() * (T.Abs(minusDI - plusDI) / tempReal)
                    : outReal[outIdx - 1];
            }
            else
            {
                // При нулевом истинном диапазоне — сохранение предыдущего значения
                outReal[outIdx] = outReal[outIdx - 1];
            }

            // Переход к следующей позиции в выходном массиве
            outIdx++;
        }
    }
}
