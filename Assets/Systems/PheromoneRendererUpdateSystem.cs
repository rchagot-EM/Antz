using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class PheromoneRendererUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var grid = GetSingleton<PheromoneGrid>().Values;
        var settings = GetSingleton<AntManagerSettings>();

        int mapSize = settings.MapSize;

        Entities.WithoutBurst().WithAll<TagPheromoneRenderer>().ForEach((RenderMesh mesh) =>
        {
            var pheromoneTexture = mesh.material.mainTexture as Texture2D;

            if (pheromoneTexture == null)
            {
                pheromoneTexture = new Texture2D(mapSize, mapSize);
                pheromoneTexture.wrapMode = TextureWrapMode.Mirror;

                mesh.material.mainTexture = pheromoneTexture;
            }

            var colors = new Color[mapSize * mapSize];

            for (int i = 0; i < mapSize * mapSize; ++i)
            {
                colors[i].r = grid[i];
            }

            pheromoneTexture.SetPixels(colors);
            pheromoneTexture.Apply();
        })
        .Run();

        return inputDeps;
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PheromoneGrid>();
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}