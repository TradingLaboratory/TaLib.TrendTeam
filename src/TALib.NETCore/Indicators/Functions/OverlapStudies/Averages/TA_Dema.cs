// Название файла: TA_Dema.cs
// Группы, к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// TrendFollowing (альтернатива для группировки по следованию за трендом)
// SmoothingFunctions (альтернатива для акцента на сглаживании данных)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Double Exponential Moving Average (Overlap Studies) — Двойная экспоненциальная скользящая средняя (Индикаторы наложения)
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
    /// <param name="optInTimePeriod">Период времени для расчета экспоненциальной скользящей средней (EMA).</param>
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
    /// Двойная экспоненциальная скользящая средняя (DEMA) предназначена для уменьшения запаздывания (lag), связанного с традиционными скользящими средними,
    /// путем комбинирования одиночной экспоненциальной скользящей средней (EMA) с EMA этой EMA (обычно называемой EMA2).
    /// </para>
    /// <para>
    /// Этот расчет приводит к более гладкой средней, которая быстрее реагирует на изменения цен, что делает её полезной для выявления
    /// трендов и разворотов с меньшим запаздыванием. Функция может улучшить отзывчивость в стратегиях следования за трендом.
    /// Комбинирование её с осцилляторами может помочь подтвердить сигналы и минимизировать задержки.
    /// </para>
    ///
    /// <b>Формула расчета</b>:
    /// <code>
    ///   DEMA = 2 * EMA(t, period) - EMA(EMA(t, period), period)
    /// </code>
    /// где:
    ///   EMA(t, period) — первая экспоненциальная скользящая средняя от исходного ряда
    ///   EMA(EMA(t, period), period) — вторая экспоненциальная скользящая средняя (EMA2)
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Dema<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        DemaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="Dema{T}">DEMA</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета экспоненциальной скользящей средней.</param>
    /// <returns>Количество периодов, необходимых до расчета первого валидного выходного значения.</returns>
    [PublicAPI]
    public static int DemaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : EmaLookback(optInTimePeriod) * 2;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Dema<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        DemaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode DemaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимально допустимого значения периода
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Объяснение функции можно найти в:
         *
         * Stocks & Commodities V. 12:1 (11-19):
         *   Smoothing Data With Faster Moving Averages
         * Stocks & Commodities V. 12:2 (72-80):
         *   Smoothing Data With Less Lag
         *
         * Обе статьи написаны Патриком Г. Маллоем (Patrick G. Mulloy)
         *
         * По сути, DEMA временного ряда "t" рассчитывается как:
         *   EMA2 = EMA(EMA(t, period), period)  — вторая экспоненциальная скользящая средняя
         *   DEMA = 2 * EMA(t, period) - EMA2    — двойная экспоненциальная скользящая средняя
         *
         * DEMA предлагает скользящую среднюю с меньшими задержками по сравнению с традиционной EMA.
         *
         * Важно: Не путайте DEMA с EMA2. Оба термина иногда называют "Двойная EMA" в литературе,
         * но EMA2 — это простая EMA от EMA, тогда как DEMA — это композиция одиночной EMA с EMA2.
         *
         * TEMA (Triple Exponential Moving Average) очень похожа на DEMA и также разработана Патриком Маллоем.
         */

        // Период обратного просмотра для расчета одной EMA
        var lookbackEMA = EmaLookback(optInTimePeriod);
        // Общий период обратного просмотра для DEMA (двойной, так как требуется две EMA)
        var lookbackTotal = DemaLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом необходимого периода обратного просмотра
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки нет данных для обработки — возвращаем успех с пустым результатом
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделяем временный буфер для хранения первой EMA
        // При возможности повторно используем выходной буфер для временных расчетов (оптимизация памяти)
        Span<T> firstEMA;
        if (inReal == outReal)
        {
            firstEMA = outReal;
        }
        else
        {
            var tempInt = lookbackTotal + (endIdx - startIdx) + 1;
            firstEMA = new T[tempInt];
        }

        // Рассчитываем коэффициент сглаживания для экспоненциальной скользящей средней
        var k = FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One);
        // Рассчитываем первую EMA на расширенном диапазоне (с учетом lookback)
        var retCode = FunctionHelpers.CalcExponentialMA(
            inReal, new Range(startIdx - lookbackEMA, endIdx), firstEMA, out var firstEMARange, optInTimePeriod, k);
        var firstEMANbElement = firstEMARange.End.Value - firstEMARange.Start.Value;
        if (retCode != Core.RetCode.Success || firstEMANbElement == 0)
        {
            return retCode;
        }

        // Выделяем временный буфер для хранения второй EMA (EMA от первой EMA)
        Span<T> secondEMA = new T[firstEMANbElement];
        // Рассчитываем вторую EMA (EMA2) на основе первой EMA
        retCode = FunctionHelpers.CalcExponentialMA(firstEMA, Range.EndAt(firstEMANbElement - 1), secondEMA, out var secondEMARange,
            optInTimePeriod, k);
        var secondEMABegIdx = secondEMARange.Start.Value;
        var secondEMANbElement = secondEMARange.End.Value - secondEMABegIdx;
        if (retCode != Core.RetCode.Success || secondEMANbElement == 0)
        {
            return retCode;
        }

        // Финальный расчет DEMA по формуле: DEMA = 2 * EMA - EMA2
        var firstEMAIdx = secondEMABegIdx;
        var outIdx = 0;
        while (outIdx < secondEMANbElement)
        {
            outReal[outIdx] = FunctionHelpers.Two<T>() * firstEMA[firstEMAIdx++] - secondEMA[outIdx];
            outIdx++;
        }

        // Устанавливаем выходной диапазон с учетом смещения от первой и второй EMA
        outRange = new Range(firstEMARange.Start.Value + secondEMABegIdx, firstEMARange.Start.Value + secondEMABegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
