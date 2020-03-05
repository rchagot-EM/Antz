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
    struct MovementSystemJob : IJobForEachWithEntity<Position, FacingAngle, Speed>
    {
        public float DeltaTime;
        public int MapSize;
        public float OutwardStrength;
        public float InwardStrength;
        public float2 ColonyPosition;
        public float ObstacleRadius;
        public int BucketResolution;
        public ObstacleBuckets OstacleBuckets; // copy :/
        [ReadOnly] public ComponentDataFromEntity<TagAntHasFood> HasFoodComponent; 
    
        public void Execute(Entity entity, int index,
            ref Position position, /*[ReadOnly] */ref FacingAngle facingAngle, [ReadOnly] ref Speed speed)
        {
            float vx = math.cos(facingAngle.Value) * speed.Value * DeltaTime;
            float vy = math.sin(facingAngle.Value) * speed.Value * DeltaTime;
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


            // Obstacles
            var nearbyObstacles = OstacleBuckets.GetObstacleBucket(position.Value.x, position.Value.y, MapSize, BucketResolution);
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
            float inwardOrOutward = -OutwardStrength;
            float pushRadius = MapSize * .4f;
            if (HasFoodComponent.HasComponent(entity))
            {
                inwardOrOutward = InwardStrength;
                pushRadius = MapSize;
            }
            float dx2 = ColonyPosition.x - position.Value.x;
            float dy2 = ColonyPosition.y - position.Value.y;
            float dist2 = math.sqrt(dx2 * dx2 + dy2 * dy2);
            inwardOrOutward *= 1f - math.saturate(dist2 / pushRadius);
            vx += dx2 / dist2 * inwardOrOutward;
            vy += dy2 / dist2 * inwardOrOutward;


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

        var colonyQuery = GetEntityQuery(
            ComponentType.ReadOnly<TagColony>(),
            ComponentType.ReadOnly<Position>());
        var colonyPositions = colonyQuery.ToComponentDataArray<Position>(Allocator.TempJob);

        //@TODO: split?
        var job = new MovementSystemJob
        {
            DeltaTime = World.Time.DeltaTime,
            MapSize = settings.MapSize,
            OutwardStrength = settings.OutwardStrength,
            InwardStrength = settings.InwardStrength,
            ColonyPosition = colonyPositions[0].Value,
            ObstacleRadius = obstacleSpawner.ObstacleRadius,
            BucketResolution = settings.BucketResolution,
            OstacleBuckets = obstacleBuckets,
            HasFoodComponent = GetComponentDataFromEntity<TagAntHasFood>(true)
        };

        colonyPositions.Dispose();

        return job.Schedule(this, inputDependencies);
    }
}