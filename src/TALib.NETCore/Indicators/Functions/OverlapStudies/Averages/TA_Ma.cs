//Название файла: TA_Ma.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по типу вычислений)
//TrendIndicators (альтернатива для акцента на трендовых индикаторах)
//Рекомендуемая подпапка: Averages

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Moving Average (Overlap Studies) — Скользящее среднее (Исследования перекрытий)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// </param>
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
    /// Диапазон, указывающий для каких ячеек входных данных для расчёта посчитаны валидные значения индикаторов 
    /// (индексы первой и последней ячейки во входных данных):
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/> (lookback).
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени (количество баров) для расчета среднего.</param>
    /// <param name="optInMAType">Тип алгоритма скользящего среднего (SMA, EMA, WMA и т.д.).</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция скользящего среднего вычисляет среднее значение серии данных за указанный период,
    /// сглаживая колебания для выявления трендов. Поддерживаются различные типы скользящих средних,
    /// что позволяет использовать разные методы сглаживания в зависимости от аналитических потребностей.
    ///
    /// <para>
    /// <b>Поддерживаемые типы скользящих средних</b> через <paramref name="optInMAType"/>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <see cref="Sma{T}">SMA</see> (Простое скользящее среднее): Простое среднее значений за указанный период.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Ema{T}">EMA</see> (Экспоненциальное скользящее среднее): Присваивает больший вес последним данным,
    ///       что делает его более чувствительным к изменениям.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Wma{T}">WMA</see> (Взвешенное скользящее среднее): Взвешивает данные линейно,
    ///       присваивая наибольший вес последним данным.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Dema{T}">DEMA</see> (Двойное экспоненциальное скользящее среднее): Композитное среднее,
    ///       предназначенное для уменьшения запаздывания.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Tema{T}">TEMA</see> (Тройное экспоненциальное скользящее среднее): Еще больше уменьшает запаздывание,
    ///       сглаживая данные.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Trima{T}">TRIMA</see> (Треугольное скользящее среднее): Метод двойного сглаживания.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Kama{T}">KAMA</see> (Адаптивное скользящее среднее Кауфмана): Корректирует чувствительность на основе волатильности рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="Mama{T}">MAMA</see> (Адаптивное скользящее среднее MESA): Адаптируется к рыночным циклам.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="T3{T}">T3</see> (Скользящее среднее T3): Гладкое скользящее среднее с уменьшенным запаздыванием.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Восходящее скользящее среднее указывает на восходящий тренд.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Нисходящее скользящее среднее указывает на нисходящий тренд.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечения различных скользящих средних (например, краткосрочных и долгосрочных) могут указывать на сигналы покупки или продажи.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Ma<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MaImpl(inReal, inRange, outReal, out outRange, optInTimePeriod, optInMAType);

    /// <summary>
    /// Возвращает количество периодов lookback для <see cref="Ma{T}">Ma</see> 
    /// (индекс первого бара во входящих данных, для которого можно будет получить валидное значение).
    /// </summary>
    /// <param name="optInTimePeriod">Период времени (количество баров) для расчета среднего.</param>
    /// <param name="optInMAType">Тип алгоритма скользящего среднего (SMA, EMA, WMA и т.д.).</param>
    /// <returns>Количество периодов, необходимых до первого вычисленного значения (lookback).</returns>
    [PublicAPI]
    public static int MaLookback(int optInTimePeriod = 30, Core.MAType optInMAType = Core.MAType.Sma) =>
        optInTimePeriod switch
        {
            < 1 => -1,
            1 => 0,
            _ => optInMAType switch
            {
                Core.MAType.Sma => SmaLookback(optInTimePeriod),
                Core.MAType.Ema => EmaLookback(optInTimePeriod),
                Core.MAType.Wma => WmaLookback(optInTimePeriod),
                Core.MAType.Dema => DemaLookback(optInTimePeriod),
                Core.MAType.Tema => TemaLookback(optInTimePeriod),
                Core.MAType.Trima => TrimaLookback(optInTimePeriod),
                Core.MAType.Kama => KamaLookback(optInTimePeriod),
                Core.MAType.Mama => MamaLookback(),
                Core.MAType.T3 => T3Lookback(optInTimePeriod),
                _ => 0
            }
        };

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Ma<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30,
        Core.MAType optInMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        MaImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod, optInMAType);

    private static Core.RetCode MaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod,
        Core.MAType optInMAType) where T : IFloatingPointIeee754<T>
    {
        // Инициализация диапазона вывода пустым значением (по умолчанию конец диапазона равен 0)
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона и получение индексов начала и конца
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение индексов начала и конца диапазона обработки
        var (startIdx, endIdx) = rangeIndices;

        // Проверка валидности периода (должен быть больше 0)
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Оптимизация для периода 1: значение индикатора равно входному значению
        if (optInTimePeriod == 1)
        {
            // Количество элементов для обработки
            var nbElement = endIdx - startIdx + 1;
            for (int todayIdx = startIdx, outIdx = 0; outIdx < nbElement; outIdx++, todayIdx++)
            {
                outReal[outIdx] = inReal[todayIdx];
            }

            // Установка диапазона вывода: от startIdx до startIdx + количество элементов
            outRange = new Range(startIdx, startIdx + nbElement);

            return Core.RetCode.Success;
        }

        // Делегирование расчета конкретному типу скользящего среднего в зависимости от optInMAType
        switch (optInMAType)
        {
            case Core.MAType.Sma:
                return Sma(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Ema:
                return Ema(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Wma:
                return Wma(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Dema:
                return Dema(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Tema:
                return Tema(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Trima:
                return Trima(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Kama:
                return Kama(inReal, inRange, outReal, out outRange, optInTimePeriod);
            case Core.MAType.Mama:
                // Временный буфер для метода Mama (требуется дополнительный выходной параметр, который здесь не используется)
                Span<T> dummyBuffer = new T[endIdx - startIdx + 1];
                return Mama(inReal, inRange, outReal, dummyBuffer, out outRange);
            case Core.MAType.T3:
                return T3(inReal, inRange, outReal, out outRange, optInTimePeriod);
            default:
                return Core.RetCode.BadParam;
        }
    }
}
