using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class BossEntityBehaviour : EntityBehaviour
{
	[Header("Bosses Step In Phase")]
	[HideInInspector] public int stepInPhaseTransition;

	public static event Action<int> OnSpawnBossAdds;

	protected override void Update()
	{
		base.Update();

		abilityHandler.BossAbilityTimerOne();
		abilityHandler.BossAbilityTimerTwo();
		abilityHandler.BossAbilityTimerThree();
	}

	//build Boss Behaviour Tree
	protected override BTNode SetupTree()
	{
		BTNode root;
		if (entityStats.statsRef.humanoidType == SOEntityStats.HumanoidTypes.isGoblinBoss)
			root = SetUpGoblinBossBehaviourTree();
		else if (entityStats.statsRef.humanoidType == SOEntityStats.HumanoidTypes.isEyeBoss)
			root = SetUpEyeBossBehaviourTree();
		else
		{
			Debug.LogError("failed to set up behaviour tree for boss enemy");
			return null;
		}
		return root;
	}

	private BTNode SetUpGoblinBossBehaviourTree()
	{
		BTNode root = new Selector(new List<BTNode> //entity Behaviour Tree
		{
			new Sequence(new List<BTNode> //attack behaviour
			{
				new Selector(new List<BTNode> //check if in phase transition
				{
					new TaskGoblinBossTransitions(this),
					new TaskGoblinBossPhases(this),
				}),

				new Sequence(new List<BTNode> //attack actions
				{
					new CheckGlobalAttackCooldown(this),
					new Selector(new List<BTNode> //attack actions (CHANGE TO SELECTOR NODE WHEN BOSS ABILITIES DONE)
					{
						new Sequence(new List<BTNode> //use ability
						{
							new TaskGoblinBossAbilities(this),
						}),
						new Sequence(new List<BTNode> //weapon attack
						{
							new CheckPlayerInAttackRange(this),
							new TaskWeaponAttack(this),
						}),
					}),
				}),
			}),

			new Sequence(new List<BTNode> //investigate behaviour
			{
				new CheckPlayersLastKnownPos(this),
				new TaskInvestigate(this),
			}),

			new Selector(new List<BTNode> //wander behaviour
			{
				new TaskIdle(this),
				new TaskWander(this),
			}),
		});

		return root;
	}
	private BTNode SetUpEyeBossBehaviourTree()
	{
		BTNode root = new Selector(new List<BTNode> //entity Behaviour Tree
		{
			new Sequence(new List<BTNode> //attack behaviour
			{
				new Selector(new List<BTNode> //check if in phase transition
				{
					new TaskEyeBossTransitions(this),
					new TaskEyeBossPhases(this),
				}),

				new Sequence(new List<BTNode> //attack actions
				{
					new CheckGlobalAttackCooldown(this),
					new Selector(new List<BTNode> //attack actions (CHANGE TO SELECTOR NODE WHEN BOSS ABILITIES DONE)
					{
						new Sequence(new List<BTNode> //use ability
						{
							new TaskEyeBossAbilities(this),
						}),
						new Sequence(new List<BTNode> //weapon attack
						{
							new CheckPlayerInAttackRange(this),
							new TaskWeaponAttack(this),
						}),
					}),
				}),
			}),

			new Sequence(new List<BTNode> //investigate behaviour
			{
				new CheckPlayersLastKnownPos(this),
				new TaskInvestigate(this),
			}),

			new Selector(new List<BTNode> //wander behaviour
			{
				new TaskIdle(this),
				new TaskWander(this),
			}),
		});

		return root;
	}

	//phase
	public void NextStepInPhase()
	{
		stepInPhaseTransition++;
	}

	//unique boss event
	public void EventSpawnBossAdds(int numToSpawn)
	{
		OnSpawnBossAdds?.Invoke(numToSpawn);
	}
}
