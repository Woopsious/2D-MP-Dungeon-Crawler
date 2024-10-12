using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskBossUseAbilities : BTNode
{
	protected BossEntityBehaviour behaviour;
	protected BossEntityStats stats;
	protected EntityEquipmentHandler equipmentHandler;

	protected virtual bool CanUseBossAbilityOne()
	{
		return false;
	}
	protected virtual bool CanUseBossAbilityTwo()
	{
		return false;
	}
	protected virtual bool CanUseBossAbilityThree()
	{
		return false;
	}
}
