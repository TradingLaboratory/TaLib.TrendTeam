// TA_Sma.cs
// Группы к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// Averages (существующая подпапка в OverlapStudies - идеальное размещение)
// TrendDirection (альтернатива для акцента на определении направления тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Simple Moving Average (Overlap Studies) — Простое скользящее среднее (Наложенные исследования)
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
    /// <param name="optInTimePeriod">Период расчета (количество баров для усреднения).</param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успешность операции.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, иначе — код ошибки.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Простое скользящее среднее (SMA) сглаживает ценовые данные за указанный период путем расчета
    /// арифметического среднего значений внутри окна периода. Каждое значение в окне имеет равный вес.
    /// </para>
    /// <para>
    /// SMA является запаздывающим (лаговым) индикатором, так как реагирует на прошлые изменения цены.
    /// Благодаря простоте и наглядности, широко применяется для:
    /// - определения направления тренда,
    /// - выявления уровней поддержки и сопротивления,
    /// - генерации торговых сигналов при пересечении нескольких SMA.
    /// </para>
    ///
    /// <b>Формула расчета:</b>
    /// <code>
    /// SMA[t] = (Close[t] + Close[t-1] + ... + Close[t-(optInTimePeriod-1)]) / optInTimePeriod
    /// </code>
    /// где:
    /// - <c>SMA[t]</c> — значение индикатора на баре t,
    /// - <c>Close</c> — цены закрытия (или другие входные данные),
    /// - <c>optInTimePeriod</c> — период усреднения.
    ///
    /// <b>Особенности индикатора:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Чем больше период (<paramref name="optInTimePeriod"/>), тем сильнее сглаживание и запаздывание сигнала.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       SMA не реагирует на экстремальные значения (в отличие от экспоненциального скользящего среднего).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Lookback-период равен <c>optInTimePeriod - 1</c> — первое валидное значение появляется после накопления данных за полный период.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sma<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        SmaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает lookback-период для <see cref="Sma{T}">Sma</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период расчета (количество баров для усреднения).</param>
    /// <returns>Количество баров, необходимых для расчета первого валидного значения индикатора.</returns>
    /// <remarks>
    /// Lookback-период равен <c>optInTimePeriod - 1</c>, так как для расчета первого значения SMA
    /// требуется накопить данные за полный период <paramref name="optInTimePeriod"/>.
    /// Например, при периоде 10 первое валидное значение будет доступно на 10-м баре (индекс 9),
    /// а индексы 0–8 будут пропущены.
    /// </remarks>
    [PublicAPI]
    public static int SmaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Вспомогательный метод для совместимости с массивами (вместо Span).
    /// Перенаправляет вызов в основную реализацию <see cref="SmaImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sma<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        SmaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode SmaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона нулевым значением (пока не рассчитано)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и границ массива
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            // Возврат ошибки при некорректном диапазоне (выход за границы массива или отрицательная длина)
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка минимального допустимого периода (требуется минимум 2 бара для расчета среднего)
        if (optInTimePeriod < 2)
        {
            // Возврат ошибки при некорректном значении периода
            return Core.RetCode.BadParam;
        }

        // Вызов вспомогательного метода для расчета простого скользящего среднего
        // Метод выполняет итеративное суммирование и деление на период для каждого бара
        return FunctionHelpers.CalcSimpleMA(
            inReal,
            new Range(rangeIndices.startIndex, rangeIndices.endIndex),
            outReal,
            out outRange,
            optInTimePeriod);
    }
}
