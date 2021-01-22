using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using System;

public class BeeTeam
{
	public static readonly int TEAM_A = 0;
	public static readonly int TEAM_B = 1;
}


public unsafe struct BeeData: IComponentData, IEquatable<BeeData> {
	public float3 velocity;
	public float3 smoothPosition;
	public float3 smoothDirection;
	public float size;

	public int teamNumber;
	public bool hasEnemy;
	public Entity enemyTarget;

	public Entity resourceTarget;
	public bool canPickupResource;

	public bool killed;
	public float deathTimer;
	public bool isAttacking;
	public bool isHoldingResource;

	public float flightJitter;
	public float rotationStiffness;
	public float damping;
	public float aggression;
	public float teamAttraction;
	public float teamRepulsion;
	public float chaseForce;
	public float attackDistance;
	public float attackForce;
	public float hitDistance;
	public float grabDistance;
	public float carryForce;

    public bool Equals(BeeData other)
    {
		return GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode()
    {
		int hash = 0;
		hash ^= teamNumber;
		hash ^= killed? 0 : 1;
		hash ^= hasEnemy? 0 : 1;
		hash ^= (int)(velocity.x);
		hash ^= (int)(velocity.y);
		hash ^= (int)(velocity.z);
		
		return hash;
    }
}
