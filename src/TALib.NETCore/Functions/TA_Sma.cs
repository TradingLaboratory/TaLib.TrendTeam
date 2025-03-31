// TA_Sma.cs

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Простое скользящее среднее (Overlap Studies)
    /// </summary>
    /// <param name="inReal">Массив входных значений.</param>
    /// <param name="inRange">Диапазон индексов, определяющий часть данных для расчета.</param>
    /// <param name="outReal">Массив для сохранения рассчитанных значений.</param>
    /// <param name="outRange">Диапазон индексов с валидными данными в выходном массиве.</param>
    /// <param name="optInTimePeriod">Период расчета (количество периодов).</param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успешность операции.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, иначе — код ошибки.
    /// </returns>
    /// <remarks>
    /// Простое скользящее среднее сглаживает данные за указанный период путем расчета
    /// несредневзвешенного среднего значений внутри периода.
    /// <para>
    /// SMA является запаздывающим индикатором, реагирующим на прошлые изменения цены.
    /// Благодаря простоте и эффективности, он широко используется для определения уровней
    /// поддержки/сопротивления, подтверждения трендов и генерации торговых сигналов.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Определение периода расчета (<paramref name="optInTimePeriod"/>).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Суммирование значений внутри периода:
    ///       <code>
    ///         Sum = data[t] + data[t-1] + ... + data[t-(optInTimePeriod-1)]
    ///       </code>
    ///       где Sum — сумма значений за период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет среднего значения:
    ///       <code>
    ///         SMA = Sum / optInTimePeriod
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Перемещение окна расчета на один период вперед и повторение операций для следующих значений.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост SMA указывает на восходящий тренд (цены растут в течение периода).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Падение SMA указывает на нисходящий тренд (цены снижаются в течение периода).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение SMA (например, краткосрочного SMA с долгосрочным) часто используется как сигнал к покупке/продаже.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       SMA может выступать в роли уровней поддержки или сопротивления.
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
    /// <param name="optInTimePeriod">Период расчета.</param>
    /// <returns>Количество периодов, необходимых для расчета первого валидного значения.</returns>
    /// <remarks>
    /// Lookback-период равен (optInTimePeriod - 1), так как первое значение SMA требует
    /// данных за optInTimePeriod предыдущих баров.
    /// </remarks>
    [PublicAPI]
    public static int SmaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
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
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam; // Возврат ошибки при невалидном диапазоне
        }

        // Проверка периода (минимум 2 бара)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam; // Возврат ошибки при некорректном периоде
        }

        // Вызов основного метода расчета простого скользящего среднего
        return FunctionHelpers.CalcSimpleMA(
            inReal,
            new Range(rangeIndices.startIndex, rangeIndices.endIndex),
            outReal,
            out outRange,
            optInTimePeriod);
    }
}
