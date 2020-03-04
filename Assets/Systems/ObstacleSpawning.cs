using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class ObstacleSpawning : JobComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<AntManagerSettings>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<TagObstacle>());
        if (!query.IsEmptyIgnoreFilter)
        {
            return inputDeps;
        }

        var settings = GetSingleton<AntManagerSettings>();
        var mapSize = settings.MapSize;

        Entities.WithStructuralChanges().ForEach((ref ObstacleSpawner spawner) =>
        {
            //TODO clean up
            //float rRadius = (spawner.ObstacleRingCount / (spawner.ObstacleRingCount + 1f)) * (mapSize * .5f);
            //float cir = rRadius * 2f * Mathf.PI;
            //int ultraMaxCount = Mathf.CeilToInt(cir / (2f * spawner.ObstacleRadius) * 2f) * spawner.ObstacleRingCount; 
            NativeList<Matrix4x4> obstaclesToSpawn = new NativeList<Matrix4x4>();// ultraMaxCount);

            for (int ring = 1; ring <= spawner.ObstacleRingCount; ring++)
            {
                float ringRadius = (ring / (spawner.ObstacleRingCount + 1f)) * (mapSize * .5f);
                float circumference = ringRadius * 2f * Mathf.PI;
                int maxCount = Mathf.CeilToInt(circumference / (2f * spawner.ObstacleRadius) * 2f);
                int offset = UnityEngine.Random.Range(0, maxCount);
                int holeCount = UnityEngine.Random.Range(1, 3);

                for (int j = 0; j < maxCount; j++)
                {
                    float t = (float)j / maxCount;
                    if ((t * holeCount) % 1f < spawner.ObstaclesPerRing)
                    {
                        float angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);
                        Vector2 pos = new Vector2(mapSize * .5f + Mathf.Cos(angle) * ringRadius, mapSize * .5f + Mathf.Sin(angle) * ringRadius);
                        obstaclesToSpawn.Add(Matrix4x4.TRS(pos / mapSize, Quaternion.identity, new Vector3(spawner.ObstacleRadius * 2f, spawner.ObstacleRadius * 2f, 1f) / mapSize));

                    }
                }
            }
            using (var obstacles = EntityManager.Instantiate(spawner.ObstaclePrefab, obstaclesToSpawn.Length, Allocator.Temp))
            {
                for (int j = 0; j < obstacles.Length; j++)
                {
                    float2 pos = new float2(obstaclesToSpawn[j].m00, obstaclesToSpawn[j].m01);
                    EntityManager.SetComponentData(obstacles[j], new Position()
                    {
                        Value = pos
                    });

                    EntityManager.SetComponentData(obstacles[j], new LocalToWorld()
                    {
                        Value = obstaclesToSpawn[j]
                    });
                }
            }

        }).Run();

        return inputDeps;
        /*

		obstacleMatrices = new Matrix4x4[Mathf.CeilToInt((float)output.Count / instancesPerBatch)][];
		for (int i = 0; i < obstacleMatrices.Length; i++)
		{
			obstacleMatrices[i] = new Matrix4x4[Mathf.Min(instancesPerBatch, output.Count - i * instancesPerBatch)];
			for (int j = 0; j < obstacleMatrices[i].Length; j++)
			{
				obstacleMatrices[i][j] = Matrix4x4.TRS(output[i * instancesPerBatch + j].position / mapSize, Quaternion.identity, new Vector3(obstacleRadius * 2f, obstacleRadius * 2f, 1f) / mapSize);
			}
		}

		obstacles = output.ToArray();

		List<Obstacle>[,] tempObstacleBuckets = new List<Obstacle>[bucketResolution, bucketResolution];

		for (int x = 0; x < bucketResolution; x++)
		{
			for (int y = 0; y < bucketResolution; y++)
			{
				tempObstacleBuckets[x, y] = new List<Obstacle>();
			}
		}

		for (int i = 0; i < obstacles.Length; i++)
		{
			Vector2 pos = obstacles[i].position;
			float radius = obstacles[i].radius;
			for (int x = Mathf.FloorToInt((pos.x - radius) / mapSize * bucketResolution); x <= Mathf.FloorToInt((pos.x + radius) / mapSize * bucketResolution); x++)
			{
				if (x < 0 || x >= bucketResolution)
				{
					continue;
				}
				for (int y = Mathf.FloorToInt((pos.y - radius) / mapSize * bucketResolution); y <= Mathf.FloorToInt((pos.y + radius) / mapSize * bucketResolution); y++)
				{
					if (y < 0 || y >= bucketResolution)
					{
						continue;
					}
					tempObstacleBuckets[x, y].Add(obstacles[i]);
				}
			}
		}

		obstacleBuckets = new Obstacle[bucketResolution, bucketResolution][];
		for (int x = 0; x < bucketResolution; x++)
		{
			for (int y = 0; y < bucketResolution; y++)
			{
				obstacleBuckets[x, y] = tempObstacleBuckets[x, y].ToArray();
			}
		}
	}*/
    }
}
