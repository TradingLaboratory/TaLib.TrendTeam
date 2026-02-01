// Название файла: Ppo.cs
// Группы, к которым можно отнести индикатор:
// MomentumIndicators (существующая папка - идеальное соответствие категории)
// PriceTransform (альтернатива, так как преобразует ценовой ряд в процентное отношение)
// TrendIndicators (альтернатива, так как помогает идентифицировать тренды через скользящие средние)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Percentage Price Oscillator (Momentum Indicators) — Осциллятор процентной цены (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (обычно цены Close)</param>
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
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней (FastMA)</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней (SlowMA)</param>
    /// <param name="optInMAType">Тип скользящей средней (SMA, EMA, WMA и др.)</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор Percentage Price Oscillator (PPO) рассчитывает процентную разницу между двумя скользящими средними
    /// временного ряда. Используется для идентификации трендов и оценки силы ценовых движений.
    /// </para>
    /// <para>
    /// Функция аналогична индикатору <see cref="Macd{T}">MACD</see>, но выражена в процентах,
    /// что упрощает сравнение между различными инструментами. PPO обеспечивает нормализованный анализ импульса.
    /// Комбинация с индикаторами <see cref="Rsi{T}">RSI</see>, <see cref="Bbands{T}">Bollinger Bands</see>
    /// или объемными индикаторами повышает надежность сигналов, основанных на импульсе.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет быстрой и медленной скользящих средних (MA) с использованием указанных
    ///       <paramref name="optInFastPeriod"/> и <paramref name="optInSlowPeriod"/>:
    /// <code>
    /// FastMA = MA(data, optInFastPeriod, optInMAType)
    /// SlowMA = MA(data, optInSlowPeriod, optInMAType)
    /// </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисление PPO как процентной разницы между быстрой и медленной скользящими средними:
    ///       <code>
    ///         PPO = ((FastMA - SlowMA) / SlowMA) * 100
    ///       </code>
    ///       где <c>FastMA</c> и <c>SlowMA</c> — скользящие средние, рассчитанные на предыдущем шаге.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительные значения указывают, что быстрая скользящая средняя находится выше медленной,
    ///       сигнализируя о восходящем импульсе.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Отрицательные значения указывают, что быстрая скользящая средняя находится ниже медленной,
    ///       сигнализируя о нисходящем импульсе.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более высокие абсолютные значения свидетельствуют о более сильном импульсе в соответствующем направлении.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Ppo<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        PpoImpl(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

    /// <summary>
    /// Возвращает период задержки (lookback) для индикатора <see cref="Ppo{T}">PPO</see>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней (FastMA)</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней (SlowMA)</param>
    /// <param name="optInMAType">Тип скользящей средней</param>
    /// <returns>Количество периодов, необходимых до первого валидного значения индикатора.</returns>
    [PublicAPI]
    public static int PpoLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInFastPeriod < 2 || optInSlowPeriod < 2 ? -1 : MaLookback(Math.Max(optInSlowPeriod, optInFastPeriod), optInMAType);

    /// <remarks>
    /// Для обеспечения совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Ppo<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInFastPeriod = 12,
        int optInSlowPeriod = 26,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        PpoImpl<T>(inReal, inRange, outReal, out outRange, optInFastPeriod, optInSlowPeriod, optInMAType);

    private static Core.RetCode PpoImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона как пустого (индекс конца < индекса начала)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и получение индексов начала и конца обработки
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности входных параметров периодов (минимум 2 периода для каждой MA)
        if (optInFastPeriod < 2 || optInSlowPeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Выделение временного буфера для промежуточных расчетов скользящих средних
        Span<T> tempBuffer = new T[endIdx - startIdx + 1];

        // Вызов вспомогательного метода для расчета осциллятора на основе разницы скользящих средних
        // Параметр 'true' указывает на расчет процентной разницы (в отличие от абсолютной)
        return FunctionHelpers.CalcPriceOscillator(inReal, new Range(startIdx, endIdx), outReal, out outRange, optInFastPeriod,
            optInSlowPeriod, optInMAType, tempBuffer, true);
    }
}
