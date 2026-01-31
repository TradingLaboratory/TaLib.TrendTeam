//Название файла: Natr.cs
//Группы к которым можно отнести индикатор:
//VolatilityIndicators (существующая папка - идеальное соответствие категории)
//MomentumIndicators (альтернатива ≥50%, так как волатильность влияет на импульс)
//RiskMetrics (альтернатива для акцента на измерении риска)

// TODO: Исправить индексацию — должно быть outReal[outIdx], а не outReal[0]
/*
Примечание: В оригинальном коде обнаружена потенциальная ошибка в строке outReal[0] = T.Zero;
(должно быть outReal[outIdx] = T.Zero;).
Рекомендуется отдельно исправить эту ошибку в проекте.

if (!T.IsZero(tempValue))
{
    outReal[outIdx] = prevATR / tempValue * FunctionHelpers.Hundred<T>();
}
else
{
    // TODO: Исправить индексацию — должно быть outReal[outIdx], а не outReal[0]
    // См. примечание от 2026-01-31: ошибка в оригинальной реализации библиотеки
    outReal[0] = T.Zero; 
}

 */

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Normalized Average True Range (Volatility Indicators) — Нормализованный средний истинный диапазон (Индикаторы волатильности)
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимальные цены баров)</param>
    /// <param name="inLow">Массив входных цен Low (минимальные цены баров)</param>
    /// <param name="inClose">Массив входных цен Close (цены закрытия баров)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатываются все элементы входных массивов.
    /// </param>
    /// <param name="outReal">
    /// Массив для хранения рассчитанных значений индикатора NATR.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует бару <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения индикатора:  
    /// - <b>Start</b>: индекс первого бара с валидным значением NATR в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего бара с валидным значением NATR в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inClose.GetUpperBound(0)</c>, если расчёт успешен.  
    /// - Если данных недостаточно (длина входных данных меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (количество баров для усреднения). Значение по умолчанию: 14.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчёта.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчёте или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция вычисляет нормализованную меру волатильности цены, выраженную в процентах от цены закрытия.  
    /// NATR улучшает традиционный <see cref="Atr{T}">ATR</see>, делая его сопоставимым для разных периодов или инструментов с различными ценовыми диапазонами.
    /// </para>
    /// <para>
    /// Индикатор особенно полезен для долгосрочного анализа, когда ценовые диапазоны значительно меняются,  
    /// а также для сравнения волатильности между разными рынками или финансовыми инструментами.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление истинного диапазона (True Range, TR) как максимального из следующих значений:
    ///       <code>
    ///         TR = max[(High - Low), abs(High - Previous Close), abs(Low - Previous Close)]
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><description><c>High - Low</c> — текущий ценовой диапазон бара</description></item>
    ///         <item><description><c>abs(High - Previous Close)</c> — разрыв вверх от предыдущего закрытия</description></item>
    ///         <item><description><c>abs(Low - Previous Close)</c> — разрыв вниз от предыдущего закрытия</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для первого значения ATR рассчитывается простое среднее арифметическое значений TR за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для последующих значений ATR применяется метод сглаживания Уайлдера (Wilder's smoothing):
    ///       <code>
    ///         ATR = [(Previous ATR * (Time Period - 1)) + Current TR] / Time Period
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Нормализация ATR путём деления на соответствующую цену закрытия (Close) и умножения на 100:
    ///       <code>
    ///         NATR = (ATR / Close) * 100
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Высокое значение указывает на повышенную волатильность цены относительно уровня цены инструмента.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Низкое значение свидетельствует о пониженной волатильности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значения выражены в процентах, что позволяет сравнивать волатильность разных инструментов независимо от их абсолютной цены.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Natr<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        NatrImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback period) для функции <see cref="Natr{T}">Natr</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта (количество баров для усреднения). Значение по умолчанию: 14.</param>
    /// <returns>Количество периодов, необходимых перед расчётом первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int NatrLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 1 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Natr);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Natr<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        NatrImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode NatrImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и проверка согласованности длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода расчёта
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Данная функция очень похожа на ATR, за исключением нормализации по формуле:
         *
         *   NATR = (ATR(period) / Close) * 100
         *
         * Нормализация делает ATR более релевантным в следующих сценариях:
         *   - Долгосрочный анализ, где цена изменяется кардинально.
         *   - Сравнение ATR между разными рынками или финансовыми инструментами.
         *
         * Дополнительная информация:
         *   Журнал "Technical Analysis of Stock & Commodities (TASC)"
         *   Май 2006, автор John Forman
         */

        /* Средний истинный диапазон (ATR) определяется как максимальное из следующих значений:
         *
         *  val1 = расстояние от сегодняшнего максимума (High) до сегодняшнего минимума (Low).
         *  val2 = расстояние от вчерашнего закрытия (Previous Close) до сегодняшнего максимума (High).
         *  val3 = расстояние от вчерашнего закрытия (Previous Close) до сегодняшнего минимума (Low).
         *
         * Эти значения усредняются за указанный период с использованием метода Уайлдера.
         * Метод имеет нестабильный период, сопоставимый с экспоненциальной скользящей средней (EMA).
         */

        // Расчёт общего периода задержки (включая нестабильный период Уайлдера)
        var lookbackTotal = NatrLookback(optInTimePeriod);
        // Корректировка начального индекса с учётом периода задержки
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — валидных данных нет
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Оптимизация для периода = 1: не требуется сглаживание, достаточно расчёта истинного диапазона (TRange)
        if (optInTimePeriod == 1)
        {
            // Прямой расчёт истинного диапазона без усреднения
            return TRange(inHigh, inLow, inClose, inRange, outReal, out outRange);
        }

        // Временный буфер для хранения промежуточных значений истинного диапазона (TR)
        Span<T> tempBuffer = new T[lookbackTotal + (endIdx - startIdx) + 1];

        // Расчёт истинного диапазона (TR) во временный буфер для всего необходимого диапазона данных
        var retCode = TRangeImpl(inHigh, inLow, inClose, new Range(startIdx - lookbackTotal + 1, endIdx), tempBuffer, out _);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Буфер для хранения предыдущего значения ATR при итеративном расчёте
        Span<T> prevATRTemp = new T[1];

        // Первое значение ATR рассчитывается как простое среднее арифметическое значений TR за указанный период
        retCode = FunctionHelpers.CalcSimpleMA(tempBuffer, new Range(optInTimePeriod - 1, optInTimePeriod - 1), prevATRTemp, out _,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Преобразование целочисленного периода в числовой тип для арифметических операций
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Инициализация переменной предыдущего значения ATR
        var prevATR = prevATRTemp[0];

        /* Последующие значения ATR сглаживаются по методу Уайлдера:
         *   1) Умножить предыдущее значение ATR на (период - 1).
         *   2) Добавить текущее значение истинного диапазона (TR).
         *   3) Разделить результат на период.
         */
        // Индекс текущего бара в буфере истинного диапазона (начинаем с позиции после первого усреднения)
        var today = optInTimePeriod;
        // Счётчик для пропуска нестабильного периода (начальное значение из настроек)
        var outIdx = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Natr);
        // Пропуск нестабильного периода: итеративное сглаживание без сохранения результатов
        while (outIdx != 0)
        {
            prevATR *= timePeriod - T.One;
            prevATR += tempBuffer[today++];
            prevATR /= timePeriod;
            outIdx--;
        }

        // Расчёт первого валидного значения NATR после нестабильного периода
        outIdx = 1;
        // Текущая цена закрытия для нормализации
        var tempValue = inClose[today];
        // Нормализация: (ATR / Close) * 100. Проверка деления на ноль.
        outReal[0] = !T.IsZero(tempValue) ? prevATR / tempValue * FunctionHelpers.Hundred<T>() : T.Zero;

        // Расчёт оставшихся значений NATR для запрошенного диапазона
        var nbATR = endIdx - startIdx + 1;

        while (--nbATR != 0)
        {
            // Сглаживание ATR по методу Уайлдера
            prevATR *= timePeriod - T.One;
            prevATR += tempBuffer[today++];
            prevATR /= timePeriod;
            // Получение текущей цены закрытия для нормализации
            tempValue = inClose[today];
            if (!T.IsZero(tempValue))
            {
                // Нормализация ATR в проценты относительно цены закрытия
                outReal[outIdx] = prevATR / tempValue * FunctionHelpers.Hundred<T>();
            }
            else
            {
                // Защита от деления на ноль: возвращаем нулевое значение
                outReal[outIdx] = T.Zero;
            }

            outIdx++;
        }

        // Установка выходного диапазона валидных значений
        outRange = new Range(startIdx, startIdx + outIdx - 1);

        return retCode;
    }
}
