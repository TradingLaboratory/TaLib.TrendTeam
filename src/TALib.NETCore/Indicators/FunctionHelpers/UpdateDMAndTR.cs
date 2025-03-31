// UpdateDMAndTR.cs
namespace TALib;

internal static partial class FunctionHelpers
{

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

}
