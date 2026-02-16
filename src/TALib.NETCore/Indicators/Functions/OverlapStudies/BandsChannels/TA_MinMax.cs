//Название файла: TA_MinMax.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - основная категория, результаты накладываются на график цены как канал экстремумов)
//StatisticFunctions (альтернатива ≥70%, так как выполняет статистические операции минимум/максимум)
//PriceTransform (альтернатива ≥60%, трансформирует цены в экстремальные значения периода)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MinMax (Overlap Studies) — Минимальные и максимальные значения за период (Исследования наложения)
    /// <para>
    /// Индикатор рассчитывает наименьшие (Lowest) и наибольшие (Highest) значения входного ряда 
    /// в скользящем окне заданной длины. Результаты отображаются поверх ценового графика, 
    /// формируя динамический канал экстремумов.
    /// </para>
    /// </summary>
    /// <param name="inReal">Входной временной ряд для анализа (обычно цены <see cref="Close"/>, <see cref="High"/>, <see cref="Low"/> или другие индикаторы)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMin">
    /// Выходной массив с минимальными значениями (Lowest) за период.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMin[i]</c> соответствует минимальному значению в окне, заканчивающемся на <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outMax">
    /// Выходной массив с максимальными значениями (Highest) за период.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMax[i]</c> соответствует максимальному значению в окне, заканчивающемся на <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMin"/> и <paramref name="outMax"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMin"/> и <paramref name="outMax"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени (количество баров в скользящем окне для поиска экстремумов). Минимальное значение: 2.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий результат операции:
    /// <see cref="Core.RetCode.Success"/> при успешном расчете или код ошибки в случае некорректных входных данных.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор MinMax определяет экстремальные значения (минимумы и максимумы) в скользящем окне данных. 
    /// Результаты часто используются для построения динамических каналов поддержки/сопротивления, 
    /// определения уровней перекупленности/перепроданности или как база для других индикаторов.
    /// </para>
    /// <para>
    /// <b>Связанные функции:</b>
    /// <list type="bullet">
    ///   <item><description>Используйте <see cref="Min{T}"/> для получения только минимальных значений (Lowest).</description></item>
    ///   <item><description>Используйте <see cref="Max{T}"/> для получения только максимальных значений (Highest).</description></item>
    ///   <item><description>Используйте <see cref="MinMaxIndex{T}"/> для получения индексов экстремумов вместо самих значений.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Алгоритм расчета:</b>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого бара <c>today</c> определяется окно поиска экстремумов: 
    ///       <c>[trailingIdx, today]</c>, где <c>trailingIdx = today - (optInTimePeriod - 1)</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       В этом окне находятся:
    ///       <code>
    /// Lowest = Min(inReal[i] for i from trailingIdx to today)  // Наименьшее значение в окне
    /// Highest = Max(inReal[i] for i from trailingIdx to today) // Наибольшее значение в окне
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Результаты сохраняются в <paramref name="outMin"/> и <paramref name="outMax"/> соответственно.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Особенности:</b>
    /// <list type="bullet">
    ///   <item><description>Lookback-период равен <c>optInTimePeriod - 1</c> — первое валидное значение появляется на этом баре.</description></item>
    ///   <item><description>Алгоритм оптимизирован для работы с перекрывающимися окнами (использует индексы предыдущих экстремумов).</description></item>
    ///   <item><description>Поддерживает работу с одним буфером для входных и выходных данных (in-place вычисления).</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MinMax<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMin,
        Span<T> outMax,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxImpl(inReal, inRange, outMin, outMax, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает количество баров, необходимых для расчета первого валидного значения индикатора <see cref="MinMax{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени (длина скользящего окна).</param>
    /// <returns>
    /// Lookback-период: <c>optInTimePeriod - 1</c>.
    /// Возвращает -1, если <paramref name="optInTimePeriod"/> меньше 2 (некорректный период).
    /// </returns>
    /// <remarks>
    /// Lookback-период определяет смещение первого валидного значения относительно начала входных данных.
    /// Например, при <c>optInTimePeriod = 14</c> первое валидное значение будет доступно на 13-м баре (индексация с 0).
    /// </remarks>
    [PublicAPI]
    public static int MinMaxLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Внутренняя реализация для совместимости с массивами (устаревший API).
    /// Перенаправляет вызов в основную реализацию через <see cref="MinMaxImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MinMax<T>(
        T[] inReal,
        Range inRange,
        T[] outMin,
        T[] outMax,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxImpl<T>(inReal, inRange, outMin, outMax, out outRange, optInTimePeriod);

    private static Core.RetCode MinMaxImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMin,
        Span<T> outMax,
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

        // Проверка минимально допустимого периода (требуется минимум 2 бара для поиска экстремумов)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет lookback-периода: количество баров, необходимых до первого валидного значения
        var lookbackTotal = MinMaxLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом lookback-периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки нет данных для обработки — выход с успешным статусом (пустой результат)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация счетчиков для обхода данных
        var outIdx = 0;                      // Индекс в выходных массивах outMin/outMax
        var today = startIdx;                // Текущий бар для обработки
        var trailingIdx = startIdx - lookbackTotal; // Начало скользящего окна поиска экстремумов

        // Кэширование индексов и значений предыдущих экстремумов для оптимизации
        int highestIdx = -1, lowestIdx = -1; // Индексы предыдущих максимума и минимума в окне
        T highest = T.Zero, lowest = T.Zero; // Значения предыдущих максимума и минимума

        // Основной цикл обработки данных в заданном диапазоне
        while (today <= endIdx)
        {
            // Обновление наибольшего значения (Highest) в текущем окне [trailingIdx, today]
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);
            // Обновление наименьшего значения (Lowest) в текущем окне [trailingIdx, today]
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

            // Сохранение результатов в выходные массивы
            outMax[outIdx] = highest;        // Максимальное значение периода → outMax
            outMin[outIdx++] = lowest;       // Минимальное значение периода → outMin
            // Сдвиг окна: переход к следующему бару
            trailingIdx++;
            today++;
        }

        // Формирование выходного диапазона: индексы в исходных данных, для которых рассчитаны значения
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
