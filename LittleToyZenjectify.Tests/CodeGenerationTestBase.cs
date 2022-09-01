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

    protected string GetGeneratedOutputFromPosition(string source, Generator generator, NullableContextOptions nullableContextOptions, int expectedCount, int position)
    {
        var trees = GetGeneratedSyntaxTrees(source, generator, nullableContextOptions).TakeLast(expectedCount).ToList();
        Assert.AreEqual(expectedCount, trees.Count);
        string output = trees.ElementAt(position).ToString();

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
using UnityEngine;

// That's actually should be in user assembly.
// here is for testing convenience
public enum InstallerNameEnum
{
    AnotherNewInstaller = 1,
    SomeNewInstaller = 2,
    SoundTest = 3,
}

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ZenGenAttribute : Attribute
{
    public string TargetInstallerNameName;
    public ZenGenTypeEnum DiTypeName;
    public bool IsLazyLoading;
    public bool BindInterfacesAndSelf;
    public string Suffix;
    
    public ZenGenAttribute( ZenGenTypeEnum diType, InstallerNameEnum targetInstallerNameName, bool bindInterfacesAndSelf = false, bool isLazyLoading = false, string suffix = """")
    {
        TargetInstallerNameName = targetInstallerNameName.ToString();
        DiTypeName = diType;
        IsLazyLoading = isLazyLoading;
        BindInterfacesAndSelf = bindInterfacesAndSelf;
        Suffix = suffix;
    } 
    
    public ZenGenAttribute( ZenGenTypeEnum diType, string targetInstallerNameName, bool bindInterfacesAndSelf = false, bool isLazyLoading = false, string suffix = """")
    {
        TargetInstallerNameName = targetInstallerNameName;
        DiTypeName = diType;
        IsLazyLoading = isLazyLoading;
        BindInterfacesAndSelf = bindInterfacesAndSelf;
        Suffix = suffix;
    } 
}

public enum ZenGenTypeEnum
{
    MonoClassWithAssetInstance,
    MonoClassWithSceneObjInstance,
    Signal,
    ClassWithoutInstance,
    Prefab,
}
namespace UnityEngine
{
    public class MonoBehaviour {}
}

namespace Zenject
{
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
    }

    public class MonoInstaller : MonoBehaviour
    {
        [Inject]
        protected DiContainer Container
        {
            get; set;
        }

        public virtual void InstallBindings()
        {
            throw new NotImplementedException();
        }
    }
}

namespace Sirenix.OdinInspector
{
    public sealed class RequiredAttribute : Attribute {}
    public sealed class SceneObjectsOnlyAttribute : Attribute {}
    public sealed class AssetsOnlyAttribute : Attribute {}
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
