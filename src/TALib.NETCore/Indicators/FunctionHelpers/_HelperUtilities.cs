namespace TALib;

internal static partial class FunctionHelpers
{
    //1. Методы для возврата числовых констант

    /// <summary>
    /// Возвращает числовое значение 2, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Two<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(2);

    /// <summary>
    /// Возвращает числовое значение 3, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Three<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(3);

    /// <summary>
    /// Возвращает числовое значение 4, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Four<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(4);

    /// <summary>
    /// Возвращает числовое значение 90, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Ninety<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(90);

    /// <summary>
    /// Возвращает числовое значение 100, преобразованное к указанному типу с плавающей точкой.
    /// </summary>
    public static T Hundred<T>() where T : IFloatingPointIeee754<T> => T.CreateChecked(100);



    //2. Валидация входного диапазона

    /// <summary>
    /// Проверяет и корректирует входной диапазон для обеспечения корректности обработки данных.
    /// </summary>
    /// <param name="inRange">Запрошенный диапазон обработки данных (начальный и конечный индексы)</param>
    /// <param name="inputLengths">Массив длин входных данных для проверки (например, длины массивов цен)</param>
    /// <returns>
    /// Кортеж с корректными индексами <c>startIndex</c> и <c>endIndex</c> для обработки, 
    /// или <c>null</c> если диапазон недопустим.
    /// </returns>
    public static (int startIndex, int endIndex)? ValidateInputRange(Range inRange, params int[] inputLengths)
    {
        // Определение минимальной длины входных данных
        var inputLength = Int32.MaxValue;
        foreach (var length in inputLengths)
        {
            if (length < inputLength)
            {
                inputLength = length;
            }
        }

        // Расчет начального индекса с учетом спецификации Range
        var startIdx = !inRange.Start.IsFromEnd
            ? inRange.Start.Value
            : inputLength - 1 - inRange.Start.Value;

        // Расчет конечного индекса с учетом спецификации Range
        var endIdx = !inRange.End.IsFromEnd
            ? inRange.End.Value
            : inputLength - 1 - inRange.End.Value;

        // Проверка корректности диапазона:
        // - Начальный индекс неотрицателен
        // - Конечный индекс больше нуля и больше/равен начального
        // - Конечный индекс не превышает длину входных данных
        return startIdx >= 0 && endIdx > 0 && endIdx >= startIdx && endIdx < inputLength
            ? (startIdx, endIdx)
            : null;
    }

}
