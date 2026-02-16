//Название файла: TA_Macd.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - основная категория по классификации TALib)
//TrendDirection (альтернатива для акцента на определении направления тренда)
//ConvergenceDivergence (рекомендуемая подпапка для группировки индикаторов на основе сходимости/расходимости)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MACD (Moving Average Convergence/Divergence) — Сходимость/расходимость скользящих средних (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены закрытия <c>Close</c>)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMACD">Массив, содержащий ТОЛЬКО валидные значения линии MACD (разность быстрой и медленной EMA).</param>
    /// <param name="outMACDSignal">Массив, содержащий ТОЛЬКО валидные значения сигнальной линии (EMA линии MACD).</param>
    /// <param name="outMACDHist">Массив, содержащий ТОЛЬКО валидные значения гистограммы MACD (разность линии MACD и сигнальной линии).</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastPeriod">Период для расчета быстрой экспоненциальной скользящей средней (EMA). Стандартное значение: 12.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной экспоненциальной скользящей средней (EMA). Стандартное значение: 26.</param>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии (EMA линии MACD). Стандартное значение: 9.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Сходимость/расходимость скользящих средних (MACD) — это трендовый импульсный индикатор, показывающий отношение
    /// между двумя экспоненциальными скользящими средними (EMA) цены инструмента. Индикатор выявляет изменения импульса,
    /// силу и направление тренда через анализ расхождений между быстрой и медленной EMA.
    /// </para>
    ///
    /// <para>
    /// Функция широко применяется в техническом анализе и часто используется совместно с <see cref="Rsi{T}">RSI</see>
    /// или <see cref="Bbands{T}">Bollinger Bands</see> для подтверждения торговых сигналов. Анализ дивергенций между
    /// ценой и линией MACD, а также пересечений линий помогает идентифицировать потенциальные развороты тренда.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать быструю экспоненциальную скользящую среднюю (EMA) входных значений за период <c>optInFastPeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать медленную EMA входных значений за период <c>optInSlowPeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить линию MACD как разность между быстрой и медленной EMA:
    ///       <code>
    ///         MACD = EMA(optInFastPeriod) - EMA(optInSlowPeriod)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать сигнальную линию (Signal Line) как EMA линии MACD за период <c>optInSignalPeriod</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить гистограмму MACD как разность между линией MACD и сигнальной линией:
    ///       <code>
    ///         MACDHist = MACD - Signal
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация сигналов</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>Пересечение линий</b>: бычий сигнал возникает при пересечении линии MACD сигнальной линии снизу вверх;
    ///       медвежий сигнал — при пересечении сверху вниз.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Гистограмма</b>: расширение столбцов гистограммы указывает на укрепление текущего импульса,
    ///       сужение — на ослабление импульса и возможный разворот.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Дивергенции</b>: бычья дивергенция (цена формирует новые минимумы, а MACD — более высокие минимумы)
    ///       сигнализирует о потенциальном развороте вверх; медвежья дивергенция — о развороте вниз.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Центральная линия</b>: пересечение линии MACD нулевого уровня (центральной линии) снизу вверх
    ///       подтверждает восходящий тренд; пересечение сверху вниз — нисходящий тренд.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Macd<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod, optInSignalPeriod);

    /// <summary>
    /// Возвращает период предыстории (lookback period) для индикатора <see cref="Macd{T}"/>.
    /// </summary>
    /// <param name="optInFastPeriod">Период быстрой EMA (по умолчанию 12).</param>
    /// <param name="optInSlowPeriod">Период медленной EMA (по умолчанию 26).</param>
    /// <param name="optInSignalPeriod">Период сигнальной линии (по умолчанию 9).</param>
    /// <returns>
    /// Минимальное количество баров, необходимых во входных данных для расчета первого валидного значения индикатора.
    /// Возвращает -1 при недопустимых параметрах (периоды меньше минимально допустимых значений).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Период предыстории рассчитывается как сумма lookback периодов двух последовательных EMA:
    /// </para>
    /// <para>
    /// <c>Lookback = EmaLookback(optInSlowPeriod) + EmaLookback(optInSignalPeriod)</c>
    /// </para>
    /// <para>
    /// где <c>optInSlowPeriod</c> — период медленной EMA (основной расчет MACD),
    /// а <c>optInSignalPeriod</c> — период сигнальной линии (вторая EMA от линии MACD).
    /// </para>
    /// <para>
    /// Примечание: быстрая EMA не учитывается отдельно, так как её период всегда меньше или равен периоду медленной EMA.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int MacdLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
    {
        // Проверка минимально допустимых значений периодов
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return -1;
        }

        // Коррекция: медленный период не может быть меньше быстрого
        if (optInSlowPeriod < optInFastPeriod)
        {
            optInSlowPeriod = optInFastPeriod;
        }

        // Расчет общего периода предыстории как суммы lookback периодов двух EMA
        return EmaLookback(optInSlowPeriod) + EmaLookback(optInSignalPeriod);
    }

    /// <remarks>
    /// Приватная реализация для совместимости с абстрактным API (массивы вместо Span).
    /// Перенаправляет вызов к основной реализации через преобразование массивов в Span.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Macd<T>(
        T[] inReal,
        Range inRange,
        T[] outMACD,
        T[] outMACDSignal,
        T[] outMACDHist,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        int optInSignalPeriod = 9) where T : IFloatingPointIeee754<T> =>
        MacdImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod,
            optInSignalPeriod);

    /// <summary>
    /// Внутренняя реализация расчета индикатора MACD.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Метод выполняет валидацию входных параметров и делегирует основной расчет
    /// вспомогательному методу <see cref="FunctionHelpers.CalcMACD"/>.
    /// </para>
    /// <para>
    /// Этапы выполнения:
    /// <list type="number">
    ///   <item>Инициализация выходного диапазона значением [0, 0)</item>
    ///   <item>Валидация входного диапазона <paramref name="inRange"/></item>
    ///   <item>Проверка корректности входных периодов</item>
    ///   <item>Вызов основного расчетного метода <see cref="FunctionHelpers.CalcMACD"/></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static Core.RetCode MacdImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        int optInSignalPeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым интервалом [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и получение индексов начала/конца обработки
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка минимально допустимых значений периодов
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Делегирование основного расчета вспомогательному методу
        return FunctionHelpers.CalcMACD(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outMACD, outMACDSignal,
            outMACDHist, out outRange, optInFastPeriod, optInSlowPeriod, optInSignalPeriod);
    }
}
