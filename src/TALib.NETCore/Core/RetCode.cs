//Название файла: RetCode.cs

namespace TALib;

public static partial class Core
{
    /// <summary>
    /// Represents the return codes for functions, indicating the outcome of an operation.
    /// </summary>
    public enum RetCode : ushort
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The operation failed due to an invalid parameter.
        /// </summary>
        BadParam,

        /// <summary>
        /// The operation failed due to an insufficient number of elements in the input data.
        /// </summary>
        OutOfRangeParam
    }
}
