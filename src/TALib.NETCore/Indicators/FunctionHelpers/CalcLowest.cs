// CalcLowest.cs

namespace TALib;

internal static partial class FunctionHelpers
{

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

}
