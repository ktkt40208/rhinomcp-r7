// .NET Framework 4.8 lacks the string.Contains(string, StringComparison) and
// string.Contains(char, StringComparison) overloads (added in .NET Core 2.1 /
// .NET Standard 2.1). The Grasshopper catalog/search code uses them heavily for
// case-insensitive matching, so this polyfill restores them.
//
// Placed in the GLOBAL namespace (no namespace declaration) so the extension
// methods are in scope in every file without adding a using directive. Guarded by
// NETFRAMEWORK so it never shadows the BCL overloads on modern .NET targets.
#if NETFRAMEWORK
using System;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class StringContainsCompat
{
    public static bool Contains(this string source, string value, StringComparison comparisonType)
        => source.IndexOf(value, comparisonType) >= 0;

    public static bool Contains(this string source, char value, StringComparison comparisonType)
        => source.IndexOf(value.ToString(), comparisonType) >= 0;
}
#endif
