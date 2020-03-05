using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class PheromoneSteeringSystem : JobComponentSystem
{
    [BurstCompile]
    struct PheromoneSteeringJob : IJobForEach<Position, FacingAngle, PheromoneSteering>
    {
        [ReadOnly] public UnsafeHashMap<int, float> Grid;
        [ReadOnly] public int MapSize;

        public void Execute([ReadOnly] ref Position position, [ReadOnly] ref FacingAngle facingAngle, ref PheromoneSteering pheromoneSteering)
        {
            float distance = 3f;
            float output = 0;

            for (int i = -1; i <= 1; i += 2)
            {
                float angle = facingAngle.Value + i * Mathf.PI * .25f;
                int x = Convert.ToInt32(position.Value.x + Mathf.Cos(angle) * distance);
                int y = Convert.ToInt32(position.Value.y + Mathf.Sin(angle) * distance);

                if (x < 0 || y < 0 || x >= MapSize || y >= MapSize)
                {
                    return;
                }

                int index = x + y * MapSize;
                float value = Grid[index];
                output += value * i;
            }

            pheromoneSteering.Value = Mathf.Sign(output);
        }
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<AntManagerSettings>();
        RequireSingletonForUpdate<PheromoneGrid>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();
        var pheromones = GetSingleton<PheromoneGrid>();

        var job = new PheromoneSteeringJob
        {
            MapSize = settings.MapSize,
            Grid = pheromones.Values
        };

        return job.ScheduleSingle(this, inputDependencies);
    }
}