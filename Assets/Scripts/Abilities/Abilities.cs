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
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
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

	public void Initilize()
	{
		name = abilityBaseRef.Name;
		abilityName = abilityBaseRef.Name;
		abilityDescription = abilityBaseRef.Description;
		abilitySprite = abilityBaseRef.abilitySprite;

		isEquippedAbility = false;
		isOnCooldown = false;
		abilityCooldownTimer = 0;
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
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(abilityBaseRef.damageValuePercentage)}% damage ";
		else if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isResistanceEffect)
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(abilityBaseRef.damageValuePercentage)}% damage res ";

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

		if (abilityBaseRef.isDOT)
			info += $"\nlasts for {abilityBaseRef.abilityDuration}s"; //optional
		return info;
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
