// CalcAccumulationDistribution.cs

namespace TALib;

internal static partial class FunctionHelpers
{

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

}
