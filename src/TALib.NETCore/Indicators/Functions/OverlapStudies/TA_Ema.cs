//Название файла: TA_Ema.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//MomentumIndicators (альтернатива, если требуется группировка по типу индикатора)
//TrendFollowing (альтернатива для акцента на следовании тренду)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Exponential Moving Average (Overlap Studies) — Экспоненциальное скользящее среднее (Наложение)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Экспоненциальное скользящее среднее (EMA) — это тип скользящего среднего, который придает больший вес недавним данным,
    /// что делает его более чувствительным к новой информации по сравнению с простым скользящим средним (SMA).
    /// <para>
    /// Функция часто используется в техническом анализе для выявления трендов и генерации торговых сигналов.
    /// Она особенно полезна для отслеживания краткосрочных движений цен или импульса на финансовых рынках.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить начальное SMA (простое скользящее среднее) за указанный период времени, чтобы использовать его в качестве первого значения EMA.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить сглаживающий коэффициент:
    ///       <code>
    ///         Сглаживающий коэффициент (k) = 2 / (Период времени + 1)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Обновить EMA для последующих периодов с использованием формулы:
    ///       <code>
    ///         EMA = (Текущая цена - Предыдущее EMA) * k + Предыдущее EMA
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
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
    /// Для совместимости с абстрактным API
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
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Вычисление сглаживающего коэффициента
        T smoothingConstant = FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One);

        // Вызов функции для вычисления экспоненциального скользящего среднего
        return FunctionHelpers.CalcExponentialMA(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outReal, out outRange,
            optInTimePeriod, smoothingConstant);
    }
}
