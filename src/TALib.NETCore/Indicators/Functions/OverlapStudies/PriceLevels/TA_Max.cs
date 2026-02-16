// Название файла: TA_Max.cs
// Рекомендуемое размещение:
// Основная категория:OverlapStudies/PriceLevels
// MathOperators (идеальное соответствие - поиск экстремумов в скользящем окне)
// Альтернативные категории: StatisticFunctions, OverlapStudies (при использовании в составе ценовых каналов)
// Предлагаемые подпапки: ExtremaFunctions, RangeFunctions, WindowFunctions

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Highest (Max) (Math Operators) — Highest / Максимум (Математические операторы)
    /// <para>
    /// Функция нахождения максимального значения (Highest) в скользящем окне за указанный период.
    /// </para>
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта (цены Close/High/Low/Open, объёмы Volume или значения других индикаторов)</param>
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
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчёт успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени (количество баров в скользящем окне для поиска максимума)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий результат вычисления:
    /// <see cref="Core.RetCode.Success"/> при успехе или соответствующий код ошибки.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Функция вычисляет Highest High (максимальное значение) в скользящем окне заданной длины.
    /// Результат представляет собой серию максимальных значений, сдвинутых на <c>optInTimePeriod - 1</c> баров относительно входных данных.
    /// </para>
    /// <para>
    /// Применение в техническом анализе:
    /// - Определение уровней сопротивления (resistance levels)
    /// - Построение ценовых каналов (Price Channels, Donchian Channels)
    /// - Выявление локальных максимумов (peaks) в ценовом ряду
    /// - Базовый компонент для расчёта волатильности (ATR, True Range)
    /// </para>
    ///
    /// <b>Алгоритм расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Инициализация: поиск максимального значения (highest) и его индекса (highestIdx) в первом окне длиной <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Скользящее окно: при сдвиге окна на один бар вправо:
    ///       <list type="bullet">
    ///         <item>Если предыдущий максимум выходит за левую границу окна — пересчитать максимум для нового окна</item>
    ///         <item>Если новый бар выше текущего максимума — обновить значение максимума и его индекс</item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Запись результата: текущее значение highest сохраняется в выходной массив для каждого бара, начиная с индекса <c>lookback = optInTimePeriod - 1</c>.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Особенности</b>:
    /// <list type="bullet">
    ///   <item>Lookback-период равен <c>optInTimePeriod - 1</c> — первое валидное значение появляется на этом баре</item>
    ///   <item>Поддерживает работу «на месте» (in-place): входной и выходной массивы могут быть одним буфером</item>
    ///   <item>Минимально допустимый период: 2 бара</item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Max<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MaxImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает lookback-период для функции <see cref="Max{T}"/>.
    /// <para>
    /// Lookback — количество баров, необходимых до появления первого валидного значения индикатора.
    /// Для функции Highest lookback равен <c>optInTimePeriod - 1</c>.
    /// </para>
    /// </summary>
    /// <param name="optInTimePeriod">Период времени (длина скользящего окна)</param>
    /// <returns>
    /// Количество баров задержки:
    /// - При <c>optInTimePeriod &gt;= 2</c>: возвращает <c>optInTimePeriod - 1</c>
    /// - При некорректном периоде (<c>&lt; 2</c>): возвращает -1 (ошибка)
    /// </returns>
    [PublicAPI]
    public static int MaxLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Вспомогательная перегрузка для совместимости с устаревшим API (массивы вместо Span).
    /// Перенаправляет вызов в основную реализацию <see cref="MaxImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Max<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MaxImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MaxImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности start/end индексов
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимально допустимого периода (требуется как минимум 2 бара для сравнения)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчёт lookback-периода: первый валидный результат появится на баре (optInTimePeriod - 1)
        var lookbackTotal = MaxLookback(optInTimePeriod);
        // Корректировка начального индекса с учётом lookback (пропуск первых баров без валидных значений)
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки диапазон пуст — расчёт завершён успешно (нет данных для обработки)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжить вычисление для запрашиваемого диапазона.
        // Алгоритм позволяет входным и выходным данным быть одним и тем же буфером.
        var outIdx = 0;                 // Индекс записи в выходной массив
        var today = startIdx;           // Текущий обрабатываемый бар
        var trailingIdx = startIdx - lookbackTotal; // Левая граница скользящего окна

        var highestIdx = -1;            // Индекс текущего максимума внутри окна
        var highest = T.Zero;           // Значение текущего максимума (Highest High)
        while (today <= endIdx)
        {
            // Расчёт/обновление максимального значения в окне [trailingIdx, today]
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);

            // Запись текущего максимума в выходной массив
            outReal[outIdx++] = highest;
            // Сдвиг окна на один бар вправо
            trailingIdx++;
            today++;
        }

        // Формирование выходного диапазона: индексы баров с валидными значениями в исходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
