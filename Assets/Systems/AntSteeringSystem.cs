using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class AntSteeringSystem : JobComponentSystem
{
    [BurstCompile]
    struct RandomSteeringJob : IJobForEach_C<RandomSteering>
    {
        public float RandomSteeringRange;

        public void Execute(ref RandomSteering randomSteering)
        {
            randomSteering.Value = Random.Range(-RandomSteeringRange, RandomSteeringRange);
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

        var job = new RandomSteeringJob
        {
            RandomSteeringRange = settings.RandomSteering
        };

        return job.Schedule(this, inputDependencies);

    }
}