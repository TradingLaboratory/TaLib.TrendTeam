namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Линейная регрессия (статистические функции)
    /// </summary>
    /// <param name="inReal">Массив входных значений для анализа.</param>
    /// <param name="inRange">Диапазон индексов, определяющий участок данных для расчета.</param>
    /// <param name="outReal">Массив для сохранения результатов расчета.</param>
    /// <param name="outRange">Диапазон индексов с валидными данными в выходном массиве.</param>
    /// <param name="optInTimePeriod">Период расчета (по умолчанию 14).</param>
    /// <typeparam name="T">
    /// Числовой тип данных (например, float или double),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>:
    /// <see cref="Core.RetCode.Success"/> при успешном расчете,
    /// иначе код ошибки.
    /// </returns>
    /// <remarks>
    /// Функция строит линию тренда методом наименьших квадратов для заданного периода.
    /// <para>
    /// Результат показывает прогнозируемое значение на последнем баре периода,
    /// что помогает определить направление тренда.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление сумм: 
    ///       - X (индексы элементов), 
    ///       - X² (квадраты индексов), 
    ///       - X*Y (произведение индексов и значений).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет наклона (m) по формуле:
    ///       <code>m = (nΣXY - ΣXΣY) / (nΣX² - (ΣX)²)</code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет смещения (b) по формуле:
    ///       <code>b = (ΣY - mΣX) / n</code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Прогноз на последнем баре периода: 
    ///       <code>y = b + m*(n-1)</code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация</b>:
    /// <list type="bullet">
    ///   <item><description>Рост значений → восходящий тренд</description></item>
    ///   <item><description>Падение значений → нисходящий тренд</description></item>
    ///   <item><description>Горизонтальная линия → отсутствие тренда</description></item>
    /// </list>
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
    /// <param name="optInTimePeriod">Период расчета.</param>
    /// <returns>Количество периодов, необходимых для первого расчета.</returns>
    [PublicAPI]
    public static int LinearRegLookback(int optInTimePeriod = 14) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
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
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального периода (не менее 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периода lookback (количество баров для первых вычислений)
        var lookbackTotal = LinearRegLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;
        var today = startIdx;

        // Преобразование периода в числовой тип T
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Сумма X = 0 + 1 + ... + (n-1) = n(n-1)/2
        var sumX = timePeriod * (timePeriod - T.One) * T.CreateChecked(0.5);

        // Сумма X² = (n-1)n(2n-1)/6
        var sumXSqr = timePeriod * (timePeriod - T.One) *
                     (timePeriod * FunctionHelpers.Two<T>() - T.One) / T.CreateChecked(6);

        // Знаменатель для формулы наклона
        var divisor = sumX * sumX - timePeriod * sumXSqr;

        // Основной цикл расчета для каждого бара
        while (today <= endIdx)
        {
            var sumXY = T.Zero; // Сумма произведений X*Y
            var sumY = T.Zero;  // Сумма Y

            // Накопление сумм для текущего периода
            for (var i = optInTimePeriod; i-- != 0;)
            {
                var tempValue = inReal[today - i];
                sumY += tempValue;
                sumXY += T.CreateChecked(i) * tempValue;
            }

            // Расчет наклона линии регрессии
            var m = (timePeriod * sumXY - sumX * sumY) / divisor;

            // Расчет точки пересечения с осью Y
            var b = (sumY - m * sumX) / timePeriod;

            // Прогнозное значение на последнем баре периода
            outReal[outIdx++] = b + m * (timePeriod - T.One);

            today++;
        }

        // Установка выходного диапазона
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
