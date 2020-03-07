using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class FoodGatheringSystem : JobComponentSystem
{
    private EntityQuery m_HoldingFoodQuery;
    private EntityQuery m_NotHoldingFoodQuery;
    private EntityQuery m_ColonyQuery;
    private EntityQuery m_FoodSourceQuery;
    private EndSimulationEntityCommandBufferSystem m_EndSimECBSystem;

    [BurstCompile]
    struct AddFoodJob : IJobForEachWithEntity<Position, FacingAngle>
    {
        public EntityCommandBuffer.Concurrent ecb;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Position> FoodSourcePos;

        public void Execute(Entity e, int index, [ReadOnly] ref Position antPos, ref FacingAngle facingAngle)
        {
            Vector2 distance = antPos.Value - FoodSourcePos[0].Value;
            if (distance.sqrMagnitude < 4f * 4f)
            {
                //ant.holdingResource = !ant.holdingResource;
                ecb.AddComponent<TagAntHasFood>(index, e);
                ecb.AddComponent<TagAntHasDirtyMesh>(index, e);
                facingAngle.Value += Mathf.PI;
            }
        }
    }

    [BurstCompile]
    struct RemoveFoodJob : IJobForEachWithEntity<Position, FacingAngle>
    {
        public EntityCommandBuffer.Concurrent ecb;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Position> ColonyPos;

        public void Execute(Entity e, int index, [ReadOnly] ref Position antPos, ref FacingAngle facingAngle)
        {
            Vector2 distance = antPos.Value - ColonyPos[0].Value;
            if (distance.sqrMagnitude < 4f * 4f)
            {
                //ant.holdingResource = !ant.holdingResource;
                ecb.RemoveComponent<TagAntHasFood>(index, e);
                ecb.AddComponent<TagAntHasDirtyMesh>(index, e);
                facingAngle.Value += Mathf.PI;
            }
        }
    }
    protected override void OnCreate()
    {
        base.OnCreate();
        m_HoldingFoodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagAntHasFood>(),
                ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadWrite<FacingAngle>()}
        });

        m_NotHoldingFoodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadWrite<FacingAngle>()},
            None = new[] { ComponentType.ReadOnly<TagAntHasFood>() }
        });

        m_ColonyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagColony>(), ComponentType.ReadOnly<Position>() }
        });

        m_FoodSourceQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagFoodSource>(), ComponentType.ReadOnly<Position>() }
        });

        m_EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<AntManagerSettings>();
        RequireSingletonForUpdate<ObstacleBuckets>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        //var settings = GetSingleton<AntManagerSettings>();

        var colonyPositions = m_ColonyQuery.ToComponentDataArrayAsync<Position>(Allocator.TempJob, out JobHandle colonyHandle);
        var foodPositions = m_FoodSourceQuery.ToComponentDataArrayAsync<Position>(Allocator.TempJob, out JobHandle foodHandle);
        inputDependencies = JobHandle.CombineDependencies(inputDependencies, foodHandle);

        var jobHasNoFood = new AddFoodJob
        {
            FoodSourcePos = foodPositions,
            ecb = m_EndSimECBSystem.CreateCommandBuffer().ToConcurrent()
        };

        var job = jobHasNoFood.Schedule(m_NotHoldingFoodQuery, inputDependencies);
        m_EndSimECBSystem.AddJobHandleForProducer(job);

        job = JobHandle.CombineDependencies(job, colonyHandle);
        var jobHasFood = new RemoveFoodJob
        {
            ColonyPos = colonyPositions,
            ecb = m_EndSimECBSystem.CreateCommandBuffer().ToConcurrent()
        };
        job = jobHasFood.Schedule(m_HoldingFoodQuery, job);
        m_EndSimECBSystem.AddJobHandleForProducer(job);
        return job;
    }
}
//create a new command buffer, pass it to job, call complete on job and call playback on command buffer