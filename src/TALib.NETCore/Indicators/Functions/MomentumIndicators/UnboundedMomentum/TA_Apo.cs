// Название файла: TA_Apo.cs
// Рекомендуемые категории для размещения файла:
// MomentumIndicators (основная категория - идеальное соответствие)
// OverlapStudies (альтернатива ≥60% - использует скользящие средние как базу)
// TrendIndicators (альтернатива ≥55% - помогает определять направление тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Absolute Price Oscillator (Momentum Indicators) — Абсолютный ценовой осциллятор (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входной временной ряд цен (обычно Close) для расчета осциллятора</param>
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
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней (Fast MA)</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней (Slow MA)</param>
    /// <param name="optInMAType">Тип скользящей средней (SMA, EMA, WMA и др.)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Абсолютный ценовой осциллятор (APO) — это индикатор импульса, который измеряет разницу между двумя скользящими средними
    /// (обычно рассчитанными на ценах закрытия <c>Close</c>). В отличие от процентного осциллятора (PPO), APO выражает разницу
    /// в абсолютных единицах цены.
    /// </para>
    /// <para>
    /// <b>Формула расчета:</b>
    /// <code>
    /// APO = FastMA(inReal, optInFastPeriod) - SlowMA(inReal, optInSlowPeriod)
    /// </code>
    /// где FastMA — быстрая скользящая средняя, SlowMA — медленная скользящая средняя.
    /// </para>
    /// <para>
    /// <b>Интерпретация сигналов:</b>
    /// <list type="bullet">
    ///   <item><description>Значение выше нуля: быстрая MA выше медленной → восходящий импульс</description></item>
    ///   <item><description>Значение ниже нуля: быстрая MA ниже медленной → нисходящий импульс</description></item>
    ///   <item><description>Пересечение нулевой линии снизу вверх: потенциальный сигнал покупки</description></item>
    ///   <item><description>Пересечение нулевой линии сверху вниз: потенциальный сигнал продажи</description></item>
    ///   <item><description>Расширение разницы между MA: усиление тренда</description></item>
    ///   <item><description>Сближение MA: ослабление импульса, возможный разворот</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Типичные параметры:</b> Fast Period = 12, Slow Period = 26 (стандартные настройки, заимствованные из MACD)
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Apo<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        ApoImpl(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback period) для индикатора <see cref="Apo{T}"/>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней (Fast MA)</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней (Slow MA)</param>
    /// <param name="optInMAType">Тип скользящей средней</param>
    /// <returns>
    /// Количество баров, необходимых до появления первого валидного значения индикатора.
    /// Рассчитывается как максимальный период скользящей средней (медленный период) минус 1.
    /// Возвращает -1 при некорректных входных параметрах (периоды меньше 2).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Период lookback определяет, сколько начальных баров входных данных будут пропущены
    /// при расчете индикатора, так как для их обработки недостаточно исторических данных.
    /// </para>
    /// <para>
    /// Для APO lookback period равен lookback period медленной скользящей средней,
    /// так как именно она требует больше исторических данных для расчета первого значения.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int ApoLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : MaLookback(Math.Max(optInSlowPeriod, optInFastPeriod), optInMAType);

    /// <summary>
    /// Внутренняя реализация индикатора APO для совместимости с устаревшим API (массивы вместо Span).
    /// </summary>
    /// <remarks>
    /// Для совместимости с абстрактным API. Рекомендуется использовать версию метода с Span для лучшей производительности.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Apo<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        ApoImpl<T>(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

    /// <summary>
    /// Внутренняя реализация расчета абсолютного ценового осциллятора (APO).
    /// </summary>
    private static Core.RetCode ApoImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона значением [0, -1] (пустой диапазон) на случай ошибки
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и длины массива
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периодов: оба периода должны быть >= 2
        if (optInFastPeriod < 2 || optInSlowPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Выделение временного буфера для хранения промежуточных значений скользящих средних
        // Размер буфера соответствует количеству обрабатываемых баров во входном диапазоне
        Span<T> tempBuffer = new T[endIdx - startIdx + 1];

        // Вызов вспомогательного метода для расчета ценового осциллятора
        // Параметр 'false' указывает на абсолютный тип осциллятора (в отличие от процентного PPO)
        return FunctionHelpers.CalcPriceOscillator(inReal, new Range(startIdx, endIdx), outReal, out outRange, optInFastPeriod,
            optInSlowPeriod, optInMAType, tempBuffer, false);
    }
}
