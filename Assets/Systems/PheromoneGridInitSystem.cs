using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

struct PheromoneGrid : IComponentData
{
    public UnsafeHashMap<int, float> Values;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class PheromoneGridInitSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        World.EntityManager.CreateEntity(typeof(PheromoneGrid));

        Entities.WithoutBurst().ForEach((ref AntManagerSettings settings) =>
        {
            int mapSize = settings.MapSize;
            var values = new UnsafeHashMap<int, float>(mapSize * mapSize, Allocator.Persistent);

            for(int i=0; i<mapSize*mapSize; ++i)
            {
                values.Add(i, 0f);
            }

            var grid = new PheromoneGrid { Values = values };
            SetSingleton(grid);
        })
        .Run();

        Enabled = false;
        return inputDeps;
    }
}