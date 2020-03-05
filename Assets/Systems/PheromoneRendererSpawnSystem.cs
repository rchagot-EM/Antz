using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class PheromoneRendererSpawnSystem : JobComponentSystem
{
    private EntityQuery m_PheromoneRendererQuery;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var settings = GetSingleton<AntManagerSettings>();
        int mapSize = settings.MapSize;

        bool hasNoPheromoneRenderer = m_PheromoneRendererQuery.IsEmptyIgnoreFilter;
        if (hasNoPheromoneRenderer)
        {
            Entity e = EntityManager.Instantiate(settings.PheromoneRendererPrefab);
        }

        return inputDeps;
    }

    protected override void OnCreate()
    {
        m_PheromoneRendererQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagPheromoneRenderer>() }
        });

        RequireSingletonForUpdate<AntManagerSettings>();
    }
}