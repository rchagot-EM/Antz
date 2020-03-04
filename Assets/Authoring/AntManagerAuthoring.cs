using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


//@TODO: split in several components (AntSpawner, etc)
public struct AntManagerSettings : IComponentData
{
	public int MapSize;
	public float AntSpeed;
	public float AntAccel;

	public float TrailAddSpeed;
	public float TrailDecay;
	public float RandomSteering;
	public float PheromoneSteerStrength;
	public float WallSteerStrength;
	public float GoalSteerStrength;
	public float OutwardStrength;
	public float InwardStrength;
	public int RotationResolution;
}


public struct AntSpawner : IComponentData
{
	public Entity AntPrefab;
	public int AntCount;
}


public struct ObstacleSpawner : IComponentData
{
	public Entity ObstaclePrefab;
	public int ObstacleRingCount;
	public int ObstaclesPerRing;
	public float ObstacleRadius;
}


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AntManagerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public GameObject antPrefab;
	public GameObject obstaclePrefab;

	public int antCount;
	public int mapSize = 128;
	public int bucketResolution;
	public float antSpeed;
	[Range(0f, 1f)]
	public float antAccel;
	public float trailAddSpeed;
	[Range(0f, 1f)]
	public float trailDecay;
	public float randomSteering;
	public float pheromoneSteerStrength;
	public float wallSteerStrength;
	public float goalSteerStrength;
	public float outwardStrength;
	public float inwardStrength;
	public int rotationResolution = 360;
	public int obstacleRingCount;
	[Range(0f, 1f)]
	public int obstaclesPerRing;
	public float obstacleRadius;


	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
		dstManager.AddComponentData(entity, new AntManagerSettings
		{
			MapSize = mapSize,
			AntSpeed = antSpeed,
			AntAccel = antAccel,
			TrailAddSpeed = trailAddSpeed,
			TrailDecay = trailDecay,
			RandomSteering = randomSteering,
			PheromoneSteerStrength = pheromoneSteerStrength,
			WallSteerStrength = wallSteerStrength,
			GoalSteerStrength = goalSteerStrength,
			OutwardStrength = outwardStrength,
			InwardStrength = inwardStrength,
			RotationResolution = rotationResolution
		});

		dstManager.AddComponentData(entity, new AntSpawner
		{
			AntPrefab = conversionSystem.GetPrimaryEntity(antPrefab),
			AntCount = antCount
		});

		dstManager.AddComponentData(entity, new ObstacleSpawner
		{
			ObstaclePrefab = conversionSystem.GetPrimaryEntity(obstaclePrefab),
			ObstacleRingCount = obstacleRingCount,
			ObstaclesPerRing = obstaclesPerRing,
			ObstacleRadius = obstacleRadius
		});
	}


	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(antPrefab);
		referencedPrefabs.Add(obstaclePrefab);
	}
}
