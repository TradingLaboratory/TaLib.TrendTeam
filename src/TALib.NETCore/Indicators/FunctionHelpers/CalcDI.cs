// CalcDI.cs
namespace TALib;

internal static partial class FunctionHelpers
{

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

}
