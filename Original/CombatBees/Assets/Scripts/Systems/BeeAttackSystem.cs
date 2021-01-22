using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

[UpdateAfter(typeof(BeeNavigationSystem))]
public class BeeAttackSystem : SystemBase
{
	EntityQuery beeQuery;

	protected override void OnCreate()
    {
        base.OnCreate();
		beeQuery = GetEntityQuery(new EntityQueryDesc
		{
			All = new[] {
				ComponentType.ReadWrite<BeeData>(),
				ComponentType.ReadWrite<Translation>(),
				ComponentType.ReadWrite<BeeAttacking>(),
			},
		});
	}

    protected unsafe override void OnUpdate()
    {
		float deltaTime = Time.fixedDeltaTime;
		NativeArray<BeeData> bees = beeQuery.ToComponentDataArray<BeeData>(Allocator.TempJob);

		if (bees.Length > 0)
		{
			Entities.WithName("AttackJob").WithBurst().WithAll<BeeData>().WithAll<BeeAttacking>()
				.ForEach((int entityInQueryIndex, in Translation beePosition) =>
			{
				BeeData bee = bees[entityInQueryIndex];
				if (bee.enemyTarget != Entity.Null)
				{
					BeeData enemyTarget = GetComponent<BeeData>(bee.enemyTarget);
					float3 enemyPosition = GetComponent<Translation>(bee.enemyTarget).Value;
					//Debug.DrawLine(beePosition.Value, enemyPosition, Color.white);
					if (enemyTarget.killed)
					{
						bee.hasEnemy = false;
					}
					else
					{
						float3 delta = enemyPosition - beePosition.Value;
						float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
						if (sqrDist > bee.attackDistance * bee.attackDistance)
						{
							bee.velocity += delta * (bee.chaseForce * deltaTime / Mathf.Sqrt(sqrDist));
						}
						else
						{
							//Debug.DrawLine(beePosition.Value, enemyPosition, Color.red);
							bee.isAttacking = true;
							bee.velocity += delta * (bee.attackForce * deltaTime / Mathf.Sqrt(sqrDist));
							if (sqrDist < bee.hitDistance * bee.hitDistance)
							{
								//TODO spawn blood particles
								enemyTarget.killed = true;
								enemyTarget.velocity *= .5f;
								bee.hasEnemy = true;
							}
						}
					}

					bees[entityInQueryIndex] = bee;
				}

			}).ScheduleParallel();

			Dependency.Complete();
		}
		
		bees.Dispose();

    }
}
