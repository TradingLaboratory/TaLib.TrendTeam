//Название файла: TA_TypPrice.cs
//Группы к которым можно отнести индикатор:
//PriceTransform (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу трансформации)
//BasicTransforms (предложение для подпапки внутри PriceTransform)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Typical Price (Price Transform) — Типичная Цена (Трансформация Цен)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены (High).</param>
    /// <param name="inLow">Входные минимальные цены (Low).</param>
    /// <param name="inClose">Входные цены закрытия (Close).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив входных данных.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.  
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует данным входных массивов по индексу <c>outRange.Start + i</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных массивах, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.  
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// <para>
    /// Lookback период для этого индикатора равен 0, поэтому валидные значения начинаются с первого бара входного диапазона.
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
    /// Типичная Цена (Typical Price) — это простое среднее арифметическое максимальных (High), минимальных (Low) 
    /// и цен закрытия (Close) финансового инструмента за определенный период времени. 
    /// Она предоставляет прямое представление о среднем движении цены в этом периоде.
    /// <para>
    /// Часто используется в качестве входных данных для других индикаторов технического анализа 
    /// вместо цены закрытия, чтобы учесть внутридневную волатильность.
    /// </para>
    ///
    /// <b>Шаги расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждого бара рассчитывается Типичная Цена по формуле:
    ///       <code>
    ///         TP = (High + Low + Close) / 3.
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode TypPrice<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TypPriceImpl(inHigh, inLow, inClose, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра (Lookback) для <see cref="TypPrice{T}">TypPrice</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчета не требуется исторических данных beyond текущего бара.</returns>
    [PublicAPI]
    public static int TypPriceLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API (работа с массивами вместо Span).
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode TypPrice<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        TypPriceImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange);

    private static Core.RetCode TypPriceImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона и длин массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов для обработки
        var (startIdx, endIdx) = rangeIndices;

        // Индекс для записи результатов в выходной массив
        var outIdx = 0;

        // Цикл по каждому бару в указанном диапазоне
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Расчет Типичной Цены для каждого периода: (High + Low + Close) / 3
            outReal[outIdx++] = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
        }

        // Установка валидного диапазона выходных данных
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
