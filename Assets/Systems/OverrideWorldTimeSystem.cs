using Unity.Core;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(UpdateWorldTimeSystem))]
public class OverrideWorldTimeSystem : JobComponentSystem
{
    public TimeData time;
    public int timeScale = 1;
    int multiplier = 1;
    ComponentSystemGroup simGroup;

    protected override void OnCreate()
    {
        base.OnCreate();

        simGroup = World.GetOrCreateSystem<SimulationSystemGroup>();
        //FixedRateUtils.EnableFixedRateWithCatchUp(simGroup, UnityEngine.Time.fixedDeltaTime);
        FixedRateUtils.EnableFixedRateSimple(simGroup, UnityEngine.Time.fixedDeltaTime);
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        if (Input.inputString == "1")
        { timeScale = 1; }

        else if (Input.inputString == "0")
        { timeScale = 0; }

        else if (Input.inputString != "" && int.TryParse(Input.inputString, out int tempTimeScale) && tempTimeScale != 0)
        { timeScale = tempTimeScale * multiplier; };

        //time = new TimeData(World.Time.ElapsedTime, World.Time.DeltaTime * timeScale);
        //World.SetTime(time);
        if (UnityEngine.Time.timeScale != timeScale)
        {
            UnityEngine.Time.timeScale = timeScale;
            FixedRateUtils.EnableFixedRateWithCatchUp(simGroup, UnityEngine.Time.fixedDeltaTime);
        }

        return inputDependencies;
    }
}