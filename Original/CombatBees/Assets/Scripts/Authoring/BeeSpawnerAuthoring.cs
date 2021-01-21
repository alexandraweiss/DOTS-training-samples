using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;


public class BeeSpawnerAuthoring: MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public GameObject beeTemplate;
	public float4[] teamColors;
	public float minBeeSize;
	public float maxBeeSize;
	public float speedStretch;
	public float rotationStiffness;
	public float aggression;
	public float flightJitter;
	public float teamAttraction;
	public float teamRepulsion;
	public float damping;
	public float chaseForce;
	public float carryForce;
	public float grabDistance;
	public float attackDistance;
	public float attackForce;
	public float hitDistance;
	public float maxSpawnSpeed;
	public int startBeeCount;
	public float deathTimer;


	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(beeTemplate);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		BeeAuthoring beeData = new BeeAuthoring
		{
			prefab = conversionSystem.GetPrimaryEntity(beeTemplate),
			teamColors = teamColors,
			minBeeSize = minBeeSize,
			maxBeeSize = maxBeeSize,
			speedStretch = speedStretch,
			rotationStiffness = rotationStiffness,
			aggression = aggression,
			flightJitter = flightJitter,
			teamAttraction = teamAttraction,
			teamRepulsion = teamRepulsion,
			damping = damping, 
			chaseForce = chaseForce,
			carryForce = carryForce, 
			grabDistance = grabDistance,
			attackDistance = attackDistance,
			attackForce = attackForce,
			hitDistance = hitDistance,
			maxSpawnSpeed = maxSpawnSpeed,
			amount = startBeeCount,
			deathTimer = deathTimer,
		};

		dstManager.AddComponentData(entity, beeData);
	}
}
