// .NET Framework 4.8 does not ship System.Runtime.CompilerServices.IsExternalInit,
// which the C# compiler requires to emit `init`-only setters and `record` types.
// This shim provides it so the existing C# 9/10/11 syntax compiles on net48.
// (On net5.0+ the type is in the BCL and this file is harmless/unused.)
#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif
