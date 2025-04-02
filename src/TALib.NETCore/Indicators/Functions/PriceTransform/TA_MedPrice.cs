//Название файла: TA_MedPrice.cs
//Группы к которым можно отнести индикатор:
//PriceTransform (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу преобразования данных)
//AverageIndicators (альтернатива для акцента на средних значениях)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Median Price (Price Transform) — Средняя цена (Трансформация цен)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены.</param>
    /// <param name="inLow">Входные минимальные цены.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inHigh[outRange.Start + i]</c> и <c>inLow[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:
    /// - <b>Start</b>: индекс первого элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inHigh"/> и <paramref name="inLow"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inHigh"/> или <paramref name="inLow"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
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
    /// Функция Средняя цена вычисляет среднее значение максимальных и минимальных цен для каждой точки данных в указанном диапазоне.
    /// Эта функция предоставляет простое представление средней точки ценового бара за указанный период времени.
    /// <para>
    /// Обычно используется в качестве базового значения для дальнейших вычислений или как сглаживающий вход для уменьшения шума цен.
    /// Функция работает с одним ценовым баром (текущие High и Low) вместо агрегации значений по нескольким барам.
    /// Для вычислений, включающих несколько ценовых баров (например, наибольшая High и наименьшая Low), рассмотрите использование
    /// функции <see cref="MidPrice{T}">MidPrice</see>.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждой точки данных в указанном диапазоне суммируются <paramref name="inHigh"/> и <paramref name="inLow"/> цены.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сумма делится на 2 для вычисления средней цены для текущего ценового бара.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Выходные данные предоставляют упрощенное представление центрального значения для каждого ценового бара.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode MedPrice<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        MedPriceImpl(inHigh, inLow, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период обратного просмотра для <see cref="MedPrice{T}">MedPrice</see>.
    /// </summary>
    /// <returns>Всегда 0, так как для этого вычисления не требуется исторических данных.</returns>
    [PublicAPI]
    public static int MedPriceLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode MedPrice<T>(
        T[] inHigh,
        T[] inLow,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        MedPriceImpl<T>(inHigh, inLow, inRange, outReal, out outRange);

    private static Core.RetCode MedPriceImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        // Проверка корректности входного диапазона
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        /* MedPrice = (High + Low ) / 2
         * Это максимальная и минимальная цены одного и того же ценового бара.
         *
         * См. MidPrice для использования наибольшей High и наименьшей Low по нескольким ценовым барам.
         */

        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Вычисление средней цены для текущего ценового бара
            outReal[outIdx++] = (inHigh[i] + inLow[i]) / FunctionHelpers.Two<T>();
        }

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
