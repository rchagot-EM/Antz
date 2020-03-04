using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(TransformSystemGroup))]
public class Pos2DToLocalToWorldSystem : JobComponentSystem
{
    EntityQuery m_Query;


    [BurstCompile]
    struct Pos2DToLocalToWorldSystemJob : IJobForEach<LocalToWorld, Position, NonUniformScale>
    {
        public float rcpMapSize;
             
        
        public void Execute(ref LocalToWorld matrix44, [ReadOnly] ref Position position, [ReadOnly] ref NonUniformScale nonUniformScale)
        {
            var rotation = quaternion.identity;
            var scale = float4x4.Scale(nonUniformScale.Value);
            var translation = new float3(position.Value * rcpMapSize, 0f);
            matrix44.Value = math.mul(new float4x4(rotation, translation), scale);
        }
    }


    protected override void OnCreate()
    {
        base.OnCreate();

        m_Query = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                ComponentType.ReadWrite<LocalToWorld>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<NonUniformScale>() },
            Options = EntityQueryOptions.FilterWriteGroup
        });

        RequireSingletonForUpdate<AntManagerSettings>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();

        var job = new Pos2DToLocalToWorldSystemJob();
        job.rcpMapSize = 1.0f / settings.MapSize;

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}
