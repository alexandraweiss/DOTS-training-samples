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
	EntityQuery resourceQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

		beeQuery = GetEntityQuery(new EntityQueryDesc { 
			All = new [] {
				ComponentType.ReadWrite<BeeData>(),
				ComponentType.ReadWrite<Translation>(),
			},
		});
		resourceQuery = GetEntityQuery(new EntityQueryDesc
		{
			All = new[] {
				ComponentType.ReadWrite<ResourceData>(),
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
		NativeArray<Entity> resourceEntities = resourceQuery.ToEntityArray(Allocator.TempJob);

		NativeHashMap<Entity, BeeData> teamA = new NativeHashMap<Entity, BeeData>(beeCount / 2, Allocator.TempJob);
		NativeArray<Translation> teamAPositions = new NativeArray<Translation>(beeCount / 2, Allocator.TempJob);
		NativeHashMap<Entity, BeeData> teamB = new NativeHashMap<Entity, BeeData>(beeCount / 2, Allocator.TempJob);
		NativeArray<Translation> teamBPositions = new NativeArray<Translation>(beeCount / 2, Allocator.TempJob);
        int aCount = 0;
        int bCount = 0;

		Entities.WithAll<BeeData>().WithAll<Translation>().ForEach( (in Entity entity, in BeeData bee, in Translation translation) => { 
			if (bee.teamNumber.Equals(BeeTeam.TEAM_A))
            {
                teamA[entity] = bee;
                teamAPositions[aCount] = translation;
                aCount++;
            }
			else
            {
                teamB[entity] = bee;
                teamBPositions[bCount] = translation;
                bCount++;
            }
		}).Run();

		Dependency.Complete();


		var navigationJob = Entities.WithAll<BeeData>().ForEach( (int entityInQueryIndex, in Entity beeEntity) => {
			BeeData bee = bees[entityInQueryIndex];
			Translation translation = beeLocations[entityInQueryIndex];

			bee.isAttacking = false;
			bee.isHoldingResource = false;
			bee.canPickupResource = false;

			int enemyCount = 0;
			
			if (bee.killed == false)
			{
				BeeData attractiveFriend;
				float3 attractFriendPos = float3.zero;

				BeeData repellentFriend;
				int repellentFriendIndex = 0;
				float3 repellentFriendPos = float3.zero;

				NativeHashMap<Entity, BeeData> enemyTeam;

				bee.velocity += new float3(random.NextFloat3Direction() * (bee.flightJitter * deltaTime));
				bee.velocity *= (1f - bee.damping);

				if (bee.teamNumber.Equals(BeeTeam.TEAM_A))
				{
					int attractiveFriendIndex = random.NextInt(0, aCount);
					attractFriendPos = teamAPositions[attractiveFriendIndex].Value;
                    Entity attrFriendEntity = teamA.GetKeyArray(Allocator.Temp)[attractiveFriendIndex];
                    attractiveFriend = teamA[attrFriendEntity];
                    attractFriendPos = teamAPositions[attractiveFriendIndex].Value;

					repellentFriendIndex = random.NextInt(0, bCount);
                    Entity repFriendEntity = teamB.GetKeyArray(Allocator.Temp)[repellentFriendIndex];
                    repellentFriend = teamB[repFriendEntity];
                    repellentFriendPos = teamBPositions[repellentFriendIndex].Value;
                    enemyTeam = teamB;
                    enemyCount = bCount;
				}
				else
				{
					int attractiveFriendIndex = random.NextInt(0, bCount);
                    Entity attrFriendEntity = teamB.GetKeyArray(Allocator.Temp)[attractiveFriendIndex];
                    attractiveFriend = teamB[attrFriendEntity];
                    attractFriendPos = teamBPositions[attractiveFriendIndex].Value;

					repellentFriendIndex = random.NextInt(0, aCount);
                    Entity repFriendEntity = teamA.GetKeyArray(Allocator.Temp)[repellentFriendIndex];
                    repellentFriend = teamA[repFriendEntity];
                    repellentFriendPos = teamAPositions[repellentFriendIndex].Value;
                    enemyTeam = teamA;
                    enemyCount = aCount;
				}

				float3 delta = attractFriendPos - translation.Value;
				float dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
				if (dist > 0f)
				{
					bee.velocity += delta * (bee.teamAttraction * deltaTime / dist);
				}

				delta = repellentFriendPos - translation.Value;
				dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
				if (dist > 0f)
				{
					bee.velocity -= delta * (bee.teamRepulsion * deltaTime / dist);
				}

				if (bee.enemyTarget == Entity.Null && bee.resourceTarget == Entity.Null)
				{
                    if (random.NextFloat() < bee.aggression)
					{
						if (enemyCount > 0)
						{
							Entity randEnemy = enemyTeam.GetKeyArray(Allocator.Temp)[random.NextInt(0, enemyCount)];
							if (randEnemy != Entity.Null && enemyTeam.ContainsKey(randEnemy))
							{
								bee.enemyTarget = randEnemy;
								bee.hasEnemy = true;
							}
                        }
					}
					else
					{
						bee.canPickupResource = true;
						Entity resouce = resourceEntities[random.NextInt(0, resourceEntities.Length)];
						if (resouce != Entity.Null)
						{
							bee.resourceTarget = resouce;
						}
                    }
				}
				else if (bee.enemyTarget != Entity.Null)
				{
                    BeeData enemyTarget = GetComponent<BeeData>(bee.enemyTarget);
					if (enemyTarget.killed)
					{
						bee.hasEnemy = false;
					}
					else
					{
						delta = GetComponent<Translation>(bee.enemyTarget).Value - translation.Value;
						Debug.DrawLine(translation.Value, GetComponent<Translation>(bee.enemyTarget).Value, Color.red);
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
				else if (bee.resourceTarget != Entity.Null)
				{
                    ResourceData resource = GetComponent<ResourceData>(bee.resourceTarget);
					Debug.DrawLine(translation.Value, GetComponent<Translation>(bee.resourceTarget).Value, Color.green);
					BeeData holderBeeData = default(BeeData);
					if (resource.holder == Entity.Null)
					{
						if (resource.dead)
						{
							bee.resourceTarget = Entity.Null;
						}
						else if (resource.stacked)
						{ //TODO && top of stack?
							bee.resourceTarget = Entity.Null;
						}
						else
						{
							delta = GetComponent<Translation>(bee.resourceTarget).Value - translation.Value;
							float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
							if (sqrDist > (bee.grabDistance * bee.grabDistance))
							{
								bee.velocity += delta * (bee.chaseForce * deltaTime / delta);
							}
							else if (resource.stacked)
							{
								resource.holder = beeEntity;
								resource.stacked = false;
								//TODO register and manage stackHeight
								holderBeeData = GetComponent<BeeData>(resource.holder);
							}
						}

					}
					else if (resource.holder.Equals(beeEntity))
					{
						float3 targetPos = new float3(-Field.size.x * .45f + Field.size.x * .9f * bee.teamNumber, 0f, translation.Value.z);
						delta = targetPos - translation.Value;
						dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
						bee.velocity += (targetPos - translation.Value) * (bee.carryForce * deltaTime / dist);
						if (dist < 1f)
						{
							resource.holder = Entity.Null;
							bee.resourceTarget = Entity.Null;
						}
						else
						{
							bee.isHoldingResource = true;
						}
					}
					else if (holderBeeData.Equals(default(BeeData))) {
						if (holderBeeData.teamNumber != bee.teamNumber)
						{
							bee.enemyTarget = resource.holder;
						}
						else if (holderBeeData.teamNumber == bee.teamNumber)
						{
							bee.resourceTarget = Entity.Null;
						}
					}
                    
                }
			}
			else
			{
                if (UnityEngine.Random.value < (bee.deathTimer - .5f) * .5f)
                {
					bee.killed = true;
                    //ParticleManager.SpawnParticle(translation.Value, ParticleType.Blood, float3.zero);
                }

                bee.velocity.y += Field.gravity * deltaTime;
				bee.deathTimer -= deltaTime / 10f;
				if (bee.deathTimer < 0f)
				{
					bee.killed = true;
				}
			}
			
			translation.Value += deltaTime * bee.velocity;
			float toleranceFactor = bee.size * 1.5f;
			if (math.abs(translation.Value.x) > (Field.size.x * 0.5f - toleranceFactor))
			{
				translation.Value.x = (Field.size.x * .5f - toleranceFactor) * math.sign(translation.Value.x);
				bee.velocity.x *= -.5f;
				bee.velocity.y *= .8f;
				bee.velocity.z *= .8f;
			}
			if (math.abs(translation.Value.z) > (Field.size.z * .5f - toleranceFactor))
			{
				translation.Value.z = (Field.size.z * .5f - toleranceFactor) * math.sign(translation.Value.z);
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
			if (math.abs(translation.Value.y) > (Field.size.y * .5f - resourceModifier - toleranceFactor))
			{
				translation.Value.y = (Field.size.y * .5f - resourceModifier - toleranceFactor) * math.sign(translation.Value.y);
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
		resourceEntities.Dispose();
		teamA.Dispose();
		teamB.Dispose();
		teamAPositions.Dispose();
		teamBPositions.Dispose();
	}
}
