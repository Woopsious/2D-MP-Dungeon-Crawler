using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossEntityBehaviour : EntityBehaviour
{
	[Header("Boss Abilities")]
	[Header("Ability One")]
	public SOClassAbilities abilityOne;
	public bool canCastAbilityOne;
	public float abilityTimerOneCounter;

	[Header("Ability Two")]
	public SOClassAbilities abilityTwo;
	public bool canCastAbilityTwo;
	public float abilityTimerTwoCounter;

	[Header("Ability Three")]
	public SOClassAbilities abilityThree;
	public bool canCastAbilityThree;
	public float abilityTimerThreeCounter;

	public static event Action<int> OnSpawnBossAdds;

	public static event Action<SOClassAbilities, List<Vector2>, Vector2, float> OnBossAbilityBeginCasting;
	public static event Action OnBossAbilityCast;

	protected override void Update()
	{
		base.Update();

		AbilityTimerOne();
		AbilityTimerTwo();
		AbilityTimerThree();
	}

	protected override void Initilize()
	{
		base.Initilize();

		SOBossEntityBehaviour behaviour = (SOBossEntityBehaviour)behaviourRef;
		abilityOne = behaviour.abilityOne;
		abilityTwo = behaviour.abilityTwo;
		abilityThree = behaviour.abilityThree;
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

	//unique boss events
	public void EventSpawnBossAdds(int numToSpawn)
	{
		OnSpawnBossAdds?.Invoke(numToSpawn);
	}
	public void EventBossAbilityBeginCasting(SOClassAbilities ability, List<Vector2> positionList, Vector2 adjustPosition, float radius)
	{
		OnBossAbilityBeginCasting?.Invoke(ability, positionList, adjustPosition, radius);
	}

	protected override void CastAbility(SOClassAbilities ability)
	{
		base.CastAbility(ability);
		OnBossAbilityCast?.Invoke();
	}

	public void ForceCastSpecialBossAbilities(SOClassAbilities ability)
	{
		CastAbility(ability);
	}

	//timers
	private void AbilityTimerOne()
	{
		if (canCastAbilityOne) return;

		abilityTimerOneCounter -= Time.deltaTime;

		if (abilityTimerOneCounter <= 0)
			canCastAbilityOne = true;
	}
	private void AbilityTimerTwo()
	{
		if (canCastAbilityTwo) return;

		abilityTimerTwoCounter -= Time.deltaTime;

		if (abilityTimerTwoCounter <= 0)
			canCastAbilityTwo = true;
	}
	private void AbilityTimerThree()
	{
		if (canCastAbilityThree) return;

		abilityTimerThreeCounter -= Time.deltaTime;

		if (abilityTimerThreeCounter <= 0)
			canCastAbilityThree = true;
	}
}
