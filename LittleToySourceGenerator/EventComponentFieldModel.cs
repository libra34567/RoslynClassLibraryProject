using Microsoft.CodeAnalysis;

namespace LittleToySourceGenerator;

internal class EventComponentFieldModel
{
    public EventComponentFieldModel(IFieldSymbol field)
    {
        this.Name = field.Name;
        this.Type = field.Type;
        this.HasMarkDirtyAttribute = field.HasAttribute(Generator.MarkDirtyAttributeType);
        this.HasSyncFieldAttribute = field.HasAttribute(Generator.SyncFieldAttributeType);
        IsProperty = false;
    }
    public EventComponentFieldModel(IPropertySymbol field)
    {
        this.Name = field.Name;
        this.Type = field.Type;
        this.HasMarkDirtyAttribute = field.HasAttribute(Generator.MarkDirtyAttributeType);
        this.HasSyncFieldAttribute = field.HasAttribute(Generator.SyncFieldAttributeType);
        IsProperty = true;
    }
    public string Name { get; }
    public ITypeSymbol Type { get; }
    public bool HasMarkDirtyAttribute { get; }
    public bool HasSyncFieldAttribute { get; }
    public bool IsProperty { get; }
}
