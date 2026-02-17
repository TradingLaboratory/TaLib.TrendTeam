//Название файла: TA_LinearReg.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//TrendIndicators (альтернатива, если требуется группировка по типу индикатора)
//RegressionAnalysis (альтернатива для акцента на методе анализа)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// LinearReg (StatisticFunctions) — Линейная регрессия (Статистические функции)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены Close, другие индикаторы или временные ряды).
    /// <para>Обычно используются цены закрытия (Close) для анализа тренда.</para>
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>- Если не указан, обрабатывается весь массив <paramref name="inReal"/>.</para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>- <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.</para>
    /// <para>- <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.</para>
    /// <para>- Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.</para>
    /// <para>- Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.</para>
    /// </param>
    /// <param name="optInTimePeriod">Период расчета линейной регрессии (количество баров для анализа, по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных (например, float или double),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>:
    /// <para><see cref="Core.RetCode.Success"/> при успешном расчете,</para>
    /// <para>иначе код ошибки.</para>
    /// </returns>
    /// <remarks>
    /// Функция строит линию тренда методом наименьших квадратов (Least Squares) для заданного периода.
    /// <para>
    /// Результат показывает прогнозируемое значение на последнем баре периода,
    /// что помогает определить направление тренда и потенциальные точки разворота.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление сумм:
    ///       <para>- X (индексы элементов временного ряда),</para>
    ///       <para>- X² (квадраты индексов),</para>
    ///       <para>- X*Y (произведение индексов и значений входных данных).</para>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет наклона (m) по формуле:
    ///       <code>m = (nΣXY - ΣXΣY) / (nΣX² - (ΣX)²)</code>
    ///       <para>где n — количество периодов, Σ — сумма.</para>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет смещения/пересечения с осью Y (b) по формуле:
    ///       <code>b = (ΣY - mΣX) / n</code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Прогноз на последнем баре периода:
    ///       <code>y = b + m*(n-1)</code>
    ///       <para>где (n-1) — индекс последнего бара в периоде.</para>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация результатов</b>:
    /// <list type="bullet">
    ///   <item><description>Рост значений → восходящий тренд (бычий)</description></item>
    ///   <item><description>Падение значений → нисходящий тренд (медвежий)</description></item>
    ///   <item><description>Горизонтальная линия → отсутствие тренда (флэт/боковик)</description></item>
    /// </list>
    ///
    /// <b>Применение в трейдинге</b>:
    /// <para>
    /// Индикатор используется для сглаживания ценовых данных и выявления основного направления тренда.
    /// Отклонение цены от линии регрессии может сигнализировать о перекупленности или перепроданности.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode LinearReg<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период lookback для функции <see cref="LinearReg{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета (количество баров для анализа).</param>
    /// <returns>
    /// Количество периодов, необходимых для первого валидного расчета индикатора.
    /// <para>Все бары с индексом меньше lookback будут пропущены при расчете.</para>
    /// <para>Возвращает -1 если период некорректен (меньше 2).</para>
    /// </returns>
    [PublicAPI]
    public static int LinearRegLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Метод для совместимости с абстрактным API библиотеки TALib.
    /// <para>Используется при работе с массивами вместо Span&lt;T&gt;.</para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode LinearReg<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        LinearRegImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode LinearRegImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального периода (требуется не менее 2 баров для регрессии)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периода lookback (количество баров для первых вычислений)
        // lookback определяет индекс первого бара, для которого можно получить валидное значение
        var lookbackTotal = LinearRegLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если данных недостаточно для расчета (startIdx больше endIdx)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;       // Индекс для записи результатов в выходной массив
        var today = startIdx; // Текущий индекс бара во входных данных

        // Преобразование периода из int в числовой тип T для расчетов
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Сумма X = 0 + 1 + ... + (n-1) = n(n-1)/2
        // Это сумма индексов временного ряда в пределах периода
        var sumX = timePeriod * (timePeriod - T.One) * T.CreateChecked(0.5);

        // Сумма X² = (n-1)n(2n-1)/6
        // Это сумма квадратов индексов, используется в формуле наименьших квадратов
        var sumXSqr = timePeriod * (timePeriod - T.One) *
                     (timePeriod * FunctionHelpers.Two<T>() - T.One) / T.CreateChecked(6);

        // Знаменатель для формулы расчета наклона линии регрессии
        // divisor = nΣX² - (ΣX)²
        var divisor = sumX * sumX - timePeriod * sumXSqr;

        // Основной цикл расчета для каждого бара во входных данных
        while (today <= endIdx)
        {
            var sumXY = T.Zero; // Сумма произведений X*Y (индекс * значение)
            var sumY = T.Zero;  // Сумма Y (значений входных данных)

            // Накопление сумм для текущего периода (обратный цикл от optInTimePeriod до 0)
            for (var i = optInTimePeriod; i-- != 0;)
            {
                var tempValue = inReal[today - i]; // Значение цены на баре (today - i)
                sumY += tempValue;                 // Накопление суммы значений
                sumXY += T.CreateChecked(i) * tempValue; // Накопление суммы произведений индекс*значение
            }

            // Расчет наклона линии регрессии (m)
            // Формула: m = (nΣXY - ΣXΣY) / (nΣX² - (ΣX)²)
            var m = (timePeriod * sumXY - sumX * sumY) / divisor;

            // Расчет точки пересечения с осью Y (b)
            // Формула: b = (ΣY - mΣX) / n
            var b = (sumY - m * sumX) / timePeriod;

            // Прогнозное значение на последнем баре периода
            // Формула: y = b + m*(n-1), где (n-1) — индекс последнего бара
            outReal[outIdx++] = b + m * (timePeriod - T.One);

            today++; // Переход к следующему бару
        }

        // Установка выходного диапазона
        // outRange.Start — индекс первого бара с валидным значением
        // outRange.End — индекс последнего бара с валидным значением
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
