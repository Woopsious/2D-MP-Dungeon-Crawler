using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EntityAbilityHandler : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;
	[HideInInspector] public EntityBehaviour behaviour;
	[HideInInspector] public AbilityIndicators abilityIndicators;

	[Header("Override Player Target")]
	private bool overridePlayerTarget;
	public PlayerController overriddenPlayerTarget;
	private Vector3 overriddenTargetPosition;

	[Header("Abilities")]
	public SOAbilities abilityBeingCasted;
	public float abilityCastingTimer;

	[Header("Healing Ability Cooldown")]
	public SOAbilities healingAbility;
	public bool canCastHealingAbility;
	public float healingAbilityTimer;

	[Header("Offensive Ability Cooldown")]
	public SOAbilities offensiveAbility;
	public bool canCastOffensiveAbility;
	public float offensiveAbilityTimer;

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

	public static event Action<SOBossAbilities, Vector2> OnBossAbilityBeginCasting;
	public static event Action OnBossAbilityCast;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		behaviour = GetComponent<EntityBehaviour>();
		abilityIndicators = GetComponentInChildren<AbilityIndicators>();
	}
	private void Start()
	{
		SetBossAbilities();
	}

	private void SetBossAbilities()
	{
		if (behaviour.behaviourRef is SOBossEntityBehaviour bossBehaviour)
		{
			abilityOne = bossBehaviour.abilityOne;
			abilityTwo = bossBehaviour.abilityTwo;
			abilityThree = bossBehaviour.abilityThree;
			markPlayerAbility = bossBehaviour.markPlayerAbility;
		}
	}

	//CASTING ABILITIES
	//incase of errors.
	protected void CancelAbility()
	{
		abilityBeingCasted = null;
		abilityCastingTimer = 0;
		if (entityStats.statsRef.isBossVersion)
			abilityIndicators.HideAoeIndicators();
	}

	//ability cooldown timers (called in EntityBehaviour scripts)
	public void HealingAbilityTimer()
	{
		if (canCastHealingAbility) return;

		healingAbilityTimer -= Time.deltaTime;

		if (healingAbilityTimer <= 0)
			canCastHealingAbility = true;
	}
	public void OffensiveAbilityTimer()
	{
		if (canCastOffensiveAbility) return;

		offensiveAbilityTimer -= Time.deltaTime;

		if (offensiveAbilityTimer <= 0)
			canCastOffensiveAbility = true;
	}
	public void BossAbilityTimerOne()
	{
		if (canCastAbilityOne) return;

		abilityTimerOneCounter -= Time.deltaTime;

		if (abilityTimerOneCounter <= 0)
			canCastAbilityOne = true;
	}
	public void BossAbilityTimerTwo()
	{
		if (canCastAbilityTwo) return;

		abilityTimerTwoCounter -= Time.deltaTime;

		if (abilityTimerTwoCounter <= 0)
			canCastAbilityTwo = true;
	}
	public void BossAbilityTimerThree()
	{
		if (canCastAbilityThree) return;

		abilityTimerThreeCounter -= Time.deltaTime;

		if (abilityTimerThreeCounter <= 0)
			canCastAbilityThree = true;
	}

	//casting timers (called in EntityBehaviour scripts)
	public void AbilityCastingTimer()
	{
		if (abilityBeingCasted != null)
		{
			abilityCastingTimer -= Time.deltaTime;

			if (abilityCastingTimer <= 0)
				CastAbility(abilityBeingCasted);
		}
	}

	//cast ability
	private void CastAbility(SOAbilities ability)
	{
		if (ability.isAOE)
			CastAoeAbility(ability);
		else if (ability.isProjectile)
			CastDirectionalAbility(ability);
		else if (ability.requiresTarget && ability.isOffensiveAbility)
		{
			if (behaviour.playerTarget == null && overriddenPlayerTarget == null)
				CancelAbility();
			else
				CastEffect(ability);
		}
		else if (ability.requiresTarget && !ability.isOffensiveAbility)   //for MP add support for friendlies
			CastEffect(ability);
		else
		{
			CancelAbility();
			Debug.LogError("failed to find ability type and cast, shouldnt happen");
			return;
		}

		if (entityStats.statsRef.isBossVersion)
			OnBossAbilityCast?.Invoke();
	}

	//types of casting
	private void CastEffect(SOAbilities ability)
	{
		if (ability.damageType == SOAbilities.DamageType.isHealing)
		{
			//eventually add support to heal friendlies
			entityStats.OnHeal(ability.damageValuePercentage, true, entityStats.healingPercentageModifier.finalPercentageValue);
		}
		if (ability.damageValue != 0)    //apply damage for insta damage abilities
		{
			DamageSourceInfo damageSourceInfo = new(null, GetComponent<Collider2D>(), ability.damageValue * entityStats.levelModifier, 
				(IDamagable.DamageType)ability.damageType, 0, false, true, false);

			if (overridePlayerTarget)
				overriddenPlayerTarget.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
			else
				behaviour.playerTarget.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
		}

		if (ability.hasStatusEffects)    //apply effects (if has any) based on what type it is.
		{
			//apply effects based on what type it is.
			if (ability.canOnlyTargetSelf)
				entityStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
			else if (ability.isOffensiveAbility && behaviour.playerTarget != null)
			{
				if (overridePlayerTarget)
					overriddenPlayerTarget.playerStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
				else
					behaviour.playerTarget.playerStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
			}
			else if (!ability.isOffensiveAbility)         //add support/option to buff other friendlies
				entityStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
		}

		OnSuccessfulCast(ability);
	}
	private void CastDirectionalAbility(SOAbilities ability)
	{
		Projectiles projectile = DungeonHandler.GetProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(behaviour.projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
			projectile.transform.SetParent(null);
		}

		if (overridePlayerTarget)
		{
			if (overriddenPlayerTarget != null)
				projectile.SetPositionAndAttackDirection(transform.position, overriddenPlayerTarget.transform.position);
			else
				projectile.SetPositionAndAttackDirection(transform.position, overriddenTargetPosition);
		}
		else
			projectile.SetPositionAndAttackDirection(transform.position, behaviour.playerTarget.transform.position);

		projectile.Initilize(null, ability, entityStats);
		OnSuccessfulCast(ability);
	}
	private void CastAoeAbility(SOAbilities ability)
	{
		AbilityAOE abilityAOE = DungeonHandler.GetAoeAbility();
		if (abilityAOE == null)
		{
			GameObject go = Instantiate(behaviour.AbilityAoePrefab, transform, true);
			abilityAOE = go.GetComponent<AbilityAOE>();
			abilityAOE.transform.SetParent(null);
		}

		if (overridePlayerTarget)
		{
			if (overriddenPlayerTarget != null)
				abilityAOE.Initilize(ability, entityStats, overriddenPlayerTarget.transform.position);
			else
				abilityAOE.Initilize(ability, entityStats, overriddenTargetPosition);
		}
		else
			abilityAOE.Initilize(ability, entityStats, behaviour.playerTarget.transform.position);

		abilityAOE.AddPlayerRef(null);
		OnSuccessfulCast(ability);
	}
	private void OnSuccessfulCast(SOAbilities ability)
	{
		if (ability.isSpell)
		{
			int totalManaCost = (int)(ability.manaCost * entityStats.levelModifier);
			entityStats.DecreaseMana(totalManaCost, false);
		}

		ResetOverridenPlayerTarget();
		abilityBeingCasted = null;

		if (entityStats.statsRef.isBossVersion)
			abilityIndicators.HideAoeIndicators();
	}

	//override current PlayerTarget, making abilities target another player or position
	private void OverrideCurrentPlayerTarget(PlayerController player)
	{
		overriddenPlayerTarget = player;
		overridePlayerTarget = true;
	}
	private void OverrideCurrentPlayerTarget(Vector3 targetPosition)
	{
		overriddenTargetPosition = targetPosition;
		overridePlayerTarget = true;
	}
	private void ResetOverridenPlayerTarget()
	{
		overridePlayerTarget = false;
		overriddenPlayerTarget = null;
		overriddenTargetPosition = Vector3.zero;
	}

	//QUEUE UP CASTING OF ABILTIES (called in Task scripts)
	//queue entity abilities
	public void CastHealingAbility()
	{
		canCastHealingAbility = false;
		healingAbilityTimer = healingAbility.abilityCooldown + healingAbility.abilityCastingTimer;
		abilityCastingTimer = healingAbility.abilityCastingTimer;
		abilityBeingCasted =healingAbility;
	}
	public void CastOffensiveAbility()
	{
		canCastOffensiveAbility = false;
	    offensiveAbilityTimer = offensiveAbility.abilityCooldown + offensiveAbility.abilityCastingTimer;
		abilityCastingTimer = offensiveAbility.abilityCastingTimer;
		abilityBeingCasted = offensiveAbility;
	}

	//queue boss entity abilities
	public void CastBossAbilityOne()
	{
		TryMarkPlayer(abilityOne);

		canCastAbilityOne = false;
		abilityTimerOneCounter = abilityOne.abilityCooldown + abilityOne.abilityCastingTimer;
		abilityCastingTimer = abilityOne.abilityCastingTimer;
		abilityBeingCasted = abilityOne;

		abilityIndicators.ShowAoeIndicators(abilityOne, behaviour);
		EventBossAbilityBeginCasting(abilityOne);
	}
	public void CastBossAbilityTwo()
	{
		TryMarkPlayer(abilityTwo);

		canCastAbilityTwo = false;
		abilityTimerTwoCounter = abilityTwo.abilityCooldown + abilityTwo.abilityCastingTimer;
		abilityCastingTimer = abilityTwo.abilityCastingTimer;
		abilityBeingCasted = abilityTwo;

		abilityIndicators.ShowAoeIndicators(abilityTwo, behaviour);
		EventBossAbilityBeginCasting(abilityTwo);
	}
	public void CastBossAbilityThree()
	{
		TryMarkPlayer(abilityThree);

		canCastAbilityThree = false;
		abilityTimerThreeCounter = abilityThree.abilityCooldown + abilityThree.abilityCastingTimer;
		abilityCastingTimer = abilityThree.abilityCastingTimer;
		abilityBeingCasted = abilityThree;

		abilityIndicators.ShowAoeIndicators(abilityThree, behaviour);
		EventBossAbilityBeginCasting(abilityThree);
	}

	//queue boss transition abilities
	public void CastTransitionAbility(SOBossAbilities abilityToCast, Vector3 position, bool isDirection)
	{
		ForbidCastingOfTransitionAbility(); //stop transition ability spam

		TryMarkPlayer(abilityToCast);

		abilityCastingTimer = abilityToCast.abilityCastingTimer;
		abilityBeingCasted = abilityToCast;

		abilityIndicators.ShowAoeIndicators(abilityToCast, behaviour);

		if (position != Vector3.zero) //ability target isnt player but a position/direction
		{
			Vector3 adjustedPosition = position;
			if (isDirection)
				adjustedPosition += behaviour.transform.position;

			abilityIndicators.ShowAoeIndicators(abilityToCast, behaviour, adjustedPosition);
			OverrideCurrentPlayerTarget(adjustedPosition);
		}

		EventBossAbilityBeginCasting(abilityToCast);
	}

	//player marking
	private void TryMarkPlayer(SOBossAbilities abilityToCast)
	{
		if (!abilityToCast.marksPlayer) return;

		PlayerController newPlayerTarget = behaviour.playerTarget;
		int chance = (int)(abilityToCast.chanceMarkedPlayerIsAggroTarget * 100);

		//mark random player that isnt current aggro player as long as aggro list count bigger then 1
		if (behaviour.playerAggroList.Count > 1 && Utilities.GetRandomNumber(100) >= chance)
			newPlayerTarget = behaviour.playerAggroList[Utilities.GetRandomNumberBetween(1, behaviour.playerAggroList.Count)].player;

		OverrideCurrentPlayerTarget(newPlayerTarget); //override for marked by boss effect
		CastAbility(markPlayerAbility);
		OverrideCurrentPlayerTarget(newPlayerTarget); //override for ability
	}

	//unique boss events
	private void EventBossAbilityBeginCasting(SOBossAbilities ability)
	{
		BossEntityStats bossStats = (BossEntityStats)entityStats;
		OnBossAbilityBeginCasting?.Invoke(ability, bossStats.roomCenterPiece.transform.position);
	}

	//transition abilities toggle (called in Task scripts)
	public void AllowCastingOfTransitionAbility()
	{
		canCastTransitionAbility = true;
	}
	public void ForbidCastingOfTransitionAbility()
	{
		canCastTransitionAbility = false;
	}

	//mana check
	public bool HasEnoughManaToCast(SOAbilities ability)
	{
		if (ability.isSpell)
		{
			int totalManaCost = (int)(ability.manaCost * entityStats.levelModifier);
			if (entityStats.currentMana <= totalManaCost)
				return false;
			else return true;
		}
		else
			return true;
	}
}
