using Unity.Collections;
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

        var colors = new NativeArray<Color>(mapSize * mapSize * 3, Allocator.TempJob);

        Job.WithCode(() =>
        {
            for (int i = 0; i < mapSize * mapSize; ++i)
            {
                colors[i] = new Color { r = grid[i] };
            }
        })
        .Run();

        Entities.WithoutBurst().WithAll<TagPheromoneRenderer>().ForEach((RenderMesh mesh) =>
        {
            var pheromoneTexture = mesh.material.mainTexture as Texture2D;

            if (pheromoneTexture == null)
            {
                pheromoneTexture = new Texture2D(mapSize, mapSize);
                pheromoneTexture.wrapMode = TextureWrapMode.Mirror;

                mesh.material.mainTexture = pheromoneTexture;
            }

            pheromoneTexture.SetPixels(colors.ToArray());
            pheromoneTexture.Apply();
        })
        .Run();

        colors.Dispose();

        return inputDeps;
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PheromoneGrid>();
        RequireSingletonForUpdate<AntManagerSettings>();
    }
}