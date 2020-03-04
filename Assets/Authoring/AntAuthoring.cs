using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AntAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Speed
        {
            Value = 0f
        });

        //TODO 0,0?
        dstManager.AddComponentData(entity, new Position
        {
            Value = new float2(0f, 0f)
        });

        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
        dstManager.AddComponent(entity, typeof(TagAnt));
        dstManager.AddComponentData(entity, new PheromoneSteering
        {
            Value = 0f
        });
        dstManager.AddComponentData(entity, new GoalSteering
        {
            Value = 0f
        });
        dstManager.AddComponentData(entity, new WallSteering
        {
            Value = 0f
        });
        dstManager.AddComponentData(entity, new RandomSteering
        {
            Value = 0f
        });
    }
}
