using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
	public float abilityCooldownTimer;

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

		if (!TryGetAbilityType(entityStats, playerController))
			return;
		
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	public void EntityUseAbility(EntityStats entityStats)
	{
		entityStats.DecreaseMana(abilityBaseRef.manaCost, false);
		isOnCooldown = true;
	}

	public bool TryGetAbilityType(EntityStats entityStats, PlayerController playerController)
	{
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

		//apply buffs to self
		if (abilityBaseRef.canOnlyTargetSelf && abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
		{
			entityStats.ApplyStatusEffect(abilityBaseRef);
			return true;
		}

		//apply buffs to friendly selected targets
		if (abilityBaseRef.requiresTarget && !abilityBaseRef.isOffensiveAbility)
		{
			//apply buff to self if not an offensive buff && has no friendly target selected
			if (!abilityBaseRef.isOffensiveAbility) //add check for friendly selected target when MP is added
			{
				entityStats.ApplyStatusEffect(abilityBaseRef);
				return true;
			}
		}
		//apply debuffs to enemy selected targets
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

		else return false;

		//add support for AOE spells and abilities
		//add support instantiate directional spells/skills
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
