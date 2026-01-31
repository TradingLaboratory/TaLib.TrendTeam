// Название файла: Obv.cs
// Группы, к которым можно отнести индикатор:
// VolumeIndicators (существующая папка - идеальное соответствие категории)
// MomentumIndicators (альтернатива ≥50%, так как OBV отражает силу движения цены через объём)
// AccumulationDistribution (альтернатива ≥50%, концептуально близок к индикаторам накопления/распределения)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// On Balance Volume (Volume Indicators) — Балансовый объём (Индикаторы объёма)
    /// </summary>
    /// <param name="inReal">Входные данные для расчёта (обычно цены закрытия <c>Close</c>).</param>
    /// <param name="inVolume">Входные данные объёмов торгов (<c>Volume</c>) за соответствующие периоды.</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив.
    /// </param>
    /// <param name="outReal">
    /// Массив для хранения рассчитанных значений OBV.  
    /// - Длина массива равна <c>outRange.End.Value - outRange.Start.Value</c> (при успешном расчёте).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует входному периоду <c>inReal[outRange.Start + i]</c>.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - При успешном расчёте <c>End</c> соответствует последнему элементу входного диапазона.
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Код возврата <see cref="Core.RetCode"/>, указывающий на успех или ошибку расчёта.  
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном выполнении или соответствующий код ошибки в противном случае.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Индикатор On Balance Volume (OBV) рассчитывает кумулятивную (накопленную) меру потока объёма для определения,
    /// поступает ли объём в финансовый инструмент или уходит из него. Индикатор основан на принципе,
    /// что изменение объёма предшествует движению цены.
    /// </para>
    ///
    /// <para>
    /// Индикатор наиболее полезен в комбинации с другими индикаторами или ценовыми паттернами,
    /// так как сам по себе не даёт однозначных сигналов на покупку или продажу.
    /// Резкие всплески объёма могут вызывать значительные изменения OBV, потенциально искажая тренд.
    /// </para>
    ///
    /// <b>Этапы расчёта</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Инициализация значения <c>OBV</c> объёмом первого периода во входном диапазоне.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Для каждого последующего периода:
    ///       <list type="bullet">
    ///         <item>
    ///           <description>
    ///             Если цена текущего периода (<c>Close</c>) выше цены предыдущего периода — к <c>OBV</c> добавляется объём текущего периода.
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             Если цена текущего периода ниже цены предыдущего периода — из <c>OBV</c> вычитается объём текущего периода.
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description>
    ///             Если цена текущего периода равна цене предыдущего периода — значение <c>OBV</c> остаётся без изменений.
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Сохранение накопленного значения <c>OBV</c> для каждого периода в выходной массив.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Рост значения указывает на положительный поток объёма, что может свидетельствовать о накоплении (accumulation).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Падение значения указывает на отрицательный поток объёма, что может свидетельствовать о распределении (distribution).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Боковое движение OBV говорит об отсутствии значимого накопления или распределения.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Obv<T>(
        ReadOnlySpan<T> inReal,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        ObvImpl(inReal, inVolume, inRange, outReal, out outRange);

    /// <summary>
    /// Возвращает период задержки (lookback) для индикатора <see cref="Obv{T}"/>.
    /// </summary>
    /// <returns>Всегда 0, так как для расчёта не требуется предыстория данных — первое значение рассчитывается на первом баре.</returns>
    [PublicAPI]
    public static int ObvLookback() => 0;

    /// <remarks>
    /// Для совместимости с абстрактным API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Obv<T>(
        T[] inReal,
        T[] inVolume,
        Range inRange,
        T[] outReal,
        out Range outRange) where T : IFloatingPointIeee754<T> =>
        ObvImpl<T>(inReal, inVolume, inRange, outReal, out outRange);

    private static Core.RetCode ObvImpl<T>(
        ReadOnlySpan<T> inReal,
        ReadOnlySpan<T> inVolume,
        Range inRange,
        Span<T> outReal,
        out Range outRange) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением [0, 0)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и получение индексов начала и конца обработки
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length, inVolume.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        // Инициализация накопленного OBV объёмом первого периода в диапазоне
        var prevOBV = inVolume[startIdx];
        // Сохранение цены предыдущего периода для сравнения
        var prevReal = inReal[startIdx];
        // Индекс для записи в выходной массив
        var outIdx = 0;

        // Цикл расчёта OBV для каждого периода в заданном диапазоне
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Текущая цена закрытия
            var tempReal = inReal[i];
            if (tempReal > prevReal)
            {
                // Цена выросла — добавляем объём к накопленному OBV (накопление)
                prevOBV += inVolume[i];
            }
            else if (tempReal < prevReal)
            {
                // Цена упала — вычитаем объём из накопленного OBV (распределение)
                prevOBV -= inVolume[i];
            }
            // Если цена не изменилась — OBV остаётся без изменений

            // Сохранение текущего значения OBV в выходной массив
            outReal[outIdx++] = prevOBV;
            // Обновление цены предыдущего периода для следующей итерации
            prevReal = tempReal;
        }

        // Формирование выходного диапазона: от первого обработанного индекса до последнего
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
