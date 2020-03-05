using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class PheromoneRendererSpawnSystem : JobComponentSystem
{
    private EntityQuery m_PheromoneRendererQuery;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        bool hasNoPheromoneRenderer = m_PheromoneRendererQuery.IsEmptyIgnoreFilter;
        if (hasNoPheromoneRenderer)
        {
            Entities.WithStructuralChanges().ForEach((ref AntManagerSettings settings) =>
            {
                int mapSize = settings.MapSize;
                Vector2 rendererPosition = Vector2.one * mapSize * .5f;

                EntityManager.Instantiate(settings.PheromoneRendererPrefab);
            })
            .Run();
        }

        return inputDeps;
    }

    protected override void OnCreate()
    {
        m_PheromoneRendererQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagPheromoneRenderer>() }
        });
    }
}