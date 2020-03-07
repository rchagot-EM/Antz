using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(PheromoneDropSystem))]
public class PheromoneDecaySystem : JobComponentSystem
{
    [BurstCompile]
    struct PheromoneDecayJob : IJobParallelFor
    {
        public UnsafeHashMap<int, float> Grid;
        [ReadOnly] public float TrailDecay;

        public void Execute(int i)
        {
            Grid[i] *= TrailDecay;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var pheromones = GetSingleton<PheromoneGrid>();
        var settings = GetSingleton<AntManagerSettings>();

        int mapSize = settings.MapSize;
        
        var prevSystem = World.GetExistingSystem<PheromoneDropSystem>() as PheromoneDropSystem;

        var jobDecay = new PheromoneDecayJob
        {
            Grid = pheromones.Values,
            TrailDecay = settings.TrailDecay
        };
        
        var jobHandle = jobDecay.Schedule(mapSize * mapSize, mapSize * mapSize / 8, prevSystem.LastJob);

        return jobHandle;
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PheromoneGrid>();
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}
