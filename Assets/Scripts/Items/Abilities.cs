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
	[Header("Ability Info")]
	public SOClassAbilities abilityBaseRef;

	public string abilityName;
	public string abilityDescription;
	public Sprite abilitySprite;

	[Header("Ability Dynamic Info")]
	public bool isEquippedAbility;
	public bool isOnCooldown;
	public float abilityCooldownTimer;
	public float abilityDuration;

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

		abilityCooldownTimer = abilityBaseRef.abilityCooldown;
	}

	public void PlayerUseAbility(EntityStats entityStats, PlayerController playerController)
	{
		GetAbilityType(entityStats, playerController);

		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		StartCoroutine(AbilityCooldown());
	}

	public void EntityUseAbility(EntityStats entityStats)
	{
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		StartCoroutine(AbilityCooldown());
	}

	private IEnumerator AbilityCooldown()
	{
		isOnCooldown = true;
		abilityCooldownTimer -= Time.deltaTime;

		yield return new WaitForSeconds(abilityBaseRef.abilityCooldown);

		isOnCooldown = false;
		abilityCooldownTimer = abilityBaseRef.abilityCooldown;
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
