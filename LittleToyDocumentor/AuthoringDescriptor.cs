using Microsoft.CodeAnalysis;

namespace LittleToyDocumentor;

internal class AuthoringDescriptor
{
    public AuthoringDescriptor(string operationName, IMethodSymbol method, ITypeSymbol type)
    {
        OperationName = operationName;
        Method = method;
        Type = type;
    }

    public string OperationName { get; }
    public IMethodSymbol Method { get; }
    public ITypeSymbol Type { get; }
}
