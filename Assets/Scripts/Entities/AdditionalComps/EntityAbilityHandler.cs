using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EntityAbilityHandler : NetworkBehaviour
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

	//SET ENTITY ABILITIES
	public void AssignEntityRandomAbilities()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		SOAbilities offensiveAbility = PickRandomAbilityFromClass(true);
		SOAbilities healingAility = PickRandomAbilityFromClass(false);

		this.offensiveAbility = offensiveAbility;
		this.healingAbility = healingAility;

		if (MultiplayerManager.IsMultiplayer())
			SyncEntityAbilitiesForClientsRPC(FindAbilityIndex(offensiveAbility), FindAbilityIndex(healingAility));
	}
	private SOAbilities PickRandomAbilityFromClass(bool offensiveAbility)
	{
		if (offensiveAbility == true)
		{
			List<SOAbilities> offensiveAbilities = new List<SOAbilities>();
			foreach (SOAbilities ability in AssetDatabase.Database.abilities)
			{
				if (ability.isOffensiveAbility && ability.damageType != IDamagable.DamageType.isHealing)
					offensiveAbilities.Add(ability);
			}

			if (offensiveAbilities.Count == 0)
				return null;
			else
				return offensiveAbilities[Utilities.GetRandomNumber(offensiveAbilities.Count - 1)];
		}
		else
		{
			List<SOAbilities> healingAbilities = new List<SOAbilities>();
			foreach (SOAbilities ability in AssetDatabase.Database.abilities)
			{
				if (ability.damageType == IDamagable.DamageType.isHealing)
					healingAbilities.Add(ability);
			}

			if (healingAbilities.Count == 0)
				return null;
			else
				return healingAbilities[Utilities.GetRandomNumber(healingAbilities.Count - 1)];
		}
	}

	//sync abilities for mp
	[Rpc(SendTo.Everyone)]
	private void SyncEntityAbilitiesForClientsRPC(int offensiiveAbilityIndex, int healingbilityIndex)
	{
		if (offensiiveAbilityIndex == -1)
			offensiveAbility = null;
		else
			offensiveAbility = AssetDatabase.Database.abilities[offensiiveAbilityIndex];

		if (healingbilityIndex == -1)
			healingAbility = null;
		else
			healingAbility = AssetDatabase.Database.abilities[healingbilityIndex];
	}
	private int FindAbilityIndex(SOAbilities abilityToMatch)
	{
		if (abilityToMatch == null) return -1; //no ability equipped

		for (int i = 0; i < AssetDatabase.Database.abilities.Count; i++)
		{
			if (AssetDatabase.Database.abilities[i] == abilityToMatch)
				return i;
		}

		Debug.LogError("no matching ability found for entity abilities");
		return -1; //no match
	}

	//duplicate ability check
	private bool IsAbilityAlreadyEquipped(SOAbilities abilityToCheck)
	{
		if (abilityToCheck == offensiveAbility) return true;
		if (abilityToCheck == healingAbility) return true;
		return false;
	}

	//reset/reroll abilities
	public void RerollEquippedAbilities()
	{
		entityStats.abilityHandler.offensiveAbility = null;
		entityStats.abilityHandler.healingAbility = null;
		AssignEntityRandomAbilities();
	}
	public void ResetEntityAbilities()
	{
		abilityBeingCasted = null;
		abilityCastingTimer = 0;

		offensiveAbilityTimer = 0;
		healingAbilityTimer = 0;

		abilityTimerOneCounter = 0;
		abilityTimerTwoCounter = 0;
		abilityTimerThreeCounter = 0;
	}

	//boss abilities
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
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncHideAoeIndicatorsRpc();
			else
				abilityIndicators.HideAoeIndicators();
		}
	}

	//ability cooldown timers (called in EntityBehaviour scripts)
	public void HealingAbilityCooldownTimer()
	{
		if (canCastHealingAbility) return;

		healingAbilityTimer -= Time.deltaTime;

		if (healingAbilityTimer <= 0)
			canCastHealingAbility = true;
	}
	public void OffensiveAbilityCooldownTimer()
	{
		if (canCastOffensiveAbility) return;

		offensiveAbilityTimer -= Time.deltaTime;

		if (offensiveAbilityTimer <= 0)
			canCastOffensiveAbility = true;
	}
	public void BossAbilityCooldownTimerOne()
	{
		if (canCastAbilityOne) return;

		abilityTimerOneCounter -= Time.deltaTime;

		if (abilityTimerOneCounter <= 0)
			canCastAbilityOne = true;
	}
	public void BossAbilityCooldownTimerTwo()
	{
		if (canCastAbilityTwo) return;

		abilityTimerTwoCounter -= Time.deltaTime;

		if (abilityTimerTwoCounter <= 0)
			canCastAbilityTwo = true;
	}
	public void BossAbilityCooldownTimerThree()
	{
		if (canCastAbilityThree) return;

		abilityTimerThreeCounter -= Time.deltaTime;

		if (abilityTimerThreeCounter <= 0)
			canCastAbilityThree = true;
	}

	//casting timer
	public void CastAbilityTimer()
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
		if (ability.isProjectile || ability.isAOE)
		{
			if (MultiplayerManager.IsMultiplayer())
				SyncSetUpAndCastAbilitiesRpc(entityStats.NetworkObjectId, GetAbilityIndex(ability), GetAbilityTargetPosition(ability));
			else
				SetUpAndCastAbilities(entityStats, ability, GetAbilityTargetPosition(ability));
		}
		else if (ability.requiresTarget)
		{
			if (behaviour.playerTarget == null && overriddenPlayerTarget == null)
				CancelAbility();
			else
				CastEffectAbilities(ability);
		}
		else
		{
			CancelAbility();
			Debug.LogError("failed to find ability type and cast, shouldnt happen");
			return;
		}
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
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncHideAoeIndicatorsRpc();
			else
				abilityIndicators.HideAoeIndicators();

			OnBossAbilityCast?.Invoke();
		}
	}

	//set up and cast projectile/aoe ability types
	[Rpc(SendTo.Server, RequireOwnership = false)]
	private void SyncSetUpAndCastAbilitiesRpc(ulong casterId, int abilityIndex, Vector2 attackPos)
	{
		EntityStats casterStats = NetworkManager.SpawnManager.SpawnedObjects[casterId].GetComponent<EntityStats>();
		SOAbilities ability = AssetDatabase.Database.abilities[abilityIndex];
		SetUpAndCastAbilities(casterStats, ability, attackPos);
	}
	private void SetUpAndCastAbilities(EntityStats casterStats, SOAbilities ability, Vector2 attackPos)
	{
		if (ability.isProjectile)
			SetUpAndCastProjectileAbility(casterStats, ability, attackPos);
		else if (ability.isAOE)
			SetUpAndCastAoeAbility(casterStats, ability, attackPos);
	}
	private void SetUpAndCastProjectileAbility(EntityStats casterStats, SOAbilities abilityRef, Vector2 attackPos)
	{
		Projectiles projectile = ObjectPoolingManager.GetInActiveProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(behaviour.projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
			ObjectPoolingManager.AddProjectileToObjectPooling(projectile);

			if (MultiplayerManager.IsMultiplayer())
				projectile.GetComponent<NetworkObject>().Spawn();
		}

		projectile.Initilize(casterStats, abilityRef, attackPos);
	}
	private void SetUpAndCastAoeAbility(EntityStats casterStats, SOAbilities abilityRef, Vector2 attackPos)
	{
		AbilityAOE abilityAOE = ObjectPoolingManager.GetInActiveAoeAbility();
		if (abilityAOE == null)
		{
			GameObject go = Instantiate(behaviour.AbilityAoePrefab, transform, true);
			abilityAOE = go.GetComponent<AbilityAOE>();
			ObjectPoolingManager.AddAoeAbilityToObjectPooling(abilityAOE);

			if (MultiplayerManager.IsMultiplayer())
				abilityAOE.GetComponent<NetworkObject>().Spawn();
		}

		//will need additional code here to handle supportive and offensive aoe abilities
		abilityAOE.Initilize(casterStats, abilityRef, attackPos);
	}

	//set up and cast effect types
	private void CastEffectAbilities(SOAbilities ability)
	{
		EntityStats target;

		if (ability.isOffensiveAbility)
		{
			if (overridePlayerTarget)
				target = overriddenPlayerTarget.playerStats;
			else
				target = behaviour.playerTarget.playerStats;
		}
		else
			target = entityStats;

		if (ability.damageType == IDamagable.DamageType.isHealing)
			CastHealingEffect(ability, target);
		else if (ability.damageValue != 0)
			CastDamageEffect(ability, target);

		if (ability.hasStatusEffects)    //apply effects if any
			target.ApplyNewStatusEffects(ability.statusEffects, entityStats);
	}
	private void CastHealingEffect(SOAbilities ability, EntityStats target)
	{
		target.RecieveHealing(ability.damageValuePercentage, true, target.healingPercentageModifier.finalPercentageValue);
	}
	private void CastDamageEffect(SOAbilities ability, EntityStats target)
	{
		DamageSourceInfo damageSourceInfo = new(entityStats, IDamagable.HitBye.entity,
			ability.damageValue * entityStats.levelModifier, ability.damageType, false);
		damageSourceInfo.SetDeathMessage(ability);

		target.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
	}

	//casting helper funcs
	private Vector2 GetAbilityTargetPosition(SOAbilities ability)
	{
		if (ability.isProjectile)
		{
			if (overridePlayerTarget)
			{
				if (overriddenPlayerTarget != null)
					return overriddenPlayerTarget.transform.position;
				else
					return overriddenTargetPosition;
			}
			else
				return behaviour.playerTarget.transform.position;
		}
		else if (ability.isAOE)
		{
			if (overridePlayerTarget)
			{
				if (overriddenPlayerTarget != null)
					return overriddenPlayerTarget.transform.position;
				else
					return overriddenTargetPosition;
			}
			else
				return behaviour.playerTarget.transform.position;
		}
		else return new Vector2(0, 0);
	}
	private int GetAbilityIndex(SOAbilities ability)
	{
		for (int i = 0; i < AssetDatabase.Database.abilities.Count; i++)
		{
			if (ability == AssetDatabase.Database.abilities[i])
				return i;
		}

		Debug.LogError("failed to get ability index, ENSURE ABILITY IS ADDED TO DATABASE");
		return 0;
	}

	//QUEUE UP CASTING OF ABILTIES (called in Task scripts)
	//queue entity abilities
	public void CastHealingAbility()
	{
		canCastHealingAbility = false;
		healingAbilityTimer = healingAbility.abilityCooldown + healingAbility.abilityCastingTimer;
		abilityCastingTimer = healingAbility.abilityCastingTimer;
		abilityBeingCasted =healingAbility;

		//add aoe indicator code here if i decide to add indicators for basic entities
	}
	public void CastOffensiveAbility()
	{
		canCastOffensiveAbility = false;
	    offensiveAbilityTimer = offensiveAbility.abilityCooldown + offensiveAbility.abilityCastingTimer;
		abilityCastingTimer = offensiveAbility.abilityCastingTimer;
		abilityBeingCasted = offensiveAbility;

		//add aoe indicator code here if i decide to add indicators for basic entities
	}

	//queue boss entity abilities
	public void CastBossAbilityOne()
	{
		TryMarkPlayer(abilityOne);

		canCastAbilityOne = false;
		abilityTimerOneCounter = abilityOne.abilityCooldown + abilityOne.abilityCastingTimer;
		abilityCastingTimer = abilityOne.abilityCastingTimer;
		abilityBeingCasted = abilityOne;

		if (overridePlayerTarget && overriddenPlayerTarget != null)
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityOne), overriddenPlayerTarget.NetworkObjectId);
			else
				abilityIndicators.ShowAoeIndicators(abilityOne, overriddenPlayerTarget.playerStats);
		}
		else
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityOne), behaviour.playerTarget.NetworkObjectId);
			else
				abilityIndicators.ShowAoeIndicators(abilityOne, behaviour.playerTarget.playerStats);
		}

		EventBossAbilityBeginCasting(abilityOne);
	}
	public void CastBossAbilityTwo()
	{
		TryMarkPlayer(abilityTwo);

		canCastAbilityTwo = false;
		abilityTimerTwoCounter = abilityTwo.abilityCooldown + abilityTwo.abilityCastingTimer;
		abilityCastingTimer = abilityTwo.abilityCastingTimer;
		abilityBeingCasted = abilityTwo;

		if (overridePlayerTarget && overriddenPlayerTarget != null)
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityTwo), overriddenPlayerTarget.NetworkObjectId);
			else
				abilityIndicators.ShowAoeIndicators(abilityTwo, overriddenPlayerTarget.playerStats);
		}
		else
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityTwo), behaviour.playerTarget.NetworkObjectId);
			else
				abilityIndicators.ShowAoeIndicators(abilityTwo, behaviour.playerTarget.playerStats);
		}

		EventBossAbilityBeginCasting(abilityTwo);
	}
	public void CastBossAbilityThree()
	{
		TryMarkPlayer(abilityThree);

		canCastAbilityThree = false;
		abilityTimerThreeCounter = abilityThree.abilityCooldown + abilityThree.abilityCastingTimer;
		abilityCastingTimer = abilityThree.abilityCastingTimer;
		abilityBeingCasted = abilityThree;

		if (overridePlayerTarget && overriddenPlayerTarget != null)
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityThree), overriddenPlayerTarget.NetworkObjectId);
			else
				abilityIndicators.ShowAoeIndicators(abilityThree, overriddenPlayerTarget.playerStats);
		}
		else
		{
			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityThree), behaviour.playerTarget.NetworkObjectId);
			else
				abilityIndicators.ShowAoeIndicators(abilityThree, behaviour.playerTarget.playerStats);
		}

		EventBossAbilityBeginCasting(abilityThree);
	}

	//queue boss transition abilities
	public void CastTransitionAbility(SOBossAbilities abilityToCast, Vector3 position, bool isDirection)
	{
		ForbidCastingOfTransitionAbility(); //stop transition ability spam

		TryMarkPlayer(abilityToCast);

		abilityCastingTimer = abilityToCast.abilityCastingTimer;
		abilityBeingCasted = abilityToCast;

		if (position == Vector3.zero)
		{
			if (overridePlayerTarget && overriddenPlayerTarget != null)
			{
				if (MultiplayerManager.IsMultiplayer())
					abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityToCast), overriddenPlayerTarget.NetworkObjectId);
				else
					abilityIndicators.ShowAoeIndicators(abilityToCast, overriddenPlayerTarget.playerStats);
			}
			else
			{
				if (MultiplayerManager.IsMultiplayer())
					abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityToCast), behaviour.playerTarget.NetworkObjectId);
				else
					abilityIndicators.ShowAoeIndicators(abilityToCast, behaviour.playerTarget.playerStats);
			}
		}
		else //ability target isnt player but a position/direction
		{
			Vector3 adjustedPosition = position;
			if (isDirection)
				adjustedPosition += behaviour.transform.position;

			if (MultiplayerManager.IsMultiplayer())
				abilityIndicators.SyncShowAoeIndicatorsRpc(GetAbilityIndex(abilityToCast), adjustedPosition);
			else
				abilityIndicators.ShowAoeIndicators(abilityToCast, adjustedPosition);

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

	//PLAYER TARGET OVERRIDING
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
