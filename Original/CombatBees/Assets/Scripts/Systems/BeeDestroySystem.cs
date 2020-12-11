using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(BeeAttackSystem))]
public class BeeDestroySystem : SystemBase
{

    EntityCommandBufferSystem ecbSys;

    protected override void OnCreate()
    {
        base.OnCreate();

		ecbSys = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        Enabled = false;
    }

    protected override void OnUpdate()
    {
		var ecbBuff = ecbSys.CreateCommandBuffer().AsParallelWriter();
		Entities.ForEach((int entityInQueryIndex, Entity entity, in BeeData bee) => { 
			if (bee.killed)
            {
				ecbBuff.RemoveComponent<BeeData>(entityInQueryIndex, entity);
            }
		}).ScheduleParallel(Dependency);
        
    }
}
