using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleToySourceGenerator;

internal static class SymbolExtensions
{
    public static bool HasAttribute(this ISymbol symbol, string attribute)
    {
        return symbol.GetAttributes().Any(a => a.AttributeClass.Name.Contains(attribute));
    }

    public static AttributeSyntax FindAttribute(this SyntaxList<AttributeListSyntax> attributeLists, string searchAttributeName)
    {
        return attributeLists.SelectMany(_ => _.Attributes).FirstOrDefault(a => a.Name.ToFullString().Contains(searchAttributeName));
    }
}
