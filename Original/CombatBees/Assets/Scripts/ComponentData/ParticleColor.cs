using UnityEngine;
using Unity.Entities;
using System;
using Unity.Rendering;
using Unity.Mathematics;

[Serializable]
[MaterialProperty("_ParticleColor", MaterialPropertyFormat.Float4)]
public struct ParticleColor : IComponentData
{
    public float4 colorValue;
}
