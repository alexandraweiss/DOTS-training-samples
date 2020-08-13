﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct Bot : IComponentData
{
    public float3 targetTranslation;
}

public struct BucketTosser : IComponentData { }

public struct BucketFiller : IComponentData { }