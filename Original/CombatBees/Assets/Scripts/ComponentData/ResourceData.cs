using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ResourceData : IComponentData {
	public float3 position;
	public bool stacked;
	public int stackIndex;
	public int gridX;
	public int gridY;
	public Entity holder;
	public float3 velocity;
	public bool dead;

}
