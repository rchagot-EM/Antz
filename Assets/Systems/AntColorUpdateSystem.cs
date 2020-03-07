using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;

[UpdateAfter(typeof(FoodGatheringSystem))]
public class AntColorUpdateSystem : JobComponentSystem
{
    private RenderMesh SearchRenderMesh;
    private RenderMesh CarryRenderMesh;
    private Material SearchMaterial;
    private Material CarryMaterial;

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

        Entities.ForEach((Entity e) =>
        {
            EntityManager.SetSharedComponentData(e, CarryRenderMesh);
            EntityManager.RemoveComponent<TagAntHasDirtyMesh>(e);
        })
        .WithAll<TagAntHasDirtyMesh>()
        .WithAll<TagAntHasFood>()
        .WithStructuralChanges()
        .Run();

        Entities.ForEach((Entity e) =>
        {
            EntityManager.SetSharedComponentData(e, SearchRenderMesh);
            EntityManager.RemoveComponent<TagAntHasDirtyMesh>(e);
        })
        .WithAll<TagAntHasDirtyMesh>()
        .WithNone<TagAntHasFood>()
        .WithStructuralChanges()
        .Run();

        return inputDeps;
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}
