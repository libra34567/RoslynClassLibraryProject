namespace LittleToySourceGenerator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsCodeGenerator;
using CsCodeGenerator.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

internal class SelectiveSystemAuthoringGenerator
{
    private readonly List<ClassDeclarationSyntax> candidateSystems;
    private readonly GeneratorExecutionContext context;
    private static DiagnosticDescriptor FatalError = new(
        "LT0100",
        "Fatal error during selecting system authoring generation",
        "Fatal error happens during generation of selective authoring for type {0}. Error: {1}",
        "LittleToy",
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Fatal error happens. This is a bug, please report back to developer");
    private static DiagnosticDescriptor DisableAutoCreationMissing = new(
        "LT0101",
        "DisableAutoCreationAttribute missing",
        "DisableAutoCreationAttribute attribute is missing on a type {0}. Generation would be ignored",
        "LittleToy",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "DisableAutoCreationAttribute should be applied if you are using GenerateSystemAuthoringAttribute");
    private static DiagnosticDescriptor WorldAttributeMissing = new(
        "LT0102",
        "Subsystem world should be specified",
        "At least one of attributes ServerWorld or ClientWorld should be applied to type {0}. Generation would be ignored",
        "LittleToy",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "At least one of attributes ServerWorld or ClientWorld should be applied to know where get data from for system awake");
    private static DiagnosticDescriptor FixedBytes16Required = new(
        "LT0103",
        "NetPrefab authoring error",
        "NetPrefab can be set only on fields of type Unity.Collections.FixedBytes16. Generation for incorrect fields would be ignored",
        "LittleToy",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "At least one of attributes ServerWorld or ClientWorld should be applied to know where get data from for system awake");

    public SelectiveSystemAuthoringGenerator(List<ClassDeclarationSyntax> candidateSystems, GeneratorExecutionContext context)
    {
        this.candidateSystems = candidateSystems;
        this.context = context;
    }

    public void Execute()
    {
        foreach (var type in candidateSystems)
        {
            try
            {
                var model = context.Compilation.GetSemanticModel(type.SyntaxTree);
                var typeSymbol = model.GetDeclaredSymbol(type) as ITypeSymbol;
                if (!typeSymbol.HasAttribute(Generator.GenerateSystemAuthoringAttributeType))
                {
                    continue;
                }

                var subsystemModel = GetSubsystemModel(typeSymbol);
                if (!subsystemModel.HasDisableAutoCreation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DisableAutoCreationMissing, type.GetLocation(), typeSymbol.ToDisplayString()));
                    continue;
                }

                if (!subsystemModel.HasServerWorld && !subsystemModel.HasClientWorld)
                {
                    context.ReportDiagnostic(Diagnostic.Create(WorldAttributeMissing, type.GetLocation(), typeSymbol.ToDisplayString()));
                    continue;
                }

                if (!subsystemModel.Fields.All(_ => _.IsValid))
                {
                    context.ReportDiagnostic(Diagnostic.Create(FixedBytes16Required, type.GetLocation(), typeSymbol.ToDisplayString()));
                }

                var file = GenerateSelectiveSystemAuthoring(subsystemModel);
                context.AddSource(file.Name, SourceText.From(file.ToString(), Encoding.UTF8));
            }
            catch (Exception ex)
            {
#if DEBUG
                var errorMessage = ex.ToString();
#else
                var errorMessage = ex.Message;
#endif
                context.ReportDiagnostic(Diagnostic.Create(FatalError, type.GetLocation(), type, errorMessage));
            }
        }
    }

    private FileModel GenerateSelectiveSystemAuthoring(SubsystemGenerationModel model)
    {
        var typeSymbol = model.Subsystem;
        var file = new FileModel(typeSymbol.Name + "SelectiveSystemAuthoring")
        {
            UsingDirectives = new List<string>
            {
                "DOTSNET;",
                "UnityEngine;",
                "Zenject;",
            },
            Header = Generator.FileHeader,
            Namespace = typeSymbol.ContainingNamespace.GetNamespace(),
        };

        if (model.HasServerWorld)
        {
            var serverSystemAuthoring = GenerateSelectiveSystemAuthoringClassModel(model, "Server");
            file.Classes.Add(serverSystemAuthoring);
        }

        if (model.HasClientWorld)
        {
            var clientSystemAuthoring = GenerateSelectiveSystemAuthoringClassModel(model, "Client");
            file.Classes.Add(clientSystemAuthoring);
        }

        return file;
    }

    private static SubsystemGenerationModel GetSubsystemModel(ITypeSymbol typeSymbol)
    {
        var model = new SubsystemGenerationModel() { Subsystem = typeSymbol };
        foreach (var field in typeSymbol.GetFields())
        {
            if (!field.HasAttribute(Generator.FieldFromAuthoringAttributeType))
            {
                continue;
            }

            var fieldPropertiesAttribute = field.GetCustomAttribute(Generator.FieldFromAuthoringAttributeType, false);
            var propertiesExprssion = fieldPropertiesAttribute.ConstructorArguments.FirstOrDefault();
            var fieldSourceType = (FieldSourceType)(int)propertiesExprssion.Value;
            model.Fields.Add(new FieldModel(field, fieldSourceType));
        }

        model.HasServerWorld = typeSymbol.HasAttribute(Generator.ServerWorldAttributeType);
        model.HasClientWorld = typeSymbol.HasAttribute(Generator.ClientWorldAttributeType);
        model.HasDisableAutoCreation = typeSymbol.HasAttribute(Generator.DisableAutoCreationAttributeType);

        return model;
    }

    private ClassModel GenerateSelectiveSystemAuthoringClassModel(SubsystemGenerationModel model, string worldName)
    {
        var typeSymbol = model.Subsystem;
        var authoringClass = new ClassModel($"{typeSymbol.Name}{worldName}Authoring")
        {
            BaseClass = $"MonoBehaviour",
            Interfaces = new() { "SelectiveSystemAuthoring" },
            AccessModifier = AccessModifier.Public,
            KeyWords = new() { KeyWord.Partial }
        };

        var validFields = model.Fields.Where(_ => _.IsValid);
        foreach (var field in validFields)
        {
            var fieldName = field.BackingFieldName;
            var fieldModel = new Field(field.BackingFieldType, fieldName);
            var fieldSourceType = field.SourceType;
            switch (fieldSourceType)
            {
                case FieldSourceType.Public:
                    fieldModel.AccessModifier = AccessModifier.Public;
                    break;
                case FieldSourceType.SerializePrivate:
                case FieldSourceType.NetPrefab:
                    fieldModel.AccessModifier = AccessModifier.Private;
                    fieldModel.Attributes.Add(new AttributeModel("SerializeField"));
                    break;
                case FieldSourceType.Inject:
                    fieldModel.AccessModifier = AccessModifier.Private;
                    fieldModel.Attributes.Add(new AttributeModel("Inject"));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown field source type {fieldSourceType}");
            }

            authoringClass.Fields.Add(fieldModel);
        }

        authoringClass.Properties.Add(new Property(typeSymbol.ToDisplayString(), "System")
        {
            AccessModifier = AccessModifier.Public,
            IsExpressBody = true,
            IsGetOnly = true,
            IsAutoImplemented = false,
            GetterBody = $"Bootstrap.{worldName}World.GetExistingSystem<{typeSymbol.ToDisplayString()}>()"
        });

        authoringClass.Methods.Add(new Method("System.Type", "GetSystemType")
        {
            AccessModifier = AccessModifier.Public,
            BodyLines = new() { $"return typeof({typeSymbol.ToDisplayString()});" },
        });

        var awakeMethod = new Method("void", "Awake")
        {
            AccessModifier = AccessModifier.Private,
            BodyLines = new(),
        };
        awakeMethod.BodyLines.Add($"var system = System;");
        foreach (var field in validFields)
        {
            if (field.SourceType != FieldSourceType.NetPrefab)
            {
                awakeMethod.BodyLines.Add($"system.{field.FieldName} = {field.BackingFieldName};");
            }
            else
            {
                awakeMethod.BodyLines.Add($"system.{field.FieldName} = Conversion.GuidToBytes16({field.BackingFieldName}.prefabId);");
            }
        }

        authoringClass.Methods.Add(awakeMethod);
        return authoringClass;
    }

    class SubsystemGenerationModel
    {
        public ITypeSymbol Subsystem { get; set; }
        public List<FieldModel> Fields { get; } = new List<FieldModel>();
        public bool HasServerWorld { get; set; }
        public bool HasClientWorld { get; set; }
        public bool HasDisableAutoCreation { get; set; }
    }

    public class FieldModel
    {
        public IFieldSymbol Field { get; }

        public FieldSourceType SourceType { get; }

        public string FieldName => Field.Name;
        public string BackingFieldName => Field.Name.ToCamel();
        public string BackingFieldType => SourceType == FieldSourceType.NetPrefab ? "NetworkIdentityAuthoring" : Field.Type.ToDisplayString();

        public bool IsValid => SourceType != FieldSourceType.NetPrefab || Field.Type.ToDisplayString() == "Unity.Collections.FixedBytes16";

        public FieldModel(IFieldSymbol field, FieldSourceType sourceType)
        {
            Field = field;
            SourceType = sourceType;
        }
    }

    public enum FieldSourceType
    {
        Public,
        SerializePrivate,
        Inject,
        NetPrefab,
    }
}
