using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class BeeSpawnerSystem : SystemBase
{
	Unity.Mathematics.Random random;


	protected override void OnCreate()
    {
        base.OnCreate();
		random = new Unity.Mathematics.Random(42);
	}

    protected override void OnUpdate()
    {
		EntityCommandBuffer buff =  new EntityCommandBuffer(Allocator.TempJob);
		random.NextUInt();
		float3 minP = new float3(0.75f * Field.size.x / 2f, 0.65f * Field.size.y / 2f, Field.size.z / 2f);

		Entities.WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in BeeAuthoring beeAuthoring, in LocalToWorld spawnPosition) => {

			for (int i = 0; i < beeAuthoring.amount; i++)
			{
				Entity spawnedBee = EntityManager.Instantiate(beeAuthoring.prefab);
				float size = UnityEngine.Random.Range(beeAuthoring.minBeeSize, beeAuthoring.maxBeeSize);
				float3 newPos = spawnPosition.Position + new float3(random.NextFloat(-minP.x, minP.x), random.NextFloat(-minP.y, minP.y), random.NextFloat(-minP.z, minP.z));
				SetBeeData(beeAuthoring, spawnedBee, buff, i % 2, size);
				buff.AddComponent(spawnedBee, new Translation
				{
					Value = new float3(newPos)
				});
				buff.AddComponent(spawnedBee, new Scale
				{
					Value = size
				});
                buff.AddComponent(spawnedBee, new BeeColour
                {
                    Value = beeAuthoring.teamColors[i % 2]
                });
            }

			buff.DestroyEntity(entity);
		}).Run();

		buff.Playback(EntityManager);
		buff.Dispose();
    }

	protected static void SetBeeData(BeeAuthoring authoringData, Entity targetEntity, EntityCommandBuffer buff, int teamNumber, float beeSize)
    {
		buff.AddComponent<BeeData>(targetEntity, new BeeData
		{
			velocity = UnityEngine.Random.insideUnitSphere * authoringData.maxSpawnSpeed,
			size = beeSize,
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
			grabDistance = authoringData.grabDistance,
			carryForce = authoringData.carryForce,
		});
	}
}
