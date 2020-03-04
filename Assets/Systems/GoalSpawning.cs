using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateAfter(typeof(InitSystem))]
public class GoalSpawning : JobComponentSystem
{
    private EntityQuery m_ColonyQuery;
    private EntityQuery m_FoodSourceQuery;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        bool hasNoColonies = m_ColonyQuery.IsEmptyIgnoreFilter;
        if (hasNoColonies)
        {
            Entities.WithStructuralChanges().ForEach((ref AntManagerSettings settings, ref GoalSpawner spawner) =>
            {
                int mapSize = settings.MapSize;
                Vector2 colonyPosition = Vector2.one * mapSize * .5f;

                var colony = EntityManager.Instantiate(spawner.ColonyPrefab);
                EntityManager.SetComponentData(colony, new Position { Value = colonyPosition });
            })
            .Run();
        }

        bool hasNoFoodSources = m_FoodSourceQuery.IsEmptyIgnoreFilter;
        if (hasNoFoodSources)
        {
            Entities.WithStructuralChanges().ForEach((ref AntManagerSettings settings, ref GoalSpawner spawner) =>
            {
                int mapSize = settings.MapSize;
                float resourceAngle = Random.value * 2f * Mathf.PI;
                Vector2 resourcePosition = Vector2.one * mapSize * .5f + new Vector2(Mathf.Cos(resourceAngle) * mapSize * .475f, Mathf.Sin(resourceAngle) * mapSize * .475f);

                var foodsource = EntityManager.Instantiate(spawner.FoodSourcePrefab);
                EntityManager.SetComponentData(foodsource, new Position { Value = resourcePosition });
            })
            .Run();
        }

        return inputDeps;
    }

    protected override void OnCreate()
    {
        m_ColonyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagColony>() }
        });

        m_FoodSourceQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagFoodSource>() }
        });
    }
}