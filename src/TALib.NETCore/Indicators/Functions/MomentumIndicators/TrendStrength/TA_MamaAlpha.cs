//Название файла: TA_MamaAlpha.cs
//Группы к которым можно отнести индикатор:
//CycleIndicators (существующая папка - идеальное соответствие категории)
//AdaptiveIndicators (предложение новой подпапки для адаптивных индикаторов)
//MathTransform (альтернатива, если требуется группировка по типу преобразования)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MESA Adaptive Moving Average Alpha Factor (Cycle Indicators) — Коэффициент адаптивности MAMA (Индикаторы цикла).
    /// </summary>
    /// <remarks>
    /// Этот метод вычисляет только адаптивный коэффициент (Alpha), который используется внутри индикатора 
    /// <see cref="Mama{T}">MAMA</see> для динамической настройки чувствительности скользящего среднего.
    /// <para>
    /// Alpha определяет, насколько быстро индикатор реагирует на изменения цены. 
    /// Значение Alpha меняется в реальном времени в зависимости от доминирующего рыночного цикла.
    /// </para>
    /// </remarks>
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
    /// <param name="outAlpha">
    /// Массив, содержащий ТОЛЬКО валидные значения адаптивного коэффициента Alpha.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start</c> (для стандартного C# Range, где End исключителен).
    /// - Каждый элемент <c>outAlpha[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Значения находятся в диапазоне [<paramref name="optInSlowLimit"/>, <paramref name="optInFastLimit"/>].
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outAlpha"/>.
    /// - <b>End</b>: индекс, следующий за последним элементом <paramref name="inReal"/>, имеющим валидное значение (стандарт C# Range).
    /// - Гарантируется: <c>End == inReal.Length</c> (конец входных данных), если расчет успешен.
    /// - Если данных недостаточно, возвращается <c>[0, 0]</c>.
    /// </para>
    /// </param>
    /// <param name="optInFastLimit">
    /// Верхняя граница для адаптивного фактора (Fast Limit).
    /// <para>
    /// Допустимый диапазон: <c>0.01..0.99</c>. Значение по умолчанию: <c>0.5</c>.
    /// Определяет максимальную скорость реакции индикатора на тренд.
    /// </para>
    /// </param>
    /// <param name="optInSlowLimit">
    /// Нижняя граница для адаптивного фактора (Slow Limit).
    /// <para>
    /// Допустимый диапазон: <c>0.01..0.99</c>. Значение по умолчанию: <c>0.05</c>.
    /// Определяет минимальную скорость реакции (максимальное сглаживание) во время флэта.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// </returns>
    [PublicAPI]
    public static Core.RetCode MamaAlpha<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outAlpha,
        out Range outRange,
        double optInFastLimit = 0.5,
        double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
        MamaAlphaImpl(inReal, inRange, outAlpha, out outRange, optInFastLimit, optInSlowLimit);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback period) для <see cref="MamaAlpha{T}">MamaAlpha</see>.
    /// </summary>
    /// <returns>
    /// Количество периодов (баров), необходимых до первого вычисленного валидного значения коэффициента Alpha.
    /// </returns>
    /// <remarks>
    /// Поскольку расчет Alpha зависит от тех же промежуточных вычислений (Hilbert Transform, Period), 
    /// что и основной индикатор <see cref="Mama{T}">MAMA</see>, период обратного просмотра идентичен.
    /// </remarks>
    [PublicAPI]
    public static int MamaAlphaLookback() => MamaLookback();

    /// <remarks>
    /// Для совместимости с абстрактным API (Abstract API compatibility).
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MamaAlpha<T>(
        T[] inReal,
        Range inRange,
        T[] outAlpha,
        out Range outRange,
        double optInFastLimit = 0.5,
        double optInSlowLimit = 0.05) where T : IFloatingPointIeee754<T> =>
        MamaAlphaImpl<T>(inReal, inRange, outAlpha, out outRange, optInFastLimit, optInSlowLimit);

    /// <summary>
    /// Внутренняя реализация расчета адаптивного коэффициента Alpha для индикатора MAMA.
    /// </summary>
    private static Core.RetCode MamaAlphaImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outAlpha,
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
        // Alpha требует той же стабилизации цикла, что и MAMA
        var lookbackTotal = MamaLookback();
        // Корректировка начального индекса с учётом lookback периода
        // Все бары с индексом меньше lookbackTotal будут пропущены
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Сохранение начального индекса для формирования выходного диапазона
        var outBegIdx = startIdx;

        // Инициализация взвешенного скользящего среднего (WMA) для сглаживания входных данных
        // Используется та же логика инициализации, что и в MamaImpl
        FunctionHelpers.HTHelper.InitWma(inReal, startIdx, lookbackTotal, out var periodWMASub, out var periodWMASum,
            out var trailingWMAValue, out var trailingWMAIdx, 9, out var today);

        // Индекс для циркулярного буфера преобразования Хилберта
        var hilbertIdx = 0;

        /* Инициализация циркулярного буфера, используемого логикой преобразования Хилберта.
         * Буфер используется для нечетных и четных дней.
         */
        Span<T> circBuffer = FunctionHelpers.HTHelper.BufferFactory<T>();

        // Индекс для записи выходных значений Alpha
        var outIdx = 0;

        // Переменные для хранения промежуточных значений расчета
        // prevI2, prevQ2 - предыдущие значения I2 и Q2 компонентов преобразования Хилберта
        // re, im - действительная и мнимая части для расчета периода
        // period - текущий оцененный период цикла
        // prevPhase - предыдущее значение фазы для расчета Delta Phase
        // i1ForOddPrev3, i1ForEvenPrev3 - предыдущие значения I1 для нечетных/четных итераций (3 периода назад)
        // i1ForOddPrev2, i1ForEvenPrev2 - предыдущие значения I1 для нечетных/четных итераций (2 периода назад)
        T prevI2, prevQ2, re, im, period, prevPhase, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2;

        // Инициализация всех промежуточных переменных нулевым значением
        period = prevI2 = prevQ2
            = re = im = prevPhase = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = T.Zero;

        // Основной цикл расчета коэффициента Alpha
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

            // Запись валидных значений в выходные массивы (только для баров после lookback периода)
            // tempReal на этом этапе содержит рассчитанный Alpha
            if (today >= startIdx)
            {
                outAlpha[outIdx++] = tempReal;
            }

            // Корректировка периода для следующего ценового бара
            // Обновление значений re, im, prevI2, prevQ2 и period на основе I2 и Q2
            FunctionHelpers.HTHelper.CalcSmoothedPeriod(ref re, i2, q2, ref prevI2, ref prevQ2, ref im, ref period);

            // Переход к следующему бару
            today++;
        }

        // Формирование выходного диапазона (outRange)
        // Start: индекс первого бара с валидным значением
        // End: индекс, следующий за последним баром с валидным значением (стандарт C# Range)
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
