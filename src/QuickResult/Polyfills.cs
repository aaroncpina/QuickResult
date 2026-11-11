// Compiler-recognised types that are missing from older target frameworks.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;
    public string ParameterName { get; }
}
#endif

// Marks a type as a C# union. The compiler recognises the attribute by name, so declaring it
// here gives consumers union behaviour on target frameworks that predate .NET 11.
#if NET && !NET11_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class UnionAttribute : Attribute
    {
    }
}
#endif
