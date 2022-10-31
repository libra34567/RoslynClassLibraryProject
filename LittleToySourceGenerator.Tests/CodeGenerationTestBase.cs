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

    protected string GetGeneratedOutput(string source, Generator generator, NullableContextOptions nullableContextOptions, string assemblyName = "Assembly-CSharp")
    {
        CSharpCompilation compilation = CreateCompilation(source, nullableContextOptions, assemblyName);

        var compileDiagnostics = compilation.GetDiagnostics();
        Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

        string output = outputCompilation.SyntaxTrees.Last().ToString();

        Console.WriteLine(output);

        return output;
    }

    protected IEnumerable<Diagnostic> GetDiagnosticsFromGenerator(string source, Generator generator, NullableContextOptions nullableContextOptions)
    {
        CSharpCompilation compilation = CreateCompilation(source, nullableContextOptions, "Assembly-CSharp");

        // var compileDiagnostics = compilation.GetDiagnostics();
        // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
        return generateDiagnostics;
    }

    private static CSharpCompilation CreateCompilation(string source, NullableContextOptions nullableContextOptions, string assemblyName)
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

    public struct long3 {}
    public class ClientWorldAttribute : Attribute {}
    public class ServerWorldAttribute : Attribute {}
}

namespace Unity.Collections
{
    public struct FixedBytes16 {}
    public struct FixedBytes30 {}
    public struct FixedBytes62 {}
    public struct FixedBytes126 {}
    public struct FixedBytes510 {}
    public struct FixedBytes4094 {}
    public struct FixedString32Bytes {}
    public struct FixedString64Bytes {}
    public struct FixedString128Bytes {}
    public struct FixedString512Bytes {}
    public struct FixedList32Bytes<T> where T: unmanaged {}
    public struct FixedList64Bytes<T> where T: unmanaged {}
    public struct FixedList128Bytes<T> where T: unmanaged {}
    public struct FixedList512Bytes<T> where T: unmanaged {}
    public struct FixedList4096Bytes<T> where T: unmanaged {}
}

namespace Unity.Mathematics
{
    public struct int2 {}
    public struct int3 {}
    public struct int4 {}
    public struct float2 {}
    public struct float3 {}
    public struct float4 {}
    public struct double2 {}
    public struct double3 {}
    public struct double4 {}
    public struct quaternion {}
}

namespace Unity.Entities
{
    public interface IComponentData
    {
    }
    public abstract class SystemBase {}
    public sealed class DisableAutoCreationAttribute : Attribute {}
}

namespace Plugins.basegame.Events
{
    using DOTSNET;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class ComponentDirtyEventAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class ComponentRemovedEventAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class ComponentAddedEventAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MarkDirtyAttribute : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SyncFieldAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OnAddedEventViewAttribute : Attribute
    {
        public readonly Type[] Types;

        public OnAddedEventViewAttribute(params Type[] types)
        {
            Types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OnDirtyEventViewAttribute : Attribute
    {
        public readonly Type[] Types;
        
        public OnDirtyEventViewAttribute(params Type[] types)
        {
            Types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OnRemovedEventViewAttribute : Attribute
    {
        public readonly Type[] Types;

        public OnRemovedEventViewAttribute(params Type[] types)
        {
            Types = types;
        }
    }

    [System.AttributeUsage(AttributeTargets.Struct)]
    public class CodeGenNetComponentAttribute : System.Attribute
    {
        public readonly SyncDirection SyncDirection;

        public CodeGenNetComponentAttribute(SyncDirection syncDirection)
        {
            SyncDirection = syncDirection;
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class CodeGenNetMessageAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ReadWriteEcsAttribute : Attribute
    {
        public readonly Type[] Types;

        public ReadWriteEcsAttribute(params Type[] types)
        {
            Types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HasComponentEcsAttribute : Attribute
    {
        public readonly Type[] Types;

        public HasComponentEcsAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}

namespace Plugins.baseGame
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class GenerateSystemAuthoringAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class FieldFromAuthoringAttribute : Attribute
    {
        public SourceType Type;

        public FieldFromAuthoringAttribute(SourceType type)
        {
            Type = type;
        }
    }

    public enum SourceType
    {
        Public,
        SerializePrivate,
        Inject,
        NetPrefab,
    }
}

public class LinkedView {}
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
            assemblyName,
            new SyntaxTree[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));
        return compilation;
    }
}
