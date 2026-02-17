//Название файла: TA_Correl.cs
//Группы к которым можно отнести индикатор:
//StatisticFunctions (существующая папка - идеальное соответствие категории)
//CorrelationIndicators (альтернатива, если требуется группировка по типу индикатора)
//RelationshipAnalysis (альтернатива для акцента на анализе взаимосвязей)

namespace TALib;

public static partial class Functions
{
    /// <summary>
    /// Pearson's Correlation Coefficient (r) (Statistic Functions) — Коэффициент корреляции Пирсона (r) (Статистические функции)
    /// </summary>
    /// <param name="inReal0">
    /// Входные данные для первого набора данных (цены, индикаторы или другие временные ряды).
    /// Обычно используется как первый актив или временной ряд для анализа корреляции.
    /// </param>
    /// <param name="inReal1">
    /// Входные данные для второго набора данных (цены, индикаторы или другие временные ряды).
    /// Обычно используется как второй актив или временной ряд для анализа корреляции.
    /// </param>
    /// <param name="inRange">
    /// Диапазон обрабатываемых данных в <paramref name="inReal0"/> и <paramref name="inReal1"/> (начальный и конечный индексы).
    /// <para>
    /// - Если не указан, обрабатывается весь массив <paramref name="inReal0"/> и <paramref name="inReal1"/>.
    /// - Оба входных массива должны иметь одинаковую длину или диапазон должен быть валидным для обоих.
    /// </para>
    /// </param>
    /// <param name="outReal">
    /// Массив, содержащий ТОЛЬКО валидные значения индикатора.
    /// <para>
    /// - Длина массива равна <c>outRange.End - outRange.Start + 1</c> (если <c>outRange</c> корректен).
    /// - Каждый элемент <c>outReal[i]</c> соответствует <c>inReal0[outRange.Start + i]</c> и <c>inReal1[outRange.Start + i]</c>.
    /// - Значения находятся в диапазоне от -1 до 1, где -1 = идеальная отрицательная корреляция, 0 = отсутствие корреляции, 1 = идеальная положительная корреляция.
    /// </para>
    /// </param>
    /// <param name="outRange">
    /// Диапазон индексов в <paramref name="inReal0"/> и <paramref name="inReal1"/>, для которых рассчитаны валидные значения:
    /// <para>
    /// - <b>Start</b>: индекс первого элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    ///   Это индекс равен <c>lookback</c> периоду — все бары с индексом меньше <c>lookback</c> пропускаются для расчёта первого валидного значения.
    /// - <b>End</b>: индекс последнего элемента <paramref name="inReal0"/> и <paramref name="inReal1"/>, имеющего валидное значение в <paramref name="outReal"/>.
    /// - Гарантируется: <c>End == inReal0.GetUpperBound(0)</c> (последний элемент входных данных), если расчет успешен.
    /// - Если данных недостаточно (например, длина <paramref name="inReal0"/> или <paramref name="inReal1"/> меньше периода индикатора), возвращается <c>[0, -1]</c>.
    /// </para>
    /// </param>
    /// <param name="optInTimePeriod">
    /// Период времени (Time Period) для расчёта корреляции.
    /// <para>
    /// - Определяет количество баров, используемых для вычисления скользящей корреляции.
    /// - Значение по умолчанию: 30 периодов.
    /// - Минимальное допустимое значение: 1.
    /// </para>
    /// </param>
    /// <typeparam name="T">
    /// Числовой тип данных, обычно <see langword="float"/> или <see langword="double"/>,
    /// реализующий интерфейс <see cref="IFloatingPointIeee754{T}"/>.
    /// </typeparam>
    /// <returns>
    /// Значение <see cref="Core.RetCode"/>, указывающее на успешность или неудачу вычисления.
    /// <para>
    /// Возвращает <see cref="Core.RetCode.Success"/> при успешном вычислении, или соответствующий код ошибки в противном случае.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Коэффициент корреляции Пирсона (r) измеряет линейную корреляцию между двумя наборами данных, показывая, насколько они движутся вместе.
    /// <para>
    /// Функция полезна для построения портфеля, парных стратегий (Pairs Trading) и диверсификации.
    /// Её можно использовать вместе с индикаторами относительной силы или спрэда для выявления разрушений или схождений корреляции.
    /// </para>
    ///
    /// <b>Этапы вычисления</b>:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Вычислить суммы двух наборов данных (sumX, sumY) и их соответствующих квадратов (sumX2, sumY2) за указанный период времени.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить произведение наборов данных для каждого периода времени и рассчитать сумму этих произведений (sumXY).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Вычислить ковариацию (Covariance) и стандартные отклонения (Standard Deviation) наборов данных.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Разделить ковариацию на произведение стандартных отклонений для вычисления коэффициента корреляции.
    ///       Формула: r = Cov(X,Y) / (σX × σY)
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Интерпретация значений</b>:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Значение -1 указывает на идеальную отрицательную линейную зависимость (когда один актив растёт, другой падает).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение 0 указывает на отсутствие линейной зависимости (движение активов не коррелирует).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Значение 1 указывает на идеальную положительную линейную зависимость (активы движутся в одном направлении).
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <b>Применение в трейдинге</b>:
    /// <para>
    /// - <b>Парный трейдинг</b>: Поиск пар активов с высокой корреляцией для статистического арбитража.
    /// - <b>Диверсификация портфеля</b>: Выбор активов с низкой или отрицательной корреляцией для снижения риска.
    /// - <b>Хеджирование</b>: Использование отрицательно коррелированных активов для защиты от убытков.
    /// - <b>Выявление режимов рынка</b>: Изменения корреляции могут сигнализировать о смене рыночного режима.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public static Core.RetCode Correl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        CorrelImpl(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

