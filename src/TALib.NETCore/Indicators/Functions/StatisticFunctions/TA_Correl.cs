//Название файла: TA_Correl.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//CorrelationIndicators (альтернатива, если требуется группировка по типу индикатора)
//RelationshipAnalysis (альтернатива для акцента на анализе взаимосвязей)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Pearson's Correlation Coefficient (r) (Statistic Functions) — Коэффициент корреляции Пирсона (r) (Статистические функции)
    /// </summary>
    /// <param name="inReal0">Входные данные для первого набора данных.</param>
    /// <param name="inReal1">Входные данные для второго набора данных.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal0"/> и <paramref name="inReal1"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i]</c> и <c>inReal1[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal0"/> и <paramref name="inReal1"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal0"/> или <paramref name="inReal1"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Коэффициент корреляции Пирсона (r) измеряет линейную корреляцию между двумя наборами данных, показывая, насколько они движутся вместе.
    /// <para>
    /// Функция полезна для построения портфеля, парных стратегий и диверсификации.
    /// Её можно использовать вместе с индикаторами относительной силы или спрэда для выявления разрушений или схождений корреляции.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить суммы двух наборов данных и их соответствующих квадратов за указанный период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить произведение наборов данных для каждого периода времени и рассчитать сумму этих произведений.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить ковариацию и стандартные отклонения наборов данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Разделить ковариацию на произведение стандартных отклонений для вычисления коэффициента корреляции.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение -1 указывает на идеальную отрицательную линейную зависимость.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение 0 указывает на отсутствие линейной зависимости.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение 1 указывает на идеальную положительную линейную зависимость.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Correl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        CorrelImpl(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предварительного просмотра для <see cref="Correl{T}">Correl</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int CorrelLookback(int optInTimePeriod = 30) => optInTimePeriod < 1 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Correl<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        CorrelImpl<T>(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode CorrelImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = CorrelLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx;
        var trailingIdx = startIdx - lookbackTotal;

        // Вычисление начальных значений.
        T sumX, sumY, sumX2, sumY2;
        var sumXY = sumX = sumY = sumX2 = sumY2 = T.Zero;
        int today;
        for (today = trailingIdx; today <= startIdx; today++)
        {
            var x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            var y = inReal1[today];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Запись первого выходного значения.
        // Сначала сохраняем отстающие значения, так как входные и выходные данные могут быть одним и тем же массивом.
        var trailingX = inReal0[trailingIdx];
        var trailingY = inReal1[trailingIdx++];
        var tempReal = (sumX2 - sumX * sumX / timePeriod) * (sumY2 - sumY * sumY / timePeriod);
        outReal[0] = tempReal > T.Zero ? (sumXY - sumX * sumY / timePeriod) / T.Sqrt(tempReal) : T.Zero;

        // Основной цикл для вычисления последующих значений.
        var outIdx = 1;
        while (today <= endIdx)
        {
            // Удаление отстающих значений
            sumX -= trailingX;
            sumX2 -= trailingX * trailingX;

            sumXY -= trailingX * trailingY;
            sumY -= trailingY;
            sumY2 -= trailingY * trailingY;

            // Добавление новых значений
            var x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            var y = inReal1[today++];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;

            // Вывод нового коэффициента.
            // Сначала сохраняем отстающие значения, так как входные и выходные данные могут быть одним и тем же массивом.
            trailingX = inReal0[trailingIdx];
            trailingY = inReal1[trailingIdx++];
            tempReal = (sumX2 - sumX * sumX / timePeriod) * (sumY2 - sumY * sumY / timePeriod);
            outReal[outIdx++] = tempReal > T.Zero ? (sumXY - sumX * sumY / timePeriod) / T.Sqrt(tempReal) : T.Zero;
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
