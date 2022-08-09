﻿using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LittleToyDocumentor.Tests;

[TestClass]
public class GenerateDoc: CodeGenerationTestBase
{
    [TestMethod]
    public void FindingCandidates()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void IgnoreDuplicateCalls()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
        EntityManager.AddComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MultipleMethods()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
        EntityManager.AddComponent<SampleComponentData>(entity);
    }

    protected override void OnCreate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnCreate, OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MultipleSystems()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
        EntityManager.AddComponent<SampleComponentData>(entity);
    }

    protected override void OnCreate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
    }
}

public partial class AnotherSystem: SystemBase
{
    protected override void OnCreate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnCreate of AnotherSystem
///      in OnCreate, OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void NonGenericMethod()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SampleComponentData
        {
            SampleComponentField = 1
        });
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void EntityCommandBuffer()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        var ecb = new EntityCommandBuffer();
        ecb.AddComponent(entity, new SampleComponentData
        {
            SampleComponentField = 1
        });
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void AnotherEntityCommandBuffer()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        var ecb = new EntityCommandBuffer();
        ecb.AddComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void MultipleComponents()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial struct SampleComponentTag : IComponentData
{    
}

public partial struct AnotherSampleComponentTag : IComponentData
{    
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
        var ecb = new EntityCommandBuffer();
        ecb.AddComponent<AnotherSampleComponentTag>(entity);
    }

    protected override void OnCreate()
    {
        var entity = EntityManager.CreateEntity();
        var parallelWriter = new EntityCommandBuffer.ParallelWriter();
        var entityInQueryIndex = 100;
        parallelWriter.AddComponent(entityInQueryIndex, entity, new SampleComponentTag());
        parallelWriter.AddComponent<SampleComponentTag>(entityInQueryIndex, entity);
    }
}

public partial class AnotherSystem: SystemBase
{
    protected override void OnCreate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = string.Join(string.Empty, this.GetGeneratedSyntaxTrees(source, generator, NullableContextOptions.Disable).Select(_ => _.ToString()));

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnCreate of AnotherSystem
///      in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// </summary>
public partial struct AnotherSampleComponentTag
{
}
// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnCreate of Position3Data
/// </summary>
public partial struct SampleComponentTag
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void RemoveComponent()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.RemoveComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Removed in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void RemoveAndAddTogether()
    {
        string source = @"
using Unity.Entities;

public partial struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
        EntityManager.RemoveComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Documentation Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591


/// <summary>
/// Added in OnUpdate of Position3Data
/// Removed in OnUpdate of Position3Data
/// </summary>
public partial struct SampleComponentData
{
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void DoNotGenerateForNonPartial()
    {
        string source = @"
using Unity.Entities;

public struct SampleComponentData : IComponentData
{
    public int SampleComponentField;
}

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<SampleComponentData>(entity);
        EntityManager.RemoveComponent<SampleComponentData>(entity);
    }
}
";
        var generator = new Generator();
        var generatedTree = this.GetGeneratedSyntaxTrees(source, generator, NullableContextOptions.Disable);

        Assert.AreEqual(0, generatedTree.Count());
    }

    [TestMethod]
    public void DoNotDocumentExternalComponentData()
    {
        string source = @"
using Unity.Entities;

public partial class Position3Data: SystemBase
{
    protected override void OnUpdate()
    {
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent<ExternalComponentData>(entity);
    }
}
";
        var generator = new Generator();
        var syntaxes = this.GetGeneratedSyntaxTrees(source, generator, NullableContextOptions.Disable);

        Assert.AreEqual(0, syntaxes.Count());
    }
}
