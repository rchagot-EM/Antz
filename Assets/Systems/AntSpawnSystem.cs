using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


[UpdateInGroup(typeof(InitializationSystemGroup))]
public class AntSpawnSystem : JobComponentSystem
{
    public int TotalAnts;
    EntityQuery m_AntQuery;
    protected override void OnCreate()
    {
        base.OnCreate();
        m_AntQuery = GetEntityQuery(ComponentType.ReadOnly<TagAnt>());
        RequireSingletonForUpdate<AntManagerSettings>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!m_AntQuery.IsEmptyIgnoreFilter)
            return inputDeps;
        var random = new Random(0x1a2b3c4d);

        var settings = GetSingleton<AntManagerSettings>();
        var mapSize = settings.MapSize;

        Entities.WithStructuralChanges().ForEach((Entity e, ref AntSpawner spawner) =>
        {
            using (var ants = EntityManager.Instantiate(spawner.AntPrefab, spawner.AntCount, Allocator.Temp))
            {
                TotalAnts = spawner.AntCount;
                for (int i = 0; i < ants.Length; ++i)
                {
                    EntityManager.SetComponentData(ants[i], new Position
                    {
                        Value = new float2
                        {
                            x = random.NextFloat(-5f, 5f) + mapSize * .5f,
                            y = random.NextFloat(-5f, 5f) + mapSize * .5f
                        }
                    });

                    EntityManager.SetComponentData(ants[i], new FacingAngle
                    {
                        Value = random.NextFloat() * math.PI * 2f
                    });

                    EntityManager.SetComponentData(ants[i], new Brightness
                    {
                        Value = random.NextFloat(.75f, 1.25f)
                    });

                    EntityManager.AddComponent<TagAntHasDirtyMesh>(ants[i]);
                }
            }
        }).Run();
        return inputDeps;
    }
}
