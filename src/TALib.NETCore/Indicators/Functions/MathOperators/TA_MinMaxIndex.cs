//MinMaxIndex.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории: поиск экстремумов через индексы)
//StatisticFunctions (альтернатива ≥70%: статистический анализ экстремальных значений)
//PriceTransform (альтернатива ≥50%: преобразование ценовых данных в индексы экстремумов)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MinMaxIndex (Math Operators) — Индексы минимального и максимального значений за период (Математические операторы)
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта (цены, индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMinIdx">
    /// Массив для хранения индексов минимальных значений (Low) в скользящем окне.
    /// - Каждый элемент <c>outMinIdx[i]</c> содержит индекс минимального значения в диапазоне <c>[trailingIdx, today]</c>.
    /// </param>
    /// <param name="outMaxIdx">
    /// Массив для хранения индексов максимальных значений (High) в скользящем окне.
    /// - Каждый элемент <c>outMaxIdx[i]</c> содержит индекс максимального значения в диапазоне <c>[trailingIdx, today]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в выходных массивах.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в выходных массивах.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> при успешном расчёте.
    /// - Если данных недостаточно (длина <paramref name="inReal"/> меньше периода), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период расчёта (количество баров в скользящем окне). Минимальное значение: 2.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчёта:
    /// - <see cref="Core.RetCode.Success"/> — расчёт успешно выполнен.
    /// - <see cref="Core.RetCode.OutOfRangeParam"/> — недопустимый диапазон входных данных.
    /// - <see cref="Core.RetCode.BadParam"/> — некорректный период (меньше 2).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция MinMaxIndex вычисляет индексы минимальных (Low) и максимальных (High) значений
    /// во входном массиве <paramref name="inReal"/> в пределах скользящего окна заданной длины.
    /// Используется для локализации экстремумов (пиков и впадин) в ценовых данных или индикаторах.
    /// </para>
    /// <para>
    /// Рекомендации по выбору функций:
    /// - Используйте <see cref="Min{T}"/> или <see cref="Max{T}"/>, если требуется только само значение экстремума.
    /// - Используйте <see cref="MinMax{T}"/>, если нужны фактические минимальные и максимальные значения (а не их индексы).
    /// - Используйте <see cref="MinMaxIndex{T}"/>, если необходимо знать позицию экстремума относительно текущего бара.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение границ скользящего окна для текущего бара <c>today</c>:
    ///       <code>
    ///         trailingIdx = today - (optInTimePeriod - 1)
    ///         Окно расчёта = [trailingIdx, today]
    ///       </code>
    ///       где <c>trailingIdx</c> — начальный индекс окна, <c>today</c> — текущий индекс.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Поиск индексов экстремумов в окне:
    ///       <code>
    ///         LowestIndex  = IndexOfMin(inReal[i] для i ∈ [trailingIdx, today])
    ///         HighestIndex = IndexOfMax(inReal[i] для i ∈ [trailingIdx, today])
    ///       </code>
    ///       Индексы возвращаются относительно начала массива <paramref name="inReal"/>.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация результатов</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Массив <paramref name="outMinIdx"/> содержит индексы минимальных значений (Low) для каждого скользящего окна.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Массив <paramref name="outMaxIdx"/> содержит индексы максимальных значений (High) для каждого скользящего окна.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Индексы экстремумов позволяют определить:
    ///       - Позицию локальных минимумов/максимумов относительно текущего бара.
    ///       - Возраст экстремума (сколько баров прошло с момента его формирования).
    ///       - Потенциальные точки разворота или уровни поддержки/сопротивления.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MinMaxIndex<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMinIdx,
        Span<T> outMaxIdx,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxIndexImpl(inReal, inRange, outMinIdx, outMaxIdx, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback) для функции <see cref="MinMaxIndex{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчёта (количество баров в скользящем окне).</param>
    /// <returns>
    /// Количество баров, необходимых до первого валидного значения индикатора.
    /// Для MinMaxIndex: <c>optInTimePeriod - 1</c>.
    /// Возвращает -1 при некорректном периоде (меньше 2).
    /// </returns>
    [PublicAPI]
    public static int MinMaxIndexLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MinMaxIndex<T>(
        T[] inReal,
        Range inRange,
        T[] outMinIdx,
        T[] outMaxIdx,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MinMaxIndexImpl<T>(inReal, inRange, outMinIdx, outMaxIdx, out outRange, optInTimePeriod);

    private static Core.RetCode MinMaxIndexImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMinIdx,
        Span<T> outMaxIdx,
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

        // Проверка минимально допустимого периода (требуется как минимум 2 бара для поиска экстремумов)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчёт периода задержки: первое валидное значение появится на баре (optInTimePeriod - 1)
        var lookbackTotal = MinMaxIndexLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учёта lookback не осталось данных для расчёта — выход с успехом (пустой результат)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация индекса записи в выходные массивы
        var outIdx = 0;
        // Текущий бар для расчёта (начинаем с первого валидного бара после lookback)
        var today = startIdx;
        // Начальный бар скользящего окна (левая граница окна)
        var trailingIdx = startIdx - lookbackTotal;

        // Кэширование индексов и значений текущих экстремумов для оптимизации расчётов
        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;

        // Основной цикл расчёта: перебор всех баров от startIdx до endIdx
        while (today <= endIdx)
        {
            // Обновление индекса и значения максимального элемента в окне [trailingIdx, today]
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);
            // Обновление индекса и значения минимального элемента в окне [trailingIdx, today]
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inReal, trailingIdx, today, lowestIdx, lowest);

            // Запись индекса максимального значения в выходной массив (преобразование в числовой тип T)
            outMaxIdx[outIdx] = T.CreateChecked(highestIdx);
            // Запись индекса минимального значения в выходной массив (преобразование в числовой тип T)
            outMinIdx[outIdx++] = T.CreateChecked(lowestIdx);
            // Сдвиг окна вправо на один бар
            trailingIdx++;
            today++;
        }

        // Формирование выходного диапазона: индексы входных данных, для которых получены валидные значения
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
