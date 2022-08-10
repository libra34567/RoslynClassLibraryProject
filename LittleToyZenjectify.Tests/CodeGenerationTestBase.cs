namespace LittleToyZenjectify.Tests;

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
        string output = GetGeneratedSyntaxTrees(source, generator, nullableContextOptions).Last().ToString();

        Console.WriteLine(output);

        return output;
    }

    protected IEnumerable<SyntaxTree> GetGeneratedSyntaxTrees(string source, Generator generator, NullableContextOptions nullableContextOptions)
    {
        CSharpCompilation compilation = CreateCompilation(source, nullableContextOptions);

        var compileDiagnostics = compilation.GetDiagnostics();
        Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        return outputCompilation.SyntaxTrees.Skip(1);
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
        // This piece of code provides subset of Unity API, so I source gen can deduce types
        // and operate on generic.
        var fakeCode = @"
using System;

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

namespace Unity.Entities
{
    public interface IComponentData
    {
    }
    public struct ComponentType {}
    public struct Entity {}
    public struct EntityManager
    {
        public Entity CreateEntity() { return new Entity(); }
        public bool AddComponent(Entity entity, ComponentType componentType) => true;
        public bool AddComponent<T>(Entity entity) => true;
        public bool AddComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData => true;
        public bool RemoveComponent<T>(Entity entity) => true;
        public bool RemoveComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData => true;
    }
    public struct EntityCommandBuffer
    {
        public struct ParallelWriter
        {
            public void AddComponent<T>(int sortKey, Entity e) where T : struct, IComponentData {}
            public void AddComponent<T>(int sortKey, Entity e, T component) where T : struct, IComponentData {}
            public void RemoveComponent<T>(int sortKey, Entity e) where T : struct, IComponentData {}
            public void RemoveComponent<T>(int sortKey, Entity e, T component) where T : struct, IComponentData {}
        }
        public bool AddComponent<T>(Entity entity, T component) where T : struct, IComponentData => true;
        public bool AddComponent<T>(Entity entity) where T : struct, IComponentData => true;
        public bool RemoveComponent<T>(Entity entity, T component) where T : struct, IComponentData => true;
        public bool RemoveComponent<T>(Entity entity) where T : struct, IComponentData => true;
    }

    public abstract class SystemBase
    {
        public EntityManager EntityManager => null;
        protected virtual void OnCreate() {};
        protected virtual void OnUpdate() {};
    }
}

public partial struct ExternalComponentData : IComponentData
{
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
