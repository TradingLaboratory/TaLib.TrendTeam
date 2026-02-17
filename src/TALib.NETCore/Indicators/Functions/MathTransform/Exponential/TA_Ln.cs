//Название файла: TA_Ln.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//StatisticFunctions (альтернатива, если требуется группировка по типу индикатора)
//LogarithmicFunctions (альтернатива для акцента на логарифмических функциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Vector Log Natural (Math Transform) — Векторный натуральный логарифм (Математическое преобразование)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal[outRange.Start + i]</c>.
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
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Возвращает значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// LN применяет натуральный логарифм к данным, что полезно в статистических и волатильных анализах, а не для прямых торговых сигналов.
    /// <para>
    /// Функция может быть интегрирована в алгоритмические модели или расчеты логарифмических доходностей.
    /// Использование её вместе с другими статистическими мерами может уточнить оценки волатильности или доходности.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Ln<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        LnImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (lookback) для <see cref="Ln{T}">Ln</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого расчета не требуется исторических данных.
    /// </returns>
    /// <remarks>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// <para>
    /// Все бары в исходных данных, индекс которых меньше чем lookback, будут пропущены, чтобы посчитать первое валидное значение индикатора.
    /// Для натурального логарифма преобразование выполняется поэлементно, поэтому задержка отсутствует.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static int LnLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Ln<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        LnImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode LnImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи в выходной массив
        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Применение натурального логарифма к каждому элементу входных данных
            outReal[outIdx++] = T.Log(inReal[i]);
        }

        // Установка валидного диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
