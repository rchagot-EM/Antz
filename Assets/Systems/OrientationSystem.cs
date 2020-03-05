using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateAfter(typeof(RandomSteeringSystem))]
public class OrientationSystem : JobComponentSystem
{
    [BurstCompile]
    struct OrientationSystemJob : IJobForEach<FacingAngle, RandomSteering, PheromoneSteering, WallSteering, GoalSteering>
    {
        public float PheromoneSteerStrength;
        public float WallSteerStrength;

        public void Execute(
            ref FacingAngle facingAngle,
            [ReadOnly] ref RandomSteering randomSteering,
            [ReadOnly] ref PheromoneSteering pheroSteering,
            [ReadOnly] ref WallSteering wallSteering,
            [ReadOnly] ref GoalSteering goalSteering)
        {
            facingAngle.Value += randomSteering.Value;
            facingAngle.Value += pheroSteering.Value * PheromoneSteerStrength;
            facingAngle.Value += wallSteering.Value * WallSteerStrength;

            //@TODO: Goal
        }
    }


    protected override void OnCreate()
    {
        base.OnCreate();

        RequireSingletonForUpdate<AntManagerAuthoring>();
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();

        var job = new OrientationSystemJob
        {
            PheromoneSteerStrength = settings.PheromoneSteerStrength,
            WallSteerStrength = settings.WallSteerStrength
        };

        return job.Schedule(this, inputDependencies);
    }
}
