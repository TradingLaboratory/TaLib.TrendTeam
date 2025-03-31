// Этот файл является частью библиотеки технического анализа для .NET.
// Название файла: FunctionHelpers.cs
namespace TALib;
internal static class FunctionHelpers
{
    /// <summary>
    /// Рассчитывает экспоненциальную скользящую среднюю (EMA) с поддержкой различных алгоритмов инициализации.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (цены или другие временные ряды)</param>
    /// <param name="inRange">Диапазон входных данных для обработки</param>
    /// <param name="outReal">Массив для сохранения результатов EMA</param>
    /// <param name="outRange">Диапазон указывающий для каких ячеек входных данных для расчёта посчитаны валидные значения индикаторов (индексы первой и последней ячейки во входных данных)</param>
    /// <param name="optInTimePeriod">Период EMA (количество баров для расчета)</param>
    /// <param name="optInK1">Коэффициент сглаживания (обычно 2/(optInTimePeriod + 1))</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcExponentialMA<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod,
        T optInK1) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Расчет lookback-периода: минимальное количество баров для первого валидного значения
        var lookbackTotal = Functions.EmaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        var outBegIdx = startIdx; // Индекс первого валидного значения в выходных данных

        /* Выполнение расчета EMA с использованием оптимизированных циклов.
         * 
         * Первое значение EMA рассчитывается особым образом и становится начальным значением (seed) для последующих расчетов.
         * 
         * Реализованы 3 варианта алгоритма инициализации:
         * 
         * Classic (Классический):
         *   Используется простое скользящее среднее (SMA) для первых 'optInTimePeriod' баров.
         *   Наиболее распространенный метод, описанный в литературе.
         *   
         * Metastock:
         *   В качестве начального значения используется значение первого бара из доступных данных.
         */

        int today; // Текущий индекс обрабатываемого бара
        T prevMA; // Предыдущее значение EMA (используется для расчета следующих значений)

        if (Core.CompatibilitySettings.Get() == Core.CompatibilityMode.Default)
        {
            // Классический метод: SMA для первых 'optInTimePeriod' баров
            today = startIdx - lookbackTotal;
            var i = optInTimePeriod; // Счетчик для суммирования первых значений
            var tempReal = T.Zero; // Временная переменная для накопления суммы

            while (i-- > 0)
            {
                tempReal += inReal[today++]; // Суммирование первых 'optInTimePeriod' значений
            }

            prevMA = tempReal / T.CreateChecked(optInTimePeriod); // SMA = Σ(close) / period
        }
        else
        {
            // Metastock-метод: начальное значение = первое доступное значение
            prevMA = inReal[0]; // Начальное значение EMA = первое значение входных данных
            today = 1; // Начинаем обработку со второго бара
        }

        // На этом этапе prevMA содержит начальное значение EMA (seed)
        // 'today' отслеживает текущую позицию в исходных данных

        // Пропуск нестабильного периода (расчет без сохранения результатов)
        while (today <= startIdx)
        {
            // Формула EMA: EMA(t) = EMA(t-1) + k1*(close(t) - EMA(t-1))
            prevMA = (inReal[today++] - prevMA) * optInK1 + prevMA;
        }

        // Сохранение первого валидного значения
        outReal[0] = prevMA;
        var outIdx = 1; // Индекс для записи результатов в выходной массив

        // Расчет оставшихся значений
        while (today <= endIdx)
        {
            prevMA = (inReal[today++] - prevMA) * optInK1 + prevMA;
            outReal[outIdx++] = prevMA; // Запись очередного значения EMA
        }

        outRange = new Range(outBegIdx, outBegIdx + outIdx); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Рассчитывает индикатор MACD (Moving Average Convergence Divergence) с сигнальной линией и гистограммой.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (например, цены закрытия)</param>
    /// <param name="inRange">Диапазон входных данных для обработки</param>
    /// <param name="outMacd">Массив для значений основной линии MACD (Fast EMA - Slow EMA)</param>
    /// <param name="outMacdSignal">Массив для значений сигнальной линии (EMA от MACD)</param>
    /// <param name="outMacdHist">Массив для гистограммы MACD (MACD - Signal)</param>
    /// <param name="outRange">Диапазон выходных данных (индексы первого и последнего валидных значений)</param>
    /// <param name="optInFastPeriod">Период быстрой EMA (по умолчанию 12)</param>
    /// <param name="optInSlowPeriod">Период медленной EMA (по умолчанию 26)</param>
    /// <param name="optInSignalPeriod">Период сигнальной линии (по умолчанию 9)</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcMACD<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outMacd,
        Span<T> outMacdSignal,
        Span<T> outMacdHist,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        int optInSignalPeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Проверка и корректировка периодов: медленный период должен быть больше быстрого
        if (optInSlowPeriod < optInFastPeriod)
        {
            (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
        }

        T k1; // Коэффициент сглаживания для медленной EMA (k = 2/(period + 1))
        T k2; // Коэффициент сглаживания для быстрой EMA

        // Обработка стандартного случая MACD(26,12,9)
        if (optInSlowPeriod != 0)
        {
            k1 = T.CreateChecked(2) / (T.CreateChecked(optInSlowPeriod) + T.One); // k1 = 2/(period + 1)
        }
        else
        {
            optInSlowPeriod = 26; // Установка стандартного значения 26
            k1 = T.CreateChecked(0.075); // k1 = 2/(26+1) ≈ 0.075
        }

        if (optInFastPeriod != 0)
        {
            k2 = T.CreateChecked(2) / (T.CreateChecked(optInFastPeriod) + T.One); // k2 = 2/(period + 1)
        }
        else
        {
            optInFastPeriod = 12; // Установка стандартного значения 12
            k2 = T.CreateChecked(0.15); // k2 = 2/(12+1) ≈ 0.15
        }

        // Расчет lookback-периодов
        var lookbackSignal = Functions.EmaLookback(optInSignalPeriod); // Для сигнальной линии
        var lookbackTotal = Functions.MacdLookback(optInFastPeriod, optInSlowPeriod, optInSignalPeriod); // Общий lookback
        startIdx = Math.Max(startIdx, lookbackTotal); // Корректировка начального индекса

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Выделение временных буферов для EMA
        var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
        Span<T> fastEMABuffer = new T[tempInteger]; // Буфер для быстрой EMA
        Span<T> slowEMABuffer = new T[tempInteger]; // Буфер для медленной EMA

        /* Расчет медленной EMA
         * 
         * Сдвигаем начальный индекс для обеспечения данных, необходимых для сигнального периода.
         * Это гарантирует, что выходные данные будут начинаться с запрошенного 'startIdx'.
         */
        tempInteger = startIdx - lookbackSignal;
        var retCode = CalcExponentialMA(inReal, new Range(tempInteger, endIdx), slowEMABuffer, out var outRange1, optInSlowPeriod, k1);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Расчет быстрой EMA
        retCode = CalcExponentialMA(inReal, new Range(tempInteger, endIdx), fastEMABuffer, out _, optInFastPeriod, k2);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement1 = outRange1.End.Value - outRange1.Start.Value; // Количество элементов в буферах EMA

        // Расчет основной линии MACD: Fast EMA - Slow EMA
        for (var i = 0; i < nbElement1; i++)
        {
            fastEMABuffer[i] -= slowEMABuffer[i];
        }

        // Копирование результата в выходной массив MACD
        fastEMABuffer.Slice(lookbackSignal, endIdx - startIdx + 1).CopyTo(outMacd);

        // Расчет сигнальной линии (EMA от MACD)
        var k1Period = T.CreateChecked(2) / (T.CreateChecked(optInSignalPeriod) + T.One); // Коэффициент для сигнальной EMA
        retCode = CalcExponentialMA(fastEMABuffer, Range.EndAt(nbElement1 - 1), outMacdSignal, out var outRange2, optInSignalPeriod, k1Period);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        var nbElement2 = outRange2.End.Value - outRange2.Start.Value; // Количество элементов сигнальной линии

        // Расчет гистограммы: MACD - Signal
        for (var i = 0; i < nbElement2; i++)
        {
            outMacdHist[i] = outMacd[i] - outMacdSignal[i];
        }

        outRange = new Range(startIdx, startIdx + nbElement2); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Рассчитывает ценовой осциллятор на основе разности/отношения двух скользящих средних.
    /// Поддерживает режимы вывода: абсолютные значения (Fast MA - Slow MA) или процентное отношение ((Fast MA - Slow MA)/Slow MA * 100).
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (например, цены закрытия)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения осциллятора.  
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
    /// <param name="optInFastPeriod">Период быстрой скользящей средней (должен быть меньше медленного периода)</param>
    /// <param name="optInSlowPeriod">Период медленной скользящей средней (должен быть больше быстрого периода)</param>
    /// <param name="optInMethod">Метод расчета скользящих средних (SMA, EMA, и др.)</param>
    /// <param name="tempBuffer">Временный буфер для хранения промежуточных значений быстрой скользящей средней</param>
    /// <param name="doPercentageOutput">
    /// Флаг режима вывода:  
    /// - <c>true</c> — результат в процентах ((Fast MA - Slow MA)/Slow MA * 100)  
    /// - <c>false</c> — абсолютная разность (Fast MA - Slow MA)
    /// </param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcPriceOscillator<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInFastPeriod,
        int optInSlowPeriod,
        Core.MAType optInMethod,
        Span<T> tempBuffer,
        bool doPercentageOutput) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        // Корректировка периодов: медленная MA должна иметь больший период
        if (optInSlowPeriod < optInFastPeriod)
        {
            (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
        }

        // Расчет быстрой скользящей средней во временный буфер
        var retCode = Functions.Ma(inReal, inRange, tempBuffer, out var outRange2, optInFastPeriod, optInMethod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Расчет медленной скользящей средней в выходной массив
        retCode = Functions.Ma(inReal, inRange, outReal, out var outRange1, optInSlowPeriod, optInMethod);
        if (retCode != Core.RetCode.Success)
        {
            return retCode;
        }

        // Синхронизация индексов между быстрой и медленной MA
        for (int i = 0, j = outRange1.Start.Value - outRange2.Start.Value;
             i < outRange1.End.Value - outRange1.Start.Value;
             i++, j++)
        {
            if (doPercentageOutput)
            {
                // Процентный осциллятор: ((Fast MA - Slow MA) / Slow MA) * 100
                var tempReal = outReal[i];
                outReal[i] = !T.IsZero(tempReal)
                    ? (tempBuffer[j] - tempReal) / tempReal * T.CreateChecked(100)
                    : T.Zero;
            }
            else
            {
                // Абсолютный осциллятор: Fast MA - Slow MA
                outReal[i] = tempBuffer[j] - outReal[i];
            }
        }

        outRange = new Range(outRange1.Start.Value, outRange1.End.Value); // Формирование выходного диапазона

        return retCode;
    }

    /// <summary>
    /// Рассчитывает простое скользящее среднее (SMA) для временного ряда.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (цены, индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения SMA.  
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
    /// <param name="optInTimePeriod">Период SMA (количество баров для расчета среднего)</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcSimpleMA<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Расчет lookback-периода: минимальное количество баров для первого валидного значения
        var lookbackTotal = Functions.SmaLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal); // Корректировка начального индекса

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success; // Недостаточно данных для расчета
        }

        var periodTotal = T.Zero; // Сумма значений в текущем окне SMA
        var trailingIdx = startIdx - lookbackTotal; // Индекс первого элемента в текущем окне
        var i = trailingIdx; // Текущий индекс для итерации по входным данным

        // Инициализация суммы для первого окна SMA (если период > 1)
        if (optInTimePeriod > 1)
        {
            while (i < startIdx)
            {
                periodTotal += inReal[i++]; // Накопление суммы для первого расчета
            }
        }

        var outIdx = 0; // Индекс для записи результатов в выходной массив
        var timePeriod = T.CreateChecked(optInTimePeriod); // Период в виде числа с плавающей точкой

        // Основной цикл расчета SMA
        do
        {
            periodTotal += inReal[i++]; // Добавление нового значения в окно
            var tempReal = periodTotal; // Сохранение текущей суммы перед удалением старого значения
            periodTotal -= inReal[trailingIdx++]; // Удаление самого старого значения из окна
            outReal[outIdx++] = tempReal / timePeriod; // Расчет SMA и сохранение результата
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Рассчитывает дисперсию временного ряда на основе скользящего окна.
    /// Формула: дисперсия = (среднее квадратов значений) - (среднее значение)^2.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inReal">Входные данные для расчета (цены, индикаторы или другие временные ряды)</param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal"/> (начальный и конечный индексы).  
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal"/>.
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения дисперсии.  
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
    /// <param name="optInTimePeriod">Период для расчета дисперсии (количество баров в окне)</param>
    /// <returns>Код возврата (успех/ошибка)</returns>
    public static Core.RetCode CalcVariance<T>(
        ReadOnlySpan<T> inReal,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0); // Инициализация выходного диапазона

        var startIdx = inRange.Start.Value; // Начальный индекс входных данных
        var endIdx = inRange.End.Value; // Конечный индекс входных данных

        // Расчет lookback-периода: минимальное количество баров для первого валидного значения
        var lookbackTotal = Functions.VarLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal); // Корректировка начального индекса

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success; // Недостаточно данных для расчета
        }

        T periodTotal1 = T.Zero; // Сумма значений в текущем окне
        T periodTotal2 = T.Zero; // Сумма квадратов значений в текущем окне
        var trailingIdx = startIdx - lookbackTotal; // Индекс первого элемента в текущем окне
        var i = trailingIdx; // Текущий индекс для итерации по входным данным

        // Инициализация сумм для первого окна (если период > 1)
        if (optInTimePeriod > 1)
        {
            while (i < startIdx)
            {
                var tempReal = inReal[i++];
                periodTotal1 += tempReal; // Сумма значений
                tempReal *= tempReal;
                periodTotal2 += tempReal; // Сумма квадратов значений
            }
        }

        var outIdx = 0; // Индекс для записи результатов в выходной массив
        var timePeriod = T.CreateChecked(optInTimePeriod); // Период в виде числа с плавающей точкой

        // Основной цикл расчета дисперсии
        do
        {
            var tempReal = inReal[i++]; // Текущее значение

            // Обновление сумм для нового значения
            periodTotal1 += tempReal; // Сумма значений
            tempReal *= tempReal;
            periodTotal2 += tempReal; // Сумма квадратов

            // Расчет средних значений
            var meanValue1 = periodTotal1 / timePeriod; // Среднее значение
            var meanValue2 = periodTotal2 / timePeriod; // Среднее квадратов

            // Удаление самого старого значения из окна
            tempReal = inReal[trailingIdx++];
            periodTotal1 -= tempReal; // Корректировка суммы значений
            tempReal *= tempReal;
            periodTotal2 -= tempReal; // Корректировка суммы квадратов

            // Формула дисперсии: Var = E[X²] - (E[X])²
            outReal[outIdx++] = meanValue2 - meanValue1 * meanValue1;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx); // Формирование выходного диапазона

        return Core.RetCode.Success;
    }

    /// <summary>
    /// Рассчитывает значение индикатора накопления/распределения (Accumulation/Distribution) для текущего бара.
    /// Формула: AD += ((Close - Low) - (High - Close)) / (High - Low) * Volume.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="high">Массив максимальных цен за период</param>
    /// <param name="low">Массив минимальных цен за период</param>
    /// <param name="close">Массив цен закрытия</param>
    /// <param name="volume">Массив объемов торгов</param>
    /// <param name="today">Текущий индекс обрабатываемого бара (будет увеличен на 1 после расчета)</param>
    /// <param name="ad">Текущее значение индикатора накопления/распределения</param>
    /// <returns>Обновленное значение индикатора AD</returns>
    public static T CalcAccumulationDistribution<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ReadOnlySpan<T> volume,
        ref int today,
        T ad) where T : IFloatingPointIeee754<T>
    {
        // Текущие значения цен и объема
        var h = high[today]; // Максимальная цена текущего бара
        var l = low[today];  // Минимальная цена текущего бара
        var c = close[today]; // Цена закрытия текущего бара

        // Диапазон цен (High - Low) для текущего бара
        var tmp = h - l;

        // Расчет изменения индикатора только при значимом ценовом диапазоне
        if (tmp > T.Zero)
        {
            // Формула: ((Close - Low) - (High - Close)) / (High - Low) * Volume
            // Упрощенно: (2*Close - High - Low) / (High - Low) * Volume
            // Показывает, ближе ли закрытие к High (накопление) или к Low (распределение)
            var moneyFlowMultiplier = (c - l) - (h - c);
            moneyFlowMultiplier /= tmp;
            ad += moneyFlowMultiplier * volume[today];
        }

        today++; // Переход к следующему бару

        return ad;
    }

    /// <summary>
    /// Рассчитывает минимальное значение в скользящем окне данных с учетом исторических значений.
    /// Используется для поиска экстремумов в временных рядах (например, минимумов цен).
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="input">Входной временной ряд (например, цены Low)</param>
    /// <param name="trailingIdx">Начальный индекс текущего окна анализа</param>
    /// <param name="today">Текущий индекс обрабатываемого бара</param>
    /// <param name="lowestIdx">Индекс предыдущего минимального значения</param>
    /// <param name="lowest">Значение предыдущего минимума</param>
    /// <returns>
    /// Кортеж: 
    /// - <b>Item1</b>: Индекс обновленного минимального значения в окне  
    /// - <b>Item2</b>: Значение обновленного минимума
    /// </returns>
    public static (int, T) CalcLowest<T>(
        ReadOnlySpan<T> input,
        int trailingIdx,
        int today,
        int lowestIdx,
        T lowest)
        where T : IFloatingPointIeee754<T>
    {
        var tmp = input[today]; // Текущее значение для сравнения
        var lIdx = lowestIdx; // Индекс текущего минимума
        var l = lowest; // Значение текущего минимума

        // Если предыдущий минимум вышел за границу окна - инициализация нового поиска
        if (lIdx < trailingIdx)
        {
            lIdx = trailingIdx; // Начинаем с нового начального индекса
            l = input[lIdx]; // Инициализация текущего минимума

            // Поиск минимума в диапазоне [trailingIdx, today]
            var i = lIdx;
            while (++i <= today)
            {
                tmp = input[i];
                if (tmp > l) continue; // Пропуск значений больше текущего минимума

                lIdx = i; // Обновление индекса минимума
                l = tmp; // Обновление значения минимума
            }
        }
        // Если текущее значение меньше или равно текущему минимуму
        else if (tmp <= l)
        {
            lIdx = today; // Обновление индекса минимума
            l = tmp; // Обновление значения минимума
        }

        return (lIdx, l); // Возврат обновленных индекса и значения
    }

    /// <summary>
    /// Рассчитывает максимальное значение в скользящем окне данных с учетом исторических значений.
    /// Используется для поиска экстремумов в временных рядах (например, максимумов цен).
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="input">Входной временной ряд (например, цены High)</param>
    /// <param name="trailingIdx">Начальный индекс текущего окна анализа</param>
    /// <param name="today">Текущий индекс обрабатываемого бара</param>
    /// <param name="highestIdx">Индекс предыдущего максимального значения</param>
    /// <param name="highest">Значение предыдущего максимума</param>
    /// <returns>
    /// Кортеж: 
    /// - <b>Item1</b>: Индекс обновленного максимального значения в окне  
    /// - <b>Item2</b>: Значение обновленного максимума
    /// </returns>
    public static (int, T) CalcHighest<T>(
        ReadOnlySpan<T> input,
        int trailingIdx,
        int today,
        int highestIdx,
        T highest)
        where T : IFloatingPointIeee754<T>
    {
        var tmp = input[today]; // Текущее значение для сравнения
        var hIdx = highestIdx; // Индекс текущего максимума
        var h = highest; // Значение текущего максимума

        // Если предыдущий максимум вышел за границу окна - инициализация нового поиска
        if (hIdx < trailingIdx)
        {
            hIdx = trailingIdx; // Начинаем с нового начального индекса
            h = input[hIdx]; // Инициализация текущего максимума

            // Поиск максимума в диапазоне [trailingIdx, today]
            var i = hIdx;
            while (++i <= today)
            {
                tmp = input[i];
                if (tmp < h) continue; // Пропуск значений меньше текущего максимума

                hIdx = i; // Обновление индекса максимума
                h = tmp; // Обновление значения максимума
            }
        }
        // Если текущее значение больше или равно текущему максимуму
        else if (tmp >= h)
        {
            hIdx = today; // Обновление индекса максимума
            h = tmp; // Обновление значения максимума
        }

        return (hIdx, h); // Возврат обновленных индекса и значения
    }

    /// <summary>
    /// Инициализирует начальные значения Directional Movement (DM) и True Range (TR) для расчета индикаторов.
    /// Выполняет предварительные вычисления для заданного периода, обновляя состояния DM и TR.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="high">Массив максимальных цен</param>
    /// <param name="low">Массив минимальных цен</param>
    /// <param name="close">Массив цен закрытия</param>
    /// <param name="prevHigh">Предыдущее значение High (будет инициализировано)</param>
    /// <param name="today">Текущий индекс обрабатываемого бара (будет увеличен в цикле)</param>
    /// <param name="prevLow">Предыдущее значение Low (будет инициализировано)</param>
    /// <param name="prevClose">Предыдущее значение Close (инициализируется нулем, если <paramref name="close"/> пуст)</param>
    /// <param name="timePeriod">Период для расчета (определяет количество итераций инициализации)</param>
    /// <param name="prevPlusDM">Ссылка на предыдущее значение плюс Directional Movement (+DM)</param>
    /// <param name="prevMinusDM">Ссылка на предыдущее значение минус Directional Movement (-DM)</param>
    /// <param name="prevTR">Ссылка на предыдущее значение True Range</param>
    public static void InitDMAndTR<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        out T prevHigh,
        ref int today,
        out T prevLow,
        out T prevClose,
        T timePeriod,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR) where T : IFloatingPointIeee754<T>
    {
        // Инициализация начальных значений на текущем баре
        prevHigh = high[today];
        prevLow = low[today];
        prevClose = !close.IsEmpty ? close[today] : T.Zero; // Если close пуст, используется 0

        // Выполняем инициализацию для (timePeriod - 1) баров
        for (var i = Int32.CreateTruncating(timePeriod) - 1; i > 0; i--)
        {
            today++; // Переход к следующему бару

            // Обновление DM и TR для подготовки начальных данных
            UpdateDMAndTR(
                high, low, close,
                ref today,
                ref prevHigh, ref prevLow, ref prevClose,
                ref prevPlusDM, ref prevMinusDM, ref prevTR,
                timePeriod,
                false); // Флаг false указывает на режим инициализации
        }
    }


    /// <summary>
    /// Обновляет значения Directional Movement (+DM/-DM) и True Range (TR) для текущего бара.
    /// Используется для расчета индикаторов, таких как ADX и DI.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="high">Массив максимальных цен</param>
    /// <param name="low">Массив минимальных цен</param>
    /// <param name="close">Массив цен закрытия</param>
    /// <param name="today">Текущий индекс обрабатываемого бара (будет увеличен при необходимости)</param>
    /// <param name="prevHigh">Предыдущее значение High (будет обновлено)</param>
    /// <param name="prevLow">Предыдущее значение Low (будет обновлено)</param>
    /// <param name="prevClose">Предыдущее значение Close (будет обновлено)</param>
    /// <param name="prevPlusDM">Ссылка на текущее значение плюс Directional Movement (+DM)</param>
    /// <param name="prevMinusDM">Ссылка на текущее значение минус Directional Movement (-DM)</param>
    /// <param name="prevTR">Ссылка на текущее значение True Range</param>
    /// <param name="timePeriod">Период для сглаживания (используется при <paramref name="applySmoothing"/>)</param>
    /// <param name="applySmoothing">
    /// Флаг применения сглаживания:  
    /// - <c>true</c> — используется экспоненциальное сглаживание (Wilders Smoothing).  
    /// - <c>false</c> — простое суммирование без сглаживания.
    /// </param>
    public static void UpdateDMAndTR<T>(
        ReadOnlySpan<T> high,
        ReadOnlySpan<T> low,
        ReadOnlySpan<T> close,
        ref int today,
        ref T prevHigh,
        ref T prevLow,
        ref T prevClose,
        ref T prevPlusDM,
        ref T prevMinusDM,
        ref T prevTR,
        T timePeriod,
        bool applySmoothing = true)
        where T : IFloatingPointIeee754<T>
    {
        // Расчет изменений (+DM и -DM) для текущего бара
        var (diffP, diffM) = CalcDeltas(high, low, today, ref prevHigh, ref prevLow);

        // Применение сглаживания к DM (Wilders Smoothing: DM = DM_prev * (period-1)/period + new_value)
        if (applySmoothing)
        {
            prevPlusDM -= prevPlusDM / timePeriod;  // DM+ = DM+ * (period-1)/period
            prevMinusDM -= prevMinusDM / timePeriod; // DM- = DM- * (period-1)/period
        }

        // Обновление DM в зависимости от направления движения
        if (diffM > T.Zero && diffP < diffM)
        {
            // Случай 2 и 4: движение вниз преобладает → обновляем -DM
            prevMinusDM += diffM;
        }
        else if (diffP > T.Zero && diffP > diffM)
        {
            // Случай 1 и 3: движение вверх преобладает → обновляем +DM
            prevPlusDM += diffP;
        }

        // Если массив close пуст — завершаем обработку (TR не требуется)
        if (close.IsEmpty)
        {
            return;
        }

        // Расчет True Range (максимальный диапазон за бар: max(H-L, |H-PrevClose|, |L-PrevClose|))
        var trueRange = TrueRange(prevHigh, prevLow, prevClose);

        // Обновление True Range с учетом сглаживания
        if (applySmoothing)
        {
            // TR = TR_prev * (period-1)/period + new_TR
            prevTR = prevTR - prevTR / timePeriod + trueRange;
        }
        else
        {
            // Простое суммирование без сглаживания
            prevTR += trueRange;
        }

        // Обновление предыдущего Close для следующего бара
        prevClose = close[today];
    }


    /// <summary>
    /// Рассчитывает дельты направленного движения (+DM и -DM) для текущего бара.
    /// Используется для определения силы восходящего и нисходящего трендов.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="inHigh">Массив максимальных цен</param>
    /// <param name="inLow">Массив минимальных цен</param>
    /// <param name="today">Текущий индекс обрабатываемого бара</param>
    /// <param name="prevHigh">Предыдущее значение High (будет обновлено текущим значением)</param>
    /// <param name="prevLow">Предыдущее значение Low (будет обновлено текущим значением)</param>
    /// <returns>
    /// Кортеж:  
    /// - <b>diffP</b>: Плюс дельта (+DM) = Current High - Previous High  
    /// - <b>diffM</b>: Минус дельта (-DM) = Previous Low - Current Low
    /// </returns>
    public static (T diffP, T diffM) CalcDeltas<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        int today,
        ref T prevHigh,
        ref T prevLow) where T : IFloatingPointIeee754<T>
    {
        // Плюс дельта: рост максимума относительно предыдущего бара
        var diffP = inHigh[today] - prevHigh;

        // Минус дельта: падение минимума относительно предыдущего бара
        var diffM = prevLow - inLow[today];

        // Обновление предыдущих значений для следующего расчета
        prevHigh = inHigh[today];
        prevLow = inLow[today];

        return (diffP, diffM);
    }

    /// <summary>
    /// Рассчитывает индексы направленного движения (DI) на основе Directional Movement и True Range.
    /// Используется для оценки силы восходящего (+DI) и нисходящего (-DI) трендов.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="prevMinusDM">Предыдущее значение минус направленного движения (-DM)</param>
    /// <param name="prevPlusDM">Предыдущее значение плюс направленного движения (+DM)</param>
    /// <param name="prevTR">Предыдущее значение истинного диапазона (True Range)</param>
    /// <returns>
    /// Кортеж:  
    /// - <b>minusDI</b>: Индекс нисходящего тренда = (-DM / TR) * 100  
    /// - <b>plusDI</b>: Индекс восходящего тренда = (+DM / TR) * 100
    /// </returns>
    public static (T minusDI, T plusDI) CalcDI<T>(
        T prevMinusDM,
        T prevPlusDM,
        T prevTR) where T : IFloatingPointIeee754<T>
    {
        // Расчет -DI: (-DM / TR) * 100 (сила нисходящего тренда)
        var minusDI = T.CreateChecked(100) * (prevMinusDM / prevTR);

        // Расчет +DI: (+DM / TR) * 100 (сила восходящего тренда)
        var plusDI = T.CreateChecked(100) * (prevPlusDM / prevTR);

        return (minusDI, plusDI);
    }


    /// <summary>
    /// Рассчитывает истинный диапазон (True Range) для текущего бара.
    /// Используется в индикаторах вроде Average True Range (ATR) для оценки волатильности.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="th">Текущий максимум (High) бара</param>
    /// <param name="tl">Текущий минимум (Low) бара</param>
    /// <param name="yc">Предыдущее значение закрытия (Yesterday's Close)</param>
    /// <returns>
    /// Максимальное значение из трех вариантов:  
    /// 1. Разница между текущими High и Low  
    /// 2. Абсолютная разница между текущим High и предыдущим Close  
    /// 3. Абсолютная разница между текущим Low и предыдущим Close
    /// </returns>
    public static T TrueRange<T>(T th, T tl, T yc) where T : IFloatingPointIeee754<T>
    {
        // 1. Базовый диапазон: High - Low
        var range = th - tl;

        // 2. Сравнение с разницей между High и предыдущим Close
        range = T.Max(range, T.Abs(th - yc));

        // 3. Сравнение с разницей между Low и предыдущим Close
        range = T.Max(range, T.Abs(tl - yc));

        return range;
    }


    /// <summary>
    /// Выполняет расчет взвешенного скользящего среднего (WMA) с учетом весовых коэффициентов.
    /// Используется для сглаживания временных рядов с акцентом на последние значения.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="real">Входной временной ряд (например, цены)</param>
    /// <param name="idx">Текущий индекс обрабатываемого бара (будет увеличен на 1)</param>
    /// <param name="periodWMASub">Сумма значений для коррекции окна (используется для удаления старых данных)</param>
    /// <param name="periodWMASum">Взвешенная сумма значений для текущего окна</param>
    /// <param name="trailingWMAValue">Значение, которое будет удалено из окна при следующей итерации</param>
    /// <param name="varNewPrice">Новое значение, добавляемое в расчет (текущая цена)</param>
    /// <param name="varToStoreSmoothedValue">Результирующее сглаженное значение WMA</param>
    public static void DoPriceWma<T>(
        ReadOnlySpan<T> real,
        ref int idx,
        ref T periodWMASub,
        ref T periodWMASum,
        ref T trailingWMAValue,
        T varNewPrice,
        out T varToStoreSmoothedValue) where T : IFloatingPointIeee754<T>
    {
        // Обновление суммы для коррекции окна: добавляем новое значение, удаляем старое
        periodWMASub += varNewPrice;
        periodWMASub -= trailingWMAValue;

        // Накопление взвешенной суммы: новое значение умножается на весовой коэффициент (4)
        periodWMASum += varNewPrice * T.CreateChecked(4); // 4 — вес для текущего значения

        // Обновление значения, которое будет удалено в следующей итерации
        trailingWMAValue = real[idx++];

        // Расчет сглаженного значения: (взвешенная сумма) * нормирующий коэффициент (0.1)
        varToStoreSmoothedValue = periodWMASum * T.CreateChecked(0.1);

        // Коррекция взвешенной суммы для следующего расчета
        periodWMASum -= periodWMASub;
    }


    /// <summary>
    /// Класс для работы с преобразованиями Хилберта (Hilbert Transform) и аналитическими сигналами.
    /// Используется для вычисления квадратурных компонент, огибающих и фазовых характеристик временных рядов.
    /// </summary>
    public static class HTHelper
    {

        /// <summary>
        /// Ключи для управления буферами и состояниями при преобразованиях Хилберта.
        /// Содержит индексы для хранения промежуточных данных фильтров и квадратурных компонент.
        /// </summary>
        public enum HilbertKeys
        {
            // Detrender (фильтрация сигнала)

            // DetrenderOdd = 0-2
            // DetrenderEven = 3-5

            /// <summary>Основной буфер детрендера</summary>
            Detrender = 6,

            // PrevDetrenderOdd = 7
            // PrevDetrenderEven = 8
            // PrevDetrenderInputOdd = 9
            // PrevDetrenderInputEven = 10

            // Q1Odd = 11-13
            // Q1Even = 14-16

            /// <summary>Буфер первой квадратурной компоненты</summary>
            Q1 = 17,

            // PrevQ1Odd = 18
            // PrevQ1Even = 19
            // PrevQ1InputOdd = 20
            // PrevQ1InputEven = 21

            // JIOdd = 22-24
            // JIEven = 25-27

            /// <summary>Буфер инвертора Джилля (квадратурная компонента)</summary>
            JI = 28,

            // PrevJIOdd = 29
            // PrevJIEven = 30
            // PrevJIInputOdd = 31
            // PrevJIInputEven = 32

            // JQOdd = 33-35
            // JQEven = 36-38

            /// <summary>Буфер квадратурного фильтра Джилля</summary>
            JQ = 39

            // PrevJQOdd = 40
            // PrevJQEven = 41
            // PrevJQInputOdd = 42
            // PrevJQInputEven = 43
        }

        /// <summary>
        /// Создает буфер фиксированного размера для хранения промежуточных данных преобразований Хилберта.
        /// Размер буфера соответствует 4 * 11 элементам, что связано с количеством внутренних состояний и коэффициентов,
        /// определенных в <see cref="HilbertKeys"/>.
        /// </summary>
        /// <typeparam name="T">Тип данных с плавающей точкой (например, <see cref="double"/> или <see cref="float"/>)</typeparam>
        /// <returns>Массив длиной 44 элемента (4 * 11) для хранения промежуточных вычислений</returns>
        public static T[] BufferFactory<T>() where T : IFloatingPointIeee754<T>
        {
            // Размер 4 * 11 обусловлен количеством буферов и их размерностью в HilbertKeys (4 группы по 11 элементов)
            return new T[4 * 11];
        }


        /// <summary>
        /// Инициализирует взвешенное скользящее среднее (WMA) для сглаживания временных рядов.
        /// Используется для подготовки буферов, необходимых при расчетах преобразований Хилберта.
        /// </summary>
        /// <param name="real">Входные данные временного ряда (например, цены)</param>
        /// <param name="startIdx">Начальный индекс во входных данных</param>
        /// <param name="lookbackTotal">Период инициализации (количество баров для расчета начального значения)</param>
        /// <param name="periodWMASub">Буфер для накопления разностей взвешенных сумм</param>
        /// <param name="periodWMASum">Буфер для накопления взвешенных сумм</param>
        /// <param name="trailingWMAValue">Текущее значение WMA для "хвостового" периода</param>
        /// <param name="trailingWMAIdx">Индекс начала "хвостового" периода в исходных данных</param>
        /// <param name="period">Период WMA (используется для полной инициализации буфера)</param>
        /// <param name="today">Индекс текущего обрабатываемого элемента во входных данных</param>
        /// <typeparam name="T">Тип данных с плавающей точкой (например, <see cref="double"/> или <see cref="float"/>)</typeparam>
        public static void InitWma<T>(
            ReadOnlySpan<T> real,
            int startIdx,
            int lookbackTotal,
            out T periodWMASub,
            out T periodWMASum,
            out T trailingWMAValue,
            out int trailingWMAIdx,
            int period,
            out int today) where T : IFloatingPointIeee754<T>
        {
            // Инициализация сглаживателя цены через взвешенное скользящее среднее (WMA)
            trailingWMAIdx = startIdx - lookbackTotal;  // Индекс начала периода инициализации
            today = trailingWMAIdx;  // Текущая позиция обработки в данных

            // Первые 3 элемента обрабатываются отдельно для оптимизации (развернутый цикл)
            var tempReal = real[today++];  // Первое значение
            periodWMASub = tempReal;        // Начальное значение разностной суммы
            periodWMASum = tempReal;        // Начальное значение взвешенной суммы

            tempReal = real[today++];       // Второе значение
            periodWMASub += tempReal;       // Добавление в разностную сумму
            periodWMASum += tempReal * Two<T>();  // Взвешивание с коэффициентом 2

            tempReal = real[today++];       // Третье значение
            periodWMASub += tempReal;       // Добавление в разностную сумму
            periodWMASum += tempReal * Three<T>();  // Взвешивание с коэффициентом 3

            trailingWMAValue = T.Zero;      // Инициализация хвостового значения WMA

            // Полная инициализация буфера для заданного периода WMA
            for (var i = period; i != 0; i--)
            {
                tempReal = real[today++];
                DoPriceWma(real, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
            }
        }


        /// <summary>
        /// Выполняет этап преобразования Хилберта для нечетных периодов, вычисляя квадратурные компоненты и обновляя буферы состояний.
        /// Используется для анализа фазовых и амплитудных характеристик сигнала.
        /// </summary>
        /// <param name="hilbertBuffer">Буфер для хранения промежуточных данных преобразования (размер определяется <see cref="HilbertKeys"/>)</param>
        /// <param name="smoothedValue">Сглаженное значение временного ряда (например, цена после WMA)</param>
        /// <param name="hilbertIdx">Индекс текущей итерации в буфере преобразования</param>
        /// <param name="adjustedPrevPeriod">Скорректированный предыдущий период для адаптации преобразования</param>
        /// <param name="i1ForEvenPrev3">Буфер для хранения детрендера с задержкой 3 бара (используется в четных периодах)</param>
        /// <param name="prevQ2">Предыдущее значение квадратурной компоненты Q2</param>
        /// <param name="prevI2">Предыдущее значение квадратурной компоненты I2</param>
        /// <param name="i1ForOddPrev3">Текущее значение детрендера для нечетных периодов с задержкой 3 бара</param>
        /// <param name="i1ForEvenPrev2">Ссылка на буфер для обновления детрендера с задержкой 2 бара (для четных периодов)</param>
        /// <param name="q2">Результирующая квадратурная компонента Q2 (сглаженная)</param>
        /// <param name="i2">Результирующая квадратурная компонента I2 (сглаженная)</param>
        /// <typeparam name="T">Тип данных с плавающей точкой (например, <see cref="double"/> или <see cref="float"/>)</typeparam>
        public static void CalcHilbertOdd<T>(
            Span<T> hilbertBuffer,
            T smoothedValue,
            int hilbertIdx,
            T adjustedPrevPeriod,
            out T i1ForEvenPrev3,
            T prevQ2,
            T prevI2,
            T i1ForOddPrev3,
            ref T i1ForEvenPrev2,
            out T q2,
            out T i2) where T : IFloatingPointIeee754<T>
        {
            var tPointTwo = T.CreateChecked(0.2);    // Коэффициент сглаживания 0.2 для фильтрации Q2 и I2
            var tPointEight = T.CreateChecked(0.8);  // Коэффициент сглаживания 0.8 для сохранения предыдущих значений

            // Этап 1: Детрендер (фильтрация исходного сигнала)
            DoHilbertTransform(hilbertBuffer, HilbertKeys.Detrender, smoothedValue, true, hilbertIdx, adjustedPrevPeriod);

            // Этап 2: Первая квадратурная компонента Q1
            var input = hilbertBuffer[(int) HilbertKeys.Detrender];
            DoHilbertTransform(hilbertBuffer, HilbertKeys.Q1, input, true, hilbertIdx, adjustedPrevPeriod);

            // Этап 3: Инвертор Джилля (JI) для коррекции фазы
            DoHilbertTransform(hilbertBuffer, HilbertKeys.JI, i1ForOddPrev3, true, hilbertIdx, adjustedPrevPeriod);

            // Этап 4: Квадратурный фильтр Джилля (JQ)
            var input1 = hilbertBuffer[(int) HilbertKeys.Q1];
            DoHilbertTransform(hilbertBuffer, HilbertKeys.JQ, input1, true, hilbertIdx, adjustedPrevPeriod);

            // Расчет сглаженных квадратурных компонент с весовыми коэффициентами
            q2 = tPointTwo * (hilbertBuffer[(int) HilbertKeys.Q1] + hilbertBuffer[(int) HilbertKeys.JI]) + tPointEight * prevQ2;
            i2 = tPointTwo * (i1ForOddPrev3 - hilbertBuffer[(int) HilbertKeys.JQ]) + tPointEight * prevI2;

            // Сохранение детрендера с задержкой 3 бара для четных периодов
            // I1ForEvenPrev3 используется как входные данные для следующих четных итераций
            i1ForEvenPrev3 = i1ForEvenPrev2;  // Предыдущее значение задержки 2 бара становится задержкой 3 баров
            i1ForEvenPrev2 = hilbertBuffer[(int) HilbertKeys.Detrender];  // Обновление задержки 2 бара текущим детрендером
        }


        /// <summary>
        /// Выполняет этап преобразования Хилберта для четных периодов, вычисляя квадратурные компоненты и обновляя буферы состояний.
        /// Используется для анализа фазовых и амплитудных характеристик сигнала в сочетании с нечетными периодами.
        /// </summary>
        /// <param name="hilbertBuffer">Буфер для хранения промежуточных данных преобразования (размер определяется <see cref="HilbertKeys"/>)</param>
        /// <param name="smoothedValue">Сглаженное значение временного ряда (например, цена после WMA)</param>
        /// <param name="hilbertIdx">Ссылка на индекс текущей итерации в буфере преобразования (циклический счетчик 0-2)</param>
        /// <param name="adjustedPrevPeriod">Скорректированный предыдущий период для адаптации преобразования</param>
        /// <param name="i1ForEvenPrev3">Детрендер с задержкой 3 бара из предыдущего четного периода</param>
        /// <param name="prevQ2">Предыдущее значение квадратурной компоненты Q2</param>
        /// <param name="prevI2">Предыдущее значение квадратурной компоненты I2</param>
        /// <param name="i1ForOddPrev3">Буфер для сохранения детрендера с задержкой 3 бара (используется в нечетных периодах)</param>
        /// <param name="i1ForOddPrev2">Ссылка на буфер для обновления детрендера с задержкой 2 бара (для нечетных периодов)</param>
        /// <param name="q2">Результирующая квадратурная компонента Q2 (сглаженная)</param>
        /// <param name="i2">Результирующая квадратурная компонента I2 (сглаженная)</param>
        /// <typeparam name="T">Тип данных с плавающей точкой (например, <see cref="double"/> или <see cref="float"/>)</typeparam>
        public static void CalcHilbertEven<T>(
            Span<T> hilbertBuffer,
            T smoothedValue,
            ref int hilbertIdx,
            T adjustedPrevPeriod,
            T i1ForEvenPrev3,
            T prevQ2,
            T prevI2,
            out T i1ForOddPrev3,
            ref T i1ForOddPrev2,
            out T q2,
            out T i2) where T : IFloatingPointIeee754<T>
        {
            var tPointTwo = T.CreateChecked(0.2);    // Коэффициент сглаживания 0.2 для фильтрации Q2 и I2
            var tPointEight = T.CreateChecked(0.8);  // Коэффициент сглаживания 0.8 для сохранения предыдущих значений

            // Этап 1: Детрендер (фильтрация исходного сигнала) для четных периодов
            DoHilbertTransform(hilbertBuffer, HilbertKeys.Detrender, smoothedValue, false, hilbertIdx, adjustedPrevPeriod);

            // Этап 2: Первая квадратурная компонента Q1
            var input = hilbertBuffer[(int) HilbertKeys.Detrender];
            DoHilbertTransform(hilbertBuffer, HilbertKeys.Q1, input, false, hilbertIdx, adjustedPrevPeriod);

            // Этап 3: Инвертор Джилля (JI) для коррекции фазы
            DoHilbertTransform(hilbertBuffer, HilbertKeys.JI, i1ForEvenPrev3, false, hilbertIdx, adjustedPrevPeriod);

            // Этап 4: Квадратурный фильтр Джилля (JQ)
            var input1 = hilbertBuffer[(int) HilbertKeys.Q1];
            DoHilbertTransform(hilbertBuffer, HilbertKeys.JQ, input1, false, hilbertIdx, adjustedPrevPeriod);

            // Циклическое обновление индекса буфера (0 → 1 → 2 → 0 ...)
            if (++hilbertIdx == 3)
            {
                hilbertIdx = 0;
            }

            // Расчет сглаженных квадратурных компонент с весовыми коэффициентами
            q2 = tPointTwo * (hilbertBuffer[(int) HilbertKeys.Q1] + hilbertBuffer[(int) HilbertKeys.JI]) + tPointEight * prevQ2;
            i2 = tPointTwo * (i1ForEvenPrev3 - hilbertBuffer[(int) HilbertKeys.JQ]) + tPointEight * prevI2;

            // Сохранение детрендера с задержкой 3 бара для нечетных периодов
            // i1ForOddPrev3 используется как входные данные для следующих нечетных итераций
            i1ForOddPrev3 = i1ForOddPrev2;  // Предыдущее значение задержки 2 бара становится задержкой 3 баров
            i1ForOddPrev2 = hilbertBuffer[(int) HilbertKeys.Detrender];  // Обновление задержки 2 бара текущим детрендером
        }

        /// <summary>
        /// Вычисляет сглаженный период сигнала на основе квадратурных компонент I2 и Q2.
        /// Используется для определения доминирующего цикла во временном ряду.
        /// </summary>
        /// <param name="re">Реальная часть аналитического сигнала (накапливается с коэффициентом 0.2)</param>
        /// <param name="i2">Текущая квадратурная компонента I2</param>
        /// <param name="q2">Текущая квадратурная компонента Q2</param>
        /// <param name="prevI2">Предыдущее значение I2 для расчета корреляции</param>
        /// <param name="prevQ2">Предыдущее значение Q2 для расчета корреляции</param>
        /// <param name="im">Мнимая часть аналитического сигнала (накапливается с коэффициентом 0.2)</param>
        /// <param name="period">Сглаженный период сигнала (в барах)</param>
        /// <typeparam name="T">Тип данных с плавающей точкой (например, <see cref="double"/> или <see cref="float"/>)</typeparam>
        public static void CalcSmoothedPeriod<T>(
            ref T re,
            T i2,
            T q2,
            ref T prevI2,
            ref T prevQ2,
            ref T im,
            ref T period) where T : IFloatingPointIeee754<T>
        {
            var tPointTwo = T.CreateChecked(0.2);    // Коэффициент экспоненциального сглаживания (20% нового значения)
            var tPointEight = T.CreateChecked(0.8);  // Коэффициент сохранения предыдущего состояния (80%)

            // Обновление реальной и мнимой частей аналитического сигнала
            re = tPointTwo * (i2 * prevI2 + q2 * prevQ2) + tPointEight * re;
            im = tPointTwo * (i2 * prevQ2 - q2 * prevI2) + tPointEight * im;

            // Сохранение текущих значений для следующей итерации
            prevQ2 = q2;
            prevI2 = i2;

            // Расчет периода через фазовый угол (в градусах)
            var tempReal1 = period;  // Сохранение предыдущего периода
            if (!T.IsZero(im) && !T.IsZero(re))
            {
                // Формула: period = (90 * 4) / (arctg(im/re) в градусах)
                period = Ninety<T>() * Four<T>() / T.RadiansToDegrees(T.Atan(im / re));
            }

            // Ограничение изменения периода относительно предыдущего значения
            var tempReal2 = T.CreateChecked(1.5) * tempReal1;  // Верхняя граница: +50%
            period = T.Min(period, tempReal2);

            tempReal2 = T.CreateChecked(0.67) * tempReal1;     // Нижняя граница: -33%
            period = T.Max(period, tempReal2);

            // Финальное ограничение периода в диапазоне [6, 50] баров
            period = T.Clamp(period, T.CreateChecked(6), T.CreateChecked(50));

            // Экспоненциальное сглаживание результата (EMA)
            period = tPointTwo * period + tPointEight * tempReal1;
        }

        /// <summary>
        /// Выполняет этап преобразования Хилберта для заданного компонента, обновляя буфер состояний.
        /// Реализует адаптивный фильтр с коэффициентами 0.0962 и 0.5769 для обработки квадратурных сигналов.
        /// </summary>
        /// <param name="buffer">Буфер для хранения промежуточных данных (размер определяется <see cref="HilbertKeys"/>)</param>
        /// <param name="baseKey">Базовый ключ для определения группы буферов (Detrender, Q1, JI, JQ)</param>
        /// <param name="input">Входное значение для текущего этапа преобразования</param>
        /// <param name="isOdd">Флаг нечетного периода (true) или четного (false)</param>
        /// <param name="hilbertIdx">Циклический индекс (0-2) для выбора подбуфера</param>
        /// <param name="adjustedPrevPeriod">Скорректированный предыдущий период для адаптации фильтра</param>
        /// <typeparam name="T">Тип данных с плавающей точкой (например, <see cref="double"/> или <see cref="float"/>)</typeparam>
        private static void DoHilbertTransform<T>(
            Span<T> buffer,
            HilbertKeys baseKey,
            T input,
            bool isOdd,
            int hilbertIdx,
            T adjustedPrevPeriod) where T : IFloatingPointIeee754<T>
        {
            var a = T.CreateChecked(0.0962);  // Коэффициент фильтра для входного сигнала
            var b = T.CreateChecked(0.5769);  // Коэффициент фильтра для обратной связи

            var hilbertTempT = a * input;  // Взвешенное входное значение

            // Вычисление индексов для доступа к буферу:
            var baseIndex = (int) baseKey;  // Базовый индекс группы буферов
            var hilbertIndex = baseIndex - (isOdd ? 6 : 3) + hilbertIdx;  // Индекс текущего подбуфера
            var prevIndex = baseIndex + (isOdd ? 1 : 2);  // Индекс предыдущего состояния
            var prevInputIndex = baseIndex + (isOdd ? 3 : 4);  // Индекс предыдущего входа

            // Обновление буфера по алгоритму Хилберта:
            buffer[baseIndex] = -buffer[hilbertIndex];  // Инверсия предыдущего значения
            buffer[hilbertIndex] = hilbertTempT;        // Сохранение текущего взвешенного входа
            buffer[baseIndex] += hilbertTempT;          // Аккумуляция текущего значения
            buffer[baseIndex] -= buffer[prevIndex];     // Коррекция на предыдущее состояние

            // Обновление обратной связи через коэффициент b:
            buffer[prevIndex] = b * buffer[prevInputIndex];  // Расчет обратной связи
            buffer[baseIndex] += buffer[prevIndex];         // Добавление обратной связи

            // Сохранение входного значения для следующей итерации:
            buffer[prevInputIndex] = input;

            // Адаптация результата к предыдущему периоду:
            buffer[baseIndex] *= adjustedPrevPeriod;  // Масштабирование по скорректированному периоду
        }

    }

    /// <summary>
    /// Возвращает числовое значение 2, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Two<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(2);

    /// <summary>
    /// Возвращает числовое значение 3, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Three<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(3);

    /// <summary>
    /// Возвращает числовое значение 4, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Four<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(4);

    /// <summary>
    /// Возвращает числовое значение 90, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Ninety<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(90);

    /// <summary>
    /// Возвращает числовое значение 100, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Hundred<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(100);

    /// <summary>
    /// Проверяет и корректирует входной диапазон для обеспечения корректности обработки данных.
    /// </summary>
    /// <param name="inRange">Запрошенный диапазон обработки данных (начальный и конечный индексы)</param>
    /// <param name="inputLengths">Массив длин входных данных для проверки (например, длины массивов цен)</param>
    /// <returns>
    /// Кортеж с корректными индексами <c>startIndex</c> и <c>endIndex</c> для обработки, 
    /// или <c>null</c> если диапазон недопустим.
    /// </returns>
    public static (int startIndex, int endIndex)? ValidateInputRange(Range inRange, params int[] inputLengths)
    {
        // Определение минимальной длины входных данных
        var inputLength = Int32.MaxValue;
        foreach (var length in inputLengths)
        {
            if (length < inputLength)
            {
                inputLength = length;
            }
        }

        // Расчет начального индекса с учетом спецификации Range
        var startIdx = !inRange.Start.IsFromEnd
            ? inRange.Start.Value
            : inputLength - 1 - inRange.Start.Value;

        // Расчет конечного индекса с учетом спецификации Range
        var endIdx = !inRange.End.IsFromEnd
            ? inRange.End.Value
            : inputLength - 1 - inRange.End.Value;

        // Проверка корректности диапазона:
        // - Начальный индекс неотрицателен
        // - Конечный индекс больше нуля и больше/равен начального
        // - Конечный индекс не превышает длину входных данных
        return startIdx >= 0 && endIdx > 0 && endIdx >= startIdx && endIdx < inputLength
            ? (startIdx, endIdx)
            : null;
    }

}

/// <summary>
/// Предоставляет функциональный слой для доступа к методам расчёта технических индикаторов.
/// </summary>
public static partial class Functions;
