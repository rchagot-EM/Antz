        using Unity.Burst;
        using Unity.Collections;
        using Unity.Entities;
        using Unity.Jobs;
        using Unity.Mathematics;
        using Unity.Transforms;
        using UnityEngine;

        public class ObstacleSpawning : JobComponentSystem
        {
            public ObstacleSpawning()
            {
            }

            protected override void OnCreate()
            {
                base.OnCreate();
		        RequireSingletonForUpdate<AntManagerSettings>();
            }

            protected override JobHandle OnUpdate(JobHandle inputDeps)
            {
		        var query = GetEntityQuery(ComponentType.ReadOnly<TagObstacle>());
                if(!query.IsEmptyIgnoreFilter)
                {
			        return inputDeps;
                }

		        var settings = GetSingleton<AntManagerSettings>();
		        var mapSize = settings.MapSize;
		        Entities.ForEach((Entity e, ref ObstacleSpawner spawner) =>
		        {
			        for (int ring = 0; ring < spawner.ObstacleRingCount; ++ring)
			        {
				        using (var obstacles = EntityManager.Instantiate(spawner.ObstaclePrefab, spawner.ObstaclesPerRing, Allocator.Temp))
				        {
						    for (int i = 0; i < obstacles.Length; ++i)
						    {
							    float ringRadius = (ring / (spawner.ObstacleRingCount + 1f)) * mapSize;
							    float circumference = ringRadius * 2f * Mathf.PI;
							    int maxCount = Mathf.CeilToInt(circumference / (2f * spawner.ObstacleRadius));
							    int offset = UnityEngine.Random.Range(0, maxCount);
							    int holeCount = UnityEngine.Random.Range(1, 3);

							    for (int j = 0; j < maxCount; j++)
							    {
								    float t = (float)j / maxCount;
								    if ((t * holeCount) % 1f < spawner.ObstaclesPerRing)
								    {
									    float angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);
									    EntityManager.SetComponentData(obstacles[i], new Position()
									    {
										    Value = new float2(mapSize * .5f + Mathf.Cos(angle) * ringRadius, mapSize * .5f + Mathf.Sin(angle) * ringRadius)
									    });
								    }
							    }
						    }
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
