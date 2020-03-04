using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PheromoneRendererAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<TagPheromoneRenderer>(entity);
        dstManager.SetComponentData(entity, new Translation
        {
            Value = new float3(0.5f, 0.5f, 0.0f)
        });
    }
}
