using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

public class BeeNavigationSystem : SystemBase
{
	EntityQuery beeQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

		beeQuery = GetEntityQuery(typeof(BeeData));
		Enabled = true;
    }


    protected unsafe override void OnUpdate()
    {
		float deltaTime = Time.DeltaTime;
		int beeCount = beeQuery.CalculateEntityCount();


		NativeArray<BeeData> bees = beeQuery.ToComponentDataArray<BeeData>(Allocator.TempJob);
		NativeArray<BeeData> teamA = new NativeArray<BeeData>(beeCount / 2, Allocator.TempJob);
		NativeArray<BeeData> teamB = new NativeArray<BeeData>(beeCount / 2, Allocator.TempJob);
		int aCount = 0; 
		int bCount = 0;

		Entities.WithAll<BeeData>().ForEach((in BeeData bee) => { 
			if (bee.teamNumber.Equals(BeeTeam.TEAM_A))
            {
				teamA[aCount] = bee;
				aCount++;
            }
			else
            {
				teamB[bCount] = bee;
				bCount++;
			}
		}).Run();



		Entities.WithAll<BeeData>().ForEach( (int entityInQueryIndex) => {
			BeeData bee = bees[entityInQueryIndex];
			bee.isAttacking = false;
			bee.isHoldingResource = false;
			bee.canPickupResource = false;

			if (bee.killed == false)
			{
				BeeData attractiveFriend;
				BeeData repellentFriend;
				NativeArray<BeeData> enemyTeam;
				bee.velocity += new float3(UnityEngine.Random.insideUnitSphere * (bee.flightJitter * deltaTime));
				bee.velocity *= (1f - bee.damping);
				
				if (bee.teamNumber.Equals(BeeTeam.TEAM_A))
				{
					attractiveFriend = teamA[UnityEngine.Random.Range(0, teamA.Length)];
					repellentFriend = teamA[UnityEngine.Random.Range(0, teamA.Length)];
					enemyTeam = teamB;
				}
				else
				{
					attractiveFriend = teamB[UnityEngine.Random.Range(0, teamB.Length)];
					repellentFriend = teamB[UnityEngine.Random.Range(0, teamB.Length)];
					enemyTeam = teamA;
				}

				float3 delta = attractiveFriend.position - bee.position;
				float dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
				if (dist > 0f)
				{
					bee.velocity += delta * (bee.teamAttraction * deltaTime / dist);
				}

				delta = attractiveFriend.position - bee.position;
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
							BeeData enemy = enemyTeam[UnityEngine.Random.Range(0, enemyTeam.Length)];
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
						delta = enemyTarget.position - bee.position;
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
								ParticleManager.SpawnParticle(enemyTarget.position, ParticleType.Blood, bee.velocity * .35f, 2f, 6);
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
				if (UnityEngine.Random.value < (bee.deathTimer - .5f) * .5f)
				{
					ParticleManager.SpawnParticle(bee.position, ParticleType.Blood, float3.zero);
				}

				bee.velocity.y += Field.gravity * deltaTime;
				bee.deathTimer -= deltaTime / 10f;
				if (bee.deathTimer < 0f)
				{
					bee.killed = true;
				}
			}
			bee.position += deltaTime * bee.velocity;


			if (math.abs(bee.position.x) > (Field.size.x * 0.5f))
			{
				bee.position.x = (Field.size.x * .5f) * math.sign(bee.position.x);
				bee.velocity.x *= -.5f;
				bee.velocity.y *= .8f;
				bee.velocity.z *= .8f;
			}
			if (math.abs(bee.position.z) > Field.size.z * .5f)
			{
				bee.position.z = (Field.size.z * .5f) * math.sign(bee.position.z);
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
			if (math.abs(bee.position.y) > Field.size.y * .5f - resourceModifier)
			{
				bee.position.y = (Field.size.y * .5f - resourceModifier) * math.sign(bee.position.y);
				bee.velocity.y *= -.5f;
				bee.velocity.z *= .8f;
				bee.velocity.x *= .8f;
			}

			// only used for smooth rotation:
			float3 oldSmoothPos = bee.smoothPosition;
			if (bee.isAttacking == false)
			{
				bee.smoothPosition = math.lerp(bee.smoothPosition, bee.position, deltaTime * bee.rotationStiffness);
			}
			else
			{
				bee.smoothPosition = bee.position;
			}
			bee.smoothDirection = bee.smoothPosition - oldSmoothPos;
		
		}).ScheduleParallel(Dependency);

		teamA.Dispose();
		teamB.Dispose();
		beeQuery.Dispose();
	}
}
