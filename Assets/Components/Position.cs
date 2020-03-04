using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WriteGroup(typeof(LocalToWorld))]
public struct Position : IComponentData
{
    public float2 Value;
}
