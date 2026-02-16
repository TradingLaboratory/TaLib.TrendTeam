//Название файла: TA_MacdFix.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators > Oscillators (идеальное соответствие - осциллятор импульса)
//TrendIndicators (альтернатива, если требуется группировка по трендовой составляющей)
//ConvergenceDivergence (предлагаемая подпапка для индикаторов типа сходимости/расходимости)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MACD Fix 12/26 (Momentum Indicators) — Сходимость/расходимость скользящих средних с фиксированными периодами (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены закрытия <see cref="Close"/>, но могут использоваться и другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMACD">Массив, содержащий ТОЛЬКО валидные значения линии MACD (разница между быстрой и медленной EMA).</param>
    /// <param name="outMACDSignal">Массив, содержащий ТОЛЬКО валидные значения сигнальной линии MACD (EMA от линии MACD).</param>
    /// <param name="outMACDHist">Массив, содержащий ТОЛЬКО валидные значения гистограммы MACD (разница между линией MACD и сигнальной линией).</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии (по умолчанию 9).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// MACD Fix — это упрощенная версия классического индикатора MACD с фиксированными периодами:
    /// быстрый период = 12, медленный период = 26. Сигнальная линия остается настраиваемой для гибкости анализа.
    /// </para>
    /// <para>
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить быструю экспоненциальную скользящую среднюю (EMA) входных значений с периодом 12.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить медленную EMA входных значений с периодом 26.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать линию MACD как разницу между быстрой и медленной EMA:
    ///       <code>
    ///         MACD = EMA(12) - EMA(26)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить сигнальную линию как EMA от линии MACD с периодом <paramref name="optInSignalPeriod"/>:
    ///       <code>
    ///         Signal = EMA(MACD, optInSignalPeriod)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать гистограмму MACD как разницу между линией MACD и сигнальной линией:
    ///       <code>
    ///         MACDHist = MACD - Signal
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Пересечение линии MACD с нулевой линией вверх — сигнал восходящего импульса, вниз — нисходящего.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Бычий перекрест: линия MACD пересекает сигнальную линию снизу вверх — потенциальный сигнал на покупку.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Медвежий перекрест: линия MACD пересекает сигнальную линию сверху вниз — потенциальный сигнал на продажу.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расширение гистограммы (увеличение высоты столбцов) указывает на усиление импульса, сужение — на его ослабление.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Особенности версии Fix</b>:
    /// В отличие от стандартного MACD, где быстрый и медленный периоды настраиваются, в версии Fix они жестко зафиксированы
    /// (12 и 26 соответственно), что обеспечивает единообразие расчетов и совместимость с классической методологией Уайлдера.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MacdFix<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdFixImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInSignalPeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для индикатора <see cref="MacdFix{T}"/>.
    /// </summary>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии (по умолчанию 9).</param>
    /// <returns>
    /// Минимальное количество баров, необходимое для расчета первого валидного значения индикатора.
    /// Рассчитывается как сумма периодов обратного просмотра для медленной EMA (26) и сигнальной линии.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Период lookback определяет, сколько предыдущих баров требуется для получения первого достоверного значения.
    /// Для MACD Fix: lookback = EmaLookback(26) + EmaLookback(optInSignalPeriod).
    /// </para>
    /// <para>
    /// Пример: при optInSignalPeriod = 9, lookback = 26 + 9 = 35 баров.
    /// Это означает, что первое валидное значение MACD будет доступно только на 35-м баре входных данных.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int MacdFixLookback(int optInSignalPeriod = 9) => EmaLookback(26) + EmaLookback(optInSignalPeriod);

    /// <summary>
    /// Внутренняя реализация индикатора MACD Fix для совместимости с массивами (устаревший API).
    /// </summary>
    /// <remarks>
    /// Приватный метод-обертка для поддержки совместимости с абстрактным API, использующим массивы вместо Span.
    /// Перенаправляет вызов в основную реализацию <see cref="MacdFixImpl{T}"/> через преобразование массивов в Span.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MacdFix<T>(
        T[] inReal,
        Range inRange,
        T[] outMACD,
        T[] outMACDSignal,
        T[] outMACDHist,
        out Range outRange,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdFixImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInSignalPeriod);

    private static Core.RetCode MacdFixImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInSignalPeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона значением [0, -1] (пустой диапазон) на случай ошибки
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и их соответствия длине входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка корректности периода сигнальной линии (должен быть >= 1)
        if (optInSignalPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Вызов универсального метода расчета MACD с фиксированными периодами:
        // optInFastPeriod = 0 означает фиксированный период 12 (коэффициент сглаживания 0.15)
        // optInSlowPeriod = 0 означает фиксированный период 26 (коэффициент сглаживания 0.075)
        return FunctionHelpers.CalcMACD(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outMACD, outMACDSignal,
            outMACDHist, out outRange,
            0, /* Фиксированный быстрый период = 12 */
            0, /* Фиксированный медленный период = 26 */
            optInSignalPeriod);
    }
}
