using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ObstacleSteeringSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<AntManagerSettings>();
        RequireSingletonForUpdate<ObstacleBuckets>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        const float distance = 1.5f;// :(
        var settings = GetSingleton<AntManagerSettings>();
        var mapSize = settings.MapSize;
        var bucketResolution = settings.BucketResolution;
        var ObstacleBucket = GetSingleton<ObstacleBuckets>();

        return Entities.WithoutBurst().WithAll<TagAnt>().ForEach((ref FacingAngle facingAngle, ref Position position, ref ObstacleSteering steering) =>
        {
            steering.Value = 0;
            for (int i = -1; i <= 1; i += 2)
            {
                float angle = facingAngle.Value + i * Mathf.PI * .25f;
                float testX = position.Value.x + Mathf.Cos(angle) * distance;
                float testY = position.Value.y + Mathf.Sin(angle) * distance;

                if (testX < 0 || testY < 0 || testX >= mapSize || testY >= mapSize)
                {
                }
                else
                {
                    int value = ObstacleBucket.GetObstacleBucketCount(testX, testY, mapSize, bucketResolution);
                    if (value > 0)
                    {
                        steering.Value -= i;
                    }
                }
            }
        }).Schedule(inputDependencies);

    }
}