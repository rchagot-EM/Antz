using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


[UpdateAfter(typeof(SpeedUpdateSystem))]
[UpdateAfter(typeof(OrientationSystem))]
public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementSystemJob : IJobForEach<Position, FacingAngle, Speed>
    {
        public int MapSize;
        public float OutwardStrength;
        public float ObstacleRadius;
        public int BucketResolution;
        public ObstacleBuckets OstacleBuckets; // copy :/
    
        public void Execute(ref Position position, /*[ReadOnly] */ref FacingAngle facingAngle, [ReadOnly] ref Speed speed)
        {
            float vx = math.cos(facingAngle.Value) * speed.Value;
            float vy = math.sin(facingAngle.Value) * speed.Value;
            float ovx = vx;
            float ovy = vy;

            // Check map bounds
            if (position.Value.x + vx < 0f || position.Value.x + vx > MapSize) {
                vx = -vx;
            }
            else {
                position.Value.x += vx;
            }
            if (position.Value.y + vy < 0f || position.Value.y + vy > MapSize) {
                vy = -vy;
            }
            else {
                position.Value.y += vy;
            }


            //@TODO: Obstacles
            var nearbyObstacles = OstacleBuckets.GetObstacleBucket(position.Value.x, position.Value.y, MapSize, BucketResolution);
            //for (int j = 0; j < nearbyObstacles.Length; j++)
            while (nearbyObstacles.MoveNext())
            {
                Position obstaclePosition = nearbyObstacles.Current;
                float dx = position.Value.x - obstaclePosition.Value.x;
                float dy = position.Value.y - obstaclePosition.Value.y;
                float sqrDist = dx * dx + dy * dy;
                if (sqrDist < ObstacleRadius * ObstacleRadius)
                {
                    float dist = math.sqrt(sqrDist);
                    dx /= dist;
                    dy /= dist;
                    position.Value.x = obstaclePosition.Value.x + dx * ObstacleRadius;
                    position.Value.y = obstaclePosition.Value.y + dy * ObstacleRadius;

                    vx -= dx * (dx * vx + dy * vy) * 1.5f;
                    vy -= dy * (dx * vx + dy * vy) * 1.5f;
                }
            }


            //@TODO: inward/outward direction


            // Update facing angle based on new position
            if (ovx != vx || ovy != vy)
            {
                facingAngle.Value = math.atan2(vy, vx);
            }
        }
    }


    protected override void OnCreate()
    {
        base.OnCreate();

        RequireSingletonForUpdate<AntManagerSettings>();
        RequireSingletonForUpdate<ObstacleSpawner>();
        RequireSingletonForUpdate<ObstacleBuckets>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();
        var obstacleSpawner = GetSingleton<ObstacleSpawner>();
        var obstacleBuckets = GetSingleton<ObstacleBuckets>();

        //@TODO: split?
        var job = new MovementSystemJob
        {
            MapSize = settings.MapSize,
            OutwardStrength = settings.OutwardStrength,
            ObstacleRadius = obstacleSpawner.ObstacleRadius,
            BucketResolution = settings.BucketResolution,
            OstacleBuckets = obstacleBuckets
        };
        
        return job.Schedule(this, inputDependencies);
    }
}