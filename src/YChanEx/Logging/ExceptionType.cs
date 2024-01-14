#nullable enable
namespace murrty.logging;
/// <summary>
/// Enum of exception types that may occur during runtime.
/// </summary>
[System.ComponentModel.DefaultValue(Unknown)]
internal enum ExceptionType {
    /// <summary>
    /// An unknown exception type, no way for the application to know the after-report state of the application itself.
    /// </summary>
    Unknown,
    /// <summary>
    /// A successfully caught exception which will terminate the try-catch block associated with it.
    /// </summary>
    Caught,
    /// <summary>
    /// An unhandled exception type which will most likely cause the application to exit.
    /// </summary>
    Unhandled,
    /// <summary>
    /// An unhandled thread exception that will allow the application to continue in most situations.
    /// </summary>
    ThreadException
}
