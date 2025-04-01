//Название файла: TA_Acos.cs
//Группы к которым можно отнести индикатор:
//MathTransform (существующая папка - идеальное соответствие категории)
//TrigonometricFunctions (альтернатива для акцента на тригонометрических операциях)
//PureMath (альтернатива для группы чисто математических функций)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Векторный тригонометрический ACos (математическое преобразование)
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
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>, 
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность расчета.
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении, иначе код ошибки.
    /// </returns>
    /// <remarks>
    /// ACos применяет функцию арккосинуса к каждой точке данных в серии, 
    /// в основном для продвинутого математического моделирования, а не для стандартного технического анализа.
    /// <para>
    /// Функция редко используется отдельно для генерации сигналов. Может быть интегрирована
    /// в специализированные/проприетарные модели в сочетании с другими математическими преобразованиями.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Acos<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AcosImpl(inReal, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период lookback для <see cref="Acos{T}">Acos</see>.
    /// </summary>
    /// <returns>Всегда 0, так как исторические данные не требуются для расчета.</returns>
    /// <remarks>
    /// Lookback период = 0 означает, что первое валидное значение рассчитывается 
    /// непосредственно для первого элемента входных данных.
    /// </remarks>
    [PublicAPI]
    public static int AcosLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Acos<T>(
        T[] inReal,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        AcosImpl<T>(inReal, inRange, outReal, out outRange);

    private static Core.RetCode AcosImpl<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация outRange пустым диапазоном
        outRange = Range.EndAt(0);

        // Проверка и получение границ диапазона входных данных
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices; // startIdx - начальный индекс, endIdx - конечный индекс

        var outIdx = 0; // Индекс для записи результатов в outReal
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Применение функции арккосинуса к каждому элементу входных данных
            outReal[outIdx++] = T.Acos(inReal[i]);
        }

        // Установка корректного диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
