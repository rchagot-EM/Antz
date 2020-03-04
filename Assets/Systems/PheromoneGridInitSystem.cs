using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

struct PheromoneGrid : IComponentData
{
    public UnsafeHashMap<int, float> Values;
    public int Height;
    public int Width;

    public static int Hash(PheromoneGrid grid, int x, int y)
    {
        return x + y * grid.Width;
    }
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

            var grid = new PheromoneGrid { Values = values, Height = mapSize, Width = mapSize };
            SetSingleton(grid);
        })
        .Run();

        Enabled = false;
        return inputDeps;
    }
}