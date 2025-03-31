// CalcHighest.cs

namespace TALib;

internal static partial class FunctionHelpers
{

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

}
