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

	public void PlayerUseAbility(EntityStats entityStats)
	{
		PlayerController playerController = entityStats.GetComponent<PlayerController>();
		if (!CanUseAbility(entityStats))
			return;

		if (abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect || abilityBaseRef.damageType == 
			SOClassAbilities.DamageType.isHealing || abilityBaseRef.damageType == SOClassAbilities.DamageType.isMana)
		{
			if (CanInstantCastEffect())
				PlayerHotbarUi.Instance.AddNewQueuedAbility(this, playerController, true);
			else
				PlayerHotbarUi.Instance.AddNewQueuedAbility(this, playerController, false);
		}
		else
		{
			if (CanInstantCastAbility())
				PlayerHotbarUi.Instance.AddNewQueuedAbility(this, playerController, true);
			else
				PlayerHotbarUi.Instance.AddNewQueuedAbility(this, playerController, false);
		}
	}
	public void EntityUseAbility(EntityStats entityStats)
	{
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	public void CastAbility(EntityStats casterStats)
	{
		if (abilityBaseRef.isSpell)
		{
			int totalManaCost = (int)(abilityBaseRef.manaCost * Utilities.GetLevelModifier(casterStats.entityLevel));
			casterStats.DecreaseMana(totalManaCost, false);
		}
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
	//cast now
	public bool CanInstantCastEffect()
	{
		if (abilityBaseRef.canOnlyTargetSelf) //for effects that can only be added to self
			return true;
		if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isHealing||
			abilityBaseRef.damageType == SOClassAbilities.DamageType.isMana) //restoration effects
		{
			//add additional checks for friendlies when mp is added
			return true;
		}
		if (abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect) //status effects
		{
			if (!abilityBaseRef.isOffensiveAbility)
				return true;
			if (abilityBaseRef.isOffensiveAbility && PlayerHotbarUi.Instance.selectedTarget != null)
				return true;
			else
				return false;
		}
		else
		{
			Debug.LogError("failed to handle ability effect, this shouldnt happen");
			return false;
		}
	}
	public bool CanInstantCastAbility()
	{
		if (abilityBaseRef.isAOE)
			return false;
		if (abilityBaseRef.isProjectile)
			return true;
		else
		{
			Debug.LogError("failed to handle ability, this shouldnt happen");
			return false;
		}
	}
}
