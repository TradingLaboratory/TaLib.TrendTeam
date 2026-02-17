//Название файла: TA_MedPrice.cs
//Группы к которым можно отнести индикатор:
//PriceTransform (существующая папка - идеальное соответствие категории)
//MathTransform (альтернатива, если требуется группировка по типу преобразования данных)
//BasicTransforms (предложение для подпапки внутри PriceTransform для базовых трансформаций цен)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Median Price (Price Transform) — Средняя цена (Трансформация цен)
    /// </summary>
    /// <param name="inHigh">Входные максимальные цены (High) для каждого бара.</param>
    /// <param name="inLow">Входные минимальные цены (Low) для каждого бара.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/> и <paramref name="inLow"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inHigh"/> и <paramref name="inLow"/>.
    /// - Учитывается при определении границ вычисления индикатора.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует средней цене для бара с индексом <c>outRange.Start + i</c> во входных данных.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inHigh"/> и <paramref name="inLow"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.
    /// - <b>End</b>: индекс последнего элемента входных данных, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inHigh.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно, возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успех или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Функция Средняя цена (Median Price) вычисляет среднее арифметическое максимальных (High) и минимальных (Low) цен для каждой точки данных в указанном диапазоне.
    /// <para>
    /// Эта функция предоставляет простое представление средней точки ценового бара за указанный период времени.
    /// Обычно используется в качестве базового значения для дальнейших вычислений или как сглаживающий вход для уменьшения шума цен.
    /// </para>
    /// <para>
    /// Функция работает с одним ценовым баром (текущие High и Low) вместо агрегации значений по нескольким барам.
    /// Для вычислений, включающих несколько ценовых баров (например, наибольшая High и наименьшая Low за период),
    /// рассмотрите использование функции <see cref="MidPrice{T}">MidPrice</see>.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Для каждой точки данных в указанном диапазоне суммируются цены <paramref name="inHigh"/> и <paramref name="inLow"/>.
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
    /// Возвращает период обратного просмотра (Lookback) для <see cref="MedPrice{T}">MedPrice</see>.
    /// </summary>
    /// <returns>
    /// Всегда 0, так как для этого вычисления не требуется исторических данных.
    /// <para>
    /// Lookback период обозначает индекс первого бара во входящих данных, для которого можно будет получить валидное значение индикатора.
    /// Поскольку Median Price использует только текущие значения High и Low, задержка отсутствует.
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int MedPriceLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API.
    /// <para>
    /// Перегрузка метода для работы с массивами вместо Span.
    /// </para>
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
        // Инициализация выходного диапазона пустым значением
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

        // Индекс для записи результатов в выходной массив outReal
        var outIdx = 0;
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Вычисление средней цены для текущего ценового бара: (High + Low) / 2
            outReal[outIdx++] = (inHigh[i] + inLow[i]) / FunctionHelpers.Two<T>();
        }

        // Установка выходного диапазона: начало совпадает со startIdx, конец определяется количеством записанных элементов
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
