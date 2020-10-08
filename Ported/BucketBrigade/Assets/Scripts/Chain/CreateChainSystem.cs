﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//[UpdateBefore(typeof(BallBrickCollisionSystem))]
public class CreateChainSystem : SystemBase
{
    private static int m_ChainID = 0;
    protected override void OnCreate()
    {
        var chainSystemQueue = EntityManager.CreateEntity();
        EntityManager.AddBuffer<CreateChainBufferElement>(chainSystemQueue);

        // TODO move this code to where we actually decide to create chain
        var fire = new float2(0.0f, 5.0f);
        var water = new float2(5.0f, 0.0f);
        var length = 5;

        CreateChain(water, fire, length);
    }

    // full chain length = 2 * length; "0" bot - fills bucket with water, "length" bot - throws water to fire
    private void CreateChain(float2 start, float2 end, int length)
    {
        var chain = EntityManager.CreateEntity();
        EntityManager.AddComponent<ChainCreateTag>(chain);
        EntityManager.AddComponentData(chain, new ChainStart() { Value = start });
        EntityManager.AddComponentData(chain, new ChainEnd() { Value = end });
        EntityManager.AddComponentData(chain, new ChainLength() { Value = length });
        EntityManager.AddComponentData(chain, new ChainID() { Value = m_ChainID });
        ++m_ChainID;
    }

    protected override void OnUpdate()
    {
        using (var ecb = new EntityCommandBuffer(Allocator.TempJob))
        {
            var chainSystemQueue = GetSingletonEntity<CreateChainBufferElement>();
            var chainQueueBuffer = EntityManager.GetBuffer<CreateChainBufferElement>(chainSystemQueue);
            Entities
                .WithName("CreateChain")
                .WithAll<ChainCreateTag>()
                .ForEach(
                    (Entity entity, int entityInQueryIndex,
                        in ChainStart chainStart, in ChainEnd chainEnd, in ChainLength chainLength, in ChainID chainID) =>
                    {
                        ecb.RemoveComponent<ChainCreateTag>(entity);
                        for (int i = 0; i < chainLength.Value * 2; ++i)
                        {
                            chainQueueBuffer.Add(new CreateChainBufferElement()
                            {
                                chainID = chainID.Value,
                                position = i,
                                start = chainStart.Value,
                                end = chainEnd.Value,
                                length = chainLength.Value,
                            });
                        }
                    })
                .Run();

            var bufferLength = chainQueueBuffer.Length;
            if (bufferLength > 0)
            {
                var bufferPos = 0;
                Entities
                    .WithName("AssignBotsToChain")
                    .WithoutBurst()
                    .WithAll<BotTag>()
                    .WithNone<SharedChainComponent>()
                    .ForEach(
                        (Entity entity, int entityInQueryIndex) =>
                        {
                            if (bufferPos < bufferLength)
                            {
                                ecb.AddComponent(entity,
                                    new ChainPosition()
                                    {
                                        Value = chainQueueBuffer[bufferPos].position
                                    });

                                ecb.AddComponent(entity,
                                    new ChainObjectType()
                                    {
                                        Value = ObjectType.Bot
                                    });

                                ecb.AddSharedComponent(entity,
                                    new SharedChainComponent()
                                    {
                                        chainID = chainQueueBuffer[bufferPos].chainID,
                                        start = chainQueueBuffer[bufferPos].start,
                                        end = chainQueueBuffer[bufferPos].end,
                                        length = chainQueueBuffer[bufferPos].length
                                    });
                                ++bufferPos;
                            }
                        })
                    .Run();

                if (bufferPos > 0)
                {
                    chainQueueBuffer.RemoveRange(0, bufferPos);
                }

                // TODO need update buffer in entity?
                ecb.Playback(EntityManager);
            }
        }
    }
}