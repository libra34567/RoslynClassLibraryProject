using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using CsCodeGenerator.Enums;
using CsCodeGenerator;

namespace LittleToySourceGenerator;

internal class NetMessageGenerator
{
    private readonly List<StructDeclarationSyntax> candidateSystems;
    private readonly GeneratorExecutionContext context;
    private static DiagnosticDescriptor FatalError = new(
        "LT0300",
        "Fatal error during selecting net message generation",
        "Fatal error happens during generation of net message for type {0}. Error: {1}",
        "LittleToy",
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Fatal error happens. This is a bug, please report back to developer");
    public NetMessageGenerator(List<StructDeclarationSyntax> candidateSystems, GeneratorExecutionContext context)
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
                var file = GenerateNetMessage(typeSymbol);

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

    private static FileModel GenerateNetMessage(ITypeSymbol netMessageType)
    {
        var file = new FileModel(netMessageType.Name + "Gen")
        {
            UsingDirectives = new List<string>
            {
                "DOTSNET;",
                "Unity.Collections;",
                "System;"
            },
            Header = Generator.FileHeader,
            Namespace = netMessageType.ContainingNamespace.GetNamespace(),
            Structs = new List<StructModel>()
        };

        var netMessageStructModel = new StructModel(netMessageType.Name)
        {
            AccessModifier = AccessModifier.Public,
            SingleKeyWord = KeyWord.Partial,
            Interfaces = new List<string> { "NetworkMessage" },
            Constructors = new List<Constructor>()
        };

        var fields = GetFieldOrProperties(netMessageType);
        var netMessageConstructor = new Constructor(netMessageType.Name)
        {
            AccessModifier = AccessModifier.Public,
            Parameters = fields.Select(f => new Parameter(f.Type.Name, f.Name.ToCamel())).ToList(),
            BodyLines = fields.Select(f => $"this.{f.Name} = {f.Name.ToCamel()};").ToList()
        };
        if (fields.Length > 0)
        {
            netMessageStructModel.Constructors.Add(netMessageConstructor);
        }

        List<ITypeSymbol> structs = new();
        DiscoverNestedStructs(netMessageType);

        foreach (var strucType in structs)
        {
            var parameterName = SymbolEqualityComparer.Default.Equals(strucType, netMessageType)
                ? "this"
                : strucType.Name.ToCamel();
            GenerateStructureMethods(netMessageStructModel, strucType, parameterName);
        }

        file.Structs.Add(netMessageStructModel);
        return file;


        void DiscoverNestedStructs(ITypeSymbol type)
        {
            var structFields = GetFieldOrProperties(type);
            structs.Add(type);
            foreach (var field in structFields)
            {
                if (field.Type.IsDotsnetCompatibleType())
                {
                    continue;
                }

                if (structs.Contains(field.Type))
                {
                    continue;
                }

                DiscoverNestedStructs(field.Type);
            }
        }
    }

    private static EventComponentFieldModel[] GetFieldOrProperties(ITypeSymbol structType)
    {
        var modelsFromFields = structType.GetFields().Where(f => f.DeclaredAccessibility != Accessibility.Private).Select(field => new EventComponentFieldModel(field));
        var modelsFromProperties = structType.GetProperties().Where(f => f.DeclaredAccessibility != Accessibility.Private).Select(property => new EventComponentFieldModel(property));
        var fields = modelsFromFields.Union(modelsFromProperties).ToArray();
        return fields;
    }

    private static void GenerateStructureMethods(StructModel netMessageStructModel, ITypeSymbol structType, string structVariableName)
    {
        var fields = GetFieldOrProperties(structType);
        
        // SerializeModel
        var netMessageSerializeMethodModel = new Method(BuiltInDataType.Bool, "Serialize")
        {
            AccessModifier = AccessModifier.Public,
            Parameters = new List<Parameter> { new Parameter("ref NetworkWriter writer") },
            BodyLines = new List<string>()
        };
        if (structVariableName != "this")
        {
            netMessageSerializeMethodModel.KeyWords = new()
            {
                KeyWord.Static,
            };
            netMessageSerializeMethodModel.Parameters.Add(new Parameter($"ref {structType.GetDotsnetCompatibleType()} {structVariableName}"));
        }

        var totalFieldsCount = fields.Length;
        if (totalFieldsCount == 0)
        {
            netMessageSerializeMethodModel.BodyLines.Add("return true;");
        }
        else
        {
            netMessageSerializeMethodModel.BodyLines.Add("return");

            for (var i = 0; i < totalFieldsCount; i++)
            {
                var fieldInfo = fields[i];
                var isValidSerializationType = fieldInfo.Type.IsDotsnetCompatibleType() || fieldInfo.Type.TypeKind == TypeKind.Struct;
                if (!isValidSerializationType)
                {
                    throw new Exception($"Type {fieldInfo.Type.Name} cannot be serialized");
                }

                var conversion = fieldInfo.Type.IsDotsnetType() || fieldInfo.Type.TypeKind == TypeKind.Struct ? string.Empty : $"({fieldInfo.Type.GetDotsnetCompatibleType().ToDisplayString()})";
                var baseMessage = fieldInfo.Type.IsDotsnetCompatibleType()
                    ? $"writer.Write{fieldInfo.Type.GetDotsnetTypeName()}({conversion}{structVariableName}.{fieldInfo.Name})"
                    : $"Serialize(ref writer, ref {conversion}{structVariableName}.{fieldInfo.Name})";
                if (i == totalFieldsCount - 1)
                {
                    //last one
                    netMessageSerializeMethodModel.BodyLines.Add($"{baseMessage};");
                }
                else
                {
                    netMessageSerializeMethodModel.BodyLines.Add(
                        $"{baseMessage} &&");
                }

            }
        }

        netMessageStructModel.Methods.Add(netMessageSerializeMethodModel);

        // DeserializeModel
        var netMessageDeserializeMethodModel = new Method(BuiltInDataType.Bool, "Deserialize")
        {
            AccessModifier = AccessModifier.Public,
            Parameters = new List<Parameter> { new Parameter("ref NetworkReader reader") },
            BodyLines = new List<string>()
        };
        if (structVariableName != "this")
        {
            netMessageDeserializeMethodModel.KeyWords = new()
            {
                KeyWord.Static,
            };
            netMessageDeserializeMethodModel.Parameters.Add(new Parameter($"ref {structType.GetDotsnetCompatibleType()} {structVariableName}"));
        }

        totalFieldsCount = fields.Length;
        if (totalFieldsCount == 0)
        {
            netMessageDeserializeMethodModel.BodyLines.Add("return true;");
        }
        else
        {
            foreach (var fieldInfo in fields)
            {
                var type = fieldInfo.Type.GetDotsnetCompatibleType();
                netMessageDeserializeMethodModel.BodyLines.Add($"{type} {fieldInfo.Name.ToCamel()} = default;");
            }

            netMessageDeserializeMethodModel.BodyLines.Add("var result =");

            for (var i = 0; i < totalFieldsCount; i++)
            {
                var fieldInfo = fields[i];
                var isValidSerializationType = fieldInfo.Type.IsDotsnetCompatibleType() || fieldInfo.Type.TypeKind == TypeKind.Struct;
                if (!isValidSerializationType)
                {
                    throw new Exception($"Failed to find {fieldInfo.Type.Name}");
                }

                var baseMessage = fieldInfo.Type.IsDotsnetCompatibleType()
                    ? $"reader.Read{fieldInfo.Type.GetDotsnetTypeName()}(out {fieldInfo.Name.ToCamel()})"
                    : $"Deserialize(ref reader, ref {fieldInfo.Name.ToCamel()})";
                if (i == totalFieldsCount - 1)
                {
                    // last one
                    netMessageDeserializeMethodModel.BodyLines.Add($"{baseMessage};");
                }
                else
                {
                    netMessageDeserializeMethodModel.BodyLines.Add($"{baseMessage} &&");
                }

            }
            netMessageDeserializeMethodModel.BodyLines.Add("if (!result) return false;");
            foreach (var fieldInfo in fields)
            {
                var conversion = fieldInfo.Type.IsDotsnetType() || fieldInfo.Type.TypeKind == TypeKind.Struct ? string.Empty : $"({fieldInfo.Type.ToDisplayString()})";
                netMessageDeserializeMethodModel.BodyLines.Add($"{structVariableName}.{fieldInfo.Name} = {conversion}{fieldInfo.Name.ToCamel()};");
            }

            netMessageDeserializeMethodModel.BodyLines.Add("return true;");
        }

        netMessageStructModel.Methods.Add(netMessageDeserializeMethodModel);
    }
}
