using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEditor;
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
	public float abilityCooldownTimer;
	public bool statusEffectActive;
	public float abilityDurationTimer;

	[Header("Spell Cost")]
	public int manaCost;

	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}

	public void Initilize()
	{
		name = abilityBaseRef.Name;
		abilityName = abilityBaseRef.Name;
		abilityDescription = abilityBaseRef.Description;
		abilitySprite = abilityBaseRef.abilitySprite;
		itemType = ItemType.isAbility;

		abilityCooldownTimer = 0;
	}

	private void Update()
	{
		AbilityCooldownTimer();
		AbilityDurationTimer();
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
	public void AbilityDurationTimer()
	{
		if (!statusEffectActive) return;

		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= abilityBaseRef.abilityDuration)
		{
			entityStatusEffectIsAppliedTo.UnApplyStatusEffect(this);
			abilityCooldownTimer = 0;
			Destroy(gameObject);
		}
	}

	public void PlayerUseAbility(EntityStats entityStats, PlayerController playerController)
	{
		if (isOnCooldown)
		{
			Debug.Log(abilityName + " on cooldown");
			return;
		}

		int totalManaCost = (int)(abilityBaseRef.manaCost * Utilities.GetStatModifier(entityStats.entityLevel, 0));
		if (entityStats.currentMana < totalManaCost)
		{
			Debug.Log("Not enough mana for " + abilityName);
			return;
		}

		GetAbilityType(entityStats, playerController);
		
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	public void EntityUseAbility(EntityStats entityStats)
	{
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	public void GetAbilityType(EntityStats entityStats, PlayerController playerController)
	{
		if (abilityBaseRef.canOnlyTargetSelf && abilityBaseRef.isHealthRestoration) //restore self
			RestoreHealth(entityStats);
		if (abilityBaseRef.canOnlyTargetSelf && abilityBaseRef.isManaRestoration)
			RestoreMana(entityStats);

		//add support for DOT status effects
		//add support for AOE spells and abilities
		//add support instantiate directional spells/skills

		//apply buffs to self
		if (abilityBaseRef.canOnlyTargetSelf && abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
			entityStats.ApplyStatusEffect(abilityBaseRef);

		//apply buffs to selected targets

		//apply buffs to friendlies in AOE

		//instantiate directional spells/skills
	}

	//health/mana restoration function
	public void RestoreHealth(EntityStats entityStats)
	{
		entityStats.OnHeal(abilityBaseRef.valuePercentage, true);
	}
	public void RestoreMana(EntityStats entityStats)
	{
		entityStats.IncreaseMana(abilityBaseRef.valuePercentage, true);
	}
}
