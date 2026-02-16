//Название файла *.cs: TA_Sum.cs
//Группы к которым можно отнести индикатор:
//MathOperators (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по статистическим функциям)
//MomentumIndicators (альтернатива для использования в расчётах импульса)

using System.Runtime.InteropServices;

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Sum (MathOperators) — Суммирование (Математические операторы)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// <para>
    /// Обычно используется с ценами <see cref="Close"/>, <see cref="Open"/>, <see cref="High"/>, <see cref="Low"/> 
    /// или объёмами <see cref="Volume"/>, но может принимать любые числовые временные ряды.
    /// </para>
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// - Определяет границы для вычисления индикатора внутри входных данных.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
    /// - Содержит результаты скользящего суммирования за указанный период.
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
    /// Период времени для скользящего суммирования.
    /// <para>
    /// - Минимальное допустимое значение: 2.
    /// - Значение по умолчанию: 30.
    /// - Определяет количество баров, используемых для расчета суммы.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу расчета.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном расчете, 
    /// или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// SUM добавляет точки данных за определенный период, что полезно для накопительных расчетов 
    /// или методов сглаживания.
    /// <para>
    /// Функция предоставляет скользящее суммирование, которое может использоваться в качестве 
    /// базового значения для других расчетов или индикаторов. Его полезность заключается в создании 
    /// пользовательских индикаторов, где накопленные данные информируют о тренде или импульсе.
    /// </para>
    /// <para>
    /// <b>Применение в техническом анализе</b>:
    /// - Расчет накопленного объёма за период.
    /// - Суммирование ценовых изменений для оценки общего движения.
    /// - Базовый компонент для других индикаторов (например, скользящих средних).
    /// </para>
    ///
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Инициализировать сумму значений за первый период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого последующего периода обновлять сумму, добавляя новое значение 
    ///       и удаляя самое старое значение из суммы (скользящее окно).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сохранять рассчитанную сумму для каждого периода в выходном массиве.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Sum<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        SumImpl(inReal, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предварительного просмотра (lookback period) для <see cref="Sum{T}">Sum</see>.
    /// </summary>
    /// <param name="optInTimePeriod">
    /// Период времени для скользящего суммирования.
    /// <para>
    /// - Минимальное допустимое значение: 2.
    /// - Значение по умолчанию: 30.
    /// </para>
    /// </param>
    /// <returns>
    /// Количество периодов, необходимых до первого выходного значения, которое может быть рассчитано.
    /// <para>
    /// - Это индекс первого бара во входных данных, для которого можно получить валидное значение индикатора.
    /// - Все бары с индексом меньше lookback будут пропущены при расчете.
    /// - Возвращает -1, если период меньше 2 (некорректный параметр).
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int SumLookback(int optInTimePeriod = 30) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Этот метод обеспечивает совместимость с версиями API, использующими массивы вместо Span.
    /// </para>
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Sum<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        SumImpl<T>(inReal, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode SumImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона - по умолчанию пустой диапазон
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона данных
        // Если диапазон некорректен, возвращаем ошибку OutOfRangeParam
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из проверенного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Проверка параметра периода времени
        // Минимальное допустимое значение периода - 2
        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        // Расчет lookback периода - количество баров, необходимых для первого валидного значения
        var lookbackTotal = SumLookback(optInTimePeriod);

        // Корректировка начального индекса с учётом lookback периода
        // Все бары с индексом меньше lookbackTotal будут пропущены
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного, данных недостаточно для расчета
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Переменная для хранения текущей суммы значений в скользящем окне
        var periodTotal = T.Zero;

        // Индекс самого старого значения в текущем скользящем окне (для удаления из суммы)
        var trailingIdx = startIdx - lookbackTotal;

        // Текущий индекс для итерации по входным данным
        var i = trailingIdx;

        // Инициализация суммы за первый период
        // Суммируем значения от trailingIdx до startIdx (исключая startIdx)
        while (i < startIdx)
        {
            periodTotal += inReal[i++];
        }

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Основной цикл расчета скользящего суммирования
        // Для каждого бара: добавляем новое значение, удаляем старое, сохраняем результат
        do
        {
            // Добавляем текущее значение к сумме
            periodTotal += inReal[i++];

            // Сохраняем текущую сумму во временную переменную для вывода
            var tempReal = periodTotal;

            // Удаляем самое старое значение из суммы (скользящее окно)
            periodTotal -= inReal[trailingIdx++];

            // Записываем рассчитанную сумму в выходной массив
            outReal[outIdx++] = tempReal;
        } while (i <= endIdx);

        // Установка выходного диапазона
        // Start: индекс первого бара с валидным значением
        // End: индекс последнего бара с валидным значением (startIdx + количество рассчитанных значений)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
