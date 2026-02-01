// Название файла: TA_Aroon.cs
// Рекомендуемые категории размещения:
// 1. MomentumIndicators (основная категория по классификации TALib)
// 2. TrendIndicators (альтернатива для группировки по определению направления тренда)
// 3. DirectionalIndicators (альтернатива для акцента на направленности движения цены)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Aroon (Momentum Indicators) — Арун (Индикаторы импульса)
    /// <para>
    /// Индикатор Арун измеряет время, прошедшее с момента достижения экстремальных значений цены
    /// (максимума для Aroon Up и минимума для Aroon Down) в рамках заданного периода.
    /// Позволяет оценить силу и направление текущего тренда, а также выявить потенциальные развороты.
    /// </para>
    /// </summary>
    /// <param name="inHigh">Массив максимальных цен (High) для расчета индикатора.</param>
    /// <param name="inLow">Массив минимальных цен (Low) для расчета индикатора.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outAroonDown">Массив, содержащий ТОЛЬКО валидные значения индикатора Aroon Down (линия для минимумов).</param>
    /// <param name="outAroonUp">Массив, содержащий ТОЛЬКО валидные значения индикатора Aroon Up (линия для максимумов).</param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outAroonDown"/> и <paramref name="outAroonUp"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outAroonDown"/> и <paramref name="outAroonUp"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> или <paramref name="inLow"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <param name="optInTimePeriod">Период времени (количество баров для анализа экстремумов). Минимальное значение: 2.</param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор Арун (Aroon) разработан Тушаром Чанде (Tushar Chande) и состоит из двух линий:
    /// <b>Aroon Up</b> (измеряет время с момента достижения максимального максимума) и 
    /// <b>Aroon Down</b> (измеряет время с момента достижения минимального минимума).
    /// </para>
    /// <para>
    /// <b>Формулы расчета:</b>
    /// <code>
    /// Aroon Up    = 100 × (Period - DaysSinceHighestHigh) / Period
    /// Aroon Down  = 100 × (Period - DaysSinceLowestLow) / Period
    /// </code>
    /// где:
    /// - <c>Period</c> — входной параметр <paramref name="optInTimePeriod"/>
    /// - <c>DaysSinceHighestHigh</c> — количество периодов с момента достижения максимального максимума (High)
    /// - <c>DaysSinceLowestLow</c> — количество периодов с момента достижения минимального минимума (Low)
    /// </para>
    /// <para>
    /// <b>Интерпретация значений:</b>
    /// <list type="bullet">
    ///   <item><description><b>Aroon Up ≈ 100</b>: недавно достигнут новый максимум (сильный восходящий тренд)</description></item>
    ///   <item><description><b>Aroon Up ≈ 0</b>: отсутствие новых максимумов в течение периода (ослабление восходящего тренда)</description></item>
    ///   <item><description><b>Aroon Down ≈ 100</b>: недавно достигнут новый минимум (сильный нисходящий тренд)</description></item>
    ///   <item><description><b>Aroon Down ≈ 0</b>: отсутствие новых минимумов в течение периода (ослабление нисходящего тренда)</description></item>
    ///   <item><description><b>Aroon Up &gt; Aroon Down</b>: преобладает восходящее движение</description></item>
    ///   <item><description><b>Aroon Down &gt; Aroon Up</b>: преобладает нисходящее движение</description></item>
    ///   <item><description><b>Пересечение линий</b>: сигнал потенциального разворота тренда</description></item>
    ///   <item><description><b>Обе линии &lt; 50</b>: боковое движение (флэт)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Особенности реализации:</b>
    /// Алгоритм использует оптимизированный подход для отслеживания экстремумов без полного пересчета
    /// на каждом баре. При сдвиге окна анализа проверяется, выходит ли предыдущий экстремум за границы окна,
    /// и при необходимости выполняется частичный пересчет.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Aroon<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outAroonDown,
        Span<T> outAroonUp,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AroonImpl(inHigh, inLow, inRange, outAroonDown, outAroonUp, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период lookback для индикатора <see cref="Aroon{T}"/>.
    /// <para>
    /// Lookback period определяет минимальное количество баров, необходимых перед началом расчета
    /// первого валидного значения индикатора. Для Aroon равен входному периоду <paramref name="optInTimePeriod"/>.
    /// </para>
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета индикатора.</param>
    /// <returns>
    /// Количество периодов, необходимых до вычисления первого валидного значения.
    /// Возвращает -1 при некорректном значении периода (< 2).
    /// </returns>
    [PublicAPI]
    public static int AroonLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod;

    /// <remarks>
    /// Вспомогательная перегрузка для совместимости с абстрактным API (работа с массивами вместо Span).
    /// Перенаправляет вызов в основную реализацию <see cref="AroonImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Aroon<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outAroonDown,
        T[] outAroonUp,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        AroonImpl<T>(inHigh, inLow, inRange, outAroonDown, outAroonUp, out outRange, optInTimePeriod);

    private static Core.RetCode AroonImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outAroonDown,
        Span<T> outAroonUp,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Валидация входного диапазона: проверка корректности индексов и равенства длин массивов High и Low
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода: минимально допустимое значение = 2 бара
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Оптимизированный алгоритм отслеживания экстремумов:
        // Вместо полного пересчета максимума/минимума на каждом шаге используется
        // инкрементальный подход с отслеживанием индексов предыдущих экстремумов.
        // При сдвиге окна проверяется, выходит ли экстремум за его границы —
        // если да, выполняется частичный пересчет только для нового бара.

        var lookbackTotal = AroonLookback(optInTimePeriod);
        // Сдвиг начального индекса с учетом необходимого lookback периода
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учета lookback периода нет данных для расчета — выход с пустым результатом
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация счетчиков для обхода данных
        var outIdx = 0;                    // Индекс в выходных массивах (outAroonUp/outAroonDown)
        var today = startIdx;              // Текущий обрабатываемый бар
        var trailingIdx = startIdx - lookbackTotal; // Начало скользящего окна анализа

        // Отслеживание экстремумов в текущем окне:
        int highestIdx = -1, lowestIdx = -1; // Индексы максимального максимума и минимального минимума
        T highest = T.Zero, lowest = T.Zero; // Соответствующие значения цен

        // Предварительный расчет множителя для формулы: 100 / Period
        var factor = FunctionHelpers.Hundred<T>() / T.CreateChecked(optInTimePeriod);

        // Основной цикл расчета индикатора для каждого бара в диапазоне
        while (today <= endIdx)
        {
            // Обновление минимального минимума (Low) в окне [trailingIdx, today]
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);

            // Обновление максимального максимума (High) в окне [trailingIdx, today]
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            // Расчет Aroon Up: 100 * (Period - дни с момента максимума) / Period
            outAroonUp[outIdx] = factor * T.CreateChecked(optInTimePeriod - (today - highestIdx));

            // Расчет Aroon Down: 100 * (Period - дни с момента минимума) / Period
            outAroonDown[outIdx] = factor * T.CreateChecked(optInTimePeriod - (today - lowestIdx));

            // Переход к следующему бару
            outIdx++;
            trailingIdx++;
            today++;
        }

        // Формирование выходного диапазона: от первого валидного бара до последнего обработанного
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
