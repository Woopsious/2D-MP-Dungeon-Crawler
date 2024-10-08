using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskWeaponAttack : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;
	NavMeshAgent navMesh;

	public TaskWeaponAttack(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(stats.name + " weapon attack task");

		if (behaviour.globalAttackTimer > 0) return NodeState.RUNNING; //always needs to be running

		Debug.LogError(stats.name + " attacking with weapon");
		TryAttackWithMainWeapon(equipmentHandler.equippedWeapon);

		//add weapon animation length here if needed, include a bool if animation should block movement
		behaviour.globalAttackTimer = 1f;
		return NodeState.SUCCESS;
	}

	public void TryAttackWithMainWeapon(Weapons weapon)
	{
		if (weapon == null) return;

		if (weapon.weaponBaseRef.isRangedWeapon)
			weapon.RangedAttack(behaviour.playerTarget.transform.position, behaviour.projectilePrefab);
		else
			weapon.MeleeAttack(behaviour.playerTarget.transform.position);
	}
}
