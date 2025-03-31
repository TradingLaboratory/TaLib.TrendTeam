// TrueRange.cs
namespace TALib;

internal static partial class FunctionHelpers
{
    /// <summary>
    /// Рассчитывает истинный диапазон (True Range) для текущего бара.
    /// Используется в индикаторах вроде Average True Range (ATR) для оценки волатильности.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="th">Текущий максимум (High) бара</param>
    /// <param name="tl">Текущий минимум (Low) бара</param>
    /// <param name="yc">Предыдущее значение закрытия (Yesterday's Close)</param>
    /// <returns>
    /// Максимальное значение из трех вариантов:  
    /// 1. Разница между текущими High и Low  
    /// 2. Абсолютная разница между текущим High и предыдущим Close  
    /// 3. Абсолютная разница между текущим Low и предыдущим Close
    /// </returns>
    public static T TrueRange<T>(T th, T tl, T yc) where T : IFloatingPointIeee754<T>
    {
        // 1. Базовый диапазон: High - Low
        var range = th - tl;

        // 2. Сравнение с разницей между High и предыдущим Close
        range = T.Max(range, T.Abs(th - yc));

        // 3. Сравнение с разницей между Low и предыдущим Close
        range = T.Max(range, T.Abs(tl - yc));

        return range;
    }

}
