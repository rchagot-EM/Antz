using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class GoalSteeringSystem : JobComponentSystem
{
    private EntityQuery m_HoldingFoodQuery;
    private EntityQuery m_NotHoldingFoodQuery;
    private EntityQuery m_ColonyQuery;
    private EntityQuery m_FoodSourceQuery;

    static bool Linecast(Vector2 point1, Vector2 point2, in ObstacleBuckets obstacleBuckets, int mapSize, int bucketResolution)
    {
        float dx = point2.x - point1.x;
        float dy = point2.y - point1.y;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        int stepCount = Mathf.CeilToInt(dist * .5f);
        for (int i = 0; i < stepCount; i++)
        {
            float t = (float)i / stepCount;
            if (obstacleBuckets.GetObstacleBucketCount(point1.x + dx * t, point1.y + dy * t, mapSize, bucketResolution) > 0)
            {
                return true;
            }
        }
        return false;
    }

    [BurstCompile]
    struct GoalSteeringJob : IJobForEach_CCC<GoalSteering, Position, FacingAngle>
    {
        public int mapSize;
        public int bucketResolution;
        [ReadOnly] public ObstacleBuckets obstacleBuckets;
        public Position targetPos;

        [BurstDiscard]
        public void Draw(Position antPos, Position targetPos, int mapSize, Color color)
        {
            Debug.DrawLine(new Vector2(antPos.Value.x, antPos.Value.y) / mapSize, new Vector2(targetPos.Value.x, targetPos.Value.y) / mapSize, color);
        }

        public void Execute(ref GoalSteering steering, [ReadOnly] ref Position antPos, [ReadOnly] ref FacingAngle facingAngle)
        {
            float antAngle = facingAngle.Value;
            if (Linecast(antPos.Value, targetPos.Value, obstacleBuckets, mapSize, bucketResolution) == false)
            {
                //Color color = Color.green;
                float targetAngle = Mathf.Atan2(targetPos.Value.y - antPos.Value.y, targetPos.Value.x - antPos.Value.x);
                if (targetAngle - antAngle > Mathf.PI)
                {
                    antAngle += Mathf.PI * 2f;
                    //color = Color.red;
                }
                else if (targetAngle - antAngle < -Mathf.PI)
                {
                    antAngle -= Mathf.PI * 2f;
                    //color = Color.red;
                }
                else
                {
                    if (Mathf.Abs(targetAngle - antAngle) < Mathf.PI * .5f)
                        steering.Value = targetAngle - antAngle;
                    else
                        steering.Value = 0f;
                }
                //Draw(antPos, targetPos, mapSize, color);
            }
        }
    }
    protected override void OnCreate()
    {
        base.OnCreate();
        m_HoldingFoodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagAntHasFood>(),
                ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadWrite<GoalSteering>(),
                ComponentType.ReadOnly<FacingAngle>()}
        });

        m_NotHoldingFoodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagAnt>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadWrite<GoalSteering>(),
                ComponentType.ReadOnly<FacingAngle>()},
            None = new[] { ComponentType.ReadOnly<TagAntHasFood>() }
        });

        m_ColonyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagColony>(), ComponentType.ReadOnly<Position>() }
        });

        m_FoodSourceQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<TagFoodSource>(), ComponentType.ReadOnly<Position>() }
        });

        RequireSingletonForUpdate<AntManagerSettings>();
        RequireSingletonForUpdate<ObstacleBuckets>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();
        var obstacleBucket = GetSingleton<ObstacleBuckets>();

        var getPosFromEntity = GetComponentDataFromEntity<Position>();
        var colonyPos = getPosFromEntity[m_ColonyQuery.GetSingletonEntity()];
        var foodSourcePos = getPosFromEntity[m_FoodSourceQuery.GetSingletonEntity()];

        var jobHasFood = new GoalSteeringJob
        {
            mapSize = settings.MapSize,
            bucketResolution = settings.BucketResolution,
            obstacleBuckets = obstacleBucket,
            targetPos = colonyPos
        };

        var job = jobHasFood.Schedule(m_HoldingFoodQuery, inputDependencies);

        var jobHasNoFood = new GoalSteeringJob
        {
            mapSize = settings.MapSize,
            bucketResolution = settings.BucketResolution,
            obstacleBuckets = obstacleBucket,
            targetPos = foodSourcePos
        };
        return jobHasNoFood.Schedule(m_NotHoldingFoodQuery, job);
    }
}