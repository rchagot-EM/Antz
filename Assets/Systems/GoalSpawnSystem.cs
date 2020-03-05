using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class GoalSpawnSystem : JobComponentSystem
{
    private EntityQuery m_ColonyQuery;
    private EntityQuery m_FoodSourceQuery;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var settings = GetSingleton<AntManagerSettings>();
        int mapSize = settings.MapSize;

        bool hasNoColonies = m_ColonyQuery.IsEmptyIgnoreFilter;
        if (hasNoColonies)
        {
            Vector2 colonyPosition = Vector2.one * mapSize * .5f;

            var colony = EntityManager.Instantiate(settings.ColonyPrefab);
            EntityManager.SetComponentData(colony, new Position { Value = colonyPosition });

            var scale = EntityManager.GetComponentData<NonUniformScale>(colony);
            EntityManager.SetComponentData(colony, new NonUniformScale { Value = scale.Value / mapSize });
        }

        bool hasNoFoodSources = m_FoodSourceQuery.IsEmptyIgnoreFilter;
        if (hasNoFoodSources)
        {
            float resourceAngle = Random.value * 2f * Mathf.PI;
            Vector2 resourcePosition = Vector2.one * mapSize * .5f + new Vector2(Mathf.Cos(resourceAngle) * mapSize * .475f, Mathf.Sin(resourceAngle) * mapSize * .475f);
            
            var foodsource = EntityManager.Instantiate(settings.FoodSourcePrefab);
            EntityManager.SetComponentData(foodsource, new Position { Value = resourcePosition });
            
            var scale = EntityManager.GetComponentData<NonUniformScale>(foodsource);
            EntityManager.SetComponentData(foodsource, new NonUniformScale { Value = scale.Value / mapSize });
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

        RequireSingletonForUpdate<AntManagerSettings>();
    }
}