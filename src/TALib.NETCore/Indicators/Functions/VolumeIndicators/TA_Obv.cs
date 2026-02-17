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
    /// <param name="inReal">
    /// Входные данные для расчёта индикатора (обычно цены закрытия <c>Close</c>).  
    /// - Используется для определения направления движения цены относительно предыдущего периода.
    /// </param>
    /// <param name="inVolume">
    /// Входные данные объёмов торгов (<c>Volume</c>) за соответствующие периоды.  
    /// - Объём добавляется или вычитается из кумулятивного значения в зависимости от направления цены.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных во входных массивах (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив входных данных.  
    /// - Определяет границы для расчёта индикатора внутри предоставленных данных.
    /// </param>
    /// <param name="outReal">
    /// Массив для хранения рассчитанных значений OBV (On Balance Volume).  
    /// - Длина массива равна <c>outRange.End.Value - outRange.Start.Value</c> (при успешном расчёте).  
    /// - Каждый элемент <c>outReal[i]</c> соответствует входному периоду <c>inReal[outRange.Start + i]</c>.  
    /// - Содержит ТОЛЬКО валидные значения индикатора.
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов во входных данных, для которых рассчитаны валидные значения:  
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal"/>, имеющего валидное значение в <paramref name="outReal"/>.  
    /// - При успешном расчёте <c>End</c> соответствует последнему элементу входного диапазона.  
    /// - Lookback период для OBV равен 0, поэтому первое значение рассчитывается на первом баре входного диапазона.
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
    /// <b>Описание индикатора</b>:  
    /// Индикатор On Balance Volume (OBV) рассчитывает кумулятивную (накопленную) меру потока объёма для определения,  
    /// поступает ли объём в финансовый инструмент или уходит из него. Индикатор основан на принципе,  
    /// что изменение объёма предшествует движению цены.
    /// </para>
    ///
    /// <para>
    /// <b>Применение</b>:  
    /// Индикатор наиболее полезен в комбинации с другими индикаторами или ценовыми паттернами,  
    /// так как сам по себе не даёт однозначных сигналов на покупку или продажу.  
    /// Резкие всплески объёма могут вызывать значительные изменения OBV, потенциально искажая тренд.
    /// </para>
    ///
    /// <para>
    /// <b>Этапы расчёта</b>:
    /// </para>
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
    /// <para>
    /// <b>Интерпретация значений</b>:
    /// </para>
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
    ///
    /// <para>
    /// <b>Особенности</b>:  
    /// - Lookback период равен 0, так как для расчёта не требуется предыстория данных.  
    /// - Первое валидное значение индикатора доступно сразу на первом баре входного диапазона.  
    /// - outRange указывает диапазон индексов входных данных, для которых рассчитаны валидные значения OBV.
    /// </para>
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
    /// <returns>
    /// Всегда 0, так как для расчёта OBV не требуется предыстория данных —  
    /// первое валидное значение рассчитывается на первом баре входного диапазона.  
    /// Все бары с индексом меньше lookback (0) не пропускаются.
    /// </returns>
    [PublicAPI]
    public static int ObvLookback() => 0;

    /// <remarks>
    /// Метод для совместимости с абстрактным API библиотеки TALib.  
    /// Перенаправляет вызов на основную реализацию <see cref="ObvImpl{T}"/>.
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
        // outRange будет содержать индексы первого и последнего бара с валидными значениями OBV
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и получение индексов начала и конца обработки
        // Проверяет корректность диапазонов для inReal и inVolume
        if (FunctionHelpers.ValidateInputRange(inRange, inReal.Length, inVolume.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение индексов начала и конца диапазона для обработки данных
        var (startIdx, endIdx) = rangeIndices;

        // Инициализация накопленного OBV объёмом первого периода в диапазоне
        // prevOBV хранит текущее кумулятивное значение балансового объёма
        var prevOBV = inVolume[startIdx];

        // Сохранение цены предыдущего периода для сравнения с текущей ценой
        // prevReal используется для определения направления движения цены
        var prevReal = inReal[startIdx];

        // Индекс для записи значений в выходной массив outReal
        // outIdx отслеживает позицию текущей записи в выходном массиве
        var outIdx = 0;

        // Цикл расчёта OBV для каждого периода в заданном диапазоне
        // Обрабатывает все бары от startIdx до endIdx включительно
        for (var i = startIdx; i <= endIdx; i++)
        {
            // Текущая цена закрытия (Close) для сравнения с предыдущим периодом
            var tempReal = inReal[i];

            if (tempReal > prevReal)
            {
                // Цена выросла — добавляем объём к накопленному OBV (накопление/accumulation)
                // Положительный поток объёма указывает на покупательский интерес
                prevOBV += inVolume[i];
            }
            else if (tempReal < prevReal)
            {
                // Цена упала — вычитаем объём из накопленного OBV (распределение/distribution)
                // Отрицательный поток объёма указывает на продавецкий интерес
                prevOBV -= inVolume[i];
            }
            // Если цена не изменилась (tempReal == prevReal) — OBV остаётся без изменений
            // Это соответствует логике индикатора: без движения цены нет потока объёма

            // Сохранение текущего значения OBV в выходной массив
            // outReal[outIdx] содержит кумулятивное значение OBV для бара с индексом i
            outReal[outIdx++] = prevOBV;

            // Обновление цены предыдущего периода для следующей итерации цикла
            // prevReal будет использован для сравнения с ценой следующего бара
            prevReal = tempReal;
        }

        // Формирование выходного диапазона: от первого обработанного индекса до последнего
        // outRange.Start = startIdx (первый бар с валидным значением OBV)
        // outRange.End = startIdx + outIdx (последний бар с валидным значением OBV)
        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }
}
