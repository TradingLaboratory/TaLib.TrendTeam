// CalcDeltas.cs
namespace TALib;

internal static partial class FunctionHelpers
{

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

}
