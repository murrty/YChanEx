#nullable enable
namespace System.Runtime.CompilerServices;
[global::System.Diagnostics.DebuggerNonUserCode]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
// This will be required to duplicated in projects that require the use of this.
internal static class RuntimeHelpers {
    public static T[] GetSubArray<T>(T[] array, Range range) {
        if (array is null) {
            throw new ArgumentNullException();
        }

        (int offset, int length) = range.GetOffsetAndLength(array.Length);

        if (default(T) is not null || typeof(T[]) == array.GetType()) {
            // We know the type of the array to be exactly T[].

            if (length == 0) {
                return [];
            }

            var dest = new T[length];
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
        else {
            // The array is actually a U[] where U:T.
            T[] dest = (T[])Array.CreateInstance(array.GetType().GetElementType()!, length);
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
    }
}
