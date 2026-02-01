// Tema.cs
// Группы, к которым можно отнести индикатор:
// OverlapStudies (существующая папка - идеальное соответствие категории)
// MomentumIndicators (альтернатива, если требуется группировка по чувствительности к импульсу)
// TrendSmoothing (альтернатива для акцента на сглаживании тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Triple Exponential Moving Average (Overlap Studies) — Тройная экспоненциальная скользящая средняя (Исследования наложения)
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
    /// <param name="optInTimePeriod">Период времени для расчета экспоненциальных скользящих средних</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Тройная экспоненциальная скользящая средняя (TEMA) — это метод сглаживания, разработанный для уменьшения лага по сравнению с традиционными скользящими средними.
    /// TEMA рассчитывается с использованием трех уровней экспоненциальных скользящих средних (EMA) для достижения большей отзывчивости
    /// к изменениям цены при одновременном минимизации шума.
    /// <para>
    /// TEMA обеспечивает более плавное представление ценового тренда по сравнению с одиночной EMA, уменьшая лаг при сохранении
    /// чувствительности к изменениям цены.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычисление первой EMA (EMA1) по входным данным (<paramref name="inReal"/>) с использованием заданного
    ///       <paramref name="optInTimePeriod"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление второй EMA (EMA2) на основе EMA1:
    ///       <code>
    ///         EMA2 = EMA(EMA1, optInTimePeriod)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление третьей EMA (EMA3) на основе EMA2:
    ///       <code>
    ///         EMA3 = EMA(EMA2, optInTimePeriod)
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Комбинирование результатов трех EMA для расчета TEMA:
    ///       <code>
    ///         TEMA = 3 * EMA1 - 3 * EMA2 + EMA3
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Tema<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        TemaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период lookback для <see cref="Tema{T}">Tema</see>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета экспоненциальных скользящих средних</param>
    /// <returns>Количество периодов, необходимых перед тем, как можно будет рассчитать первое выходное значение.</returns>
    [PublicAPI]
    public static int TemaLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : EmaLookback(optInTimePeriod) * 3;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Tema<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        TemaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode TemaImpl<T>(
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

        // Проверка минимального допустимого периода (должен быть >= 2)
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
         * Формула TEMA для временного ряда "t":
         *   EMA1 = EMA(t, period)       — первая экспоненциальная скользящая средняя
         *   EMA2 = EMA(EMA(t, period), period)  — вторая EMA, рассчитанная на основе первой
         *   EMA3 = EMA(EMA(EMA(t, period), period))  — третья EMA, рассчитанная на основе второй
         *   TEMA = 3 * EMA1 - 3 * EMA2 + EMA3  — финальная формула TEMA
         *
         * TEMA обеспечивает скользящую среднюю с меньшим лагом по сравнению с традиционной EMA.
         *
         * ВАЖНО: TEMA не следует путать с EMA3. Оба варианта в литературе иногда называют "Тройная EMA".
         * DEMA (Double Exponential Moving Average) очень похож на TEMA (и также разработан тем же автором).
         */

        // Период lookback для одной EMA
        var lookbackEMA = EmaLookback(optInTimePeriod);
        // Общий период lookback для TEMA (три последовательных EMA)
        var lookbackTotal = TemaLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом необходимого периода для расчета
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки начальный индекс превышает конечный — валидных данных нет
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Расчет размера временного буфера для хранения промежуточных EMA
        var tempInt = lookbackTotal + (endIdx - startIdx) + 1;
        // Коэффициент сглаживания для экспоненциальной скользящей средней: α = 2 / (period + 1)
        var k = FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One);

        // Буфер для хранения первой EMA (EMA1)
        Span<T> firstEMA = new T[tempInt];
        // Расчет первой EMA на расширенном диапазоне (с учетом двух предыдущих уровней сглаживания)
        var retCode = FunctionHelpers.CalcExponentialMA(inReal, new Range(startIdx - lookbackEMA * 2, endIdx), firstEMA,
            out var firstEMARange, optInTimePeriod, k);
        if (retCode != Core.RetCode.Success || firstEMARange.End.Value == 0)
        {
            return retCode;
        }

        // Количество элементов в первой EMA
        var firstEMANbElement = firstEMARange.End.Value - firstEMARange.Start.Value;
        // Буфер для хранения второй EMA (EMA2)
        Span<T> secondEMA = new T[firstEMANbElement];
        // Расчет второй EMA на основе первой EMA
        retCode = FunctionHelpers.CalcExponentialMA(firstEMA, Range.EndAt(firstEMANbElement - 1), secondEMA, out var secondEMARange,
            optInTimePeriod, k);
        if (retCode != Core.RetCode.Success || secondEMARange.End.Value == 0)
        {
            return retCode;
        }

        // Количество элементов во второй EMA
        var secondEMANbElement = secondEMARange.End.Value - secondEMARange.Start.Value;
        // Расчет третьей EMA (EMA3) напрямую в выходной буфер outReal
        retCode = FunctionHelpers.CalcExponentialMA(secondEMA, Range.EndAt(secondEMANbElement - 1), outReal, out var thirdEMARange,
            optInTimePeriod, k);
        if (retCode != Core.RetCode.Success || thirdEMARange.End.Value == 0)
        {
            return retCode;
        }

        // Индекс начала данных в первой EMA, соответствующих рассчитанным значениям EMA3
        var firstEMAIdx = thirdEMARange.Start.Value + secondEMARange.Start.Value;
        // Индекс начала данных во второй EMA, соответствующих рассчитанным значениям EMA3
        var secondEMAIdx = thirdEMARange.Start.Value;
        // Начальный индекс валидных данных в исходном массиве inReal
        var outBegIdx = firstEMAIdx + firstEMARange.Start.Value;

        // Количество рассчитанных элементов в третьей EMA (EMA3)
        var thirdEMANbElement = thirdEMARange.End.Value - thirdEMARange.Start.Value;
        // Итерация по значениям EMA3 (выходной буфер) с корректировкой значения по формуле TEMA:
        // TEMA = 3 * EMA1 - 3 * EMA2 + EMA3
        var outIdx = 0;
        while (outIdx < thirdEMANbElement)
        {
            outReal[outIdx++] += FunctionHelpers.Three<T>() * firstEMA[firstEMAIdx++] -
                                 FunctionHelpers.Three<T>() * secondEMA[secondEMAIdx++];
        }

        // Установка диапазона валидных выходных данных
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
