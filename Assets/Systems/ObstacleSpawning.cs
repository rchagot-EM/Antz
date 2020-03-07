using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

struct ObstacleBuckets : IComponentData
{
    public UnsafeMultiHashMap<int, Position> Values;

    public int GetObstacleBucketCount(float posX, float posY, int mapSize, int bucketResolution)
    {
        int x = (int)(posX / mapSize * bucketResolution);
        int y = (int)(posY / mapSize * bucketResolution);
        return Values.CountValuesForKey(Hash(mapSize, x, y));
    }

    public UnsafeMultiHashMap<int, Position>.Enumerator GetObstacleBucket(float posX, float posY, int mapSize, int bucketResolution)
    {
        int x = (int)(posX / mapSize * bucketResolution);
        int y = (int)(posY / mapSize * bucketResolution);
        return Values.GetValuesForKey(Hash(mapSize, x, y));
    }
    public static int Hash(int mapSize, int x, int y)
    {
        return (x * mapSize) + y;
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class ObstacleSpawning : JobComponentSystem
{
    private EntityQuery m_ObstacleQuery;
    protected override void OnCreate()
    {
        base.OnCreate();
        m_ObstacleQuery = GetEntityQuery(ComponentType.ReadOnly<TagObstacle>());
        RequireSingletonForUpdate<AntManagerSettings>();

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!m_ObstacleQuery.IsEmptyIgnoreFilter)
        {
            return inputDeps;
        }

        var settings = GetSingleton<AntManagerSettings>();
        var obstacleSpawner = GetSingleton<ObstacleSpawner>();
        float radius = obstacleSpawner.ObstacleRadius;
        var mapSize = settings.MapSize;
        var bucketResolution = settings.BucketResolution;
        World.EntityManager.CreateEntity(typeof(ObstacleBuckets));
        var values = new UnsafeMultiHashMap<int, Position>(bucketResolution * bucketResolution, Allocator.Persistent);
        var buckets = new ObstacleBuckets { Values = values };
        SetSingleton(buckets);


        Entities.WithStructuralChanges().ForEach((ref ObstacleSpawner spawner) =>
        {
            var scale = new Vector3(spawner.ObstacleRadius * 2f, spawner.ObstacleRadius * 2f, 1f) / mapSize;
            float obstaclesPerRing = spawner.ObstaclesPerRing;
            for (int ring = 1; ring <= spawner.ObstacleRingCount; ring++)
            {
                float ringRadius = (ring / (spawner.ObstacleRingCount + 1f)) * (mapSize * .5f);
                float circumference = ringRadius * 2f * Mathf.PI;
                int maxCount = Mathf.CeilToInt(circumference / (2f * spawner.ObstacleRadius) * 2f);
                int offset = UnityEngine.Random.Range(0, maxCount);
                int holeCount = UnityEngine.Random.Range(1, 3);
                NativeList<Vector2> obstaclesToSpawn = new NativeList<Vector2>(maxCount, Allocator.TempJob);

                for (int j = 0; j < maxCount; j++)
                {
                    float t = (float)j / maxCount;
                    if ((t * holeCount) % 1f < obstaclesPerRing)
                    {
                        float angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);
                        Vector2 pos = new Vector2(mapSize * .5f + Mathf.Cos(angle) * ringRadius, mapSize * .5f + Mathf.Sin(angle) * ringRadius);
                        obstaclesToSpawn.Add(pos);

                    }
                }
                using (var obstacles = EntityManager.Instantiate(spawner.ObstaclePrefab, obstaclesToSpawn.Length, Allocator.TempJob))
                {
                    for (int j = 0; j < obstacles.Length; j++)
                    {
                        Vector2 pos = obstaclesToSpawn[j];
                        EntityManager.SetComponentData(obstacles[j], new Position()
                        {
                            Value = pos
                        });

                        EntityManager.SetComponentData(obstacles[j], new LocalToWorld()
                        {
                            Value = Matrix4x4.TRS(pos / mapSize, Quaternion.identity, scale)
                        });
                    }
                    obstacles.Dispose();
                }
                obstaclesToSpawn.Dispose();
            }

        }).Run();

        Entities.WithStructuralChanges().WithAll<TagObstacle>().ForEach((Position pos) =>
        {
            for (int x = Mathf.FloorToInt((pos.Value.x - radius) / mapSize * bucketResolution); x <= Mathf.FloorToInt((pos.Value.x + radius) / mapSize * bucketResolution); x++)
            {
                if (x < 0 || x >= bucketResolution)
                {
                    continue;
                }
                for (int y = Mathf.FloorToInt((pos.Value.y - radius) / mapSize * bucketResolution); y <= Mathf.FloorToInt((pos.Value.y + radius) / mapSize * bucketResolution); y++)
                {
                    if (y < 0 || y >= bucketResolution)
                    {
                        continue;
                    }
                    buckets.Values.Add(ObstacleBuckets.Hash(mapSize, x, y), pos);
                }
            }
        }).Run();

        return inputDeps;

    }
}
