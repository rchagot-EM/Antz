using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(PheromoneDropSystem))]
public class PheromoneDecaySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var grid = GetSingleton<PheromoneGrid>().Values;
        var settings = GetSingleton<AntManagerSettings>();

        int mapSize = settings.MapSize;
        float trailDecay = settings.TrailDecay;
        
        var prevSystem = World.GetExistingSystem<PheromoneDropSystem>() as PheromoneDropSystem;
        
        var jobHandle = Job.WithCode(() =>
        {
            for (int i = 0; i < mapSize * mapSize; ++i)
            {
                grid[i] *= trailDecay;
            }
        })
        .WithName("PheromoneDecay")
        .Schedule(prevSystem.LastJob);

        return jobHandle;
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PheromoneGrid>();
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}
