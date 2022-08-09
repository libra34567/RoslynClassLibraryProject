﻿using CsCodeGenerator;
using CsCodeGenerator.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleToyDocumentor;

[Generator]
public class Generator : ISourceGenerator
{
    private const string FileHeader = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591";

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        // Retrieve the populated receiver
        if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
        {
            return;
        }

        if (context.Compilation.AssemblyName.StartsWith("Unity.", StringComparison.InvariantCultureIgnoreCase)
            || context.Compilation.AssemblyName.StartsWith("UnityEngine.", StringComparison.InvariantCultureIgnoreCase)
            || context.Compilation.AssemblyName.StartsWith("UnityEditor.", StringComparison.InvariantCultureIgnoreCase)
            || context.Compilation.AssemblyName.Equals("Unity", StringComparison.InvariantCultureIgnoreCase)
            || context.Compilation.AssemblyName.StartsWith("DOTSNET.", StringComparison.InvariantCultureIgnoreCase)
            || context.Compilation.AssemblyName.Equals("DOTSNET", StringComparison.InvariantCultureIgnoreCase))
        {
            // Ignore Unity and DOTSNET assemblies, mostly useful for debugging. Can unlock this, if need to generate docs on their classes.
            return;
        }

        var symbols = GetSymbols(context.Compilation, receiver.MemberAccessExpressionSyntaxes).Distinct().ToList();
        foreach (var typeInformation in symbols.GroupBy(s => s.Type))
        {
            var typeDeclarationSytaxes = typeInformation.Key.DeclaringSyntaxReferences.Select(_ => _.GetSyntax() as TypeDeclarationSyntax);
            var isPartial = typeDeclarationSytaxes.All(typeDeclarationSyntax => typeDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
            if (!isPartial)
            {
                continue;
            }

            if (typeInformation.Key.ContainingAssembly != context.Compilation.Assembly)
            {
                // Skip type definitions from other assemblies, since we cannot generate anything for them.
                continue;
            }

            var file = new FileModel(typeInformation.Key.Name + "Documentation")
            {
                Header = FileHeader,
                Namespace = typeInformation.Key.ContainingNamespace?.Name ?? "",
            };
            StringBuilder comment = new StringBuilder();
            bool firstOperation = true;
            foreach (var operationInformation in typeInformation.GroupBy(ti => ti.OperationName))
            {
                if (!firstOperation)
                {
                    comment.AppendLine();
                }

                bool firstLine = true;
                var callSites = operationInformation.Select(_ => (_.Method.Name, _.Method.ReceiverType));
                foreach (var callSiteInformation in callSites.OrderBy(type => type.ReceiverType.Name).GroupBy(_ => _.ReceiverType))
                {
                    var usedMethods = callSiteInformation.Select(_ => _.Name).OrderBy(_ => _);
                    if (firstLine)
                    {
                        comment.AppendLine($"{operationInformation.Key} in {string.Join(", ", usedMethods)} of {callSiteInformation.Key.Name}");
                        firstLine = false;
                    }
                    else
                    {
                        comment.AppendLine($"     in {string.Join(", ", usedMethods)} of {callSiteInformation.Key.Name}");
                    }
                }

                firstOperation = false;
            }

            if (typeInformation.Key.IsValueType)
            {
                file.Structs.Add(new StructModel(typeInformation.Key.Name)
                {
                    Comment = comment.ToString(),
                    UseXmlDocCommentStyle = true,
                    SingleKeyWord = KeyWord.Partial,
                });
            }
            else
            {
                file.Classes.Add(new ClassModel(typeInformation.Key.Name)
                {
                    Comment = comment.ToString(),
                    UseXmlDocCommentStyle = true,
                    SingleKeyWord = KeyWord.Partial,
                });
            }
            context.AddSource(file.Name, SourceText.From(file.ToString(), Encoding.UTF8));
        }
    }

    private IEnumerable<(/*MemberAccessExpressionSyntax SyntaxNode, */string OperationName, IMethodSymbol Method, ITypeSymbol Type)> GetSymbols(Compilation compilation, IList<MemberAccessExpressionSyntax> syntaxes)
    {
        foreach (var target in syntaxes)
        {
            var model = compilation.GetSemanticModel(target.SyntaxTree);
            var methodSymbol = model.GetSymbolInfo(target).Symbol as IMethodSymbol;
            if (!IsValidMethod(methodSymbol))
            {
                continue;
            }
            
            if (methodSymbol.IsGenericMethod)
            {
                var operationName = methodSymbol.Name.StartsWith("Add") ? "Added" : "Removed";
                var componentType = methodSymbol.TypeArguments[0];
                var methodDeclaration = FindMethodDeclaration(target);
                methodSymbol = model.GetDeclaredSymbol(methodDeclaration);
                yield return (/*target, */operationName, methodSymbol, componentType);
            }
            else
            {
                continue;
                // throw new NotImplementedException();
            }
        }    
    }

    private static MethodDeclarationSyntax FindMethodDeclaration(SyntaxNode node)
    {
        while (node != null)
        {
            if (node is MethodDeclarationSyntax method)
            {
                return method;
            }

            node = node.Parent;
        }

        return null;
    }

    private static bool IsValidMethod(IMethodSymbol methodSymbol)
    {
        var type = methodSymbol.ReceiverType;
        var wellKnownTypes = new[] { "Unity.Entities.EntityManager", "Unity.Entities.EntityCommandBuffer", "Unity.Entities.EntityCommandBuffer.ParallelWriter" };
        if (!wellKnownTypes.Contains(type.ToDisplayString()))
        {
            return false;
        }

        var wellKnownMethods = new[] { "AddComponent", "AddComponentData", "RemoveComponent", "RemoveComponentData" };
        if (!wellKnownMethods.Contains(methodSymbol.Name))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MemberAccessExpressionSyntax> MemberAccessExpressionSyntaxes = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    if (memberAccessExpressionSyntax.OperatorToken.IsKind(SyntaxKind.DotToken) && 
                        (memberAccessExpressionSyntax.Name.ToFullString().StartsWith("AddComponent") || memberAccessExpressionSyntax.Name.ToFullString().StartsWith("RemoveComponent")))
                    {
                        MemberAccessExpressionSyntaxes.Add(memberAccessExpressionSyntax);
                    }
                }
            }
        }
    }
}
