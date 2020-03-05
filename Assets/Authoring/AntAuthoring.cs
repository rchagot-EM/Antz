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
        dstManager.AddComponent<Speed>(entity);
        dstManager.AddComponent<Position>(entity);
        dstManager.AddComponentData(entity, new Brightness { Value = 1.0f });
        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
        dstManager.AddComponent<TagAnt>(entity);
        dstManager.AddComponent<FacingAngle>(entity);
        dstManager.AddComponent<PheromoneSteering>(entity);
        dstManager.AddComponent<GoalSteering>(entity);
        dstManager.AddComponent<ObstacleSteering>(entity);
        dstManager.AddComponent<RandomSteering>(entity);
    }
}
