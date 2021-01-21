using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

public class ResourceSpawnerSystem : SystemBase
{
    Unity.Mathematics.Random random;
    protected override void OnCreate()
    {
        base.OnCreate();
        random = new Unity.Mathematics.Random(42);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        float minPX = 0.75f * Field.size.x * 0.5f;
        float yPos = - Field.size.y * 0.5f;
        float minPZ = Field.size.z * 0.5f;
        random.NextUInt();

        Entities.WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in ResourceAuthoring resourceAuthoring, in LocalToWorld localToWorld) => {
        for (int i = 0; i < resourceAuthoring.amount; i++)
        {
            Entity spawnedResource = EntityManager.Instantiate(resourceAuthoring.prefab);
            // Spawn on a random position on the ground
            float3 spawnPosition = new float3(random.NextFloat(-minPX, minPX), yPos + localToWorld.Value.c1.y, random.NextFloat(-minPZ, minPZ));
            ecb.AddComponent<Translation>(spawnedResource, new Translation { 
                Value = spawnPosition
            });

            ecb.AddComponent<ResourceData>(spawnedResource, new ResourceData
            {
                position = localToWorld.Position,
                stacked = false,
                stackIndex = resourceAuthoring.stackIndex,
                gridX = resourceAuthoring.gridX,
                gridY = resourceAuthoring.gridY,
                velocity = resourceAuthoring.velocity,
                dead = false,
            });
        }

        ecb.DestroyEntity(entity);
        }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
