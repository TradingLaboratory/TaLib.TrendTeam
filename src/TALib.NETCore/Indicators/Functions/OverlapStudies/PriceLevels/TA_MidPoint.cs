//Название файла: TA_MidPoint.cs
//Рекомендуемое размещение:
//  Основная папка: OverlapStudies
//  Рекомендуемая подпапка: PriceLevels
//Альтернативные категории:
//  PriceTransform (преобразование ценового ряда)
//  TrendIndicators (выделение центральной тенденции движения цены)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// MidPoint (Overlap Studies) — MidPoint / Средняя точка (Наложенные исследования)
    /// </summary>
    /// <param name="inReal">Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды)</param>
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
    /// <param name="optInTimePeriod">Период времени для расчета диапазона (количество баров). Минимальное значение: 2.</param>
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
    /// Индикатор <b>MidPoint</b> вычисляет среднюю арифметическую точку между <b>Highest High</b> (максимальным значением) 
    /// и <b>Lowest Low</b> (минимальным значением) за указанный период:
    /// <code>
    /// MidPoint = (Highest Value + Lowest Value) / 2
    /// </code>
    /// </para>
    /// <para>
    /// Индикатор представляет собой сглаженную линию центральной тенденции, фильтрующую краткосрочные колебания цены.
    /// В отличие от <see cref="MidPrice{T}"/>, который работает с отдельными массивами High и Low, 
    /// MidPoint рассчитывается на основе одного временного ряда (обычно цен закрытия <c>Close</c>).
    /// </para>
    /// <para>
    /// <b>Практическое применение:</b>
    /// <list type="bullet">
    ///   <item><description>Идентификация центральной линии тренда</description></item>
    ///   <item><description>Определение уровней поддержки/сопротивления в боковом движении</description></item>
    ///   <item><description>Фильтрация шума при анализе ценовых движений</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Lookback период:</b> <c>optInTimePeriod - 1</c>. Первое валидное значение индикатора появляется 
    /// на баре с индексом <c>optInTimePeriod - 1</c> (отсчет с нуля), так как для расчета требуется 
    /// полный период данных для определения максимума и минимума.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MidPoint<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MidPointImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для индикатора <see cref="MidPoint{T}"/>.
    /// </summary>
    /// <param name="optInTimePeriod">Период времени для расчета диапазона.</param>
    /// <returns>
    /// Количество баров, необходимых до появления первого валидного значения индикатора.
    /// Возвращает <c>optInTimePeriod - 1</c> при корректном периоде (≥2), иначе <c>-1</c>.
    /// </returns>
    [PublicAPI]
    public static int MidPointLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Вспомогательный метод для совместимости с массивами (а не Span) в абстрактном API.
    /// Перенаправляет вызов в основную реализацию <see cref="MidPointImpl{T}"/>.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MidPoint<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        MidPointImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode MidPointImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Проверка минимального допустимого периода (требуется как минимум 2 бара для определения диапазона)
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        /* Алгоритм расчета:
         * 1. Для каждого бара определяется скользящее окно размером optInTimePeriod
         * 2. В пределах окна находятся экстремумы: Lowest (минимум) и Highest (максимум)
         * 3. Средняя точка рассчитывается как (Highest + Lowest) / 2
         * 
         * Примечание: в отличие от MidPrice, здесь используется один временной ряд (обычно Close),
         * а не отдельные массивы High и Low.
         */

        // Расчет количества баров, которые необходимо пропустить до первого валидного значения
        var lookbackTotal = MidPointLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если после учета lookback периода нет данных для расчета — возвращаем пустой результат
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Инициализация индексов для скользящего окна
        var outIdx = 0;           // Индекс записи в выходной массив
        var today = startIdx;     // Текущий бар для расчета
        var trailingIdx = startIdx - lookbackTotal; // Начало скользящего окна (левая граница)

        // Основной цикл расчета индикатора
        while (today <= endIdx)
        {
            // Инициализация экстремумов первым значением в окне
            var lowest = inReal[trailingIdx++];
            var highest = lowest;

            // Поиск минимального и максимального значений в скользящем окне
            for (var i = trailingIdx; i <= today; i++)
            {
                var tmp = inReal[i];
                if (tmp < lowest)
                {
                    lowest = tmp; // Обновление минимального значения
                }
                else if (tmp > highest)
                {
                    highest = tmp; // Обновление максимального значения
                }
            }

            // Расчет средней точки диапазона: (максимум + минимум) / 2
            outReal[outIdx++] = (highest + lowest) / FunctionHelpers.Two<T>();
            today++;
        }

        // Установка диапазона валидных значений относительно исходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
