using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
	public EntityStats entityStatusEffectIsAppliedTo;

	[Header("Ability Info")]
	public SOClassAbilities abilityBaseRef;

	public string abilityName;
	public string abilityDescription;
	public Image abilityImage;
	public Sprite abilitySprite;

	[Header("Ability Dynamic Info")]
	public bool isEquippedAbility;
	public bool isOnCooldown;
	public bool isAbilityQueuedUp;
	public float abilityCooldownTimer;

	[Header("Spell Cost")]
	public int manaCost;

	public static event Action OnAbilityQueueUp;

	public void Initilize()
	{
		name = abilityBaseRef.Name;
		abilityName = abilityBaseRef.Name;
		abilityDescription = abilityBaseRef.Description;
		abilitySprite = abilityBaseRef.abilitySprite;

		abilityCooldownTimer = 0;
	}

	private void Update()
	{
		AbilityCooldownTimer();
	}
	public void AbilityCooldownTimer()
	{
		if (!isOnCooldown) return;

		abilityCooldownTimer += Time.deltaTime;
		abilityImage.fillAmount = abilityCooldownTimer / abilityBaseRef.abilityCooldown;

		if (abilityCooldownTimer >= abilityBaseRef.abilityCooldown)
		{
			isOnCooldown = false;
			abilityImage.fillAmount = 1;
			abilityCooldownTimer = 0;
		}
	}
	public void AbilityAoePlacement()
	{
		if (!isAbilityQueuedUp) return;
	}

	public void PlayerUseAbility(EntityStats entityStats, PlayerController playerController)
	{
		if (!CanUseAbility(entityStats))
			return;

		if (!CanGetAbilityType(entityStats, playerController))
			return;
		
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	public void EntityUseAbility(EntityStats entityStats)
	{
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	//cooldown and mana cost check
	public bool CanUseAbility(EntityStats entityStats)
	{
		if (isOnCooldown)
		{
			Debug.Log(abilityName + " on cooldown");
			return false;
		}

		int totalManaCost = (int)(abilityBaseRef.manaCost * Utilities.GetLevelModifier(entityStats.entityLevel));
		if (entityStats.currentMana < totalManaCost)
		{
			Debug.Log("Not enough mana for " + abilityName);
			return false;
		}

		return true;
	}
	//target checking + getting correct ability type to instantiate
	public bool CanGetAbilityType(EntityStats entityStats, PlayerController playerController)
	{
		//apply status effects
		if (abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
		{
			//apply buffs to self
			if (abilityBaseRef.canOnlyTargetSelf)
			{
				entityStats.ApplyStatusEffect(abilityBaseRef);
				return true;
			}
			//apply buffs to friendly targets
			if (abilityBaseRef.requiresTarget & !abilityBaseRef.isOffensiveAbility)
			{
				//apply buff to self if not an offensive buff && has no friendly target selected
				//add check for friendly selected target when MP is added
				entityStats.ApplyStatusEffect(abilityBaseRef);
				return true;
			}
			//apply debuffs to selected enemy targets
			if (abilityBaseRef.requiresTarget && abilityBaseRef.isOffensiveAbility)
			{
				if (PlayerHotbarUi.Instance.selectedTarget == null)
				{
					Debug.Log("No Enemy Target selected");
					return false;
				}
				else
				{
					entityStats.GetComponent<PlayerController>().selectedTarget.ApplyStatusEffect(abilityBaseRef);
					return true;
				}
			}
		}

		//restoration type abilities
		if (abilityBaseRef.canOnlyTargetSelf && !abilityBaseRef.isOffensiveAbility)
		{
			if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isHealing)
			{
				RestoreHealth(entityStats);
				return true;
			}
			else if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isMana)
			{
				RestoreMana(entityStats);
				return true;
			}
		}

		//projectile abilities
		if (abilityBaseRef.isProjectile)
		{
			playerController.CastAbility(abilityBaseRef);
			return true;
		}

		//aoe abilities
		if (abilityBaseRef.isAOE)
		{
			isAbilityQueuedUp = true;
			return true;
		}

		else
		{
			Debug.LogError("No correct ability type");
			return false;
		}

			//add support for AOE spells and abilities
			//add support instantiate directional spells/skills
			//apply buffs to friendlies in AOE
			//instantiate directional spells/skills
	}

		//health/mana restoration function
	public void RestoreHealth(EntityStats entityStats)
	{
		entityStats.OnHeal(abilityBaseRef.damageValuePercentage, true);
	}
	public void RestoreMana(EntityStats entityStats)
	{
		entityStats.IncreaseMana(abilityBaseRef.damageValuePercentage, true);
	}
}
