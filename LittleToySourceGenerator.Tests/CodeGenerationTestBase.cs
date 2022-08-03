namespace LittleToySourceGenerator.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class CodeGenerationTestBase
{
    private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    protected string GetGeneratedOutput(string source, Generator generator, NullableContextOptions nullableContextOptions)
    {
        CSharpCompilation compilation = CreateCompilation(source, nullableContextOptions);

        // var compileDiagnostics = compilation.GetDiagnostics();
        // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        string output = outputCompilation.SyntaxTrees.Last().ToString();

        Console.WriteLine(output);

        return output;
    }

    protected IEnumerable<Diagnostic> GetDiagnosticsFromGenerator(string source, Generator generator, NullableContextOptions nullableContextOptions)
    {
        CSharpCompilation compilation = CreateCompilation(source, nullableContextOptions);

        // var compileDiagnostics = compilation.GetDiagnostics();
        // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        return generateDiagnostics;
    }

    private static CSharpCompilation CreateCompilation(string source, NullableContextOptions nullableContextOptions)
    {
        var fakeCode = @"
using System;

namespace DOTSNET
{
    public enum SyncDirection : byte
    {
        // server to client is the first (default) value!
        SERVER_TO_CLIENT,
        CLIENT_TO_SERVER
    }
}

namespace Plugins.basegame.Events
{
    using DOTSNET;

    [System.AttributeUsage(AttributeTargets.Struct)]
    public class CodeGenNetComponentAttribute : System.Attribute
    {
        public readonly SyncDirection SyncDirection;

        public CodeGenNetComponentAttribute(SyncDirection syncDirection)
        {
            SyncDirection = syncDirection;
        }
    }
}
";
        List<MetadataReference> returnList = new();
        returnList.Add(CorlibReference);

        var fakeCompilation = CSharpCompilation.Create(
            "shared",
            new SyntaxTree[] { CSharpSyntaxTree.ParseText(fakeCode) },
            returnList,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        references.Add(fakeCompilation.ToMetadataReference());

        var compilation = CSharpCompilation.Create(
            "foo",
            new SyntaxTree[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));
        return compilation;
    }
}
