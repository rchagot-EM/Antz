using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;

[UpdateAfter(typeof(FoodGatheringSystem))]
public class AntColorUpdateSystem : JobComponentSystem
{
    private EntityQuery m_FoodHolderQuery;
    private EntityQuery m_FoodSeekerQuery;

    private EndSimulationEntityCommandBufferSystem m_EndSimECBSystem;

    private RenderMesh SearchRenderMesh;
    private RenderMesh CarryRenderMesh;
    private Material SearchMaterial;
    private Material CarryMaterial;

    /*[BurstCompile]
    [RequireComponentTag(typeof(TagAnt))]
    struct AntColorUpdateJob : IJobForEachWithEntity<Brightness>
    {
        public EntityCommandBuffer.Concurrent Ecb;

        [ReadOnly] public RenderMesh Mesh;

        public void Execute(Entity e, int index, [ReadOnly] ref Brightness brightness)
        {
            Ecb.SetSharedComponent(index, e, Mesh);
        }
    }*/

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if(SearchMaterial == null)
        {
            var settings = GetSingleton<AntManagerSettings>();
            var antSpawner = GetSingleton<AntSpawner>();

            var searchColor = settings.SearchColor;
            var carryColor = settings.CarryColor;

            Entity antPrefab = antSpawner.AntPrefab;
            var antPrefabRenderMesh = EntityManager.GetSharedComponentData<RenderMesh>(antPrefab);
            var antPrefabMaterial = antPrefabRenderMesh.material;

            SearchMaterial = new Material(antPrefabMaterial);
            CarryMaterial = new Material(antPrefabMaterial);

            SearchMaterial.color = searchColor;
            CarryMaterial.color = carryColor;

            SearchRenderMesh = antPrefabRenderMesh;
            CarryRenderMesh = antPrefabRenderMesh;

            SearchRenderMesh.material = SearchMaterial;
            CarryRenderMesh.material = CarryMaterial;
        }

        /*var jobCarrierUpdate = new AntColorUpdateJob
        {
            Ecb = m_EndSimECBSystem.CreateCommandBuffer().ToConcurrent(),
            Mesh = CarryRenderMesh
        };

        var jobSeekerColorUpdate = new AntColorUpdateJob
        {
            Ecb = m_EndSimECBSystem.CreateCommandBuffer().ToConcurrent(),
            Mesh = SearchRenderMesh
        };

        var h1 = jobCarrierUpdate.Schedule(m_FoodSeekerQuery, inputDeps);
        var h2 = jobSeekerColorUpdate.Schedule(m_FoodHolderQuery, h1);

        m_EndSimECBSystem.AddJobHandleForProducer(h1);
        m_EndSimECBSystem.AddJobHandleForProducer(h2);

        return h2;*/

        Entities.ForEach((Entity e) =>
        {
            var mesh = EntityManager.GetSharedComponentData<RenderMesh>(e);
            if (mesh.material != CarryMaterial)
            {
                EntityManager.SetSharedComponentData(e, CarryRenderMesh);
            }
        })
        .WithAll<TagAnt>()
        .WithAll<TagAntHasFood>()
        .WithStructuralChanges()
        .Run();

        Entities.ForEach((Entity e) =>
        {
            var mesh = EntityManager.GetSharedComponentData<RenderMesh>(e);
            if (mesh.material != SearchMaterial)
            {
                EntityManager.SetSharedComponentData(e, SearchRenderMesh);
            }
        })
        .WithAll<TagAnt>()
        .WithNone<TagAntHasFood>()
        .WithStructuralChanges()
        .Run();

        return inputDeps;
    }

    protected override void OnCreate()
    {
        m_FoodHolderQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<TagAntHasFood>()
            }
        });

        m_FoodSeekerQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<TagAnt>()
            },
            None = new[]
            {
                ComponentType.ReadOnly<TagAntHasFood>()
            }
        });

        m_EndSimECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        RequireSingletonForUpdate<AntManagerSettings>();
    }
}
