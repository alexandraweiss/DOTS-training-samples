using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;



[UpdateInGroup(typeof(InitializationSystemGroup))]
public class BeeSpawnerSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
	}

    protected override void OnUpdate()
    {
		EntityCommandBuffer buff =  new EntityCommandBuffer(Allocator.TempJob);

		Entities.WithStructuralChanges().ForEach( (Entity entity, int entityInQueryIndex, in BeeAuthoring beeAuthoring) => {

			for (int i = 0; i < beeAuthoring.amount; i++)
            {
				Entity spawnedBee = EntityManager.Instantiate(beeAuthoring.Prefab);

				SpawnBee(beeAuthoring, spawnedBee, buff, i % 2 );
			}
			EntityManager.DestroyEntity(entity);

		}).Run();

		buff.Dispose();
    }

	protected static void SpawnBee(BeeAuthoring authoringData, Entity targetEntity, EntityCommandBuffer buff, int teamNumber)
    {
		float3 pos = math.right() * (-Field.size.x * .4f + Field.size.x * .8f * teamNumber);
		buff.AddComponent<BeeData>(targetEntity, new BeeData {
			position = pos,
			velocity = UnityEngine.Random.insideUnitSphere * authoringData.maxSpawnSpeed,
			size = UnityEngine.Random.Range(authoringData.minBeeSize, authoringData.maxBeeSize),
			teamNumber = teamNumber,
			canPickupResource = false,
			killed = false,
			deathTimer = authoringData.deathTimer,
			rotationStiffness = authoringData.rotationStiffness,
			aggression = authoringData.aggression,
			flightJitter = authoringData.flightJitter,
			teamAttraction = authoringData.teamAttraction,
			teamRepulsion = authoringData.teamRepulsion,
			damping = authoringData.damping,
			chaseForce = authoringData.chaseForce,
			attackDistance = authoringData.attackDistance,
			attackForce = authoringData.attackForce,
			hitDistance = authoringData.hitDistance,
		});
	}
}
