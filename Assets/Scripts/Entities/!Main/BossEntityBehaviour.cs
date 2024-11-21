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

	[Header("Boss Abilities")]
	[Header("Ability One")]
	public SOBossAbilities abilityOne;
	public bool canCastAbilityOne;
	public float abilityTimerOneCounter;

	[Header("Ability Two")]
	public SOBossAbilities abilityTwo;
	public bool canCastAbilityTwo;
	public float abilityTimerTwoCounter;

	[Header("Ability Three")]
	public SOBossAbilities abilityThree;
	public bool canCastAbilityThree;
	public float abilityTimerThreeCounter;

	[Header("Transition Abilities")]
	public bool canCastTransitionAbility;

	[Header("Mark Player Ability")]
	private SOAbilities markPlayerAbility;

	public static event Action<int> OnSpawnBossAdds;

	public static event Action<SOBossAbilities, Vector2> OnBossAbilityBeginCasting;
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
		markPlayerAbility = behaviour.markPlayerAbility;
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

	//unique boss events
	public void EventSpawnBossAdds(int numToSpawn)
	{
		OnSpawnBossAdds?.Invoke(numToSpawn);
	}
	public void EventBossAbilityBeginCasting(SOBossAbilities ability)
	{
		BossEntityStats bossStats = (BossEntityStats)entityStats;
		OnBossAbilityBeginCasting?.Invoke(ability, bossStats.roomCenterPiece.transform.position);
	}

	//override target for abilities
	public void OverrideTargetForAbilities(PlayerController player, Vector3 position, bool isDirection)
	{
		Vector3 adjustedPosition = position;
		if (isDirection)
			adjustedPosition += transform.position;

		OverrideCurrentPlayerTarget(player);
		OverrideCurrentPlayerTarget(adjustedPosition);
	}

	//boos ability casting
	public void ForceCastMarkPlayerBossAbility()
	{
		CastAbility(markPlayerAbility);
	}
	protected override void CastAbility(SOAbilities ability)
	{
		base.CastAbility(ability);
		OnBossAbilityCast?.Invoke();
	}

	//transition abilities
	public void AllowCastingOfTransitionAbility()
	{
		canCastTransitionAbility = true;
	}
	public void ForbidCastingOfTransitionAbility()
	{
		canCastTransitionAbility = false;
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
