using Microsoft.CodeAnalysis;
using System;

namespace LittleToyDocumentor;

internal class AuthoringDescriptor : IEquatable<AuthoringDescriptor>
{
    public AuthoringDescriptor(string operationName, IMethodSymbol method, ITypeSymbol type)
    {
        OperationName = operationName;
        Method = method;
        Type = type;
        ViaAuthoring = method == null;
    }

    public string OperationName { get; }
    public IMethodSymbol Method { get; }
    public ITypeSymbol Type { get; }

    public bool ViaAuthoring { get; }

    public bool Equals(AuthoringDescriptor other)
    {
        if (other == null) return false;
        return OperationName == other.OperationName
            && SymbolEqualityComparer.Default.Equals(Method, other.Method)
            && SymbolEqualityComparer.Default.Equals(Type, other.Type)
            && ViaAuthoring == other.ViaAuthoring;
    }

    public override bool Equals(object obj)
    {
        return this.Equals(obj as AuthoringDescriptor);
    }
}
