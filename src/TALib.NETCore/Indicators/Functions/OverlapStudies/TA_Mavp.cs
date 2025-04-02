//Название файла: TA_Mavp.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//VolatilityIndicators (альтернатива, если требуется группировка по типу индикатора)
//AdaptiveIndicators (альтернатива для акцента на адаптивности индикатора)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Moving average with variable period (Overlap Studies) — Скользящее среднее с переменным периодом (Индикаторы наложения)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inPeriods">Массив периодов, определяющих длину скользящего среднего для каждой точки данных.</param>
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
    /// <param name="optInMinPeriod">Минимально допустимый период для расчета скользящего среднего.</param>
    /// <param name="optInMaxPeriod">Максимально допустимый период для расчета скользящего среднего.</param>
    /// <param name="optInMAType">Тип скользящего среднего.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция скользящего среднего с переменным периодом рассчитывает скользящее среднее, где период может варьироваться для каждой точки данных.
    /// Эта гибкость позволяет скользящему среднему динамически адаптироваться к изменяющимся условиям,
    /// таким как волатильность или пользовательские периоды.
    /// <para>
    /// Функция особенно полезна в сценариях, где требуется адаптация к рыночным условиям или специфическим пользовательским периодам.
    /// Выбор <paramref name="optInMAType"/> и диапазон, определяемый <paramref name="optInMinPeriod"/> и
    /// <paramref name="optInMaxPeriod"/>, значительно влияют на поведение и чувствительность MAVP.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Усечение входных периодов <paramref name="inPeriods"/> для соответствия указанным MinPeriod и MaxPeriod.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление скользящего среднего для каждой точки данных с использованием соответствующего периода из усеченного ряда.
    ///       Тип скользящего среднего <paramref name="optInMAType"/> определяет метод расчета (например, SMA, EMA и т.д.).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Заполнение выходного массива рассчитанными значениями скользящего среднего для каждого периода.
    ///       Избегайте избыточных вычислений, повторно используя результаты для одинаковых периодов во входных данных.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходные данные MAVP отражают динамически скорректированное скользящее среднее на основе входных периодов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более короткие периоды приводят к более реактивным и чувствительным выходным данным, тогда как более длинные периоды обеспечивают более сглаженные и менее чувствительные выходные данные.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Mavp<T>(
        ReadOnlySpan<T> inReal,
        ReadOnlySpan<T> inPeriods,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInMinPeriod = 2,
        int optInMaxPeriod = 30,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MavpImpl(inReal, inPeriods, inRange, outReal, out outRange, optInMinPeriod, optInMaxPeriod, optInMAType);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="Mavp{T}">Mavp</see>.
    /// </summary>
    /// <param name="optInMaxPeriod">Максимально допустимый период для расчета скользящего среднего.</param>
    /// <param name="optInMAType">Тип скользящего среднего.</param>
    /// <returns>Количество периодов, необходимых до первого выходного значения.</returns>
    [PublicAPI]
    public static int MavpLookback(int optInMaxPeriod = 30, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInMaxPeriod < 2 ? -1 : MaLookback(optInMaxPeriod, optInMAType);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Mavp<T>(
        T[] inReal,
        T[] inPeriods,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInMinPeriod = 2,
        int optInMaxPeriod = 30,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MavpImpl<T>(inReal, inPeriods, inRange, outReal, out outRange, optInMinPeriod, optInMaxPeriod, optInMAType);

    private static Core.RetCode MavpImpl<T>(
        ReadOnlySpan<T> inReal,
        ReadOnlySpan<T> inPeriods,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInMinPeriod,
        int optInMaxPeriod,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length, inPeriods.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального и максимального периодов
        if (optInMinPeriod < 2 || optInMaxPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = MavpLookback(optInMaxPeriod, optInMAType);
        if (inPeriods.Length < lookbackTotal)
        {
            return Core.RetCode.BadParam;
        }

        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var tempInt = lookbackTotal > startIdx ? lookbackTotal : startIdx;
        if (tempInt > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outputSize = endIdx - tempInt + 1;

        // Выделение промежуточного локального буфера
        Span<T> localOutputArray = new T[outputSize];
        Span<int> localPeriodArray = new int[outputSize];

        // Копирование массива периодов в локальный буфер с одновременным усечением до min/max
        for (var i = 0; i < outputSize; i++)
        {
            var period = Int32.CreateTruncating(inPeriods[startIdx + i]);
            localPeriodArray[i] = Math.Clamp(period, optInMinPeriod, optInMaxPeriod);
        }

        var intermediateOutput = outReal == inReal ? new T[outputSize] : outReal;

        /* Обработка каждого элемента входных данных.
         * Для каждого возможного значения периода MA рассчитывается только один раз.
         * Затем outReal заполняется для всех элементов с одинаковым периодом.
         * Устанавливается локальный флаг (значение 0) в localPeriodArray, чтобы избежать повторного вычисления.
         */
        var retCode = CalcMovingAverages(inReal, localPeriodArray, localOutputArray, new Range(startIdx, endIdx), outputSize, optInMAType,
            intermediateOutput);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Копирование промежуточного буфера в выходной буфер, если необходимо
        if (intermediateOutput != outReal)
        {
            intermediateOutput[..outputSize].CopyTo(outReal);
        }

        outRange = new Range(startIdx, startIdx + outputSize);

        return Core.RetCode.Success;
    }

    private static Core.RetCode CalcMovingAverages<T>(
        ReadOnlySpan<T> real,
        Span<int> periodArray,
        Span<T> outputArray,
        Range range,
        int outputSize,
        Core.MAType maType,
        Span<T> intermediateOutput) where T : IFloatingPointIeee754<T>
    {
        for (var i = 0; i < outputSize; i++)
        {
            var curPeriod = periodArray[i];
            if (curPeriod == 0)
            {
                continue;
            }

            // Вычисление MA требуется
            var retCode = MaImpl(real, range, outputArray, out _, curPeriod, maType);
            if (retCode != Core.RetCode.Success)
            {
                return retCode;
            }

            intermediateOutput[i] = outputArray[i];
            for (var j = i + 1; j < outputSize; j++)
            {
                if (periodArray[j] == curPeriod)
                {
                    periodArray[j] = 0; // Флаг для избежания повторного вычисления
                    intermediateOutput[j] = outputArray[j];
                }
            }
        }

        return Core.RetCode.Success;
    }
}
