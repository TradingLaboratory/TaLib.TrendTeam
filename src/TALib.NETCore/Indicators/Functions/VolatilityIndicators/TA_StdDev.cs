//Название файла TA_StdDev.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//VolatilityIndicators (альтернатива, если требуется группировка по типу индикатора)
//MathOperators (альтернатива для акцента на математических операциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Standard Deviation (Statistical Functions) — Стандартное отклонение (Статистические функции)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены Close, другие индикаторы или другие временные ряды).
    /// Обычно используются цены закрытия (Close) для оценки волатильности.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Позволяет вычислить индикатор только для части входных данных.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    ///   Это индекс, с которого начинается lookback период (минимальное количество баров для расчета).
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <param name="optInTimePeriod">
    /// Период времени (количество периодов для расчета скользящего окна).
    /// <para>
    /// - Минимальное значение: 2
    /// - Рекомендуемое значение по умолчанию: 5
    /// - Влияет на чувствительность индикатора: больший период = более сглаженные значения.
    /// </para>
    /// </param>
    /// <param name="optInNbDev">
    /// Количество стандартных отклонений (множитель для масштабирования результата).
    /// <para>
    /// - Значение по умолчанию: 1.0
    /// - Используется для расширения/сужения полос волатильности.
    /// - Например, 2.0 означает удвоенное стандартное отклонение.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных (обычно <see langword="float"/> или <see langword="double"/>),
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или ошибку расчета.
    /// <para>
    /// - Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении.
    /// - Иначе возвращает код ошибки (например, <see cref="Core.RetCode.BadParam"/> или <see cref="Core.RetCode.OutOfRangeParam"/>).
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <b>Стандартное отклонение (StdDev)</b> — статистическая мера, количественно оценивающая разброс данных
    /// относительно их среднего значения (Mean). В техническом анализе используется для оценки волатильности
    /// финансовых инструментов и построения полос волатильности (например, Bollinger Bands).
    /// </para>
    ///
    /// <para>
    /// <b>Этапы расчета</b>:
    /// </para>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление арифметического среднего (Mean) для данных в скользящем окне длиной <c>optInTimePeriod</c>:
    ///       <code>
    ///         Mean = Σ(Value) / TimePeriod
    ///       </code>
    ///       где Σ — сумма всех значений в окне, Value — текущее значение.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расчет дисперсии (Variance) путем суммирования квадратов разностей между значениями и средним,
    ///       деленного на количество данных:
    ///       <code>
    ///         Variance = Σ((Value - Mean)^2) / TimePeriod
    ///       </code>
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
    ///       Умножение стандартного отклонения на коэффициент <c>optInNbDev</c> для масштабирования результата:
    ///       <code>
    ///         Result = StdDev × NbDev
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// <b>Интерпретация значений</b>:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>Высокие значения</b> указывают на большую волатильность и значительный разброс данных.
    ///       Сигнализирует о нестабильности рынка или сильных ценовых движениях.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Низкие значения</b> свидетельствуют о стабильности и малой изменчивости данных.
    ///       Сигнализирует о консолидации или спокойном рынке.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// <b>Применение в трейдинге</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Построение полос волатильности (Bollinger Bands, Keltner Channels).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Оценка риска: высокое StdDev = высокий риск.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определение периодов сжатия волатильности (предвестник сильного движения).
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Lookback период</b>: количество баров, которые необходимо пропустить перед началом расчета
    /// валидных значений индикатора. Для StdDev lookback равен <c>optInTimePeriod - 1</c>.
    /// Все бары с индексом меньше lookback не будут иметь валидных значений.
    /// </para>
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
    /// Возвращает минимальный период (lookback) для первого валидного значения индикатора <see cref="StdDev{T}">StdDev</see>.
    /// </summary>
    /// <param name="optInTimePeriod">
    /// Период времени (количество периодов для расчета скользящего окна).
    /// Минимальное допустимое значение: 2.
    /// </param>
    /// <returns>
    /// Количество баров, которые необходимо пропустить перед началом расчета,
    /// чтобы получить первое корректное значение индикатора (lookback период).
    /// <para>
    /// - Возвращает <c>-1</c>, если <c>optInTimePeriod &lt; 2</c> (некорректный параметр).
    /// - Иначе возвращает значение из <see cref="VarLookback"/>, которое равно <c>optInTimePeriod - 1</c>.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Lookback период определяет индекс первого бара во входных данных, для которого можно получить
    /// валидное значение рассчитываемого индикатора. Все бары с индексом меньше lookback будут пропущены.
    /// </remarks>
    [PublicAPI]
    public static int StdDevLookback(int optInTimePeriod = 5) => optInTimePeriod < 2 ? -1 : VarLookback(optInTimePeriod);

    /// <remarks>
    /// Для совместимости с абстрактным API (перегрузка для массивов вместо Span).
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
        // Инициализация outRange пустым диапазоном (будет обновлен после расчета)
        outRange = Range.EndAt(0);

        // Проверка валидности входного диапазона
        // Возвращает null, если диапазон некорректен или выходит за границы массива
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Проверка минимального периода (не менее 2 для корректного расчета дисперсии)
        // Период меньше 2 не позволяет вычислить статистически значимое отклонение
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет дисперсии через вспомогательный метод CalcVariance
        // outReal временно содержит значения дисперсии (Variance), а не StdDev
        var retCode = FunctionHelpers.CalcVariance(inReal, new Range(rangeIndices.startIndex, rangeIndices.endIndex), outReal, out outRange,
            optInTimePeriod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Количество элементов с валидными значениями в выходном массиве
        var nbElement = outRange.End.Value - outRange.Start.Value;

        // Вычисляем квадратный корень из каждой дисперсии — это стандартное отклонение (StdDev).
        // Также умножаем на указанный коэффициент optInNbDev для масштабирования результата.
        // Формула: StdDev = √Variance × NbDev
        if (!optInNbDev.Equals(1.0))
        {
            // Если множитель отличается от 1.0, применяем масштабирование
            for (var i = 0; i < nbElement; i++)
            {
                // Временная переменная для хранения текущего значения дисперсии
                var tempReal = outReal[i];
                // Вычисляем квадратный корень и умножаем на множитель
                // Если значение <= 0, возвращаем 0 (защита от отрицательных значений под корнем)
                outReal[i] = tempReal > T.Zero ? T.Sqrt(tempReal) * T.CreateChecked(optInNbDev) : T.Zero;
            }
        }
        else
        {
            // Если множитель равен 1.0, просто извлекаем квадратный корень
            for (var i = 0; i < nbElement; i++)
            {
                // Временная переменная для хранения текущего значения дисперсии
                var tempReal = outReal[i];
                // Вычисляем квадратный корень без дополнительного масштабирования
                // Если значение <= 0, возвращаем 0 (защита от отрицательных значений под корнем)
                outReal[i] = tempReal > T.Zero ? T.Sqrt(tempReal) : T.Zero;
            }
        }

        return Core.RetCode.Success;
    }
}
