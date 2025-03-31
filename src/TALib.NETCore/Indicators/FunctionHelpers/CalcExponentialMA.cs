// CalcExponentialMA.cs
namespace TALib;
internal static partial class FunctionHelpers
{

    /// <summary>
    /// Рассчитывает экспоненциальную скользящую среднюю (EMA) с поддержкой различных алгоритмов инициализации.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (цены или другие временные ряды)</param>
    /// <param name="inRange">Диапазон входных данных для обработки</param>
    /// <param name="outReal">Массив для сохранения результатов EMA</param>
    /// <param name="outRange">Диапазон указывающий для каких ячеек входных данных для расчёта посчитаны валидные значения индикаторов (индексы первой и последней ячейки во входных данных)</param>
    /// <param name="optInTimePeriod">Период EMA (количество баров для расчета)</param>
    /// <param name="optInK1">Коэффициент сглаживания (обычно 2/(optInTimePeriod + 1))</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcExponentialMA<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod,
        T optInK1) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Расчет lookback-периода: минимальное количество баров для первого валидного значения
        var lookbackTotal = Functions.EmaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx; // Индекс первого валидного значения в выходных данных

        /* Выполнение расчета EMA с использованием оптимизированных циклов.
         * 
         * Первое значение EMA рассчитывается особым образом и становится начальным значением (seed) для последующих расчетов.
         * 
         * Реализованы 3 варианта алгоритма инициализации:
         * 
         * Classic (Классический):
         *   Используется простое скользящее среднее (SMA) для первых 'optInTimePeriod' баров.
         *   Наиболее распространенный метод, описанный в литературе.
         *   
         * Metastock:
         *   В качестве начального значения используется значение первого бара из доступных данных.
         */

        int today; // Текущий индекс обрабатываемого бара
        T prevMA; // Предыдущее значение EMA (используется для расчета следующих значений)

        if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Default)
        {
            // Классический метод: SMA для первых 'optInTimePeriod' баров
            today = startIdx - lookbackTotal;
            var i = optInTimePeriod; // Счетчик для суммирования первых значений
            var tempReal = T.Zero; // Временная переменная для накопления суммы

            while (i-- > 0)
            {
                tempReal += inReal[today++]; // Суммирование первых 'optInTimePeriod' значений
            }

            prevMA = tempReal / T.CreateChecked(optInTimePeriod); // SMA = Σ(close) / period
        }
        else
        {
            // Metastock-метод: начальное значение = первое доступное значение
            prevMA = inReal[0]; // Начальное значение EMA = первое значение входных данных
            today = 1; // Начинаем обработку со второго бара
        }

        // На этом этапе prevMA содержит начальное значение EMA (seed)
        // 'today' отслеживает текущую позицию в исходных данных

        // Пропуск нестабильного периода (расчет без сохранения результатов)
        while (today <= startIdx)
        {
            // Формула EMA: EMA(t) = EMA(t-1) + k1*(close(t) - EMA(t-1))
            prevMA = (inReal[today++] - prevMA) * optInK1 + prevMA;
        }

        // Сохранение первого валидного значения
        outReal[0] = prevMA;
        var outIdx = 1; // Индекс для записи результатов в выходной массив

        // Расчет оставшихся значений
        while (today <= endIdx)
        {
            prevMA = (inReal[today++] - prevMA) * optInK1 + prevMA;
            outReal[outIdx++] = prevMA; // Запись очередного значения EMA
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

}
