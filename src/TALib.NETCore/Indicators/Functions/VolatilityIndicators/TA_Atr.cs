// файл TA_Atr.cs
// Группы к которым можно отнести индикатор:
// VolatilityIndicators (существующая папка - идеальное соответствие категории)
// MomentumIndicators (альтернатива, если требуется группировка по типу индикатора)
// PriceRangeIndicators (альтернатива для акцента на диапазоне цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// ATR (Volatility Indicators) — Средний Истинный Диапазон (Индикаторы Волатильности)
    /// </summary>
    /// <param name="inHigh">
    /// Входные данные для максимальных цен (High).  
    /// Массив содержащий максимальные цены за каждый период.
    /// </param>
    /// <param name="inLow">
    /// Входные данные для минимальных цен (Low).  
    /// Массив содержащий минимальные цены за каждый период.
    /// </param>
    /// <param name="inClose">
    /// Входные данные для цен закрытия (Close).  
    /// Массив содержащий цены закрытия за каждый период.
    /// </param>
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
    /// <param name="optInTimePeriod">
    /// Период времени для расчета ATR.  
    /// - Значение по умолчанию: 14 периодов.  
    /// - Должно быть больше или равно 1.
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
    /// Средний Истинный Диапазон (ATR) — это индикатор волатильности, который измеряет степень изменения цены за указанный период времени.
    /// <para>
    /// Функция широко используется для установки уровней стоп-лосс, управления размерами позиций и оценки условий волатильности.  
    /// Она может быть интегрирована с индикаторами тренда или импульса для улучшения управления рисками.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить Истинный Диапазон (TR), который является наибольшим из следующих значений:
    ///       <code>
    ///         TR = max[(High - Low), abs(High - Previous Close), abs(Low - Previous Close)]
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для первого значения ATR вычислить простое среднее значений TR за указанный период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для последующих значений ATR использовать метод сглаживания Уайлдера:
    ///       <code>
    ///         ATR = [(Previous ATR * (Time Period - 1)) + Current TR] / Time Period
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокое значение указывает на высокую волатильность рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкое значение свидетельствует о снижении волатильности.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       ATR не указывает направление движения цены, но отражает степень волатильности за указанный период.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Atr<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AtrImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предыстории (lookback) для <see cref="Atr{T}">Atr</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета ATR.</param>
    /// <returns>
    /// Количество периодов, необходимых до вычисления первого валидного выходного значения.  
    /// Это значение обозначает индекс первого бара во входящих данных, для которого можно получить валидное значение индикатора.
    /// </returns>
    [PublicAPI]
    public static int AtrLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 1 ? -1 : optInTimePeriod + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Atr);

    /// <remarks>
    /// Для совместимости с абстрактным API.  
    /// Перегрузка метода для работы с массивами вместо Span.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Atr<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AtrImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode AtrImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализируем outRange как пустой диапазон, начинающийся с 0
        outRange = Range.EndAt(0);

        // Проверяем корректность входных диапазонов для всех трёх массивов цен
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлекаем начальный и конечный индексы из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Проверяем, что период времени корректен (должен быть >= 1)
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* Средний Истинный Диапазон — это наибольшее из следующих значений:
         *
         *   val1 = расстояние от сегодняшнего максимума до сегодняшнего минимума (High - Low).
         *   val2 = расстояние от вчерашнего закрытия до сегодняшнего максимума (abs(High - Previous Close)).
         *   val3 = расстояние от вчерашнего закрытия до сегодняшнего минимума (abs(Low - Previous Close)).
         *
         * Эти значения усредняются за указанный период с использованием метода сглаживания Уайлдера.
         * Метод имеет нестабильный период, сравнимый с экспоненциальным скользящим средним (EMA).
         */

        // Вычисляем общее количество периодов lookback, необходимых для первого валидного значения ATR
        var lookbackTotal = AtrLookback(optInTimePeriod);
        // Корректируем начальный индекс с учётом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, нет данных для обработки
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Учитываем случай, когда сглаживание не требуется (период = 1).
        if (optInTimePeriod == 1)
        {
            return TRange(inHigh, inLow, inClose, inRange, outReal, out outRange);
        }

        // Создаём временный буфер для хранения значений True Range (TR)
        Span<T> tempBuffer = new T[lookbackTotal + (endIdx - startIdx) + 1];
        // Вычисляем значения True Range для всего необходимого диапазона
        var retCode = TRangeImpl(inHigh, inLow, inClose, new Range(startIdx - lookbackTotal + 1, endIdx), tempBuffer, out _);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Первое значение ATR — это простое среднее (SMA) значений True Range за указанный период.
        Span<T> prevATRTemp = new T[1];
        retCode = FunctionHelpers.CalcSimpleMA(tempBuffer, new Range(optInTimePeriod - 1, optInTimePeriod - 1), prevATRTemp, out _,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Преобразуем период времени в тип T для математических операций
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Получаем первое значение ATR из рассчитанного простого среднего
        var prevATR = prevATRTemp[0];

        /* Последующие значения сглаживаются с использованием предыдущего значения ATR (метод Уайлдера).
         *   1) Умножить предыдущее значение ATR на 'период - 1'.
         *   2) Добавить сегодняшнее значение TR.
         *   3) Разделить на 'период'.
         * Формула: ATR = [(Previous ATR * (Time Period - 1)) + Current TR] / Time Period
         */
        var today = optInTimePeriod;
        // Получаем значение нестабильного периода из настроек
        var outIdx = Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Atr);
        // Пропускаем нестабильный период для обеспечения стабильности значений
        while (outIdx != 0)
        {
            prevATR *= timePeriod - T.One;
            prevATR += tempBuffer[today++];
            prevATR /= timePeriod;
            outIdx--;
        }

        outIdx = 1;
        outReal[0] = prevATR;

        // Выполняем количество запрошенных значений ATR
        var nbATR = endIdx - startIdx + 1;

        while (--nbATR != 0)
        {
            prevATR *= timePeriod - T.One;
            prevATR += tempBuffer[today++];
            prevATR /= timePeriod;
            outReal[outIdx++] = prevATR;
        }

        // Устанавливаем выходной диапазон: от startIdx до startIdx + outIdx
        outRange = new Range(startIdx, startIdx + outIdx);

        return retCode;
    }
}
