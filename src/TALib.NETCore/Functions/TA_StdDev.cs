//Название файла TA_StdDev.cs

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Стандартное отклонение (Статистические функции)
    /// </summary>
    /// <param name="inReal">Диапазон входных значений.</param>
    /// <param name="inRange">Диапазон индексов, определяющий часть данных для расчета во входном наборе.</param>
    /// <param name="outReal">Диапазон для сохранения рассчитанных значений.</param>
    /// <param name="outRange">Диапазон индексов, представляющий валидные данные в выходном наборе.</param>
    /// <param name="optInTimePeriod">Период времени (количество периодов).</param>
    /// <param name="optInNbDev">Количество стандартных отклонений (множитель).</param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или ошибку расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении, иначе код ошибки.
    /// </returns>
    /// <remarks>
    /// Стандартное отклонение — статистическая мера, количественно оценивающая разброс данных
    /// относительно их среднего значения. В техническом анализе используется для оценки волатильности финансовых инструментов.
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление арифметического среднего для данных в скользящем окне длиной `optInTimePeriod`.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет дисперсии путем суммирования квадратов разностей между значениями и средним, деленного на количество данных:
    ///       <code>
    ///         Variance = Σ((Value - Mean)^2) / TimePeriod
    ///       </code>
    ///       где Σ — сумма, Value — текущее значение, Mean — среднее арифметическое.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Извлечение квадратного корня из дисперсии для получения стандартного отклонения:
    ///       <code>
    ///         StdDev = √Variance
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Умножение стандартного отклонения на коэффициент `optInNbDev` для масштабирования результата.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Высокие значения указывают на большую волатильность и разброс данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Низкие значения свидетельствуют о стабильности и малой изменчивости данных.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode StdDev<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 5,
        double optInNbDev = 1.0) where T : IFloatingPointIeee754<T> =>
        StdDevImpl(inReal, inRange, outReal, out outRange, optInTimePeriod, optInNbDev);

    /// <summary>
    /// Возвращает минимальный период для первого валидного значения индикатора <see cref="StdDev{T}">StdDev</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени (количество периодов).</param>
    /// <returns>
    /// Количество баров, которые необходимо пропустить перед началом расчета,
    /// чтобы получить первое корректное значение индикатора.
    /// </returns>
    [PublicAPI]
    public static int StdDevLookback(int optInTimePeriod = 5) => optInTimePeriod < 2 ? -1 : VarLookback(optInTimePeriod);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode StdDev<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 5,
        double optInNbDev = 1.0) where T : IFloatingPointIeee754<T> =>
        StdDevImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod, optInNbDev);

    private static Core.RetCode StdDevImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod,
        double optInNbDev) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка валидности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка минимального периода (не менее 2)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет дисперсии через вспомогательный метод
        var retCode = FunctionHelpers.CalcVariance(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outReal, out outRange,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement = outRange.End.Value - outRange.Start.Value;
        // Вычисляем квадратный корень из каждой дисперсии — это стандартное отклонение.
        // Также умножаем на указанный коэффициент optInNbDev
        if (!optInNbDev.Equals(1.0))
        {
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = outReal[i];
                outReal[i] = tempReal > T.Zero ? T.Sqrt(tempReal) * T.CreateChecked(optInNbDev) : T.Zero;
            }
        }
        else
        {
            for (var i = 0; i < nbElement; i++)
            {
                var tempReal = outReal[i];
                outReal[i] = tempReal > T.Zero ? T.Sqrt(tempReal) : T.Zero;
            }
        }

        return Core.RetCode.Success;
    }
}
