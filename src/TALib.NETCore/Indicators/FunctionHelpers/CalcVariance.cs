// CalcVariance.cs

namespace TALib;

internal static partial class FunctionHelpers
{

    /// <summary>
    /// Рассчитывает дисперсию временного ряда на основе скользящего окна.
    /// Формула: дисперсия = (среднее квадратов значений) - (среднее значение)^2.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (цены, индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения дисперсии.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период для расчета дисперсии (количество баров в окне)</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcVariance<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Расчет lookback-периода: минимальное количество баров для первого валидного значения
        var lookbackTotal = Functions.VarLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal); // Корректировка начального индекса

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success; // Недостаточно данных для расчета
        }

        T periodTotal1 = T.Zero; // Сумма значений в текущем окне
        T periodTotal2 = T.Zero; // Сумма квадратов значений в текущем окне
        var trailingIdx = startIdx - lookbackTotal; // Индекс первого элемента в текущем окне
        var i = trailingIdx; // Текущий индекс для итерации по входным данным

        // Инициализация сумм для первого окна (если период > 1)
        if (optInTimePeriod > 1)
        {
            while (i < startIdx)
            {
                var tempReal = inReal[i++];
                periodTotal1 += tempReal; // Сумма значений
                tempReal *= tempReal;
                periodTotal2 += tempReal; // Сумма квадратов значений
            }
        }

        var outIdx = 0; // Индекс для записи результатов в выходной массив
        var timePeriod = T.CreateChecked(optInTimePeriod); // Период в виде числа с плавающей точкой

        // Основной цикл расчета дисперсии
        do
        {
            var tempReal = inReal[i++]; // Текущее значение

            // Обновление сумм для нового значения
            periodTotal1 += tempReal; // Сумма значений
            tempReal *= tempReal;
            periodTotal2 += tempReal; // Сумма квадратов

            // Расчет средних значений
            var meanValue1 = periodTotal1 / timePeriod; // Среднее значение
            var meanValue2 = periodTotal2 / timePeriod; // Среднее квадратов

            // Удаление самого старого значения из окна
            tempReal = inReal[trailingIdx++];
            periodTotal1 -= tempReal; // Корректировка суммы значений
            tempReal *= tempReal;
            periodTotal2 -= tempReal; // Корректировка суммы квадратов

            // Формула дисперсии: Var = E[X²] - (E[X])²
            outReal[outIdx++] = meanValue2 - meanValue1 * meanValue1;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

}
