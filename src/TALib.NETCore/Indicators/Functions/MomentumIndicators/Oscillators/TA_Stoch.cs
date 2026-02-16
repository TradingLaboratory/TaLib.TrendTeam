// Название файла: TA_Stoch.cs
// Рекомендуемое размещение:
//   Основная папка: MomentumIndicators
//   Подпапка: Oscillators (уже существует)
// Альтернативные варианты подпапки (если потребуется создание новой):
//   - StochasticOscillators
//   - RangeOscillators
//   - MomentumOscillators

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Stochastic Oscillator (Momentum Indicators) — Стохастический осциллятор (Индикаторы импульса)
    /// <para>
    /// Индикатор, определяющий положение цены закрытия относительно ценового диапазона за заданный период.
    /// Используется для выявления зон перекупленности/перепроданности и потенциальных разворотов тренда.
    /// </para>
    /// </summary>
    /// <param name="inHigh">Массив входных цен High (максимумы баров).</param>
    /// <param name="inLow">Массив входных цен Low (минимумы баров).</param>
    /// <param name="inClose">Массив входных цен Close (цены закрытия баров).</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/> (начальный и конечный индексы).
    /// <para>- Если не указан, обрабатывается весь массив <paramref name="inHigh"/>, <paramref name="inLow"/> и <paramref name="inClose"/>.</para>
    /// </param>
    /// <param name="outSlowK">
    /// Массив, содержащий ТОЛЬКО валидные значения медленной линии %K (Slow %K).
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outSlowK[i]</c> соответствует <c>inClose[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outSlowD">
    /// Массив, содержащий ТОЛЬКО валидные значения медленной линии %D (Slow %D).
    /// <para>- Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).</para>
    /// <para>- Каждый элемент <c>outSlowD[i]</c> соответствует <c>inClose[outRange.Start + i]</c>.</para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inClose"/>, для которых рассчитаны валидные значения:
    /// <para>- <b>Start</b>: индекс первого элемента <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outSlowK"/> и <paramref name="outSlowD"/>.</para>
    /// <para>- <b>End</b>: индекс последнего элемента <paramref name="inClose"/>, имеющего валидное значение в <paramref name="outSlowK"/> и <paramref name="outSlowD"/>.</para>
    /// <para>- Гарантируется: <c>End == inClose.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.</para>
    /// <para>- Если данных недостаточно (например, длина <paramref name="inClose"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.</para>
    /// </param>
    /// <param name="optInFastKPeriod">Период для расчета быстрой линии %K (Fast %K). По умолчанию: 5.</param>
    /// <param name="optInSlowKPeriod">Период сглаживания для преобразования Fast %K в медленную линию %K (Slow %K). По умолчанию: 3.</param>
    /// <param name="optInSlowKMAType">Тип скользящей средней для сглаживания Fast %K при расчете Slow %K. По умолчанию: простая скользящая средняя (SMA).</param>
    /// <param name="optInSlowDPeriod">Период для расчета сигнальной линии %D (сглаживание линии Slow %K). По умолчанию: 3.</param>
    /// <param name="optInSlowDMAType">Тип скользящей средней для сглаживания линии Slow %K при расчете %D. По умолчанию: простая скользящая средняя (SMA).</param>
    /// <typeparam name="T">Числовой тип данных (float/double).</typeparam>
    /// <returns>Код результата выполнения (<see cref="Core.RetCode"/>).</returns>
    /// <remarks>
    /// <para>
    /// Стохастический осциллятор измеряет импульс цены, сравнивая текущую цену закрытия с ценовым диапазоном за заданный период.
    /// Значения индикатора колеблются в диапазоне от 0 до 100.
    /// </para>
    /// 
    /// <b>Этапы расчета</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Расчет сырого значения Fast %K:
    ///       <code>
    ///         Fast %K = 100 * ((Close - LowestLow) / (HighestHigh - LowestLow))
    ///       </code>
    ///       где:
    ///       <list type="bullet">
    ///         <item><description><b>Close</b> — цена закрытия текущего периода</description></item>
    ///         <item><description><b>LowestLow</b> — минимальное значение цены Low за период <paramref name="optInFastKPeriod"/></description></item>
    ///         <item><description><b>HighestHigh</b> — максимальное значение цены High за период <paramref name="optInFastKPeriod"/></description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item><description>Сглаживание Fast %K за период <paramref name="optInSlowKPeriod"/> с использованием типа скользящей средней <paramref name="optInSlowKMAType"/> → получаем линию Slow %K</description></item>
    ///   <item><description>Сглаживание Slow %K за период <paramref name="optInSlowDPeriod"/> с использованием типа скользящей средней <paramref name="optInSlowDMAType"/> → получаем сигнальную линию %D</description></item>
    /// </list>
    /// 
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item><description>Значения выше 80: зона перекупленности (потенциальный разворот вниз)</description></item>
    ///   <item><description>Значения ниже 20: зона перепроданности (потенциальный рост)</description></item>
    ///   <item><description>Пересечения линий:
    ///     <list type="bullet">
    ///       <item><description>Линия %K пересекает линию %D снизу вверх — возможный сигнал на покупку</description></item>
    ///       <item><description>Линия %K пересекает линию %D сверху вниз — возможный сигнал на продажу</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>Дивергенция между осциллятором и ценой указывает на ослабление текущего тренда</description></item>
    /// </list>
    /// 
    /// <b>Важно</b>:
    /// <para>
    /// Метод возвращает "медленную" версию стохастического осциллятора (Slow Stochastic),
    /// которая включает двойное сглаживание для уменьшения количества ложных сигналов.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Stoch<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outSlowK,
        Span<T> outSlowD,
        out Range outRange,
        int optInFastKPeriod = 5,
        int optInSlowKPeriod = 3,
        Core.MAType optInSlowKMAType = Core.MAType.Sma,
        int optInSlowDPeriod = 3,
        Core.MAType optInSlowDMAType = Core.MAType.Sma) where T : IFloatingPointIeee754<T> =>
        StochImpl(inHigh, inLow, inClose, inRange, outSlowK, outSlowD, out outRange, optInFastKPeriod, optInSlowKPeriod, optInSlowKMAType,
            optInSlowDPeriod, optInSlowDMAType);

    /// <summary>
    /// Возвращает период "просмотра назад" (lookback period) для функции <see cref="Stoch{T}"/>.
    /// <para>
    /// Значение определяет минимальное количество баров, необходимых для расчета первого валидного значения индикатора.
    /// Все бары с индексом меньше этого значения будут пропущены при расчете.
    /// </para>
    /// </summary>
    /// <param name="optInFastKPeriod">Период для расчета быстрой линии %K (Fast %K). По умолчанию: 5.</param>
    /// <param name="optInSlowKPeriod">Период сглаживания для преобразования Fast %K в медленную линию %K (Slow %K). По умолчанию: 3.</param>
    /// <param name="optInSlowKMAType">Тип скользящей средней для сглаживания Fast %K. По умолчанию: простая скользящая средняя (SMA).</param>
    /// <param name="optInSlowDPeriod">Период для расчета сигнальной линии %D. По умолчанию: 3.</param>
    /// <param name="optInSlowDMAType">Тип скользящей средней для сглаживания линии Slow %K при расчете %D. По умолчанию: простая скользящая средняя (SMA).</param>
    /// <returns>Количество периодов, необходимых для первого расчета валидного значения индикатора.</returns>
    [PublicAPI]
    public static int StochLookback(
        int optInFastKPeriod = 5,
        int optInSlowKPeriod = 3,
        Core.MAType optInSlowKMAType = Core.MAType.Sma,
        int optInSlowDPeriod = 3,
        Core.MAType optInSlowDMAType = Core.MAType.Sma)
    {
        // Проверка корректности входных параметров периода
        if (optInFastKPeriod < 1 || optInSlowKPeriod < 1 || optInSlowDPeriod < 1)
        {
            return -1;
        }
        // Базовый период для расчета Fast %K (минус 1, так как текущий бар включается в расчет)
        var retValue = optInFastKPeriod - 1;
        // Добавляем период сглаживания для преобразования Fast %K → Slow %K
        retValue += MaLookback(optInSlowKPeriod, optInSlowKMAType);
        // Добавляем период сглаживания для расчета сигнальной линии %D
        retValue += MaLookback(optInSlowDPeriod, optInSlowDMAType);
        return retValue;
    }

    // Реализация метода расчета стохастического осциллятора
    private static Core.RetCode StochImpl<T>(
        ReadOnlySpan<T> inHigh,      // Входной массив цен High (максимумы)
        ReadOnlySpan<T> inLow,       // Входной массив цен Low (минимумы)
        ReadOnlySpan<T> inClose,     // Входной массив цен Close (закрытия)
        Range inRange,               // Диапазон обрабатываемых данных
        Span<T> outSlowK,            // Выходной массив для медленной линии %K
        Span<T> outSlowD,            // Выходной массив для сигнальной линии %D
        out Range outRange,          // Диапазон индексов с валидными значениями
        int optInFastKPeriod,        // Период для расчета Fast %K
        int optInSlowKPeriod,        // Период сглаживания для получения Slow %K
        Core.MAType optInSlowKMAType,// Тип скользящей средней для сглаживания %K
        int optInSlowDPeriod,        // Период для расчета линии %D
        Core.MAType optInSlowDMAType // Тип скользящей средней для сглаживания %D
        ) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона нулевым значением (пока нет валидных данных)
        outRange = Range.EndAt(0);

        // Валидация входного диапазона и длины массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }
        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности параметров периодов
        if (optInFastKPeriod < 1 || optInSlowKPeriod < 1 || optInSlowDPeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        /* В стохастическом осцилляторе определяются 4 линии:
         *   FastK, FastD, SlowK и SlowD
         *
         *   FastK(Kperiod) = 100 * (Close - LowestLow) / (HighestHigh - LowestLow)
         *   FastD = Скользящая средняя FastK за период FastD
         *   SlowK = Скользящая средняя FastK за период SlowK (медленная линия %K)
         *   SlowD = Скользящая средняя SlowK за период SlowD (сигнальная линия %D)
         */

        // Период просмотра назад для расчета экстремумов (минус 1, так как текущий бар включается)
        var lookbackK = optInFastKPeriod - 1;
        // Период просмотра назад для сглаживания линии %D
        var lookbackDSlow = MaLookback(optInSlowDPeriod, optInSlowDMAType);
        // Общий период просмотра назад для всего индикатора
        var lookbackTotal = StochLookback(optInFastKPeriod, optInSlowKPeriod, optInSlowKMAType, optInSlowDPeriod, optInSlowDMAType);
        // Корректировка начального индекса с учетом периода просмотра назад
        startIdx = Math.Max(startIdx, lookbackTotal);
        // Если после корректировки нет данных для расчета — возвращаем успех с пустым результатом
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Индекс для записи в промежуточный буфер
        var outIdx = 0;
        // Индекс начала окна для поиска экстремумов
        var trailingIdx = startIdx - lookbackTotal;
        // Текущий индекс обработки (смещен на период поиска экстремумов)
        var today = trailingIdx + lookbackK;

        // Выбор буфера для хранения промежуточных значений Fast %K / Slow %K
        Span<T> tempBuffer;
        if (outSlowK == inHigh || outSlowK == inLow || outSlowK == inClose)
        {
            // Используем выходной буфер SlowK, если он не пересекается с входными данными
            tempBuffer = outSlowK;
        }
        else if (outSlowD == inHigh || outSlowD == inLow || outSlowD == inClose)
        {
            // Или используем выходной буфер SlowD
            tempBuffer = outSlowD;
        }
        else
        {
            // Создаем новый временный буфер
            tempBuffer = new T[endIdx - today + 1];
        }

        // Индексы и значения текущих экстремумов в окне поиска
        int highestIdx = -1, lowestIdx = -1;
        T highest = T.Zero, lowest = T.Zero;

        // Основной цикл расчета сырого значения Fast %K для каждого бара
        while (today <= endIdx)
        {
            // Поиск минимального значения цены Low в окне [trailingIdx, today]
            (lowestIdx, lowest) = FunctionHelpers.CalcLowest(inLow, trailingIdx, today, lowestIdx, lowest);
            // Поиск максимального значения цены High в окне [trailingIdx, today]
            (highestIdx, highest) = FunctionHelpers.CalcHighest(inHigh, trailingIdx, today, highestIdx, highest);

            // Расчет разницы между экстремумами (деленная на 100 для последующего умножения)
            var diff = (highest - lowest) / FunctionHelpers.Hundred<T>();
            // Расчет значения %K: (Close - LowestLow) / (HighestHigh - LowestLow) * 100
            tempBuffer[outIdx++] = !T.IsZero(diff) ? (inClose[today] - lowest) / diff : T.Zero;

            // Сдвиг окна поиска экстремумов
            trailingIdx++;
            today++;
        }

        // Сглаживание промежуточного буфера для получения линии Slow %K
        var retCode = MaImpl(tempBuffer, Range.EndAt(outIdx - 1), tempBuffer, out outRange, optInSlowKPeriod, optInSlowKMAType);
        // Проверка успешности сглаживания
        if (retCode != Core.RetCode.Success || outRange.End.Value == 0)
        {
            return retCode;
        }
        // Количество валидных элементов после первого сглаживания
        var nbElement = outRange.End.Value - outRange.Start.Value;

        // Сглаживание линии Slow %K для получения сигнальной линии %D
        retCode = MaImpl(tempBuffer, Range.EndAt(nbElement - 1), outSlowD, out outRange, optInSlowDPeriod, optInSlowDMAType);
        // Обновление количества валидных элементов после второго сглаживания
        nbElement = outRange.End.Value - outRange.Start.Value;

        // Копирование значений линии Slow %K в выходной массив (с учетом периода сглаживания %D)
        tempBuffer.Slice(lookbackDSlow, nbElement).CopyTo(outSlowK);
        // Проверка успешности второго сглаживания
        if (retCode != Core.RetCode.Success)
        {
            outRange = Range.EndAt(0);
            return retCode;
        }
        // Установка финального диапазона валидных значений (относительно исходных данных)
        outRange = new Range(startIdx, startIdx + nbElement);
        return Core.RetCode.Success;
    }
}
