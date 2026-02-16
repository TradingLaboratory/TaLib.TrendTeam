// Название файла: TA_Ema.cs
// Рекомендуемое размещение:
// Основная папка: OverlapStudies
// Подпапка: Averages (существующая)
// Альтернативные категории: TrendFollowing (для акцента на следовании тренду)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Exponential Moving Average (Overlap Studies) — Экспоненциальное скользящее среднее (Индикаторы наложения)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены Close, Open, High, Low или другие временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период времени (количество баров для расчета). Минимальное значение: 2.</param>
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
    /// Экспоненциальное скользящее среднее (EMA) — это взвешенное скользящее среднее, которое придает больший вес недавним ценам,
    /// обеспечивая более быструю реакцию на изменения цены по сравнению с простым скользящим средним (SMA).
    /// </para>
    /// <para>
    /// EMA широко применяется в техническом анализе для:
    /// <list type="bullet">
    ///   <item><description>идентификации направления тренда;</description></item>
    ///   <item><description>генерации торговых сигналов при пересечении ценой или другими EMA;</description></item>
    ///   <item><description>сглаживания ценовых колебаний для фильтрации рыночного шума.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Формула расчета:</b>
    /// <code>
    ///   k = 2 / (Time Period + 1)          // Сглаживающий коэффициент (smoothing constant)
    ///   EMA_today = (Close_today - EMA_yesterday) * k + EMA_yesterday
    /// </code>
    /// где:
    ///   - <c>k</c> — сглаживающий коэффициент (обычно обозначается как α);
    ///   - <c>Close_today</c> — цена закрытия текущего бара;
    ///   - <c>EMA_yesterday</c> — значение EMA предыдущего бара.
    /// </para>
    /// <para>
    /// <b>Особенности реализации:</b>
    /// Первое значение EMA рассчитывается как SMA (простое скользящее среднее) за указанный период,
    /// после чего применяется рекурсивная формула экспоненциального сглаживания.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Ema<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        EmaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предыстории для <see cref="Ema{T}">Ema</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int EmaLookback(int optInTimePeriod = 30) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod - 1 + Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Ema);


    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span)
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Ema<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        EmaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode EmaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона нулевым значением (будет перезаписан при успешном расчете)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и достаточности данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка минимально допустимого периода (меньше 2 баров невозможно рассчитать EMA)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет сглаживающего коэффициента (smoothing constant) по формуле: k = 2 / (Period + 1)
        // Этот коэффициент определяет вес недавних данных в расчете EMA
        T smoothingConstant = FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One);

        // Вызов универсального метода расчета экспоненциального скользящего среднего
        // Передаем входные данные, диапазон обработки, выходной массив, период и рассчитанный коэффициент сглаживания
        return FunctionHelpers.CalcExponentialMA(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outReal, out outRange,
            optInTimePeriod, smoothingConstant);
    }
}
