// DoPriceWma.cs
namespace TALib;

internal static partial class FunctionHelpers
{

    /// <summary>
    /// Выполняет расчет взвешенного скользящего среднего (WMA) с учетом весовых коэффициентов.
    /// Используется для сглаживания временных рядов с акцентом на последние значения.
    /// </summary>
    /// <typeparam name="T">Тип данных с плавающей точкой (например, double)</typeparam>
    /// <param name="real">Входной временной ряд (например, цены)</param>
    /// <param name="idx">Текущий индекс обрабатываемого бара (будет увеличен на 1)</param>
    /// <param name="periodWMASub">Сумма значений для коррекции окна (используется для удаления старых данных)</param>
    /// <param name="periodWMASum">Взвешенная сумма значений для текущего окна</param>
    /// <param name="trailingWMAValue">Значение, которое будет удалено из окна при следующей итерации</param>
    /// <param name="varNewPrice">Новое значение, добавляемое в расчет (текущая цена)</param>
    /// <param name="varToStoreSmoothedValue">Результирующее сглаженное значение WMA</param>
    public static void DoPriceWma<T>(
        ReadOnlySpan<T> real,
        ref int idx,
        ref T periodWMASub,
        ref T periodWMASum,
        ref T trailingWMAValue,
        T varNewPrice,
        out T varToStoreSmoothedValue) where T : IFloatingPointIeee754<T>
    {
        // Обновление суммы для коррекции окна: добавляем новое значение, удаляем старое
        periodWMASub += varNewPrice;
        periodWMASub -= trailingWMAValue;

        // Накопление взвешенной суммы: новое значение умножается на весовой коэффициент (4)
        periodWMASum += varNewPrice * T.CreateChecked(4); // 4 — вес для текущего значения

        // Обновление значения, которое будет удалено в следующей итерации
        trailingWMAValue = real[idx++];

        // Расчет сглаженного значения: (взвешенная сумма) * нормирующий коэффициент (0.1)
        varToStoreSmoothedValue = periodWMASum * T.CreateChecked(0.1);

        // Коррекция взвешенной суммы для следующего расчета
        periodWMASum -= periodWMASub;
    }

}
