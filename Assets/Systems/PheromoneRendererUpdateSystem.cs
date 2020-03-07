using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class PheromoneRendererUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct FillJob : IJobParallelFor
    {
        public NativeArray<uint> Colors;
        [ReadOnly] public UnsafeHashMap<int, float> Grid;

        public void Execute(int i)
        {
            Colors[i] = (uint)(Grid[i] * 255);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var grid = GetSingleton<PheromoneGrid>().Values;
        var settings = GetSingleton<AntManagerSettings>();

        int mapSize = settings.MapSize;

        var colors = new NativeArray<uint>(mapSize * mapSize, Allocator.TempJob);

        var job = new FillJob()
        {
            Colors = colors,
            Grid = grid,
        };

        JobHandle jobHandle = job.Schedule(colors.Length, colors.Length / 8);
        jobHandle.Complete();

        Entities.WithoutBurst().WithAll<TagPheromoneRenderer>().ForEach((RenderMesh mesh) =>
        {
            var pheromoneTexture = mesh.material.mainTexture as Texture2D;

            if (pheromoneTexture == null)
            {
                pheromoneTexture = new Texture2D(mapSize, mapSize);
                pheromoneTexture.wrapMode = TextureWrapMode.Mirror;

                mesh.material.mainTexture = pheromoneTexture;
            }

            pheromoneTexture.SetPixelData(colors, 0);
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