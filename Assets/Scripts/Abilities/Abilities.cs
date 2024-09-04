using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
	private bool isStatusEffectTimerForUi;

	[Header("Ability Info")]
	private ToolTipUi toolTip;
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

	private void Update()
	{
		if (isStatusEffectTimerForUi)
			StatusEffectUiCooldownTimer();
		else
			AbilityCooldownTimer();
	}

	//set data types
	public void Initilize()
	{
		isStatusEffectTimerForUi = false;
		name = abilityBaseRef.Name;
		abilityName = abilityBaseRef.Name;
		abilityDescription = abilityBaseRef.Description;
		abilitySprite = abilityBaseRef.abilitySprite;

		isEquippedAbility = false;
		isOnCooldown = false;
		abilityCooldownTimer = 0;
	}
	public void InitilizeStatusEffectUiTimer(SOClassAbilities ability, float currentTimer)
	{
		abilityBaseRef = ability;
		isStatusEffectTimerForUi = true;
		name = abilityBaseRef.Name;
		abilityName = abilityBaseRef.Name;
		if (abilityBaseRef.statusEffectName != "")
			abilityDescription = abilityBaseRef.statusEffectName;
		else
			abilityDescription = abilityBaseRef.Name;

		abilitySprite = abilityBaseRef.abilitySprite;
		abilityCooldownTimer = currentTimer;

		GetComponent<ToolTipUi>().tipToShow = $"{abilityDescription}	";
	}

	//tool tip
	public virtual void SetToolTip(EntityStats playerStats)
	{
		toolTip = GetComponent<ToolTipUi>();
		string info = $"{abilityDescription}\n";

		if (abilityBaseRef.canOnlyTargetSelf)
			info += "\n Can only cast on self";
		else if (abilityBaseRef.requiresTarget && abilityBaseRef.isOffensiveAbility)
			info += "\nNeeds selected enemy target";
		else if (abilityBaseRef.requiresTarget && !abilityBaseRef.isOffensiveAbility)
			info += "\nNeeds selected friendly target";

		if (abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
			info = SetStatusEffectToolTip(info);
		else if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.noEffect)
			info = SetAbilityToolTip(info, playerStats);
		else
			Debug.LogError("Setting up ability tool tip failed");

		if (abilityBaseRef.isSpell) //optional
			info += $"\nCosts {(int)(abilityBaseRef.manaCost * Utilities.GetLevelModifier(playerStats.entityLevel))} mana";

		toolTip.tipToShow = $"{info}";
	}
	private string SetStatusEffectToolTip(string info)
	{
		if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isDamageEffect)
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(abilityBaseRef.statusEffectPercentageModifier)}% damage ";
		else if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isResistanceEffect)
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(abilityBaseRef.statusEffectPercentageModifier)}% damage res ";
		else if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isDamageRecievedEffect)
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(abilityBaseRef.statusEffectPercentageModifier)}" +
				$"% damage recieved modifier ";

		if (abilityBaseRef.canOnlyTargetSelf)
			info += "buff to yourself";
		else
		{
			if (abilityBaseRef.isOffensiveAbility && abilityBaseRef.isAOE)
				info += "debuff to enemies inside AoE";
			else if (!abilityBaseRef.isOffensiveAbility && abilityBaseRef.isAOE)
				info += "buff to friendlies/self inside AoE";

			if (abilityBaseRef.isOffensiveAbility && !abilityBaseRef.isAOE)
				info += "debuff to selected enemy";
			else if (!abilityBaseRef.isOffensiveAbility && !abilityBaseRef.isAOE)
				info += "buff to selected friendlies or self";
		}
		return info += $"\nEffect lasts for {abilityBaseRef.abilityDuration}s";
	}
	private string SetAbilityToolTip(string info, EntityStats playerStats)
	{
		if (abilityBaseRef.damageType != SOClassAbilities.DamageType.isHealing && abilityBaseRef.isOffensiveAbility)
		{
			int damage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(playerStats.entityLevel));

			if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isPhysicalDamageType)
				damage = (int)(damage * playerStats.physicalDamagePercentageModifier.finalPercentageValue);
			if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isPoisonDamageType)
				damage = (int)(damage * playerStats.poisonDamagePercentageModifier.finalPercentageValue);
			if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isFireDamageType)
				damage = (int)(damage * playerStats.fireDamagePercentageModifier.finalPercentageValue);
			if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isIceDamageType)
				damage = (int)(damage * playerStats.iceDamagePercentageModifier.finalPercentageValue);

			if (abilityBaseRef.isDOT)
				info += $"\nDeals {damage * abilityBaseRef.abilityDuration} damage to enemies over {abilityBaseRef.abilityDuration}s ";
			else
				info += $"\nDeals {damage} damage to enemies ";

			if (abilityBaseRef.isAOE) //optional
				info += "inside AoE";
		}
		else if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isHealing && !abilityBaseRef.isOffensiveAbility)
		{
			float healing = Utilities.ConvertFloatToUiPercentage(abilityBaseRef.damageValuePercentage);

			if (abilityBaseRef.isAOE)
				info += $"\nHeals for {healing}% of health for friendlies inside AoE";
			else
				info += $"\nHeals for {healing}% of health for selected friendlies or self ";
		}
		else
			Debug.LogError("Setting up non effect ability tool tip failed");

		if (abilityBaseRef.hasDuration)
			info += $"\nlasts for {abilityBaseRef.abilityDuration}s"; //optional
		return info;
	}

	//timer types
	private void AbilityCooldownTimer()
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
	private void StatusEffectUiCooldownTimer()
	{
		abilityCooldownTimer += Time.deltaTime;
		abilityImage.fillAmount = abilityCooldownTimer / abilityBaseRef.abilityDuration;

		if (abilityCooldownTimer >= abilityBaseRef.abilityDuration)
		{
			isOnCooldown = false;
			abilityImage.fillAmount = 1;
			abilityCooldownTimer = 0;
			gameObject.SetActive(false);
		}
	}

	//bool checks
	public bool CanUseAbility(EntityStats entityStats)
	{
		if (isOnCooldown)
			return false;

		if (!CanAffordManaCost(entityStats))
			return false;

		return true;
	}
	public bool CanAffordManaCost(EntityStats entityStats)
	{
		int totalManaCost = (int)(abilityBaseRef.manaCost * Utilities.GetLevelModifier(entityStats.entityLevel));
		if (entityStats.currentMana < totalManaCost)
			return false;
		else return true;
	}
	public bool CanInstantCastAbility(EntityStats selectedEnemy)
	{
		if (abilityBaseRef.canOnlyTargetSelf)
			return true;
		if (abilityBaseRef.isAOE)
			return false;
		if (abilityBaseRef.isProjectile)
			return true;

		if (abilityBaseRef.requiresTarget)
		{
			if (abilityBaseRef.damageType == SOClassAbilities.DamageType.isHealing) //add support for healing other players in MP
				return true;
			else if (!abilityBaseRef.isOffensiveAbility)
				return true;
			else if (abilityBaseRef.isOffensiveAbility && selectedEnemy != null)
				return true;
			else if (abilityBaseRef.isOffensiveAbility && selectedEnemy == null)
				return false;
			else
			{
				Debug.LogError("failed to figure out if ability can be insta casted while requiring a target");
				return false;
			}
		}
		else
		{
			Debug.LogError("failed to figure out if ability can be insta casted");
			return false;
		}
	}
}
