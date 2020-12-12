using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

public class BeeNavigationSystem : SystemBase
{
	EntityQuery beeQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

		beeQuery = GetEntityQuery(new EntityQueryDesc { 
			All = new [] {
				ComponentType.ReadWrite<BeeData>(),
				ComponentType.ReadWrite<Translation>(),
			},
		});
		Enabled = true;
    }


    protected unsafe override void OnUpdate()
    {
		float deltaTime = Time.DeltaTime;
		int beeCount = beeQuery.CalculateEntityCount();
		Unity.Mathematics.Random random = new Unity.Mathematics.Random(42);

		NativeArray<BeeData> bees = beeQuery.ToComponentDataArray<BeeData>(Allocator.TempJob);
		NativeArray<Translation> beeLocations = beeQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

		NativeArray<BeeData> teamA = new NativeArray<BeeData>(beeCount / 2, Allocator.TempJob);
		NativeArray<Translation> teamAPositions = new NativeArray<Translation>(beeCount / 2, Allocator.TempJob);
		NativeArray<BeeData> teamB = new NativeArray<BeeData>(beeCount / 2, Allocator.TempJob);
		NativeArray<Translation> teamBPositions = new NativeArray<Translation>(beeCount / 2, Allocator.TempJob);
        int aCount = 0;
        int bCount = 0;

		Entities.WithAll<BeeData>().WithAll<Translation>().ForEach((in BeeData bee, in Translation translation) => { 
			if (bee.teamNumber.Equals(BeeTeam.TEAM_A))
            {
                teamA[aCount] = bee;
                teamAPositions[aCount] = translation;
                aCount++;
            }
			else
            {
                teamB[bCount] = bee;
                teamBPositions[bCount] = translation;
                bCount++;
            }
		}).Run();

		Dependency.Complete();

		var navigationJob = Entities.WithAll<BeeData>().ForEach( (int entityInQueryIndex) => {
			BeeData bee = bees[entityInQueryIndex];
			Translation translation = beeLocations[entityInQueryIndex];

			bee.isAttacking = false;
			bee.isHoldingResource = false;
			bee.canPickupResource = false;

			if (bee.killed == false)
			{
				BeeData attractiveFriend;
				float3 attractFriendPos = float3.zero;

				BeeData repellentFriend;
				int repellentFriendIndex = 0;
				float3 repellentFriendPos = float3.zero;

				NativeArray<BeeData> enemyTeam;

				bee.velocity += new float3(random.NextFloat3Direction() * (bee.flightJitter * deltaTime));
				bee.velocity *= (1f - bee.damping);

				if (bee.teamNumber.Equals(BeeTeam.TEAM_A))
				{
					int attractiveFriendIndex = random.NextInt(0, teamAPositions.Length);
                    attractFriendPos = teamAPositions[attractiveFriendIndex].Value;
                    attractiveFriend = teamA[attractiveFriendIndex];

                    repellentFriendIndex = random.NextInt(0, teamB.Length);
                    repellentFriend = teamB[repellentFriendIndex];
                    repellentFriendPos = teamBPositions[repellentFriendIndex].Value;
                    enemyTeam = teamB;
				}
				else
				{
					int attractiveFriendIndex = random.NextInt(0, teamB.Length);
                    attractiveFriend = teamB[attractiveFriendIndex];
                    attractFriendPos = teamBPositions[attractiveFriendIndex].Value;

                    repellentFriendIndex = random.NextInt(0, teamA.Length);
                    repellentFriend = teamA[repellentFriendIndex];
                    repellentFriendPos = teamAPositions[repellentFriendIndex].Value;
                    enemyTeam = teamA;
				}

				float3 delta = attractFriendPos - translation.Value;
				float dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
				if (dist > 0f)
				{
					bee.velocity += delta * (bee.teamAttraction * deltaTime / dist);
				}

				delta = attractFriendPos - translation.Value;
				dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
				if (dist > 0f)
				{
					bee.velocity -= delta * (bee.teamRepulsion * deltaTime / dist);
				}

				if (bee.enemyTarget == null && bee.resourceTarget == null)
				{
					if (UnityEngine.Random.value < bee.aggression)
					{
						if (enemyTeam.Length > 0)
						{
							BeeData enemy = enemyTeam[random.NextInt(0, enemyTeam.Length)];
						}
					}
					else
					{
						bee.canPickupResource = true;
					}
				}
				else if (bee.hasEnemy == true && bee.enemyTarget != null)
				{
					BeeData enemyTarget = GetComponent<BeeData>(bee.enemyTarget);
					if (enemyTarget.killed)
					{
						bee.hasEnemy = false;
					}
					else
					{
						delta = GetComponent<Translation>(bee.enemyTarget).Value - translation.Value;
						float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
						if (sqrDist > bee.attackDistance * bee.attackDistance)
						{
							bee.velocity += delta * (bee.chaseForce * deltaTime / Mathf.Sqrt(sqrDist));
						}
						else
						{
							bee.isAttacking = true;
							bee.velocity += delta * (bee.attackForce * deltaTime / Mathf.Sqrt(sqrDist));
							if (sqrDist < bee.hitDistance * bee.hitDistance)
							{
								//ParticleManager.SpawnParticle(enemyTarget.position, ParticleType.Blood, bee.velocity * .35f, 2f, 6);
								enemyTarget.killed = true;
								enemyTarget.velocity *= .5f;
								bee.hasEnemy = false;
							}
						}
					}
				}
			}
			else
			{
				//if (UnityEngine.Random.value < (bee.deathTimer - .5f) * .5f)
				//{
				//	ParticleManager.SpawnParticle(translation.Value, ParticleType.Blood, float3.zero);
				//}

				bee.velocity.y += Field.gravity * deltaTime;
				bee.deathTimer -= deltaTime / 10f;
				if (bee.deathTimer < 0f)
				{
					bee.killed = true;
				}
			}
			translation.Value += deltaTime * bee.velocity;
			
			
			if (math.abs(translation.Value.x) > (Field.size.x * 0.5f))
			{
				translation.Value.x = (Field.size.x * .5f) * math.sign(translation.Value.x);
				bee.velocity.x *= -.5f;
				bee.velocity.y *= .8f;
				bee.velocity.z *= .8f;
			}
			if (math.abs(translation.Value.z) > Field.size.z * .5f)
			{
				translation.Value.z = (Field.size.z * .5f) * math.sign(translation.Value.z);
				bee.velocity.z *= -.5f;
				bee.velocity.x *= .8f;
				bee.velocity.y *= .8f;
			}
			float resourceModifier = 0f;
			if (bee.isHoldingResource)
			{
				//get from resource manager or other field
				resourceModifier = 0.75f;
			}
			if (math.abs(translation.Value.y) > Field.size.y * .5f - resourceModifier)
			{
				translation.Value = (Field.size.y * .5f - resourceModifier) * math.sign(translation.Value.y);
				bee.velocity.y *= -.5f;
				bee.velocity.z *= .8f;
				bee.velocity.x *= .8f;
			}

			// only used for smooth rotation:
			float3 oldSmoothPos = bee.smoothPosition;
			if (bee.isAttacking == false)
			{
				bee.smoothPosition = math.lerp(bee.smoothPosition, translation.Value, deltaTime * bee.rotationStiffness);
			}
			else
			{
				bee.smoothPosition = translation.Value;
			}
			bee.smoothDirection = bee.smoothPosition - oldSmoothPos;
			

			bees[entityInQueryIndex] = bee;
			beeLocations[entityInQueryIndex] = translation;

		}).Schedule(Dependency);

		navigationJob.Complete();

		var fillLists = Entities.WithName("WriteBackJob").WithAll<BeeData>().WithAll<Translation>().ForEach( (int entityInQueryIndex, ref BeeData bee, ref Translation translation) => {
			bee = bees[entityInQueryIndex];
			translation = beeLocations[entityInQueryIndex];
		}).Schedule(navigationJob);

		fillLists.Complete();

		bees.Dispose();
		beeLocations.Dispose();
		teamA.Dispose();
		teamB.Dispose();
		teamAPositions.Dispose();
		teamBPositions.Dispose();
	}

    protected override void OnDestroy()
    {
        base.OnDestroy();
		beeQuery.Dispose();
    }
}
