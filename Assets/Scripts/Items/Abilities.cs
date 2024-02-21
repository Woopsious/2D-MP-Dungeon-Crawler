using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities : MonoBehaviour
{
	[Header("Ability Info")]
	public string abilityName;
	public Sprite abilityImage;

	public SOClassAbilities abilityBaseRef;

	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}
}
