//Название файла: TA_MacdExt.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (существующая папка - основная категория)
//Oscillators (подпапка внутри MomentumIndicators - идеальное соответствие)
//TrendFollowing (альтернатива для акцента на трендовой составляющей)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MACD with controllable MA type (Momentum Indicators) — MACD с настраиваемым типом скользящей средней (Индикаторы импульса)
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
    /// Массив, содержащий ТОЛЬКО валидные значения сигнальной линии Signal.  
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
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней (Fast MA).</param>
    /// <param name="optInFastMAType">Тип скользящей средней для быстрой линии (например, SMA, EMA).</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней (Slow MA).</param>
    /// <param name="optInSlowMAType">Тип скользящей средней для медленной линии (например, SMA, EMA).</param>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии Signal.</param>
    /// <param name="optInSignalMAType">Тип скользящей средней для сигнальной линии Signal.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Расширенная версия индикатора Схождения-Расхождения Скользящих Средних (MACD — Moving Average Convergence Divergence)
    /// позволяет гибко настраивать типы скользящих средних для всех трех компонентов индикатора: быстрой линии (Fast MA),
    /// медленной линии (Slow MA) и сигнальной линии (Signal). Это дает возможность адаптировать чувствительность индикатора
    /// под различные рыночные условия и торговые стратегии.
    /// </para>
    /// <para>
    /// <b>Этапы расчета:</b>
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Рассчитывается медленная скользящая средняя (Slow MA) с периодом <paramref name="optInSlowPeriod"/>
    ///       и типом <paramref name="optInSlowMAType"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитывается быстрая скользящая средняя (Fast MA) с периодом <paramref name="optInFastPeriod"/>
    ///       и типом <paramref name="optInFastMAType"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Линия MACD вычисляется как разница между быстрой и медленной скользящими средними:
    ///       <code>
    ///         MACD = FastMA - SlowMA
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сигнальная линия (Signal) рассчитывается как скользящая средняя от линии MACD
    ///       с периодом <paramref name="optInSignalPeriod"/> и типом <paramref name="optInSignalMAType"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Гистограмма MACD вычисляется как разница между линией MACD и сигнальной линией:
    ///       <code>
    ///         MACDHist = MACD - Signal
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Интерпретация сигналов:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Пересечение линии MACD и сигнальной линии снизу вверх (бычий кросс) — потенциальный сигнал на покупку.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение линии MACD и сигнальной линии сверху вниз (медвежий кросс) — потенциальный сигнал на продажу.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Расширение гистограммы (увеличение столбцов) указывает на усиление импульса в текущем направлении.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сжатие гистограммы (уменьшение столбцов) может сигнализировать об ослаблении импульса или подготовке к развороту.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение линии MACD нулевой линии вверх — подтверждение восходящего тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечение линии MACD нулевой линии вниз — подтверждение нисходящего тренда.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Особенности настройки:</b>
    /// Выбор типа скользящей средней (SMA, EMA, WMA и др.) для каждой компоненты позволяет тонко настраивать
    /// реакцию индикатора на рыночные изменения. Например, использование EMA делает индикатор более чувствительным
    /// к недавним ценовым движениям, в то время как SMA обеспечивает более сглаженные сигналы.
    /// </para>
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
    /// Возвращает период lookback (минимальное количество баров, необходимых для расчета первого валидного значения)
    /// для индикатора <see cref="MacdExt{T}"/>.
    /// </summary>
    /// <param name="optInFastPeriod">Период для расчета быстрой скользящей средней (Fast MA).</param>
    /// <param name="optInFastMAType">Тип скользящей средней для быстрой линии.</param>
    /// <param name="optInSlowPeriod">Период для расчета медленной скользящей средней (Slow MA).</param>
    /// <param name="optInSlowMAType">Тип скользящей средней для медленной линии.</param>
    /// <param name="optInSignalPeriod">Период для расчета сигнальной линии Signal.</param>
    /// <param name="optInSignalMAType">Тип скользящей средней для сигнальной линии.</param>
    /// <returns>
    /// Количество периодов, необходимых до первого вычисленного значения индикатора.
    /// Возвращает -1 при некорректных входных параметрах.
    /// </returns>
    [PublicAPI]
    public static int MacdExtLookback(
        int optInFastPeriod = 12,
        Core.MAType optInFastMAType = Core.MAType.Sma,
        int optInSlowPeriod = 26,
        Core.MAType optInSlowMAType = Core.MAType.Sma,
        int optInSignalPeriod = 9,
        Core.MAType optInSignalMAType = Core.MAType.Sma)
    {
        // Проверка корректности входных периодов: быстрый и медленный периоды должны быть >= 2, период сигнала >= 1
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return -1;
        }

        // Определение максимального lookback периода среди быстрой и медленной скользящих средних
        var lookbackLargest = MaLookback(optInFastPeriod, optInFastMAType);
        var tempInteger = MaLookback(optInSlowPeriod, optInSlowMAType);
        if (tempInteger > lookbackLargest)
        {
            lookbackLargest = tempInteger;
        }

        // Общий lookback период = максимальный период быстрой/медленной MA + период сигнальной линии
        return lookbackLargest + MaLookback(optInSignalPeriod, optInSignalMAType);
    }

    /// <remarks>
    /// Приватный метод для совместимости с абстрактным API (работа с массивами вместо Span)
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

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периодов скользящих средних
        if (optInFastPeriod < 2 || optInSlowPeriod < 2 || optInSignalPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Обеспечение корректного соотношения периодов: медленный период должен быть больше быстрого
        if (optInSlowPeriod < optInFastPeriod)
        {
            (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
            (optInSlowMAType, optInFastMAType) = (optInFastMAType, optInSlowMAType);
        }

        // Расчет необходимого сдвига для получения валидных значений сигнальной линии
        var lookbackSignal = MaLookback(optInSignalPeriod, optInSignalMAType);
        var lookbackTotal = MacdExtLookback(optInFastPeriod, optInFastMAType, optInSlowPeriod, optInSlowMAType, optInSignalPeriod,
            optInSignalMAType);

        // Сдвиг начального индекса с учетом общего lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после сдвига начальный индекс превышает конечный — валидных данных нет
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделение временных буферов для хранения промежуточных результатов быстрой и медленной скользящих средних
        var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
        Span<T> fastMABuffer = new T[tempInteger];
        Span<T> slowMABuffer = new T[tempInteger];

        /* Расчет медленной скользящей средней (Slow MA).
         *
         * Сдвигаем начальный индекс назад на величину lookbackSignal, чтобы получить достаточно данных
         * для последующего расчета сигнальной линии. Это гарантирует, что первое валидное значение
         * в выходном массиве будет соответствовать запрошенному начальному индексу (startIdx).
         */
        tempInteger = startIdx - lookbackSignal;
        var retCode = MaImpl(inReal, new Range(tempInteger, endIdx), slowMABuffer, out var outRange1, optInSlowPeriod, optInSlowMAType);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Расчет быстрой скользящей средней (Fast MA) на том же расширенном диапазоне данных
        retCode = MaImpl(inReal, new Range(tempInteger, endIdx), fastMABuffer, out _, optInFastPeriod, optInFastMAType);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Вычисление линии MACD как разницы между быстрой и медленной скользящими средними
        var nbElement1 = outRange1.End.Value - outRange1.Start.Value;
        for (var i = 0; i < nbElement1; i++)
        {
            fastMABuffer[i] -= slowMABuffer[i]; // MACD = FastMA - SlowMA
        }

        // Копирование рассчитанной линии MACD в выходной массив (пропуская данные, необходимые только для расчета сигнала)
        fastMABuffer.Slice(lookbackSignal, endIdx - startIdx + 1).CopyTo(outMACD);

        // Расчет сигнальной линии (Signal) как скользящей средней от линии MACD
        retCode = MaImpl(fastMABuffer, Range.EndAt(nbElement1 - 1), outMACDSignal, out var outRange2, optInSignalPeriod, optInSignalMAType);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Расчет гистограммы MACD как разницы между линией MACD и сигнальной линией
        var nbElement2 = outRange2.End.Value - outRange2.Start.Value;
        for (var i = 0; i < nbElement2; i++)
        {
            outMACDHist[i] = outMACD[i] - outMACDSignal[i]; // MACDHist = MACD - Signal
        }

        // Формирование выходного диапазона с валидными значениями
        outRange = new Range(startIdx, startIdx + nbElement2);

        return Core.RetCode.Success;
    }
}
