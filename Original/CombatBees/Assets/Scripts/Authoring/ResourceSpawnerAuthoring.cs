using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;


public class ResourceSpawnerAuthoring: MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public GameObject resourceTemplate;
	public float amount;
	public float3 position;
	public bool stacked;
	public int stackIndex;
	public int gridX;
	public int gridY;
	public Entity holder;
	public float3 velocity;



	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(resourceTemplate);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		ResourceAuthoring resourceData = new ResourceAuthoring
		{
			prefab = conversionSystem.GetPrimaryEntity(resourceTemplate),
			amount = amount,
			position = position,
			stacked = stacked,
			stackIndex = stackIndex,
			gridX = gridX,
			gridY = gridY,
			velocity = velocity
		};

		dstManager.AddComponentData(entity, resourceData);
	}
}
