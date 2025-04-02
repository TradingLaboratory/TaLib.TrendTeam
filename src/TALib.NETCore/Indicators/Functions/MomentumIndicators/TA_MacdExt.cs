//Название файла: TA_MacdExt.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - идеальное соответствие категории)
//TrendFollowing (альтернатива, если требуется группировка по типу индикатора)
//CustomizableIndicators (альтернатива для акцента на возможности настройки)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MACD with controllable MA type (Momentum Indicators) — MACD с настраиваемым типом скользящего среднего (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outMACD">
    /// Массив, содержащий ТОЛЬКО валидные значения линии MACD.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMACD[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outMACDSignal">
    /// Массив, содержащий ТОЛЬКО валидные значения линии Signal.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMACDSignal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outMACDHist">
    /// Массив, содержащий ТОЛЬКО валидные значения гистограммы MACD.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMACDHist[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMACD"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней.</param>
    /// <param name="optInFastMAType">Тип скользящей средней, используемый для быстрой скользящей средней.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней.</param>
    /// <param name="optInSlowMAType">Тип скользящей средней, используемый для медленной скользящей средней.</param>
    /// <param name="optInSignalPeriod">Период для расчета линии Signal.</param>
    /// <param name="optInSignalMAType">Тип скользящей средней, используемый для линии Signal.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Расширенная Сходимость-Расходимость Скользящих Средних (MACD) позволяет настраивать типы скользящих средних
    /// для расчета линии MACD, линии Signal и гистограммы MACD. Эта гибкость позволяет адаптировать индикатор
    /// к различным рыночным условиям и стратегиям анализа.
    /// <para>
    /// Выбор типов скользящих средних (например, SMA, EMA) для быстрой, медленной и линии Signal влияет на чувствительность индикатора.
    /// Функция может быть адаптирована для определенных активов или условий волатильности. Комбинирование с индикаторами объема
    /// может усилить сигналы, основанные на импульсе.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитать медленную скользящую среднюю с использованием указанных <paramref name="optInSlowPeriod"/> и <paramref name="optInSlowMAType"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать быструю скользящую среднюю с использованием указанных <paramref name="optInFastPeriod"/> и <paramref name="optInFastMAType"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать линию MACD как разницу между быстрой и медленной скользящими средними:
    ///       <code>
    ///         MACD = FastMA - SlowMA
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать линию Signal как скользящую среднюю линии MACD с использованием указанных <paramref name="optInSignalPeriod"/>
    ///       и <paramref name="optInSignalMAType"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитать гистограмму MACD как разницу между линией MACD и линией Signal:
    ///       <code>
    ///         MACDHist = MACD - Signal
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Положительное значение линии MACD указывает на восходящий импульс, отрицательное — на нисходящий.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Линия Signal используется для идентификации потенциальных сигналов на покупку или продажу. Бычий перекрест происходит, когда линия MACD
    ///       пересекает линию Signal снизу вверх, медвежий перекрест — когда линия MACD пересекает линию Signal сверху вниз.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Гистограмма MACD отражает силу импульса: большие столбцы указывают на сильный импульс в направлении линии MACD, уменьшающиеся столбцы могут сигнализировать о возможном развороте или ослаблении импульса.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MacdExt<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInFastPeriod = 12,
        Core.MAType optInFastMAType = Core.MAType.Sma,
        int optInSlowPeriod = 26,
        Core.MAType optInSlowMAType = Core.MAType.Sma,
        int optInSignalPeriod = 9,
        Core.MAType optInSignalMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MacdExtImpl(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInFastMAType, optInSlowPeriod,
            optInSlowMAType, optInSignalPeriod, optInSignalMAType);

    /// <summary>
    /// Возвращает период lookback для <see cref="MacdExt{T}">MacdExt</see>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней.</param>
    /// <param name="optInFastMAType">Тип скользящей средней, используемый для быстрой скользящей средней.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней.</param>
    /// <param name="optInSlowMAType">Тип скользящей средней, используемый для медленной скользящей средней.</param>
    /// <param name="optInSignalPeriod">Период для расчета линии Signal.</param>
    /// <param name="optInSignalMAType">Тип скользящей средней, используемый для линии Signal.</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения.</returns>
    [PublicAPI]
    public static int MacdExtLookback(
        int optInFastPeriod = 12,
        Core.MAType optInFastMAType = Core.MAType.Sma,
        int optInSlowPeriod = 26,
        Core.MAType optInSlowMAType = Core.MAType.Sma,
        int optInSignalPeriod = 9,
        Core.MAType optInSignalMAType = Core.MAType.Sma)
    {
        // Проверка корректности периодов
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return -1;
        }

        // Определение максимального lookback периода
        var lookbackLargest = MaLookback(optInFastPeriod, optInFastMAType);
        var tempInteger = MaLookback(optInSlowPeriod, optInSlowMAType);
        if (tempInteger > lookbackLargest)
        {
            lookbackLargest = tempInteger;
        }

        // Возвращаем суммарный lookback период
        return lookbackLargest + MaLookback(optInSignalPeriod, optInSignalMAType);
    }

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MacdExt<T>(
        T[] inReal,
        Range inRange,
        T[] outMACD,
        T[] outMACDSignal,
        T[] outMACDHist,
        out Range outRange,
        int optInFastPeriod = 12,
        Core.MAType optInFastMAType = Core.MAType.Sma,
        int optInSlowPeriod = 26,
        Core.MAType optInSlowMAType = Core.MAType.Sma,
        int optInSignalPeriod = 9,
        Core.MAType optInSignalMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MacdExtImpl<T>(inReal, inRange, outMACD, outMACDSignal, outMACDHist, out outRange, optInFastPeriod, optInFastMAType,
            optInSlowPeriod, optInSlowMAType, optInSignalPeriod, optInSignalMAType);

    private static Core.RetCode MacdExtImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMACD,
        Span<T> outMACDSignal,
        Span<T> outMACDHist,
        out Range outRange,
        int optInFastPeriod,
        Core.MAType optInFastMAType,
        int optInSlowPeriod,
        Core.MAType optInSlowMAType,
        int optInSignalPeriod,
        Core.MAType optInSignalMAType) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периодов
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Убедимся, что медленный период действительно медленнее быстрого. Если нет, меняем их местами.
        if (optInSlowPeriod < optInFastPeriod)
        {
            (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
            (optInSlowMAType, optInFastMAType) = (optInFastMAType, optInSlowMAType);
        }

        // Добавляем lookback, необходимый для линии Signal
        var lookbackSignal = MaLookback(optInSignalPeriod, optInSignalMAType);
        var lookbackTotal = MacdExtLookback(optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod,
            optInSignalMAType);

        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделяем промежуточный буфер для быстрой и медленной скользящей средней
        var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
        Span<T> fastMABuffer = new T[tempInteger];
        Span<T> slowMABuffer = new T[tempInteger];

        /* Рассчитываем медленную скользящую среднюю.
         *
         * Сдвигаем startIdx назад, чтобы получить достаточно данных для периода Signal.
         * Таким образом, после расчета Signal, все выходные данные начнутся с запрашиваемого 'startIdx'.
         */
        tempInteger = startIdx - lookbackSignal;
        var retCode = MaImpl(inReal, new Range(tempInteger, endIdx), slowMABuffer, out var outRange1, optInSlowPeriod, optInSlowMAType);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Рассчитываем быструю скользящую среднюю
        retCode = MaImpl(inReal, new Range(tempInteger, endIdx), fastMABuffer, out _, optInFastPeriod, optInFastMAType);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement1 = outRange1.End.Value - outRange1.Start.Value;
        // Рассчитываем разницу между быстрой и медленной скользящей средней
        for (var i = 0; i < nbElement1; i++)
        {
            fastMABuffer[i] -= slowMABuffer[i];
        }

        // Копируем результат в выходной массив
        fastMABuffer.Slice(lookbackSignal, endIdx - startIdx + 1).CopyTo(outMACD);

        // Рассчитываем линию Signal
        retCode = MaImpl(fastMABuffer, Range.EndAt(nbElement1 - 1), outMACDSignal, out var outRange2, optInSignalPeriod, optInSignalMAType);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement2 = outRange2.End.Value - outRange2.Start.Value;
        // Рассчитываем гистограмму MACD
        for (var i = 0; i < nbElement2; i++)
        {
            outMACDHist[i] = outMACD[i] - outMACDSignal[i];
        }

        outRange = new Range(startIdx, startIdx + nbElement2);

        return Core.RetCode.Success;
    }
}
