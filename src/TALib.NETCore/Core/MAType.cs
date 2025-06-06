//Файл MAType.cs

namespace TALib;

public static partial class Core
{
    /// <summary>
    /// Определяет различные типы скользящих средних.
    /// </summary>
    public enum MAType
    {
        /// <summary>
        /// Невзвешенное арифметическое среднее (SMA).
        /// </summary>
        Sma,

        /// <summary>
        /// Стандартная экспоненциальная скользящая средняя (EMA), использующая коэффициент сглаживания 2/(n+1).
        /// </summary>
        Ema,

        /// <summary>
        /// Взвешенная экспоненциальная скользящая средняя (WMA), использующая коэффициент сглаживания 1/n и простую скользящую среднюю в качестве начального значения.
        /// </summary>
        Wma,

        /// <summary>
        /// Двойная экспоненциальная скользящая средняя (DEMA).
        /// </summary>
        Dema,

        /// <summary>
        /// Тройная экспоненциальная скользящая средняя (TEMA).
        /// </summary>
        Tema,

        /// <summary>
        /// Треугольная скользящая средняя (TRIMA).
        /// </summary>
        Trima,

        /// <summary>
        /// Адаптивная скользящая средняя Кауфмана (KAMA).
        /// </summary>
        Kama,

        /// <summary>
        /// Адаптивная скользящая средняя MESA (MAMA).
        /// </summary>
        Mama,

        /// <summary>
        /// Утроенная обобщённая двойная экспоненциальная скользящая средняя (T3).
        /// </summary>
        T3
    }
}
