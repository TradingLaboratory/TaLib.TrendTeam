// Название файла: HilbertTransform.cs

namespace TALib;

internal static partial class FunctionHelpers
{
    // Вложенный класс HTHelper

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



    // Сюда переносим методы, связанные с Хилбертом:
    // public static void DoPriceWma<T>(...) { ... }
    // public static void InitWma<T>(...) { ... }
}
