using Unity.Entities;


public struct PheromoneSteering : IComponentData
{
    public float Value;
}

public struct GoalSteering : IComponentData
{
    public float Value;
}

public struct WallSteering : IComponentData
{
    public float Value;
}

public struct RandomSteering : IComponentData
{
    public float Value;
}