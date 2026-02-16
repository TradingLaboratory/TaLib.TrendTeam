//Название файла: TA_MidPrice.cs
//Рекомендуемое размещение: OverlapStudies/PriceLevels
//Группы к которым можно отнести индикатор:
//OverlapStudies (100% - основная категория: индикатор строится поверх ценового графика)
//PriceTransform (70% - альтернатива: преобразует цены в центральный уровень диапазона)
//TrendDirection (55% - альтернатива: может использоваться для определения направления тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Midpoint Price over period (Overlap Studies) — Средняя цена за период (Индикаторы перекрытия)
    /// </summary>
    /// <param name="inHigh">Входные данные: максимальные цены (High) за каждый период.</param>
    /// <param name="inLow">Входные данные: минимальные цены (Low) за каждый период.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c> и <c>inLow[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> или <paramref name="inLow"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени (количество баров для расчёта экстремумов). Минимальное значение: 2.</param>
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
    /// Индикатор MidPrice вычисляет среднюю цену как полусумму максимальной (Highest High) и минимальной (Lowest Low) цены за указанный период:
    /// <code>
    /// MidPrice = (Highest High + Lowest Low) / 2
    /// </code>
    /// </para>
    /// <para>
    /// Индикатор отражает центральную точку ценового диапазона и часто используется для:
    /// <list type="bullet">
    ///   <item><description>Определения уровня поддержки/сопротивления в середине диапазона</description></item>
    ///   <item><description>Идентификации трендовых движений относительно центрального уровня</description></item>
    ///   <item><description>Фильтрации рыночного шума при анализе ценовых уровней</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// При использовании с периодом 1 индикатор эквивалентен <see cref="MedPrice{T}"/> (медианной цене текущего бара).
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MidPrice<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MidPriceImpl(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для индикатора <see cref="MidPrice{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчёта индикатора.</param>
    /// <returns>
    /// Количество баров, необходимых до первого валидного значения индикатора.
    /// Рассчитывается как <c>optInTimePeriod - 1</c>. При некорректном периоде возвращает -1.
    /// </returns>
    /// <remarks>
    /// Lookback период определяет минимальное количество исторических данных,
    /// необходимых для расчёта первого валидного значения индикатора.
    /// Для MidPrice требуется <c>optInTimePeriod</c> баров для определения экстремумов.
    /// </remarks>
    [PublicAPI]
    public static int MidPriceLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Вспомогательный метод для совместимости с массивами (устаревший интерфейс).
    /// Перенаправляет вызов в основную реализацию через Span-интерфейс.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MidPrice<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MidPriceImpl<T>(inHigh, inLow, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MidPriceImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и длины массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода: минимальное значение = 2 (для расчёта экстремумов)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Формула расчёта индикатора MidPrice:
         * MidPrice = (Highest High + Lowest Low) / 2
         * где:
         *   Highest High — максимальная цена (High) в окне периода
         *   Lowest Low   — минимальная цена (Low) в окне периода
         * 
         * Примечание: при периоде = 1 эквивалентен медианной цене (MedPrice)
         */

        // Расчёт минимального количества баров для первого валидного значения
        var lookbackTotal = MidPriceLookback(optInTimePeriod);
        // Корректировка начального индекса с учётом периода обратного просмотра
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Проверка достаточности данных для расчёта
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;           // Индекс для записи результатов в выходной массив
        var today = startIdx;     // Текущий индекс обработки (правая граница окна)
        var trailingIdx = startIdx - lookbackTotal; // Левая граница скользящего окна

        // Основной цикл расчёта индикатора
        while (today <= endIdx)
        {
            // Инициализация экстремумов значениями первого бара в окне
            var lowest = inLow[trailingIdx];   // Минимальная цена (Low) в окне периода
            var highest = inHigh[trailingIdx++]; // Максимальная цена (High) в окне периода

            // Поиск экстремумов в текущем окне периода
            for (var i = trailingIdx; i <= today; i++)
            {
                // Поиск минимальной цены (Lowest Low) в окне
                var tmp = inLow[i];
                if (tmp < lowest)
                {
                    lowest = tmp;
                }

                // Поиск максимальной цены (Highest High) в окне
                tmp = inHigh[i];
                if (tmp > highest)
                {
                    highest = tmp;
                }
            }

            // Расчёт средней цены за период: полусумма экстремумов
            outReal[outIdx++] = (highest + lowest) / FunctionHelpers.Two<T>();
            today++;
        }

        // Установка диапазона валидных значений в выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
