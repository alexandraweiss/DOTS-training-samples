using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;


public class ParticleSpawnerAuthoring: MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public GameObject particleTemplate;
	public ParticleType type;
	public Vector3 velocity;
	public Vector3 size;
	public float life;
	public float lifeDuration;
	public Vector4 color;
	public bool stuck;
	public Matrix4x4 cachedMatrix;



	public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(particleTemplate);
	}

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		ParticleAuthoring particleData = new ParticleAuthoring
		{
			prefab = conversionSystem.GetPrimaryEntity(particleTemplate),
			type = type,
			velocity = velocity,
			size = size,
			life = life,
			lifeDuration = lifeDuration,
			color = color,
			stuck = stuck,
			cachedMatrix = cachedMatrix
};

		dstManager.AddComponentData(entity, particleData);
	}
}
