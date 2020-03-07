using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class FoodSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // TODO: Get real value from mesh...
        float sphereRadius = 0.5f;

        dstManager.AddComponent<TagFoodSource>(entity);
        dstManager.AddComponent<Position>(entity);
        dstManager.AddComponentData(entity, new Radius
        {
            Value = transform.localScale.magnitude * sphereRadius
        });

        dstManager.RemoveComponent<Translation>(entity);
        dstManager.RemoveComponent<Rotation>(entity);
    }
}
