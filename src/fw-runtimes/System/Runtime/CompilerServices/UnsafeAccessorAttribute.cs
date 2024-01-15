#nullable enable
namespace System.Runtime.CompilerServices;
/// <summary>
/// Provides access to an inaccessible member of a specific type.
/// </summary>
/// <remarks>
/// This attribute may be applied to an <c>extern static</c> method.
/// The implementation of the <c>extern static</c> method annotated with
/// this attribute will be provided by the runtime based on the information in
/// the attribute and the signature of the method that the attribute is applied to.
/// The runtime will try to find the matching method or field and forward the call
/// to it. If the matching method or field is not found, the body of the <c>extern</c>
/// <para>
/// method will throw <see cref="MissingFieldException" /> or <see cref="MissingMethodException" />.
/// Only the specific type defined will be examined for inaccessible members. The type hierarchy
/// is not walked looking for a match.
/// </para>
/// <para>
/// For <see cref="UnsafeAccessorKind.Method"/>, <see cref="UnsafeAccessorKind.StaticMethod"/>,
/// <see cref="UnsafeAccessorKind.Field"/>, and <see cref="UnsafeAccessorKind.StaticField"/>, the type of
/// the first argument of the annotated
/// </para>
/// <para>
/// <c>extern</c> method identifies the owning type.
/// The value of the first argument is treated as <c>this</c> pointer for instance fields and methods.
/// The first argument must be passed as <c>ref</c> for instance fields and methods on structs.
/// The value of the first argument is not used by the implementation for <c>static</c> fields and methods.
/// </para>
/// <para>
/// Return type is considered for the signature match. modreqs and modopts are initially not considered for
/// the signature match. However, if an ambiguity exists ignoring modreqs and modopts, a precise match
/// is attempted. If an ambiguity still exists <see cref="System.Reflection.AmbiguousMatchException" /> is thrown.
/// </para>
/// <para>
/// By default, the attributed method's name dictates the name of the method/field. This can cause confusion
/// in some cases since language abstractions, like C# local functions, generate mangled IL names. The
/// solution to this is to use the <c>nameof</c> mechanism and define the <see cref="Name"/> property.
/// </para>
///
/// <code>
/// public void Method(Class c)
/// {
///     PrivateMethod(c);
///
///     [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(PrivateMethod))]
///     extern static void PrivateMethod(Class c);
/// }
/// </code>
/// </remarks>
[global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[global::System.Diagnostics.DebuggerNonUserCode]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
//[global::System.Diagnostics.Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class UnsafeAccessorAttribute : Attribute {
    // Block of text to include above when Generics support is added:
    //
    // The generic parameters of the <code>extern static</code> method are a concatenation of the type and
    // method generic arguments of the target method. For example,
    // <code>extern static void Method1&lt;T1, T2&gt;(Class1&lt;T1&gt; @this)</code>
    // can be used to call <code>Class1&lt;T1&gt;.Method1&lt;T2&gt;()</code>. The generic constraints of the
    // <code>extern static</code> method must match generic constraints of the target type, field or method.

    /// <summary>
    /// Instantiates an <see cref="UnsafeAccessorAttribute"/> providing access to a member of kind <see cref="UnsafeAccessorKind"/>.
    /// </summary>
    /// <param name="kind">The kind of the target to which access is provided.</param>
    public UnsafeAccessorAttribute(UnsafeAccessorKind kind)
        => Kind = kind;

    /// <summary>
    /// Gets the kind of member to which access is provided.
    /// </summary>
    public UnsafeAccessorKind Kind { get; }

    /// <summary>
    /// Gets or sets the name of the member to which access is provided.
    /// </summary>
    /// <remarks>
    /// The name defaults to the annotated method name if not specified.
    /// The name must be unset/<c>null</c> for <see cref="UnsafeAccessorKind.Constructor"/>.
    /// </remarks>
    public string? Name { get; set; }
}

/// <summary>
/// Specifies the kind of target to which an <see cref="UnsafeAccessorAttribute" /> is providing access.
/// </summary>
public enum UnsafeAccessorKind {
    /// <summary>
    /// Provide access to a constructor.
    /// </summary>
    Constructor,
    /// <summary>
    /// Provide access to a method.
    /// </summary>
    Method,
    /// <summary>
    /// Provide access to a static method.
    /// </summary>
    StaticMethod,
    /// <summary>
    /// Provide access to a field.
    /// </summary>
    Field,
    /// <summary>
    /// Provide access to a static field.
    /// </summary>
    StaticField
}
