using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateAfter(typeof(PheromoneSteeringSystem))]
[UpdateAfter(typeof(ObstacleSteeringSystem))]
public class SpeedUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct SpeedUpdateSystemJob : IJobForEach_CCC<Speed, PheromoneSteering, ObstacleSteering>
    {
        public float AntSpeed;
        public float AntAccel;
       
        public void Execute(ref Speed speed, [ReadOnly] ref PheromoneSteering pheromone, [ReadOnly] ref ObstacleSteering obstacle)
        {
            float targetSpeed = AntSpeed;
            targetSpeed *= 1f - (math.abs(pheromone.Value) + math.abs(obstacle.Value)) / 3f;
            speed.Value += (targetSpeed - speed.Value) * AntAccel;
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

        var job = new SpeedUpdateSystemJob
        {
            AntSpeed = settings.AntSpeed,
            AntAccel = settings.AntAccel
        };
        return job.Schedule(this, inputDependencies);
    }
}
