using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

public class BeeAttackSystem : SystemBase
{
	EntityQuery beeQuery;

	protected override void OnCreate()
    {
        base.OnCreate();
		beeQuery = GetEntityQuery(typeof(BeeData));
		Enabled = false;
    }

    protected unsafe override void OnUpdate()
    {
		float deltaTime = Time.fixedDeltaTime;
		NativeArray<BeeData> bees = beeQuery.ToComponentDataArray<BeeData>(Allocator.TempJob);

		Entities.WithBurst().ForEach( (int entityInQueryIndex) => {
			BeeData bee = bees[entityInQueryIndex];
            if (bee.enemyTarget != null) {

				BeeData enemyTarget = GetComponent<BeeData>(bee.enemyTarget);
				if (enemyTarget.killed)
				{
					bee.hasEnemy = true;
				}
				else
				{
					float3 delta = enemyTarget.position - bee.position;
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
							bee.hasEnemy = true;
						}
					}
				}
			}
        }).ScheduleParallel(Dependency);
    }
}
