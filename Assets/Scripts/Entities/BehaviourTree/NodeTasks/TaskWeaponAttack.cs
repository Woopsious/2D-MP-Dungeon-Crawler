using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
		TryMainWeaponAttack();

		//add weapon animation length here if needed, include a bool if animation should block movement
		behaviour.globalAttackTimer = 1f;
		return NodeState.SUCCESS;
	}

	private void TryMainWeaponAttack()
	{
		if (equipmentHandler.equippedWeapon == null || behaviour.playerTarget == null) return;

		if (MultiplayerManager.IsMultiplayer())
			SyncMainWeaponAttackRpc(behaviour.playerTarget.transform.position);
		else
			MainWeaponAttack(behaviour.playerTarget.transform.position);
	}
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncMainWeaponAttackRpc(Vector3 attackPos)
	{
		MainWeaponAttack(attackPos);
	}
	private void MainWeaponAttack(Vector3 attackPos)
	{
		equipmentHandler.equippedWeapon.Attack(attackPos);
	}

	public bool WeaponAttackOnCooldown(Weapons weapon)
	{
		if (behaviour.globalAttackTimer > 0 || !weapon.canAttackAgain)
			return true;
		else
			return false;
	}
}
