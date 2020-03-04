using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AntManagerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

    }
}
