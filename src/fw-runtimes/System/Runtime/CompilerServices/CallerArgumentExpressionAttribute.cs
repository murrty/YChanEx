#nullable enable
namespace System.Runtime.CompilerServices;
[global::System.AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[global::System.Diagnostics.DebuggerNonUserCode]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class CallerArgumentExpressionAttribute : Attribute {
    public CallerArgumentExpressionAttribute(string parameterName) {
        ParameterName = parameterName;
    }
    public string ParameterName { get; }
}
