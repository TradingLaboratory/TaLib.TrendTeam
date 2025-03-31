// InitDMAndTR.cs
namespace TALib;

internal static partial class FunctionHelpers
{
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


}
