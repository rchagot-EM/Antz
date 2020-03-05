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
    private EntityQuery m_GridQuery;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        bool hasNoGrid = m_GridQuery.IsEmptyIgnoreFilter;
        if (hasNoGrid)
        {
            var settings = GetSingleton<AntManagerSettings>();
            int mapSize = settings.MapSize;

            World.EntityManager.CreateEntity(typeof(PheromoneGrid));

            var values = new UnsafeHashMap<int, float>(mapSize * mapSize, Allocator.Persistent);

            for (int i = 0; i < mapSize * mapSize; ++i)
            {
                values.Add(i, 0f);
            }

            var grid = new PheromoneGrid { Values = values };
            SetSingleton(grid);
        }

        return inputDeps;
    }


    protected override void OnCreate()
    {
        m_GridQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<PheromoneGrid>() }
        });

        RequireSingletonForUpdate<AntManagerSettings>();
    }
}