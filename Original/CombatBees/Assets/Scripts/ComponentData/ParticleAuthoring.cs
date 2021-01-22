using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class ParticleAuthoring : IComponentData
{
	public Entity prefab;
	public ParticleType type;
	public Vector3 velocity;
	public Vector3 size;
	public float life;
	public float lifeDuration;
	public Vector4 color;
	public bool stuck;
	public Matrix4x4 cachedMatrix;
}
