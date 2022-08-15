namespace LittleToyDocumentor;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

internal class TypeDocumentationModel
{
    public TypeDocumentationModel(ITypeSymbol type, IEnumerable<AuthoringDescriptor> descriptors)
    {
        Type = type;
        Descriptors = descriptors;
    }

    public ITypeSymbol Type { get; }
    public IEnumerable<AuthoringDescriptor> Descriptors { get; }
    public bool ViaAuthoring => Descriptors.Any(_ => _.ViaAuthoring);
}
