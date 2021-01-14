using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[Serializable]
[MaterialProperty("_BeeColor", MaterialPropertyFormat.Float4)]
public struct BeeColour : IComponentData
{
    public float4 Value;
}
