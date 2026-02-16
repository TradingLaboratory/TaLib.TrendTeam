// TA_Cci.cs
// Рекомендуемые категории для группировки файла:
// 1. MomentumIndicators (основная категория - 100%)
// 2. CycleIndicators (альтернатива ~60% - выявление циклических движений)
// 3. Oscillators (альтернатива ~55% - осцилляторный характер индикатора)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Commodity Channel Index (Momentum Indicators) — Индекс Товарного Канала (Индикаторы импульса)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены (High).</param>
    /// <param name="inLow">Входные минимальные цены (Low).</param>
    /// <param name="inClose">Входные цены закрытия (Close).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c>, <c>inLow[outRange.Start + i]</c> и <c>inClose[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени для расчёта индикатора (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индекс Товарного Канала (CCI) — осциллятор импульса, измеряющий отклонение типичной цены (Typical Price)
    /// от её простого скользящего среднего (SMA) за указанный период, нормализованное относительно среднего абсолютного отклонения.
    /// </para>
    /// <para>
    /// Основные применения:
    /// - Выявление состояний перекупленности (значения > +100) и перепроданности (значения < -100)
    /// - Обнаружение дивергенций между ценой и индикатором
    /// - Определение силы и направления тренда
    /// - Выявление циклических движений на рынке
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление Типичной Цены (TP) для каждого бара:
    ///       <code>
    ///         TP = (High + Low + Close) / 3
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт простого скользящего среднего (SMA) типичной цены за период <c>optInTimePeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление среднего абсолютного отклонения (Mean Deviation) типичной цены от SMA:
    ///       <code>
    ///         MeanDev = Σ|TP - SMA| / optInTimePeriod
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчёт финального значения CCI:
    ///       <code>
    ///         CCI = (TP - SMA) / (0.015 × MeanDev)
    ///       </code>
    ///       Коэффициент 0.015 подобран так, чтобы ~70-80% значений индикатора находились в диапазоне [-100, +100].
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация сигналов</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>CCI > +100</b>: цена значительно выше среднего → состояние перекупленности, возможен разворот вниз.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>CCI < -100</b>: цена значительно ниже среднего → состояние перепроданности, возможен разворот вверх.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Пересечение нулевой линии снизу вверх</b>: бычий сигнал (рост импульса).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Пересечение нулевой линии сверху вниз</b>: медвежий сигнал (падение импульса).
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Cci<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CciImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback period) для индикатора <see cref="Cci{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчёта индикатора.</param>
    /// <returns>
    /// Количество баров, необходимых до первого валидного значения индикатора.
    /// Для CCI lookback = optInTimePeriod - 1, так как для расчёта требуется полный период данных.
    /// Возвращает -1, если <paramref name="optInTimePeriod"/> меньше 2 (некорректный период).
    /// </returns>
    /// <remarks>
    /// Lookback period определяет минимальное количество исторических данных, необходимых
    /// для получения первого валидного значения индикатора. Все бары с индексом меньше lookback
    /// будут пропущены при расчёте.
    /// </remarks>
    [PublicAPI]
    public static int CciLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Вспомогательный метод для совместимости с массивами (вместо Span).
    /// Перенаправляет вызов в основную реализацию <see cref="CciImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Cci<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CciImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Основная реализация расчёта индикатора Commodity Channel Index (CCI).
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High).</param>
    /// <param name="inLow">Массив минимальных цен (Low).</param>
    /// <param name="inClose">Массив цен закрытия (Close).</param>
    /// <param name="inRange">Диапазон обрабатываемых данных во входных массивах.</param>
    /// <param name="outReal">Выходной массив для хранения рассчитанных значений CCI.</param>
    /// <param name="outRange">Выходной диапазон, указывающий индексы валидных значений в исходных данных.</param>
    /// <param name="optInTimePeriod">Период времени для расчёта индикатора.</param>
    /// <typeparam name="T">Числовой тип с плавающей точкой.</typeparam>
    /// <returns>Код результата операции.</returns>
    private static Core.RetCode CciImpl<T>(
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

        // Проверка корректности периода (минимум 2 бара для расчёта)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчёт минимального индекса для первого валидного значения
        var lookbackTotal = CciLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback не осталось данных для расчёта — возвращаем успех с пустым результатом
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделение циклического буфера для хранения типичных цен за период
        // Циклический буфер позволяет эффективно обновлять данные без повторного суммирования всего периода
        Span<T> circBuffer = new T[optInTimePeriod];
        var circBufferIdx = 0;
        var maxIdxCircBuffer = optInTimePeriod - 1;

        // Инициализация буфера: заполнение данными из начального периода (до startIdx)
        // Вычисление типичной цены (Typical Price) для каждого бара: (High + Low + Close) / 3
        var i = startIdx - lookbackTotal;
        while (i < startIdx)
        {
            circBuffer[circBufferIdx++] = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
            i++;
            if (circBufferIdx > maxIdxCircBuffer)
            {
                circBufferIdx = 0;
            }
        }

        // Предварительно созданные константы для оптимизации расчётов
        var timePeriod = T.CreateChecked(optInTimePeriod);      // Период как значение типа T
        var tPointZeroOneFive = T.CreateChecked(0.015);         // Константа 0.015 для нормализации CCI

        // Основной цикл расчёта значений CCI для запрашиваемого диапазона
        var outIdx = 0;
        do
        {
            // Вычисление типичной цены текущего бара
            var lastValue = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
            // Сохранение в циклический буфер (старое значение автоматически перезаписывается)
            circBuffer[circBufferIdx++] = lastValue;

            // Расчёт простого скользящего среднего (SMA) типичных цен за период
            var theAverage = CalcAverage(circBuffer, timePeriod);

            // Расчёт суммы абсолютных отклонений от среднего (для последующего вычисления среднего отклонения)
            var tempReal2 = CalcSummation(circBuffer, theAverage);

            // Вычисление числителя формулы CCI: отклонение текущей типичной цены от среднего
            var tempReal = lastValue - theAverage;
            // Вычисление знаменателя формулы CCI: 0.015 × (среднее абсолютное отклонение)
            var denominator = tPointZeroOneFive * (tempReal2 / timePeriod);
            // Финальный расчёт CCI с защитой от деления на ноль
            outReal[outIdx++] = !T.IsZero(tempReal) && !T.IsZero(denominator) ? tempReal / denominator : T.Zero;

            // Циклическое обновление индекса буфера
            if (circBufferIdx > maxIdxCircBuffer)
            {
                circBufferIdx = 0;
            }

            i++;
        } while (i <= endIdx);

        // Установка выходного диапазона: первый и последний индексы с валидными значениями
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Вычисляет простое скользящее среднее (SMA) значений в циклическом буфере.
    /// </summary>
    /// <param name="circBuffer">Циклический буфер с типичными ценами за период.</param>
    /// <param name="timePeriod">Длина периода (количество элементов в буфере).</param>
    /// <typeparam name="T">Числовой тип с плавающей точкой.</typeparam>
    /// <returns>Среднее арифметическое всех значений в буфере.</returns>
    private static T CalcAverage<T>(Span<T> circBuffer, T timePeriod) where T : IFloatingPointIeee754<T>
    {
        var theAverage = T.Zero;
        foreach (var t in circBuffer)
        {
            theAverage += t;
        }

        theAverage /= timePeriod;
        return theAverage;
    }

    /// <summary>
    /// Вычисляет сумму абсолютных отклонений значений в буфере от заданного среднего.
    /// </summary>
    /// <param name="circBuffer">Циклический буфер с типичными ценами за период.</param>
    /// <param name="theAverage">Среднее значение (обычно SMA типичных цен).</param>
    /// <typeparam name="T">Числовой тип с плавающей точкой.</typeparam>
    /// <returns>Сумма абсолютных отклонений |значение - среднее| для всех элементов буфера.</returns>
    /// <remarks>
    /// Результат используется для расчёта среднего абсолютного отклонения (Mean Deviation):
    /// MeanDev = CalcSummation(circBuffer, theAverage) / timePeriod
    /// </remarks>
    private static T CalcSummation<T>(Span<T> circBuffer, T theAverage) where T : IFloatingPointIeee754<T>
    {
        var tempReal2 = T.Zero;
        foreach (var t in circBuffer)
        {
            tempReal2 += T.Abs(t - theAverage);
        }

        return tempReal2;
    }
}
