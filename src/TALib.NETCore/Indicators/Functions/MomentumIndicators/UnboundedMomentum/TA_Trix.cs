// Файл: Trix.cs
// Рекомендуемые категории для размещения файла:
// MomentumIndicators (основная категория - идеальное соответствие)
// TrendIndicators (альтернатива для группировки по трендовой направленности)
// Oscillators (альтернатива, так как TRIX часто используется как осциллятор)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// TRIX (Triple Exponential Moving Average Rate-Of-Change) (Momentum Indicators) — TRIX (Темп изменения тройной экспоненциальной скользящей средней) (Индикаторы импульса)
    /// <para>
    /// TRIX измеряет процентное изменение тройной экспоненциально сглаженной скользящей средней (Triple Smoothed EMA).
    /// Индикатор фильтрует краткосрочный шум и помогает выявлять устойчивые тренды и потенциальные точки разворота.
    /// </para>
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены Close или другие временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период сглаживания для экспоненциальной скользящей средней (EMA)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// TRIX применяется для:
    /// <list type="bullet">
    ///   <item><description>Выявления устойчивых трендов при подавлении краткосрочных колебаний</description></item>
    ///   <item><description>Определения потенциальных точек разворота при пересечении нулевой линии</description></item>
    ///   <item><description>Оценки силы импульса: положительные значения — восходящий тренд, отрицательные — нисходящий</description></item>
    /// </list>
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет первой экспоненциальной скользящей средней (EMA) входных данных за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение второй EMA к результату первой EMA для двойного сглаживания.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Применение третьей EMA к результату второй EMA для тройного сглаживания.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление 1-дневного темпа изменения (Rate-Of-Change, ROC) тройной сглаженной EMA для получения значений TRIX.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения TRIX — восходящий тренд (рост цен).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения TRIX — нисходящий тренд (падение цен).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение нулевой линии снизу вверх — сигнал к покупке (бычий сигнал).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение нулевой линии сверху вниз — сигнал к продаже (медвежий сигнал).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Дивергенции между ценой и TRIX могут предвещать разворот тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Trix<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        TrixImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период задержки (lookback) для индикатора <see cref="Trix{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период сглаживания для экспоненциальной скользящей средней (EMA)</param>
    /// <returns>Количество периодов, необходимых до появления первого валидного значения индикатора.</returns>
    /// <remarks>
    /// Период задержки рассчитывается как: 3 × период EMA + период для 1-дневного ROC.
    /// Все бары с индексом меньше lookback будут пропущены при расчете.
    /// </remarks>
    [PublicAPI]
    public static int TrixLookback(int optInTimePeriod = 30) =>
        optInTimePeriod < 1 ? -1 : EmaLookback(optInTimePeriod) * 3 + RocRLookback(1);

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Trix<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        TrixImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode TrixImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода сглаживания
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет периода задержки для одной EMA
        var emaLookback = EmaLookback(optInTimePeriod);
        // Общий период задержки для тройного сглаживания + ROC
        var lookbackTotal = TrixLookback(optInTimePeriod);
        // Корректировка начального индекса с учетом периода задержки
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после корректировки нет данных для расчета — выход с успешным статусом (пустой результат)
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс первого валидного значения в выходных данных
        var outBegIdx = startIdx;
        // Количество элементов для промежуточного буфера (включая период задержки)
        var nbElementToOutput = endIdx - startIdx + 1 + lookbackTotal;
        // Промежуточный буфер для хранения результатов тройного сглаживания
        Span<T> tempBuffer = new T[nbElementToOutput];

        // Коэффициент сглаживания для EMA: k = 2 / (Period + 1)
        var k = FunctionHelpers.Two<T>() / (T.CreateChecked(optInTimePeriod) + T.One);

        // Первое сглаживание: расчет EMA от исходных данных
        var retCode = FunctionHelpers.CalcExponentialMA(inReal, new Range(startIdx - lookbackTotal, endIdx), tempBuffer, out var range,
            optInTimePeriod, k);
        if (retCode != Core.RetCode.Success || range.End.Value == 0)
        {
            return retCode;
        }

        // Подготовка к второму сглаживанию
        nbElementToOutput--;

        // Второе сглаживание: расчет EMA от первой EMA
        nbElementToOutput -= emaLookback;
        retCode = FunctionHelpers.CalcExponentialMA(tempBuffer, Range.EndAt(nbElementToOutput), tempBuffer, out range, optInTimePeriod, k);
        if (retCode != Core.RetCode.Success || range.End.Value == 0)
        {
            return retCode;
        }

        // Третье сглаживание: расчет EMA от второй EMA
        nbElementToOutput -= emaLookback;
        retCode = FunctionHelpers.CalcExponentialMA(tempBuffer, Range.EndAt(nbElementToOutput), tempBuffer, out range, optInTimePeriod, k);
        if (retCode != Core.RetCode.Success || range.End.Value == 0)
        {
            return retCode;
        }

        // Расчет 1-дневного темпа изменения (Rate-Of-Change) от тройной сглаженной EMA
        nbElementToOutput -= emaLookback;
        retCode = RocImpl(tempBuffer, Range.EndAt(nbElementToOutput), outReal, out range, 1);
        if (retCode != Core.RetCode.Success || range.End.Value == 0)
        {
            return retCode;
        }

        // Формирование выходного диапазона валидных значений
        outRange = new Range(outBegIdx, outBegIdx + range.End.Value - range.Start.Value);

        return Core.RetCode.Success;
    }
}
