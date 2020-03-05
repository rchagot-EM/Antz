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

        for (int i = 0; i < mapSize * mapSize; ++i)
        {
            grid[i] *= trailDecay;
        }

        return inputDeps;
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PheromoneGrid>();
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}
