using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(TransformSystemGroup))]
public class Map2DToLocalToWorldSystem : JobComponentSystem
{
    //EntityQuery m_Pos2Query;
    //EntityQuery m_Pos2AngleQuery;


    [BurstCompile]
    [ExcludeComponent(typeof(FacingAngle))]
    struct Pos2DToLocalToWorldSystemJob : IJobForEach<LocalToWorld, Position, NonUniformScale>
    {
        public float rcpMapSize;   
        
        public void Execute(ref LocalToWorld matrix44,
            [ReadOnly] ref Position position,
            [ReadOnly] ref NonUniformScale nonUniformScale)
        {
            var rotation = quaternion.identity;
            var scale = float4x4.Scale(nonUniformScale.Value);
            var translation = new float3(position.Value * rcpMapSize, 0f);
            matrix44.Value = math.mul(new float4x4(rotation, translation), scale);
        }
    }


    [BurstCompile]
    struct Pos2DAngleToLocalToWorldSystemJob : IJobForEach<LocalToWorld, Position, FacingAngle, NonUniformScale>
    {
        public float rcpMapSize;
        
        public void Execute(ref LocalToWorld matrix44,
            [ReadOnly] ref Position position,
            [ReadOnly] ref FacingAngle facingAngle,
            [ReadOnly] ref NonUniformScale nonUniformScale)
        {
            var rotation = quaternion.Euler(0f, 0f, facingAngle.Value);
            var scale = float4x4.Scale(nonUniformScale.Value);
            var translation = new float3(position.Value * rcpMapSize, 0f);
            matrix44.Value = math.mul(new float4x4(rotation, translation), scale);
        }
    }


    protected override void OnCreate()
    {
        base.OnCreate();

        /*m_Pos2Query = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                ComponentType.ReadWrite<LocalToWorld>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<NonUniformScale>() },
            None = new[] { ComponentType.ReadOnly<FacingAngle>() },
            Options = EntityQueryOptions.FilterWriteGroup
        });

        m_Pos2AngleQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                ComponentType.ReadWrite<LocalToWorld>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<FacingAngle>(),
                ComponentType.ReadOnly<NonUniformScale>() },
            Options = EntityQueryOptions.FilterWriteGroup
        });*/

        RequireSingletonForUpdate<AntManagerSettings>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();

        var job1 = new Pos2DToLocalToWorldSystemJob
        {
            rcpMapSize = 1.0f / settings.MapSize
        };
        var jobHandle1 = job1.Schedule(this, inputDependencies);

        var job2 = new Pos2DAngleToLocalToWorldSystemJob
        {
            rcpMapSize = 1.0f / settings.MapSize
        };
#if false
        var jobHandle2 = job2.Schedule(this, inputDependencies);

        return JobHandle.CombineDependencies(jobHandle1, jobHandle2);
#else
        var jobHandle2 = job2.Schedule(this, jobHandle1);

        return jobHandle2;
#endif
    }
}
