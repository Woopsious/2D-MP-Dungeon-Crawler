using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}

	public void UseAbility()
	{
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
}
