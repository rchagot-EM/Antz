using Unity.Entities;


public struct PheromoneSteering : IComponentData
{
    public float Value;
}

public struct GoalSteering : IComponentData
{
    public float Value;
}

public struct ObstacleSteering : IComponentData
{
    public float Value;
}

public struct RandomSteering : IComponentData
{
    public float Value;
}