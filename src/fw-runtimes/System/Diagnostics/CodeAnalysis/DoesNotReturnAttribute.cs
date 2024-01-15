#nullable enable
namespace System.Diagnostics.CodeAnalysis;
/// <summary>
///     Specifies that a method that will never return under any circumstance.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
public sealed class DoesNotReturnAttribute : Attribute {
    /// <summary>
    ///     Initializes a new instance of the <see cref="DoesNotReturnAttribute"/> class.
    /// </summary>
    ///
    public DoesNotReturnAttribute() { }
}
