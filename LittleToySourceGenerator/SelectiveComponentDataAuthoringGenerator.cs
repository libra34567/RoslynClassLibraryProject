using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using CsCodeGenerator;
using CsCodeGenerator.Enums;

namespace LittleToySourceGenerator;

internal class SelectiveComponentDataAuthoringGenerator
{
    private readonly List<StructDeclarationSyntax> candidateSystems;
    private readonly GeneratorExecutionContext context;
    private static DiagnosticDescriptor FatalError = new(
        "LT0200",
        "Fatal error during selecting component data authoring generation",
        "Fatal error happens during generation of selective component data authoring for type {0}. Error: {1}",
        "LittleToy",
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Fatal error happens. This is a bug, please report back to developer");

    public SelectiveComponentDataAuthoringGenerator(List<StructDeclarationSyntax> candidateSystems, GeneratorExecutionContext context)
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
                if (!typeSymbol.HasAttribute(Generator.GenerateSelectiveComponentDataAuthoringAttributeType))
                {
                    continue;
                }

                var subsystemModel = GetModel(typeSymbol);
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

    private FileModel GenerateSelectiveSystemAuthoring(ComponentDataGenerationModel model)
    {
        var typeSymbol = model.ComponentData;
        var file = new FileModel(typeSymbol.Name + "Authoring")
        {
            UsingDirectives = new List<string>
            {
                "Unity.Entities;",
                "UnityEngine;",
            },
            Header = Generator.FileHeader,
            Namespace = typeSymbol.ContainingNamespace.GetNamespace(),
        };

        var serverSystemAuthoring = GenerateSelectiveSystemAuthoringClassModel(model);
        file.Classes.Add(serverSystemAuthoring);

        return file;
    }

    private ClassModel GenerateSelectiveSystemAuthoringClassModel(ComponentDataGenerationModel model)
    {
        var typeSymbol = model.ComponentData;
        var authoringClass = new ClassModel($"{typeSymbol.Name}Authoring")
        {
            BaseClass = $"SelectiveComponentDataAuthoring",
            AccessModifier = AccessModifier.Public,
            KeyWords = new() { KeyWord.Partial }
        };

        string backingFieldName = typeSymbol.Name.ToCamel();
        authoringClass.Fields.Add(new Field(typeSymbol.ToDisplayString(), backingFieldName)
        {
            AccessModifier = AccessModifier.Private,
            Attributes = new()
            {
                new AttributeModel("SerializeField"),
            },
        });

        authoringClass.Methods.Add(new Method("void", "SelectiveConvert")
        {
            AccessModifier = AccessModifier.Protected,
            KeyWords = new()
            {
                KeyWord.Override,
            },
            Parameters = new()
            {
                new Parameter("Entity", "entity"),
                new Parameter("EntityManager", "dstManager"),
                new Parameter("GameObjectConversionSystem", "conversionSystem"),
            },
            BodyLines = new() { $"dstManager.AddComponentData(entity, {backingFieldName});" },
        });
        return authoringClass;
    }

    private static ComponentDataGenerationModel GetModel(ITypeSymbol typeSymbol)
    {
        var model = new ComponentDataGenerationModel() { ComponentData = typeSymbol };
        return model;
    }

    class ComponentDataGenerationModel
    {
        public ITypeSymbol ComponentData { get; set; }
    }
}
