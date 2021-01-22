using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;


public class ParticleSpawnerSystem : SystemBase
{
    Unity.Mathematics.Random random;

    protected override void OnCreate()
    {
        base.OnCreate();
        random = new Unity.Mathematics.Random(42);
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer buff = new EntityCommandBuffer(Allocator.TempJob);
        random.NextFloat();

        Entities.WithStructuralChanges().WithAll<BeeData>().
            ForEach((Entity entity, int entityInQueryIndex, in BeeData beeData, in ParticleAuthoring particleAuthoring, in LocalToWorld position) => {
                if (beeData.killed) {
                    Debug.LogWarning("SPAWNING BLOOD");
                    for (int i = 0; i < 6; i++)
                    {
                        Entity spawnedParticle = EntityManager.Instantiate(particleAuthoring.prefab);
                        particleAuthoring.type = ParticleType.Blood;
                        particleAuthoring.velocity = beeData.velocity * 0.35f + random.NextFloat() * 2f;

                        buff.AddComponent(spawnedParticle, new Translation { 
                            Value = position.Position 
                        });

                        if (particleAuthoring.type == ParticleType.Blood)
                        {
                            buff.AddComponent(spawnedParticle, new ParticleColor
                            {
                                colorValue = new float4(0.8f, 0.1f, 0.1f, 1f)
                            });
                        } else
                        {
                            buff.AddComponent(spawnedParticle, new ParticleColor
                            {
                                colorValue = new float4(0.1f, 0.1f, 0.1f, 1f)
                            });
                        }
                    }
                }
                buff.DestroyEntity(entity);
            }).Run();


        buff.Playback(EntityManager);
        buff.Dispose();
    }
}
