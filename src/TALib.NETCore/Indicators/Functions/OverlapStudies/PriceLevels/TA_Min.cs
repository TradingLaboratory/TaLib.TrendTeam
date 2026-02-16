//Название файла: TA_Min.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - основная категория согласно оригинальной классификации TA-Lib)
//OverlapStudies/PriceLevels (альтернатива, если требуется группировка по перекрывающим график цен уровням)
//PriceTransform (альтернатива для акцента на преобразовании ценовых данных)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MIN (Lowest) — Минимальное значение за период
    /// <para>Вычисляет Lowest (минимальное значение) в скользящем окне за указанный период.</para>
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены Low, но могут быть и другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
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
    /// <param name="optInTimePeriod">Период времени для поиска минимального значения (по умолчанию 30).</param>
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
    /// Функция MIN вычисляет минимальное значение (Lowest) в скользящем окне данных за указанный период.
    /// Результат представляет собой ряд значений, где каждое значение — это Lowest в окне размером <paramref name="optInTimePeriod"/>.
    /// </para>
    /// <para>
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определить начальный индекс окна (<c>trailingIdx</c>) и текущий индекс (<c>today</c>):
    ///       <code>
    ///         trailingIdx = startIdx - lookbackTotal
    ///         today = startIdx
    ///       </code>
    ///       где <c>lookbackTotal = optInTimePeriod - 1</c> — период обратного просмотра.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого бара найти минимальное значение в диапазоне [<c>trailingIdx</c>, <c>today</c>]:
    ///       <code>
    ///         Lowest = Min(inReal[i] for i from trailingIdx to today)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сдвинуть окно: увеличить <c>trailingIdx</c> и <c>today</c> на 1 для следующей итерации.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Интерпретация значения</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходное значение представляет минимальную цену (Lowest Low) в скользящем окне, определенном параметром <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Часто используется для определения уровней поддержки, построения каналов или как часть других индикаторов (например, Stochastic).
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Min<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для функции <see cref="Min{T}"/>.
    /// <para>Период обратного просмотра — количество баров, необходимых до появления первого валидного значения индикатора.</para>
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для поиска минимального значения.</param>
    /// <returns>Количество периодов, необходимых до вычисления первого выходного значения (равно <c>optInTimePeriod - 1</c>).</returns>
    [PublicAPI]
    public static int MinLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Min<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MinImpl<T>(
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

        // Проверка корректности периода (минимум 2 бара для поиска минимума)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периода обратного просмотра: первое валидное значение появится на баре (optInTimePeriod - 1)
        var lookbackTotal = MinLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учета lookback не осталось данных для обработки — выход с пустым результатом
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outIdx = 0;              // Индекс в выходном массиве outReal
        var today = startIdx;        // Текущий индекс обработки (правая граница окна)
        var trailingIdx = startIdx - lookbackTotal; // Левая граница скользящего окна

        var lowestIdx = -1;          // Индекс минимального значения в текущем окне
        var lowest = T.Zero;         // Значение минимальной цены (Lowest) в текущем окне

        // Основной цикл: скользящее окно от startIdx до endIdx
        while (today <= endIdx)
        {
            // Поиск минимального значения в диапазоне [trailingIdx, today]
            // Функция возвращает как индекс, так и значение минимума для оптимизации последующих итераций
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

            // Сохранение найденного минимального значения в выходной массив
            outReal[outIdx++] = lowest;

            // Сдвиг окна вправо на один бар
            trailingIdx++;
            today++;
        }

        // Установка диапазона валидных значений в выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
