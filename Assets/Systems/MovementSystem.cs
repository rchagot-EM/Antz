using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


[UpdateAfter(typeof(SpeedUpdateSystem))]
//[UpdateAfter(typeof(FacingAngleSystem))] //@TODO
public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementSystemJob : IJobForEach<Position, FacingAngle, Speed>
    {
        public int MapSize;
        public float OutwardStrength;
    
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
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var settings = GetSingleton<AntManagerSettings>();

        //@TODO: split?
        var job = new MovementSystemJob
        {
            MapSize = settings.MapSize,
            OutwardStrength = settings.OutwardStrength
        };
        
        return job.Schedule(this, inputDependencies);
    }
}