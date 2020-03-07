using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//[UpdateAfter(typeof(CollisionSystem))]
[UpdateAfter(typeof(MovementSystem))]
public class PheromoneDropSystem : JobComponentSystem
{
    private EntityQuery m_FoodHolderQuery;
    private EntityQuery m_FoodSeekerQuery;

    public JobHandle LastJob; // HACK

    [BurstCompile]
    [RequireComponentTag(typeof(TagAnt))]
    struct PheromoneDropJob : IJobForEach<Position, Speed>
    {
        public UnsafeMultiHashMap<int, float>.ParallelWriter GridUpdates;

        [ReadOnly] public int MapSize;
        [ReadOnly] public float AntSpeed;
        [ReadOnly] public float TrailAddSpeed;
        [ReadOnly] public float Excitement;
        [ReadOnly] public float DeltaTime;

        public void Execute([ReadOnly] ref Position position, [ReadOnly] ref Speed speed)
        {
            int x = Mathf.FloorToInt(position.Value.x);
            int y = Mathf.FloorToInt(position.Value.y);
            if (x < 0 || y < 0 || x >= MapSize || y >= MapSize)
            {
                return;
            }

            float excitement = Excitement * speed.Value / AntSpeed;

            int hash = x + y * MapSize;

            GridUpdates.Add(hash, excitement * TrailAddSpeed * DeltaTime);
        }
    }

    [BurstCompile]
    struct PheromoneGatherUpdatesJob : IJobParallelFor
    {
        public UnsafeHashMap<int, float> Grid;
        [ReadOnly] public UnsafeMultiHashMap<int, float> GridUpdates;

        public void Execute(int i)
        {
            var it = GridUpdates.GetValuesForKey(i);

            while (it.MoveNext())
            {
                Grid[i] += it.Current * (1f - Grid[i]);
            }

            Grid[i] = Mathf.Min(1f, Grid[i]);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var pheromones = GetSingleton<PheromoneGrid>();
        var settings = GetSingleton<AntManagerSettings>();

        int mapSize = settings.MapSize;
        float antSpeed = settings.AntSpeed;
        float trailAddSpeed = settings.TrailAddSpeed;
        float deltaTime = Time.DeltaTime;

        var gridUpdates = new UnsafeMultiHashMap<int, float>(mapSize * mapSize, Allocator.Temp);

        var jobDropLow = new PheromoneDropJob
        {
            GridUpdates = gridUpdates.AsParallelWriter(),
            MapSize = mapSize,
            AntSpeed = antSpeed,
            TrailAddSpeed = trailAddSpeed,
            Excitement = .3f,
            DeltaTime = deltaTime,
        };

        var jobDropHigh = new PheromoneDropJob
        {
            GridUpdates = gridUpdates.AsParallelWriter(),
            MapSize = mapSize,
            AntSpeed = antSpeed,
            TrailAddSpeed = trailAddSpeed,
            Excitement = 1f,
            DeltaTime = deltaTime,
        };

        var jobGather = new PheromoneGatherUpdatesJob
        {
            Grid = pheromones.Values,
            GridUpdates = gridUpdates,
        };

        var h1 = jobDropLow.Schedule(m_FoodSeekerQuery, inputDeps);
        var h2 = jobDropHigh.Schedule(m_FoodHolderQuery, h1);

        var h3 = jobGather.Schedule(mapSize * mapSize, mapSize * mapSize / 8, h2);

        LastJob = h3;
        return h3;
    }

    protected override void OnCreate()
    {
        m_FoodHolderQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Speed>(),
                ComponentType.ReadOnly<TagAntHasFood>()
            }
        });

        m_FoodSeekerQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Speed>()
            },
            None = new[]
            {
                ComponentType.ReadOnly<TagAntHasFood>()
            }
        });

        RequireSingletonForUpdate<PheromoneGrid>();
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}
