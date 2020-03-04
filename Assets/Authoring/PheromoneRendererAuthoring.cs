using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PheromoneRendererAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<TagPheromoneRenderer>(entity);
        dstManager.AddComponent<Position>(entity);
        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
    }
}
