﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class FireSimulationInitSystem : SystemBase
{
    Random m_Random;

    protected override void OnCreate()
    {
        m_Random = new Random(0x98209104);
    }

    protected override void OnUpdate()
    {
        Entities.
            WithStructuralChanges().
            WithNone<FireSimulationStarted>().
            ForEach((Entity fireSimulationEntity, in FireSimulation simulation, in Translation simulationTranslation, in Rotation simulationRotation) =>
        {
            int cellCount = simulation.rows * simulation.columns;
            float cellSize = simulation.cellSize;

            var entities = EntityManager.Instantiate(simulation.fireCellPrefab, cellCount, Unity.Collections.Allocator.TempJob);

            var neutralColor = new float4(simulation.fireCellColorNeutral.r, simulation.fireCellColorNeutral.g, simulation.fireCellColorNeutral.b, simulation.fireCellColorNeutral.a);

            for (int y = 0; y < simulation.rows; ++y)
            {
                for (int x = 0; x < simulation.columns; ++x)
                {
                    var entity = entities[y * simulation.rows + x];
                    float posX = x * cellSize;
                    float posY = -(simulation.maxFlameHeight * 0.5f) + m_Random.NextFloat(0.01f, 0.02f);
                    float posZ = y * cellSize;
                    var translation = simulationTranslation.Value + new float3(posX, posY, posZ);
                    EntityManager.SetComponentData(entity, new Translation { Value = translation });
                    EntityManager.SetComponentData(entity, simulationRotation);
                    EntityManager.AddComponent<NonUniformScale>(entity);
                    EntityManager.SetComponentData(entity, new NonUniformScale { Value = new float3(cellSize, simulation.maxFlameHeight, cellSize) });
                    EntityManager.SetComponentData(entity, new BaseColor { Value = neutralColor });
                }
            }

            EntityManager.AddComponent<FireSimulationStarted>(fireSimulationEntity);

            entities.Dispose();
        }).Run();
    }
}