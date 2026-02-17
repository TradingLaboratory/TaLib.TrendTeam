//Название файла *.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//MathOperators (альтернатива, если требуется группировка по типу операций)
//ExponentialFunctions (альтернатива для акцента на экспоненциальных функциях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Exp (Math Transform) — Экспоненциальная функция (Математическое преобразование)
    /// </summary>
    /// <param name="inReal">
    /// Входные данные для расчета индикатора (цены, другие индикаторы или другие временные ряды).
    /// Для данного метода это массив числовых значений, к которым применяется математическая функция.
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
    /// Диапазон индексов в <paramref name="inReal"/>, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inReal.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно (например, длина <paramref name="inReal"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// Функция применяет экспоненциальную функцию (e^x) к каждой точке данных в ряду.
    /// <para>
    /// Это математическое преобразование часто используется в сложных моделях волатильности,
    /// статистических расчетах или пользовательских стратегиях, требующих нелинейных преобразований данных.
    /// </para>
    /// <para>
    /// Редко применяется отдельно как торговый сигнал, чаще выступает вспомогательным инструментом.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Exp<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        ExpImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период предыстории (Lookback) для <see cref="Exp{T}">Exp</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0. Lookback период обозначает индекс первого бара во входящих данных,
    /// для которого можно будет получить валидное значение рассчитываемого индикатора.
    /// Для экспоненциальной функции предыстория не требуется, так как расчет зависит только от текущего значения.
    /// </returns>
    [PublicAPI]
    public static int ExpLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span).
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Exp<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        ExpImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode ExpImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением по умолчанию
        outRange = Range.EndAt(0);

        // Проверка корректности диапазона входных данных
        // Возвращает кортеж (startIdx, endIdx) или null, если диапазон невалиден
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов для обработки входных данных
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив outReal
        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Применение экспоненциальной функции к каждому элементу входных данных
            // Math: outReal[i] = e^(inReal[i])
            outReal[outIdx++] = T.Exp(inReal[i]);
        }

        // Установка диапазона выходных данных:
        // Start совпадает с началом обработки входных данных
        // End определяется количеством успешно рассчитанных значений (startIdx + outIdx)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
