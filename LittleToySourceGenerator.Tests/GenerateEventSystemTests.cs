﻿namespace LittleToySourceGenerator.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class GenerateEventSystemTests : CodeGenerationTestBase
{
    [TestMethod]
    public void ComponentDirtyEvent()
    {
        string source = @"
[Plugins.basegame.Events.ComponentDirtyEvent]
public partial struct Position3Data : Unity.Entities.IComponentData
{
    [MarkDirty] public Unity.Mathematics.float3 Value;
}

[OnDirtyEventView(typeof(Position3Data))]
public partial class TransformView : LinkedView
{
    public virtual void OnPosition3Changed(float3 value)
    {
        transform.position = value;
    }
}
";
        var generator = new Generator();
        generator.DisableAllGeneration();
        generator.GenerateEventSystem = true;
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591
using System;
using Unity.Entities;
using Unity.Mathematics;
using Plugins.basegame.Events;
using DOTSNET;

[UpdateBefore(typeof(CleanUpAddedRemovedEventSystem))]
[ClientWorld]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class Position3EventSystem : SystemBase
{
    private EntityQuery _entityWithTransformViewAndPosition3DataQuery;
    private EntityQuery _entityWithPosition3DataQuery;
    private ComponentTypeHandle<Position3Data> _Position3DataROComponentTypeHandle;
    private ComponentTypeHandle<Position3Data> _Position3DataRWComponentTypeHandle;

    protected override void OnCreate()
    {
        base.OnCreate();
        _entityWithTransformViewAndPosition3DataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadWrite<TransformView>(),
                ComponentType.ReadOnly<Position3Data>(),
            }
        });
        _entityWithTransformViewAndPosition3DataQuery.SetChangedVersionFilter(ComponentType.ReadOnly<Position3Data>());

        _entityWithPosition3DataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadWrite<Position3Data>(),
            }
        });
        _entityWithPosition3DataQuery.SetChangedVersionFilter(ComponentType.ReadOnly<Position3Data>());
        _Position3DataROComponentTypeHandle = GetComponentTypeHandle<Position3Data>(true);
        _Position3DataRWComponentTypeHandle = GetComponentTypeHandle<Position3Data>(false);
    }

    protected override void OnUpdate()
    {
        _Position3DataROComponentTypeHandle.Update(this);
        var notifyTransformViewPosition3DataDirtyJob = new NotifyPosition3DataDirtyJob<TransformView>
        {
            EntityManager = this.EntityManager,
            DataTypeHandle = _Position3DataROComponentTypeHandle,
            ListenerTypeHandle = EntityManager.GetComponentTypeHandle<TransformView>(false)
        };
        CompleteDependency();
        JobEntityBatchExtensions.RunWithoutJobs(ref notifyTransformViewPosition3DataDirtyJob, _entityWithTransformViewAndPosition3DataQuery);

        _Position3DataRWComponentTypeHandle.Update(this);
        var jobPosition3DataResetDirty = new Position3DataResetDirtyJob
        {
            DataTypeHandle = _Position3DataRWComponentTypeHandle,
        };
        CompleteDependency();
        JobEntityBatchExtensions.RunWithoutJobs(ref jobPosition3DataResetDirty, _entityWithPosition3DataQuery);
    }

    private struct NotifyPosition3DataDirtyJob<T> : IJobEntityBatch where T : class, IPosition3Listener
    {
        public EntityManager EntityManager;
        public ComponentTypeHandle<Position3Data> DataTypeHandle;
        public ComponentTypeHandle<T> ListenerTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var listenerAccessor = batchInChunk.GetManagedComponentAccessor(ListenerTypeHandle, EntityManager);
            var dataArray = batchInChunk.GetNativeArray(DataTypeHandle);
            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var data = dataArray[i];
                var listener = listenerAccessor[i];
                if (data.IsDirty && listener != null)
                {
                    listener.OnPosition3Changed(data.Value);
                }
            }
        }
    }

    private struct Position3DataResetDirtyJob : IJobEntityBatch
    {
        public ComponentTypeHandle<Position3Data> DataTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var dataArray = batchInChunk.GetNativeArray(DataTypeHandle);
            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var data = dataArray[i];
                if (data.IsDirty)
                {
                    data.IsDirty = false;
                }
                dataArray[i] = data;
            }
        }
    }
}
";
        Assert.AreEqual(expectedOutput, output);
    }
    [TestMethod]
    public void MultipleParametersForComponentDirtyEvent()
    {
        string source = @"
[Plugins.basegame.Events.ComponentDirtyEvent]
public partial struct Position3Data : Unity.Entities.IComponentData
{
    [MarkDirty] public Unity.Mathematics.float3 Value;
    [MarkDirty] public Unity.Mathematics.float3 SecondaryValue;
}

[OnDirtyEventView(typeof(Position3Data))]
public partial class TransformView : LinkedView
{
    public virtual void OnPosition3Changed(float3 value)
    {
        transform.position = value;
    }
}
";
        var generator = new Generator();
        generator.DisableAllGeneration();
        generator.GenerateEventSystem = true;
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591
using System;
using Unity.Entities;
using Unity.Mathematics;
using Plugins.basegame.Events;
using DOTSNET;

[UpdateBefore(typeof(CleanUpAddedRemovedEventSystem))]
[ClientWorld]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class Position3EventSystem : SystemBase
{
    private EntityQuery _entityWithTransformViewAndPosition3DataQuery;
    private EntityQuery _entityWithPosition3DataQuery;
    private ComponentTypeHandle<Position3Data> _Position3DataROComponentTypeHandle;
    private ComponentTypeHandle<Position3Data> _Position3DataRWComponentTypeHandle;

    protected override void OnCreate()
    {
        base.OnCreate();
        _entityWithTransformViewAndPosition3DataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadWrite<TransformView>(),
                ComponentType.ReadOnly<Position3Data>(),
            }
        });
        _entityWithTransformViewAndPosition3DataQuery.SetChangedVersionFilter(ComponentType.ReadOnly<Position3Data>());

        _entityWithPosition3DataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadWrite<Position3Data>(),
            }
        });
        _entityWithPosition3DataQuery.SetChangedVersionFilter(ComponentType.ReadOnly<Position3Data>());
        _Position3DataROComponentTypeHandle = GetComponentTypeHandle<Position3Data>(true);
        _Position3DataRWComponentTypeHandle = GetComponentTypeHandle<Position3Data>(false);
    }

    protected override void OnUpdate()
    {
        _Position3DataROComponentTypeHandle.Update(this);
        var notifyTransformViewPosition3DataDirtyJob = new NotifyPosition3DataDirtyJob<TransformView>
        {
            EntityManager = this.EntityManager,
            DataTypeHandle = _Position3DataROComponentTypeHandle,
            ListenerTypeHandle = EntityManager.GetComponentTypeHandle<TransformView>(false)
        };
        CompleteDependency();
        JobEntityBatchExtensions.RunWithoutJobs(ref notifyTransformViewPosition3DataDirtyJob, _entityWithTransformViewAndPosition3DataQuery);

        _Position3DataRWComponentTypeHandle.Update(this);
        var jobPosition3DataResetDirty = new Position3DataResetDirtyJob
        {
            DataTypeHandle = _Position3DataRWComponentTypeHandle,
        };
        CompleteDependency();
        JobEntityBatchExtensions.RunWithoutJobs(ref jobPosition3DataResetDirty, _entityWithPosition3DataQuery);
    }

    private struct NotifyPosition3DataDirtyJob<T> : IJobEntityBatch where T : class, IPosition3Listener
    {
        public EntityManager EntityManager;
        public ComponentTypeHandle<Position3Data> DataTypeHandle;
        public ComponentTypeHandle<T> ListenerTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var listenerAccessor = batchInChunk.GetManagedComponentAccessor(ListenerTypeHandle, EntityManager);
            var dataArray = batchInChunk.GetNativeArray(DataTypeHandle);
            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var data = dataArray[i];
                var listener = listenerAccessor[i];
                if (data.IsDirty && listener != null)
                {
                    listener.OnPosition3Changed(data.Value, data.SecondaryValue);
                }
            }
        }
    }

    private struct Position3DataResetDirtyJob : IJobEntityBatch
    {
        public ComponentTypeHandle<Position3Data> DataTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var dataArray = batchInChunk.GetNativeArray(DataTypeHandle);
            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var data = dataArray[i];
                if (data.IsDirty)
                {
                    data.IsDirty = false;
                }
                dataArray[i] = data;
            }
        }
    }
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void ComponentAddedEvent()
    {
        string source = @"
[Plugins.basegame.Events.ComponentAddedEvent]
public partial struct Position3Data : Unity.Entities.IComponentData
{
    [MarkDirty] public Unity.Mathematics.float3 Value;
}

[OnAddedEventView(typeof(Position3Data))]
public partial class TransformView : LinkedView
{
    public virtual void OnPosition3Added()
    {
    }
}
";
        var generator = new Generator();
        generator.DisableAllGeneration();
        generator.GenerateEventSystem = true;
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591
using System;
using Unity.Entities;
using Unity.Mathematics;
using Plugins.basegame.Events;
using DOTSNET;

[UpdateBefore(typeof(CleanUpAddedRemovedEventSystem))]
[ClientWorld]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class Position3EventSystem : SystemBase
{
    private EntityQuery _entityWithTransformViewAndAddedComponentArrayDataQuery;
    private ComponentTypeHandle<AddedComponentArrayData> _addedComponentArrayDataROComponentTypeHandle;

    protected override void OnCreate()
    {
        base.OnCreate();
        _entityWithTransformViewAndAddedComponentArrayDataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadWrite<TransformView>(),
                ComponentType.ReadOnly<AddedComponentArrayData>(),
            }
        });

        _addedComponentArrayDataROComponentTypeHandle = GetComponentTypeHandle<AddedComponentArrayData>(true);
    }

    protected override void OnUpdate()
    {
        _addedComponentArrayDataROComponentTypeHandle.Update(this);

        var notifyTransformViewAddPosition3DataJob = new NotifyAddPosition3DataJob<TransformView>
        {
            EntityManager = this.EntityManager,
            DataTypeHandle = _addedComponentArrayDataROComponentTypeHandle,
            ListenerTypeHandle = EntityManager.GetComponentTypeHandle<TransformView>(false),
        };
        CompleteDependency();
        JobEntityBatchExtensions.RunWithoutJobs(ref notifyTransformViewAddPosition3DataJob, _entityWithTransformViewAndAddedComponentArrayDataQuery);
    }

    private struct NotifyAddPosition3DataJob<T> : IJobEntityBatch where T : class, IPosition3AddedListener
    {
        public EntityManager EntityManager;
        public ComponentTypeHandle<AddedComponentArrayData> DataTypeHandle;
        public ComponentTypeHandle<T> ListenerTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var listenerAccessor = batchInChunk.GetManagedComponentAccessor(ListenerTypeHandle, EntityManager);
            var dataArray = batchInChunk.GetNativeArray(DataTypeHandle);
            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var data = dataArray[i];
                var listener = listenerAccessor[i];
                for (int j = 0; j < data.Value.Length; j++)
                {
                    if (data.Value[j] == ComponentType.ReadWrite<Position3Data>().TypeIndex)
                    {
                        listener.OnPosition3Added();
                    }
                }
            }
        }
    }
}
";
        Assert.AreEqual(expectedOutput, output);
    }

    [TestMethod]
    public void ComponentRemovedEvent()
    {
        string source = @"
[Plugins.basegame.Events.ComponentRemovedEvent]
public partial struct Position3Data : Unity.Entities.IComponentData
{
    [MarkDirty] public Unity.Mathematics.float3 Value;
}

[OnRemovedEventView(typeof(Position3Data))]
public partial class TransformView : LinkedView
{
    public virtual void OnPosition3Removed()
    {
    }
}
";
        var generator = new Generator();
        generator.DisableAllGeneration();
        generator.GenerateEventSystem = true;
        string output = this.GetGeneratedOutput(source, generator, NullableContextOptions.Disable);

        Assert.IsNotNull(output);

        var expectedOutput = @"// <auto-generated>
// Code generated by LittleToy Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591
using System;
using Unity.Entities;
using Unity.Mathematics;
using Plugins.basegame.Events;
using DOTSNET;

[UpdateBefore(typeof(CleanUpAddedRemovedEventSystem))]
[ClientWorld]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class Position3EventSystem : SystemBase
{
    private EntityQuery _entityWithTransformViewAndRemovedComponentArrayDataQuery;
    private ComponentTypeHandle<RemovedComponentArrayData> _removedComponentArrayDataROComponentTypeHandle;

    protected override void OnCreate()
    {
        base.OnCreate();
        _entityWithTransformViewAndRemovedComponentArrayDataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []
            {
                ComponentType.ReadWrite<TransformView>(),
                ComponentType.ReadOnly<RemovedComponentArrayData>(),
            }
        });

        _removedComponentArrayDataROComponentTypeHandle = GetComponentTypeHandle<RemovedComponentArrayData>(true);
    }

    protected override void OnUpdate()
    {
        _removedComponentArrayDataROComponentTypeHandle.Update(this);

        var notifyTransformViewRemovePosition3DataJob = new NotifyRemovePosition3DataJob<TransformView>
        {
            EntityManager = this.EntityManager,
            DataTypeHandle = _removedComponentArrayDataROComponentTypeHandle,
            ListenerTypeHandle = EntityManager.GetComponentTypeHandle<TransformView>(false),
        };
        CompleteDependency();
        JobEntityBatchExtensions.RunWithoutJobs(ref notifyTransformViewRemovePosition3DataJob, _entityWithTransformViewAndRemovedComponentArrayDataQuery);
    }

    private struct NotifyRemovePosition3DataJob<T> : IJobEntityBatch where T : class, IPosition3RemovedListener
    {
        public EntityManager EntityManager;
        public ComponentTypeHandle<RemovedComponentArrayData> DataTypeHandle;
        public ComponentTypeHandle<T> ListenerTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var listenerAccessor = batchInChunk.GetManagedComponentAccessor(ListenerTypeHandle, EntityManager);
            var dataArray = batchInChunk.GetNativeArray(DataTypeHandle);
            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var data = dataArray[i];
                var listener = listenerAccessor[i];
                for (int j = 0; j < data.Value.Length; j++)
                {
                    if (data.Value[j] == ComponentType.ReadWrite<Position3Data>().TypeIndex)
                    {
                        listener.OnPosition3Removed();
                    }
                }
            }
        }
    }
}
";
        Assert.AreEqual(expectedOutput, output);
    }
}