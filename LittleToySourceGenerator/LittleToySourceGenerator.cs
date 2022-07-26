﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleToySourceGenerator;

[Generator]
public class Generator : ISourceGenerator
{
    public bool GenerateEventData { get; set; } = false;
    public bool GenerateEventViewInterface { get; set; } = true;

    public void DisableAllGeneration()
    {
        GenerateEventData = false;
        GenerateEventViewInterface = false;
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        // Retrieve the populated receiver
        if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
        {
            return;
        }

        if (GenerateEventData)
        {
            GenerateEventDataFiles(context, receiver);
        }

        if (GenerateEventViewInterface)
        {
            GenerateEventViewInterfaceFiles(context, receiver);
        }
    }

    private static void GenerateEventDataFiles(GeneratorExecutionContext context, SyntaxReceiver receiver)
    {
        foreach (var type in receiver.ComponentDirtyEventStructs)
        {
            var model = context.Compilation.GetSemanticModel(type.SyntaxTree);
            var typeSymbol = model.GetDeclaredSymbol(type) as ITypeSymbol;
            var builder = new IndentedStringBuilder(
            @"// <auto-generated>
// Code generated by LittleToy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591
");
            builder.AppendLine("using System;");
            builder.AppendLine("using Unity.Entities;");
            builder.AppendLine("using Unity.Mathematics;");
            builder.AppendLine("using Plugins.basegame.Events;");
            builder.AppendLine("using DOTSNET;");
            builder.AppendLine();

            if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.AppendLine($@"namespace {typeSymbol.ContainingNamespace.ToDisplayString()}");
                builder.OpenBraces();
            }

            if (typeSymbol.HasAttribute("ComponentDirtyEvent"))
            {
                var fields = typeSymbol.GetMembers().OfType<IFieldSymbol>()
                    .Where(field => field.Name != "IsDirty" && !field.HasAttribute("IgnoreDirty"));
                GenerateEventStructModel(typeSymbol, builder, fields);
                builder.AppendLine();
                GenerateComponentDirtyEventInterfaceModel(typeSymbol, builder, fields);
            }

            if (typeSymbol.HasAttribute("ComponentRemovedEvent"))
            {
                GenerateComponentRemovedEventInterfaceModel(typeSymbol, builder);
            }

            if (typeSymbol.HasAttribute("ComponentAddedEvent"))
            {
                GenerateComponentAddedEventInterfaceModel(typeSymbol, builder);
            }

            if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.CloseBraces();
            }


            context.AddSource(typeSymbol.Name + "EventData", SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    private static void GenerateEventViewInterfaceFiles(GeneratorExecutionContext context, SyntaxReceiver receiver)
    {
        foreach (var type in receiver.ComponentWithEventStructs)
        {
            var model = context.Compilation.GetSemanticModel(type.SyntaxTree);
            var typeSymbol = model.GetDeclaredSymbol(type) as ITypeSymbol;
            var builder = new IndentedStringBuilder(
            @"// <auto-generated>
// Code generated by LittleToy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591
");
            builder.AppendLine("using System;");
            builder.AppendLine("using Unity.Entities;");
            builder.AppendLine("using Unity.Mathematics;");
            builder.AppendLine("using Plugins.basegame.Events;");
            builder.AppendLine("using DOTSNET;");
            builder.AppendLine();

            if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.AppendLine($@"namespace {typeSymbol.ContainingNamespace.ToDisplayString()}");
                builder.OpenBraces();
            }

            var onDirtyEventViewAttribute = type.AttributeLists.FindAttribute("OnDirtyEventView");
            var typeOfExpressions = onDirtyEventViewAttribute.ArgumentList.Arguments.Select(_ => _.Expression).OfType<TypeOfExpressionSyntax>();
            var name = typeSymbol.Name;
            var validTypes = typeOfExpressions.Select(typeOfExpression => model.GetTypeInfo(typeOfExpression.Type)).Where(_ => _.Type != null);
            string interfacesList = string.Join(", ", validTypes.Select(typeInfo =>
            {
                return $"I{GetNameRootFromEventComponentType(typeInfo.Type)}Listener";
            }));
            builder.AppendLine($@"public partial class {name} : {interfacesList}");
            builder.OpenBraces();
            builder.CloseBraces();

            if (!typeSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.CloseBraces();
            }

            context.AddSource(typeSymbol.Name + "Event", SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    private static void GenerateEventStructModel(ITypeSymbol typeSymbol, IndentedStringBuilder builder, IEnumerable<IFieldSymbol> fields)
    {
        var name = typeSymbol.Name;
        builder.AppendLine($@"public partial struct {name}");
        builder.OpenBraces();
        builder.AppendLine($@"public bool IsDirty {{ get; set; }}");
        var parameters = string.Join(", ", fields.Select(f => $"{f.Type.ToDisplayString()} {LowercaseName(f.Name)}"));
        builder.AppendLine($@"public {typeSymbol.ToDisplayString()} Update({parameters})");
        builder.OpenBraces();
        var comparison = string.Join(" && ", fields.Select(f => $"{f.Name}.Equals({LowercaseName(f.Name)})"));
        builder.AppendLine($@"if({comparison}) return this;");
        builder.AppendLine();
        builder.AppendLine("IsDirty = true;");
        foreach (var field in fields)
        {
            builder.AppendLine($"{field.Name} = {LowercaseName(field.Name)};");
        }

        builder.AppendLine();
        builder.AppendLine("return this;");
        builder.CloseBraces();

        builder.CloseBraces();
    }

    private static void GenerateComponentDirtyEventInterfaceModel(ITypeSymbol typeSymbol, IndentedStringBuilder builder, IEnumerable<IFieldSymbol> fields)
    {
        var nameRoot = GetNameRootFromEventComponentType(typeSymbol);
        builder.AppendLine($@"public interface I{nameRoot}Listener");
        builder.OpenBraces();
        var parameters = string.Join(", ", fields.Select(f => $"{f.Type.ToDisplayString()} {LowercaseName(f.Name)}"));
        builder.AppendLine($@"public void On{nameRoot}Changed({parameters});");
        builder.CloseBraces();
    }

    private static void GenerateComponentRemovedEventInterfaceModel(ITypeSymbol typeSymbol, IndentedStringBuilder builder)
    {
        var nameRoot = GetNameRootFromEventComponentType(typeSymbol);
        builder.AppendLine($@"public interface I{nameRoot}RemovedListener");
        builder.OpenBraces();
        builder.AppendLine($@"public void On{nameRoot}Removed();");
        builder.CloseBraces();
    }

    private static void GenerateComponentAddedEventInterfaceModel(ITypeSymbol typeSymbol, IndentedStringBuilder builder)
    {
        var nameRoot = GetNameRootFromEventComponentType(typeSymbol);
        builder.AppendLine($@"public interface I{nameRoot}AddedListener");
        builder.OpenBraces();
        builder.AppendLine($@"public void On{nameRoot}Added();");
        builder.CloseBraces();
    }

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static string LowercaseName(string identifier)
    {
        return identifier.Substring(0, 1).ToLowerInvariant() + identifier.Substring(1);
    }

    /// <summary>
    /// ComponentData struct usually suffix with Tag or Data indicating what they are used for.
    /// In order to generate related system or partial structs, those suffix are stripped in order to get the name of the ComponentData struct.
    /// </summary>
    /// <param name="eventComponentType"></param>
    /// <returns></returns>
    private static string GetNameRootFromEventComponentType(ITypeSymbol eventComponentType)
    {
        var typeName = eventComponentType.Name;
        if (typeName.EndsWith("Tag"))
        {
            return typeName.Substring(0, typeName.Length - 3);
        }
        else if (typeName.EndsWith("Data"))
        {
            return typeName.Substring(0, typeName.Length - 4);
        }
        else
        {
            return typeName;
        }
    }

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<StructDeclarationSyntax> ComponentDirtyEventStructs { get; } = new();
        public List<ClassDeclarationSyntax> ComponentWithEventStructs { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode context)
        {
            // any field with at least one attribute is a candidate for property generation
            if (context is StructDeclarationSyntax structDeclarationSyntax
                && structDeclarationSyntax.AttributeLists.Count > 0)
            {
                var componentDirtyEventAttribute = structDeclarationSyntax.AttributeLists.FindAttribute("ComponentDirtyEvent");
                var componentRemovedEventAttribute = structDeclarationSyntax.AttributeLists.FindAttribute("ComponentRemovedEvent");
                var componentAddedEventAttribute = structDeclarationSyntax.AttributeLists.FindAttribute("ComponentAddedEvent");
                if (componentDirtyEventAttribute != null || componentRemovedEventAttribute != null || componentAddedEventAttribute != null)
                {
                    this.ComponentDirtyEventStructs.Add(structDeclarationSyntax);
                }
            }

            if (context is ClassDeclarationSyntax classtDeclarationSyntax
                && classtDeclarationSyntax.AttributeLists.Count > 0)
            {
                var onAddedEventViewAttribute = classtDeclarationSyntax.AttributeLists.FindAttribute("OnAddedEventView");
                var onRemovedEventViewAttribute = classtDeclarationSyntax.AttributeLists.FindAttribute("OnRemovedEventView");
                var onDirtyEventViewAttribute = classtDeclarationSyntax.AttributeLists.FindAttribute("OnDirtyEventView");
                if (onAddedEventViewAttribute != null || onRemovedEventViewAttribute != null || onDirtyEventViewAttribute != null)
                {
                    this.ComponentWithEventStructs.Add(classtDeclarationSyntax);
                }
            }
        }
    }
}
