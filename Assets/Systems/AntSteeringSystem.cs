using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


public class AntSteeringSystem : JobComponentSystem
{
    EntityQuery m_AntQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_AntQuery = GetEntityQuery(ComponentType.ReadOnly<TagAnt>());
        RequireForUpdate(m_AntQuery);
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        int count = m_AntQuery.CalculateEntityCount();
        var randomSteering   = new NativeArray<float>(count, Allocator.TempJob);
        var pheroSteering    = new NativeArray<float>(count, Allocator.TempJob);
        var obstacleSteering = new NativeArray<float>(count, Allocator.TempJob);
        var goalSteering     = new NativeArray<float>(count, Allocator.TempJob);


        var outputDependencies = inputDependencies;
        randomSteering.Dispose(outputDependencies);
        pheroSteering.Dispose(outputDependencies);
        obstacleSteering.Dispose(outputDependencies);
        goalSteering.Dispose(outputDependencies);

        return outputDependencies;
    }
}