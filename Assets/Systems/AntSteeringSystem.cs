using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class AntSteeringSystem : JobComponentSystem
{
    Random RandomGen;

    [BurstCompile]
    struct RandomSteeringJob : IJobForEach_C<RandomSteering>
    {
        public float RandomSteeringRange;
        public Random random;

        public void Execute(ref RandomSteering randomSteering)
        {
            randomSteering.Value = random.NextFloat(-RandomSteeringRange, RandomSteeringRange);
        }
    }

    protected override void OnCreate()
    {
        RandomGen = new Random(123456);
        base.OnCreate();
        RequireSingletonForUpdate<AntManagerSettings>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        var settings = GetSingleton<AntManagerSettings>();

        var job = new RandomSteeringJob
        {
            random = RandomGen,
            RandomSteeringRange = settings.RandomSteering
        };

        return job.Schedule(this, inputDependencies);

    }
}