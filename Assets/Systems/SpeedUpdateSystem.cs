using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class SpeedUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct SpeedUpdateSystemJob : IJobForEach<Speed>
    {
        public float AntSpeed;
        
        //@TODO: take PheroSteering and ObstacleSteering into account
        public void Execute(ref Speed speed)
        {
            speed.Value = AntSpeed;
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
            AntSpeed = settings.AntSpeed
        };
        return job.Schedule(this, inputDependencies);
    }
}
