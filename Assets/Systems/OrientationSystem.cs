using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateAfter(typeof(RandomSteeringSystem))]
[UpdateAfter(typeof(PheromoneSteeringSystem))]
[UpdateAfter(typeof(ObstacleSteeringSystem))]
[UpdateAfter(typeof(GoalSteeringSystem))]
public class OrientationSystem : JobComponentSystem
{
    [BurstCompile]
    struct OrientationSystemJob : IJobForEach<FacingAngle, RandomSteering, PheromoneSteering, ObstacleSteering, GoalSteering>
    {
        public float PheromoneSteerStrength;
        public float WallSteerStrength;
        public float GoalSteerStrength;

        public void Execute(
            ref FacingAngle facingAngle,
            [ReadOnly] ref RandomSteering randomSteering,
            [ReadOnly] ref PheromoneSteering pheroSteering,
            [ReadOnly] ref ObstacleSteering wallSteering,
            [ReadOnly] ref GoalSteering goalSteering)
        {
            facingAngle.Value += randomSteering.Value;
            facingAngle.Value += pheroSteering.Value * PheromoneSteerStrength;
            facingAngle.Value += wallSteering.Value * WallSteerStrength;
            facingAngle.Value += goalSteering.Value * GoalSteerStrength;
        }
    }


    protected override void OnCreate()
    {
        base.OnCreate();

        RequireSingletonForUpdate<AntManagerSettings>();
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();

        var job = new OrientationSystemJob
        {
            PheromoneSteerStrength = settings.PheromoneSteerStrength,
            WallSteerStrength = settings.WallSteerStrength,
            GoalSteerStrength = settings.GoalSteerStrength
        };

        return job.Schedule(this, inputDependencies);
    }
}
