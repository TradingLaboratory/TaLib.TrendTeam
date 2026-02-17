//Название файла: TA_KaufmanER.cs
//Группы к которым можно отнести индикатор:
//MomentumIndicators (основная категория - индикаторы импульса/эффективности)
//VolatilityIndicators (альтернатива, так как учитывает волатильность)
//TrendStrength (альтернатива для акцента на силе тренда)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Kaufman Efficiency Ratio (Momentum Indicators) — Коэффициент эффективности Кауфмана (Индикаторы импульса)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены Close, другие индикаторы или другие временные ряды).
    /// <para>
    /// Обычно используются цены закрытия (Close) для расчета ER.
    /// Может принимать любой числовой временной ряд для анализа эффективности движения.
    /// </para>
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет subset данных для вычисления индикатора.
    /// - Позволяет ограничить расчет определенной частью входных данных.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Содержит рассчитанные значения Efficiency Ratio для валидного диапазона.
    /// - Значения находятся в диапазоне от 0 до 1.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <param name="optInTimePeriod">
    /// Период времени для расчета коэффициента эффективности (Time Period).
    /// <para>
    /// - Рекомендуемое значение по умолчанию: 30.
    /// - Минимальное допустимое значение: 2.
    /// - Определяет количество баров для расчета изменения цены и волатильности.
    /// - Большие значения сглаживают индикатор, меньшие — делают его более чувствительным.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Коэффициент эффективности Кауфмана (Efficiency Ratio - ER) измеряет эффективность движения цены.
    /// <para>
    /// Он сравнивает чистое изменение цены за период с суммой абсолютных изменений цен за тот же период.
    /// Значение ER находится в диапазоне от 0 до 1.
    /// </para>
    /// <para>
    /// - <b>ER ≈ 1</b>: Сильный тренд. Цена движется в одном направлении с минимальным шумом.
    /// - <b>ER ≈ 0</b>: Боковое движение (флэт). Высокая волатильность без направленного движения.
    /// </para>
    ///
    /// <b>Формула расчета</b>:
    /// <code>
    ///   ER = Abs(PriceChange) / Sum(Abs(PriceChange over TimePeriod))
    /// </code>
    /// <para>
    /// где:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>PriceChange</c> = Текущая Цена - Цена <c>TimePeriod</c> баров назад.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Sum(Abs(PriceChange))</c> = Сумма абсолютных разниц между соседними ценами за <c>TimePeriod</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Высокие значения (близкие к 1) указывают на наличие тренда. Это сигнал для использования трендовых стратегий.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Низкие значения (близкие к 0) указывают на шум или консолидацию. Это сигнал для избежания входа в рынок или использования осцилляторов.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       ER является ключевым компонентом для расчета адаптивного скользящего среднего Кауфмана (KAMA).
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Применение в трейдинге</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Фильтрация торговых сигналов: использовать трендовые индикаторы при высоком ER, осцилляторы при низком ER.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Адаптация параметров других индикаторов на основе текущей эффективности рынка.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Определение моментов перехода от тренда к флэту и наоборот.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode KaufmanER<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        KaufmanERImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (Lookback Period) для <see cref="KaufmanER{T}">KaufmanER</see>.
    /// </summary>
    /// <param name="optInTimePeriod">
    /// Период времени (Time Period) для расчета индикатора.
    /// <para>
    /// - Минимальное значение: 2.
    /// - Значение по умолчанию: 30.
    /// - Определяет количество исторических баров, необходимых для расчета первого валидного значения.
    /// </para>
    /// </param>
    /// <returns>
    /// Количество периодов (баров), необходимых до расчета первого валидного выходного значения.
    /// <para>
    /// - Возвращает -1 если optInTimePeriod &lt; 2 (некорректный параметр).
    /// - Для ER период обратного просмотра равен <c>optInTimePeriod</c>.
    /// - Все бары с индексом меньше lookback будут пропущены при расчете.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно получить валидное значение ER.
    /// <para>
    /// Все бары с индексом меньше lookback будут пропущены при расчете.
    /// В отличие от KAMA, ER не требует дополнительного нестабильного периода, так как не является рекурсивным сглаживанием.
    /// </para>
    /// <para>
    /// Этот метод полезен для предварительного выделения буфера нужного размера перед вызовом основного метода расчета.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int KaufmanERLookback(int optInTimePeriod = 30) =>
        optInTimePeriod < 2 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Для совместимости с абстрактным API TALib.
    /// <para>
    /// Этот метод обеспечивает совместимость с массивами вместо Span&lt;T&gt;.
    /// Позволяет использовать индикатор с традиционными массивами C#.
    /// </para>
    /// <para>
    /// Внутренне вызывает <see cref="KaufmanERImpl{T}"/> для выполнения фактических вычислений.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode KaufmanER<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        KaufmanERImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode KaufmanERImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        // outRange будет установлен в конце метода после успешного расчета
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        // Возвращает null если диапазон некорректен или выходит за границы массива
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального периода (должен быть >= 2)
        // Период меньше 2 не позволяет рассчитать изменение цены
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет общего lookback периода для определения первого валидного бара
        // lookbackTotal определяет сколько баров нужно пропустить перед первым валидным значением
        var lookbackTotal = KaufmanERLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если startIdx > endIdx, нет данных для расчета
        // Возвращаем Success так как это не ошибка, просто недостаточно данных
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // sumROC1: сумма абсолютных изменений цены за период (волатильность)
        // Используется для знаменателя в формуле Efficiency Ratio
        var sumROC1 = T.Zero;

        // today: текущий индекс обрабатываемого бара во входных данных
        // Инициализируем позицию перед началом валидного диапазона для подготовки суммы
        var today = startIdx - lookbackTotal;

        // trailingIdx: индекс отстающего бара для скользящего окна
        // Используется для отслеживания цены TimePeriod баров назад
        var trailingIdx = today;

        // Инициализация суммы ROC (Rate of Change) за период
        // Используется та же логика, что и в KAMA для обеспечения консистентности
        // Заполняет sumROC1 суммой абсолютных изменений за optInTimePeriod периодов
        InitSumROC(inReal, ref sumROC1, ref today, optInTimePeriod);

        // На этом этапе sumROC1 представляет суммирование однодневной разницы цен за optInTimePeriod периодов
        // today указывает на первый бар, для которого будет рассчитан валидный ER

        // Цена вчера используется здесь как предыдущее значение для расчета изменения.
        // Это начальное значение для рекурсивной формулы обновления суммы
        var tempReal = inReal[trailingIdx++];

        // periodROC: изменение цены за период (PriceChange)
        // Числитель в формуле Efficiency Ratio - чистое изменение цены за период
        var periodROC = inReal[today] - tempReal;

        // Сохранение отстающего значения (trailingValue).
        // Делается это потому, что входные и выходные данные могут указывать на один и тот же буфер памяти.
        // Предотвращает перезапись данных до их использования в расчетах
        var trailingValue = tempReal;

        // Расчет коэффициента эффективности (Efficiency Ratio)
        // ER = |PriceChange| / Sum(|PriceChange| за период)
        var efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);

        // Запись первого валидного значения ER в выходной массив.
        outReal[0] = efficiencyRatio;

        // outIdx: индекс для записи следующего значения в выходной массив outReal
        var outIdx = 1;

        // outBegIdx: индекс первого бара с валидным значением ER во входных данных
        // Используется для установки outRange.Start в конце метода
        var outBegIdx = today;

        // Переход к следующему бару для продолжения цикла
        today++;

        // Основной цикл обработки баров от startIdx + 1 до endIdx (валидный диапазон)
        // Обрабатывает все оставшиеся бары в диапазоне для расчета ER
        while (today <= endIdx)
        {
            // Обновление суммы ROC с учетом нового и отстающего значения
            // Добавляет новое абсолютное изменение и вычитает отстающее для скользящего окна
            UpdateSumROC(inReal, ref sumROC1, ref today, ref trailingIdx, ref trailingValue);

            // Расчет изменения цены за период
            // Разница между текущей ценой и ценой optInTimePeriod баров назад
            periodROC = inReal[today] - inReal[trailingIdx - 1];

            // Пересчет коэффициента эффективности
            // Обновляет ER для текущего бара с новыми значениями sumROC1 и periodROC
            efficiencyRatio = CalcEfficiencyRatio(sumROC1, periodROC);

            // Запись рассчитанного значения ER в выходной массив
            outReal[outIdx++] = efficiencyRatio;

            // Переход к следующему бару
            today++;
        }

        // Установка выходного диапазона (outRange)
        // Start: индекс первого бара с валидным ER
        // End: индекс последнего бара с валидным ER (исключая, так как Range использует exclusive end)
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
