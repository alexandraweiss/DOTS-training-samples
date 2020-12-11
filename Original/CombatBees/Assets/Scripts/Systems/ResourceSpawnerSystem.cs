using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

public class ResourceSpawnerSystem : MonoBehaviour, IConvertGameObjectToEntity
{
	public int startBeeCount;
	public GameObject resourcePrefab;

	NativeList<Entity> resourceEntities;
	List<Resource> resources;

	int[,] stackHeights;


	EntityCommandBuffer.ParallelWriter ecbBuff;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
		
    }


 //   public unsafe Entity* TryGetRandomResource()
	//{
	//	if (resources.Count == 0)
	//	{
	//		return null;
	//	}
	//	else
	//	{
	//		Resource resource = resources[UnityEngine.Random.Range(0, resources.Count)];
	//		int stackHeight = stackHeights[resource.gridX, resource.gridY];
	//		if (resource.holder == null || resource.stackIndex == stackHeight - 1)
	//		{
	//			Entity e = resourceEntities[UnityEngine.Random.Range(0, resources.Count)];
	//			return &e;
	//		}
	//		else
	//		{
	//			return null;
	//		}
	//	}
	//}
   
}
