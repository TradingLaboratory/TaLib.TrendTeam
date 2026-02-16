//Название файла: TA_MaxIndex.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по статистическим функциям)
//MomentumIndicators (альтернатива для акцента на поиске экстремумов тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Index of highest value over a specified period (Math Operators) — Индекс наибольшего значения за указанный период (Математические операторы)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outInteger">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outInteger[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outInteger"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция индекса наибольшего значения определяет индекс наибольшего значения за указанный период.
    /// Эта функция полезна для точного определения позиции пиков в временном ряду внутри скользящего окна.
    /// <para>
    /// Функция <see cref="Max{T}">Max</see> может быть использована для получения самого наибольшего значения, а не его индекса.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Инициализация наибольшего значения и его индекса для первого периода в указанном диапазоне.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого последующего периода обновление наибольшего значения и его индекса путем сравнения нового значения с текущим наибольшим.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходной результат представляет собой индекс наибольшего значения внутри скользящего окна, определяемого параметром <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MaxIndex<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<int> outInteger,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MaxIndexImpl(inReal, inRange, outInteger, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="MaxIndex{T}">MaxIndex</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до вычисления первого выходного значения.</returns>
    [PublicAPI]
    public static int MaxIndexLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MaxIndex<T>(
        T[] inReal,
        Range inRange,
        int[] outInteger,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        MaxIndexImpl<T>(inReal, inRange, outInteger, out outRange, optInTimePeriod);

    private static Core.RetCode MaxIndexImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<int> outInteger,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности временного периода
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периода обратного просмотра (lookback)
        var lookbackTotal = MaxIndexLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом lookback
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, выход
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Продолжение вычисления для запрашиваемого диапазона
        // Индекс для записи в выходной массив
        var outIdx = 0;
        // Текущий индекс обрабатываемого бара
        var today = startIdx;
        // Индекс начала скользящего окна
        var trailingIdx = startIdx - lookbackTotal;

        // Индекс наибольшего значения в окне
        var highestIdx = -1;
        // Наибольшее значение в окне
        var highest = T.Zero;
        while (today <= endIdx)
        {
            // Обновление индекса и значения наибольшего значения
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inReal, trailingIdx, today, highestIdx, highest);

            // Запись индекса наибольшего значения в выходной массив
            outInteger[outIdx++] = highestIdx;
            // Сдвиг начала окна вперед
            trailingIdx++;
            // Переход к следующему бару
            today++;
        }

        // Установка выходного диапазона валидных значений
        // Сохранение начального индекса относительно входных данных перед возвратом
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