    /// <summary>
    /// Возвращает период предварительного просмотра (Lookback Period) для <see cref="Correl{T}">Correl</see>.
    /// </summary>
    /// <param name="optInTimePeriod">
    /// Период времени (Time Period) для расчёта корреляции.
    /// Определяет количество баров, необходимых для вычисления первого валидного значения индикатора.
    /// </param>
    /// <returns>
    /// Количество периодов, необходимых до первого вычисленного значения (lookback период).
    /// <para>
    /// - Все бары в исходных данных с индексом меньше чем <c>lookback</c> будут пропущены.
    /// - Возвращает <c>optInTimePeriod - 1</c> для валидного периода.
    /// - Возвращает <c>-1</c> если период некорректен (меньше 1).
    /// </para>
    /// </returns>
    [PublicAPI]
    public static int CorrelLookback(int optInTimePeriod = 30) => optInTimePeriod < 1 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// Для совместимости с абстрактным API (Abstract API Compatibility).
    /// Этот метод обеспечивает обратную совместимость с предыдущими версиями библиотеки.
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Correl<T>(
        T[] inReal0,
        T[] inReal1,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 30) where T : IFloatingPointIeee754<T> =>
        CorrelImpl<T>(inReal0, inReal1, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode CorrelImpl<T>(
        ReadOnlySpan<T> inReal0,
        ReadOnlySpan<T> inReal1,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        // Инициализация выходного диапазона пустым значением
        // outRange обозначает диапазон индексов входных данных, для которых посчитаны валидные значения индикатора
        outRange = Range.EndAt(0);

        // Валидация входного диапазона для обоих массивов данных
        // Проверяет что inRange корректен и находится в пределах длины обоих входных массивов
        if (FunctionHelpers.ValidateInputRange(inRange, inReal0.Length, inReal1.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        // Извлечение начального и конечного индексов из валидированного диапазона
        var (startIdx, endIdx) = rangeIndices;

        // Проверка корректности периода времени
        // optInTimePeriod должен быть не меньше 1 для корректного расчёта
        if (optInTimePeriod < 1)
        {
            return Core.RetCode.BadParam;
        }

        // Вычисление lookback периода - количества баров необходимых для первого валидного значения
        // Все бары с индексом меньше lookbackTotal будут пропущены при расчёте
        var lookbackTotal = CorrelLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        // Если начальный индекс больше конечного - нет данных для расчёта
        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Сохранение начального индекса для выходного диапазона
        // outBegIdx будет использоваться для формирования outRange
        var outBegIdx = startIdx;

        // Индекс отслеживания отстающих значений для скользящего окна
        // trailingIdx указывает на элемент который будет удалён из окна при переходе к следующему бару
        var trailingIdx = startIdx - lookbackTotal;

        // Вычисление начальных значений для скользящего окна
        // sumX - сумма значений первого набора данных (inReal0) за период
        // sumY - сумма значений второго набора данных (inReal1) за период
        // sumX2 - сумма квадратов значений первого набора данных за период
        // sumY2 - сумма квадратов значений второго набора данных за период
        // sumXY - сумма произведений соответствующих значений двух наборов данных за период
        T sumX, sumY, sumX2, sumY2;
        var sumXY = sumX = sumY = sumX2 = sumY2 = T.Zero;
        int today;

        // Инициализация скользящего окна - вычисление сумм для первого периода
        // Цикл проходит от trailingIdx до startIdx включительно
        for (today = trailingIdx; today <= startIdx; today++)
        {
            // Получение значения из первого набора данных и обновление сумм
            var x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            // Получение значения из второго набора данных и обновление сумм
            var y = inReal1[today];
            sumXY += x * y;  // Произведение для ковариации
            sumY += y;
            sumY2 += y * y;
        }

        // Преобразование периода времени в тип T для математических операций
        var timePeriod = T.CreateChecked(optInTimePeriod);

        // Запись первого выходного значения коэффициента корреляции
        // Сначала сохраняем отстающие значения, так как входные и выходные данные могут быть одним и тем же массивом
        // Это предотвращает перезапись данных до их использования
        var trailingX = inReal0[trailingIdx];
        var trailingY = inReal1[trailingIdx++];

        // Вычисление знаменателя формулы корреляции Пирсона
        // tempReal = (ΣX² - (ΣX)²/n) × (ΣY² - (ΣY)²/n)
        // Это произведение дисперсий обоих наборов данных
        var tempReal = (sumX2 - sumX * sumX / timePeriod) * (sumY2 - sumY * sumY / timePeriod);

        // Вычисление коэффициента корреляции по формуле Пирсона
        // r = (ΣXY - ΣX×ΣY/n) / √((ΣX² - (ΣX)²/n) × (ΣY² - (ΣY)²/n))
        // Если знаменатель <= 0, возвращаем 0 (нет корреляции или ошибка вычисления)
        outReal[0] = tempReal > T.Zero ? (sumXY - sumX * sumY / timePeriod) / T.Sqrt(tempReal) : T.Zero;

        // Основной цикл для вычисления последующих значений коэффициента корреляции
        // outIdx отслеживает позицию в выходном массиве
        var outIdx = 1;
        while (today <= endIdx)
        {
            // Удаление отстающих значений из скользящего окна
            // Вычитаем значения которые выходят за пределы окна при его сдвиге
            sumX -= trailingX;
            sumX2 -= trailingX * trailingX;

            sumXY -= trailingX * trailingY;
            sumY -= trailingY;
            sumY2 -= trailingY * trailingY;

            // Добавление новых значений в скользящее окно
            // Получаем текущие значения из обоих наборов данных
            var x = inReal0[today];
            sumX += x;
            sumX2 += x * x;

            var y = inReal1[today++];
            sumXY += x * y;
            sumY += y;
            sumY2 += y * y;

            // Вывод нового коэффициента корреляции
            // Сначала сохраняем отстающие значения, так как входные и выходные данные могут быть одним и тем же массивом
            // Это предотвращает перезапись данных до их использования в следующей итерации
            trailingX = inReal0[trailingIdx];
            trailingY = inReal1[trailingIdx++];

            // Повторное вычисление знаменателя и коэффициента корреляции для нового окна
            tempReal = (sumX2 - sumX * sumX / timePeriod) * (sumY2 - sumY * sumY / timePeriod);
            outReal[outIdx++] = tempReal > T.Zero ? (sumXY - sumX * sumY / timePeriod) / T.Sqrt(tempReal) : T.Zero;
        }

        // Формирование выходного диапазона
        // outRange.Start = outBegIdx - индекс первого бара с валидным значением
        // outRange.End = outBegIdx + outIdx - индекс последнего бара с валидным значением
        outRange = new Range(outBegIdx, outBegIdx + outIdx);

        return Core.RetCode.Success;
    }
}
