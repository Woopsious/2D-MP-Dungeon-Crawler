using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskWeaponAttack : BTNode
{
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;

	public TaskWeaponAttack(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (WeaponAttackOnCooldown(equipmentHandler.equippedWeapon)) return NodeState.RUNNING; //always needs to be running

		//Debug.LogError(stats.name + " attacking with weapon");
		AttackWithMainWeapon(equipmentHandler.equippedWeapon);

		//add weapon animation length here if needed, include a bool if animation should block movement
		behaviour.globalAttackTimer = 1f;
		return NodeState.SUCCESS;
	}

	public void AttackWithMainWeapon(Weapons weapon)
	{
		if (weapon == null || behaviour.playerTarget == null) return;

		if (weapon.weaponBaseRef.isRangedWeapon)
			weapon.RangedAttack(behaviour.playerTarget.transform.position, behaviour.projectilePrefab);
		else
			weapon.MeleeAttack(behaviour.playerTarget.transform.position);
	}
	public bool WeaponAttackOnCooldown(Weapons weapon)
	{
		if (behaviour.globalAttackTimer > 0 || !weapon.canAttackAgain)
			return true;
		else
			return false;
	}
}
