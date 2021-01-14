using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BeeAuthoring : IComponentData
{
    public Entity Prefab;
	public float4[] teamColors;
	public float minBeeSize;
	public float maxBeeSize;
	public float speedStretch;
	public float rotationStiffness;
	public float aggression;
	public float flightJitter;
	public float teamAttraction;
	public float teamRepulsion;
	public float damping;
	public float chaseForce;
	public float carryForce;
	public float grabDistance;
	public float attackDistance;
	public float attackForce;
	public float hitDistance;
	public float maxSpawnSpeed;
	public int amount;
	public float deathTimer;
}
