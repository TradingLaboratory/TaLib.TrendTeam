//Название файла: TA_Mama.cs
//Группы к которым можно отнести индикатор:
//OverlapStudies (существующая папка - идеальное соответствие категории)
//AdaptiveIndicators (альтернатива, если требуется группировка по типу индикатора)
//TrendFollowing (альтернатива для акцента на следовании тренду)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MESA Adaptive Moving Average (Overlap Studies) — Адаптивное скользящее среднее MESA (Индикаторы наложения)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// Обычно используются цены <see cref="Close">Close</see> (цены закрытия).
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет начальную и конечную точки для вычисления индикатора.
    /// </para>
    /// </param>
    /// <param name="outMAMA">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора MAMA (MESA Adaptive Moving Average).
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outMAMA[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - MAMA — основная адаптивная линия, отслеживающая тренд с минимальной задержкой.
    /// </para>
    /// </param>
    /// <param name="outFAMA">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора FAMA (Following Adaptive Moving Average).
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outFAMA[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - FAMA — сглаженная версия MAMA, выступает в роли сигнальной линии.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMAMA"/> и <paramref name="outFAMA"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outMAMA"/> и <paramref name="outFAMA"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <param name="optInFastLimit">
    /// Верхняя граница для адаптивного фактора (Fast Limit):
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения увеличивают чувствительность к изменениям цен, делая MAMA более восприимчивым к рыночным трендам.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения снижают чувствительность, сглаживая выходные данные MAMA.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Допустимый диапазон: <c>0.01..0.99</c>. Значения ближе к 0.99 максимально повышают чувствительность, тогда как значения ближе к 0.01 приоритизируют сглаживание.
    /// Значение по умолчанию: <c>0.5</c>.
    /// </para>
    /// </param>
    /// <param name="optInSlowLimit">
    /// Нижняя граница для адаптивного фактора (Slow Limit):
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Более высокие значения снижают минимальную чувствительность, добавляя стабильности MAMA во время консолидации рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Более низкие значения увеличивают минимальную чувствительность, позволяя быстрее реагировать на изменения рынка.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Допустимый диапазон: <c>0.01..0.99</c>. Значения ближе к 0.99 снижают шум, тогда как значения ближе к 0.01 позволяют большую гибкость.
    /// Значение по умолчанию: <c>0.05</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Адаптивное скользящее среднее MESA (MESA Adaptive Moving Average) динамически настраивает свою чувствительность 
    /// на основе доминирующего цикла на рынке. Оно использует комбинацию преобразования Хилберта и альфа-фактора 
    /// для адаптации к изменяющимся рыночным условиям, производя два выхода: MAMA и FAMA (Following Adaptive Moving Average).
    /// <para>
    /// Адаптивность функции позволяет ей быстро реагировать на тренды, минимизируя ложные сигналы в фазах консолидации.
    /// Комбинирование с <see cref="Adx{T}">ADX</see>, <see cref="Rsi{T}">RSI</see>,
    /// или мерами волатильности, такими как <see cref="Atr{T}">ATR</see>, может уточнить разработку стратегии.
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Применяется взвешенное скользящее среднее (WMA) для сглаживания входных цен и уменьшения шума.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Выполняется преобразование Хилберта на сглаженных данных для извлечения согласованных (I) и квадратурных (Q) компонентов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычисляется доминирующий период цикла на основе фазовых различий между последовательными согласованными и квадратурными значениями.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Рассчитывается альфа-фактор с использованием быстрых и медленных ограничений, который определяет уровень чувствительности:
    ///       <code>
    ///         Alpha = FastLimit / DeltaPhase
    ///       </code>
    ///       Корректировки проводятся для обеспечения того, чтобы альфа оставалась в пределах, определенных медленными и быстрыми ограничениями.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Обновляется MAMA с использованием текущей цены и альфа, и рассчитывается FAMA как сглаженная версия MAMA.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <i>MAMA</i> отслеживает доминирующий тренд с минимальной задержкой.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <i>FAMA</i> обеспечивает дополнительное сглаживание, выступая в роли сигнальной линии для идентификации изменений тренда.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Пересечения между MAMA и FAMA могут указывать на потенциальные сигналы покупки или продажи: 
    ///       бычье пересечение происходит, когда MAMA пересекает FAMA снизу вверх, 
    ///       а медвежье пересечение — когда MAMA пересекает FAMA сверху вниз.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Mama<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMAMA,
        Span<T> outFAMA,
        out Range outRange,
        double optInFastLimit = 0.5,
        double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
        MamaImpl(inReal, inRange, outMAMA, outFAMA, out outRange, optInFastLimit, optInSlowLimit);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback period) для <see cref="Mama{T}">Mama</see>.
    /// </summary>
    /// <returns>
    /// Количество периодов (баров), необходимых до первого вычисленного валидного значения индикатора.
    /// Все бары с индексом меньше этого значения будут пропущены при расчете.
    /// </returns>
    /// <remarks>
    /// Фиксированный период обратного просмотра составляет 32 и устанавливается следующим образом:
    /// <list type="bullet">
    /// <item><description>12 ценовых баров для совместимости с реализацией TradeStation, описанной в книге Джона Элерса.</description></item>
    /// <item><description>6 ценовых баров для <c>Detrender</c> (детрендера)</description></item>
    /// <item><description>6 ценовых баров для <c>Q1</c> (квадратурный компонент 1)</description></item>
    /// <item><description>3 ценовых бара для <c>JI</c> (согласованный компонент J)</description></item>
    /// <item><description>3 ценовых бара для <c>JQ</c> (квадратурный компонент J)</description></item>
    /// <item><description>1 ценовой бар для <c>Re</c>/<c>Im</c> (действительная/мнимая часть)</description></item>
    /// <item><description>1 ценовой бар для <c>Delta Phase</c> (разница фаз)</description></item>
    /// <item><description>————————</description></item>
    /// <item><description>32 всего (итоговый lookback период)</description></item>
    /// </list>
    /// <para>
    /// <b>Примечание</b>: Lookback период обозначает индекс первого бара во входящих данных, 
    /// для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int MamaLookback() => Core.UnstablePeriodSettings.Get(Core.UnstableFunc.Mama) + 32;

    /// <remarks>
    /// Для совместимости с абстрактным API (Abstract API compatibility).
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Mama<T>(
        T[] inReal,
        Range inRange,
        T[] outMAMA,
        T[] outFAMA,
        out Range outRange,
        double optInFastLimit = 0.5,
        double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
        MamaImpl<T>(inReal, inRange, outMAMA, outFAMA, out outRange, optInFastLimit, optInSlowLimit);

    private static Core.RetCode MamaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMAMA,
        Span<T> outFAMA,
        out Range outRange,
        double optInFastLimit,
        double optInSlowLimit) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Валидация параметров FastLimit и SlowLimit (должны быть в диапазоне 0.01..0.99)
        if (optInFastLimit < 0.01 || optInFastLimit > 0.99 || optInSlowLimit < 0.01 || optInSlowLimit > 0.99)
        {
            return Core.RetCode.BadParam;
        }

        // Получение общего периода обратного просмотра (lookback) для индикатора
        var lookbackTotal = MamaLookback();
        // Корректировка начального индекса с учётом lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Сохранение начального индекса для формирования выходного диапазона
        var outBegIdx = startIdx;

        // Инициализация взвешенного скользящего среднего (WMA) для сглаживания входных данных
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        // Индекс для циркулярного буфера преобразования Хилберта
        var hilbertIdx = 0;

        /* Инициализация циркулярного буфера, используемого логикой преобразования Хилберта.
         * Буфер используется для нечетных и четных дней.
         * Это минимизирует количество операций доступа к памяти и операций с плавающей точкой,
         * необходимых для выполнения промежуточных вычислений.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        // Индекс для записи выходных значений MAMA и FAMA
        var outIdx = 0;

        // Переменные для хранения промежуточных значений расчета
        // prevI2, prevQ2 - предыдущие значения I2 и Q2 компонентов преобразования Хилберта
        // re, im - действительная и мнимая части для расчета периода
        // mama, fama - текущие значения индикаторов MAMA и FAMA
        // i1ForOddPrev3, i1ForEvenPrev3 - предыдущие значения I1 для нечетных/четных итераций (3 периода назад)
        // i1ForOddPrev2, i1ForEvenPrev2 - предыдущие значения I1 для нечетных/четных итераций (2 периода назад)
        // prevPhase - предыдущее значение фазы для расчета Delta Phase
        T prevI2, prevQ2, re, im, mama, fama, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, prevPhase;

        // Инициализация всех промежуточных переменных нулевым значением
        var period = prevI2 = prevQ2
            = re = im = mama = fama = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = prevPhase = T.Zero;

        // Код оптимизирован по скорости и может быть сложным для понимания, если вы не знакомы с оригинальным алгоритмом.
        // Основной цикл расчета индикатора MAMA/FAMA
        while (today <= endIdx)
        {
            // Расчет скорректированного предыдущего периода для преобразования Хилберта
            // Формула: adjustedPrevPeriod = 0.075 * period + 0.54
            var adjustedPrevPeriod = T.CreateChecked(0.075) * period + T.CreateChecked(0.54);

            // Получение текущего значения из входных данных
            var todayValue = inReal[today];

            // Расчет взвешенного скользящего среднего (WMA) для сглаживания цены
            FunctionHelpers.DoPriceWma(inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, todayValue,
                out var smoothedValue);

            // Выполнение преобразования Хилберта для получения I2 и Q2 компонентов
            // Возвращает текущее значение фазы (tempReal2)
            var tempReal2 = PerformMAMAHilbertTransform(today, circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod,
                ref i1ForOddPrev3, ref i1ForEvenPrev3, ref i1ForOddPrev2, ref i1ForEvenPrev2, prevQ2, prevI2, out var i2, out var q2);

            // Разница фаз (Delta Phase) помещается в tempReal
            // Формула: DeltaPhase = prevPhase - currentPhase
            var tempReal = prevPhase - tempReal2;
            prevPhase = tempReal2;

            // Ограничение минимального значения Delta Phase единицей
            if (tempReal < T.One)
            {
                tempReal = T.One;
            }

            // Расчет альфа-фактора (коэффициента адаптивности)
            // Альфа помещается в tempReal
            if (tempReal > T.One)
            {
                // Формула: Alpha = FastLimit / DeltaPhase
                tempReal = T.CreateChecked(optInFastLimit) / tempReal;

                // Ограничение альфа снизу значением SlowLimit
                if (tempReal < T.CreateChecked(optInSlowLimit))
                {
                    tempReal = T.CreateChecked(optInSlowLimit);
                }
            }
            else
            {
                // Если Delta Phase <= 1, используем FastLimit напрямую
                tempReal = T.CreateChecked(optInFastLimit);
            }

            // Расчет MAMA и FAMA
            // Формула MAMA: MAMA = Alpha * Price + (1 - Alpha) * prevMAMA
            mama = tempReal * todayValue + (T.One - tempReal) * mama;

            // Формула FAMA: FAMA = (Alpha/2) * MAMA + (1 - Alpha/2) * prevFAMA
            tempReal *= T.CreateChecked(0.5);
            fama = tempReal * mama + (T.One - tempReal) * fama;

            // Запись валидных значений в выходные массивы (только для баров после lookback периода)
            if (today >= startIdx)
            {
                outMAMA[outIdx] = mama;
                outFAMA[outIdx++] = fama;
            }

            // Корректировка периода для следующего ценового бара
            // Обновление значений re, im, prevI2, prevQ2 и period на основе I2 и Q2
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            // Переход к следующему бару
            today++;
        }

        // Формирование выходного диапазона (outRange)
        // Start: индекс первого бара с валидным значением
        // End: индекс последнего бара с валидным значением
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Выполняет преобразование Хилберта для расчета индикатора MAMA.
    /// </summary>
    /// <param name="today">Текущий индекс бара во входных данных.</param>
    /// <param name="circBuffer">Циркулярный буфер для хранения промежуточных значений преобразования Хилберта.</param>
    /// <param name="smoothedValue">Сглаженное значение цены (после WMA).</param>
    /// <param name="hilbertIdx">Ссылка на текущий индекс в циркулярном буфере.</param>
    /// <param name="adjustedPrevPeriod">Скорректированный предыдущий период цикла.</param>
    /// <param name="i1ForOddPrev3">Ссылка на предыдущее значение I1 для нечетных итераций (3 периода назад).</param>
    /// <param name="i1ForEvenPrev3">Ссылка на предыдущее значение I1 для четных итераций (3 периода назад).</param>
    /// <param name="i1ForOddPrev2">Ссылка на предыдущее значение I1 для нечетных итераций (2 периода назад).</param>
    /// <param name="i1ForEvenPrev2">Ссылка на предыдущее значение I1 для четных итераций (2 периода назад).</param>
    /// <param name="prevQ2">Предыдущее значение Q2 компонента.</param>
    /// <param name="prevI2">Предыдущее значение I2 компонента.</param>
    /// <param name="i2">Выходное значение I2 компонента преобразования Хилберта.</param>
    /// <param name="q2">Выходное значение Q2 компонента преобразования Хилберта.</param>
    /// <typeparam name="T">Числовой тип данных, реализующий <see cref="IFloatingPointIeee754{T}"/>.</typeparam>
    /// <returns>Текущее значение фазы в градусах (для расчета Delta Phase).</returns>
    private static T PerformMAMAHilbertTransform<T>(
        int today,
        Span<T> circBuffer,
        T smoothedValue,
        ref int hilbertIdx,
        T adjustedPrevPeriod,
        ref T i1ForOddPrev3,
        ref T i1ForEvenPrev3,
        ref T i1ForOddPrev2,
        ref T i1ForEvenPrev2,
        T prevQ2,
        T prevI2,
        out T i2,
        out T q2) where T : IFloatingPointIeee754<T>
    {
        T tempReal2;

        // Обработка четных и нечетных итераций для оптимизации вычислений
        if (today % 2 == 0)
        {
            // Расчет для четных дней
            FunctionHelpers.HTHelper.CalcHilbertEven(circBuffer, smoothedValue, ref hilbertIdx, adjustedPrevPeriod, i1ForEvenPrev3, prevQ2,
                prevI2,
                out i1ForOddPrev3, ref i1ForOddPrev2, out q2, out i2);

            // Расчет фазы: Phase = atan(Q1 / I1) в градусах
            tempReal2 = !T.IsZero(i1ForEvenPrev3)
                ? T.RadiansToDegrees(T.Atan(circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1] / i1ForEvenPrev3))
                : T.Zero;
        }
        else
        {
            // Расчет для нечетных дней
            FunctionHelpers.HTHelper.CalcHilbertOdd(circBuffer, smoothedValue, hilbertIdx, adjustedPrevPeriod, out i1ForEvenPrev3, prevQ2,
                prevI2,
                i1ForOddPrev3, ref i1ForEvenPrev2, out q2, out i2);

            // Расчет фазы: Phase = atan(Q1 / I1) в градусах
            tempReal2 = !T.IsZero(i1ForOddPrev3)
                ? T.RadiansToDegrees(T.Atan(circBuffer[(int) FunctionHelpers.HTHelper.HilbertKeys.Q1] / i1ForOddPrev3))
                : T.Zero;
        }

        return tempReal2;
    }
}
