using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilitiesScriptableObject", menuName = "Abilities")]
public class SOAbilities : ScriptableObject
{
	/// <summary>
	/// ABILITIES spells and skills are both under abilities, only difference being spells requite mana
	/// they are both unlocked from classes skill tree, spell book and abilities tab for inventory so player can equip them to hotbar
	/// abilities once unlocked are added to respective tabs in inventory, reset and rebuild unlocked abilities on class change
	/// some spells may just have a hard % cost if they are more unique like reviving spell (atm dont know what to do on death esp for MP)
	/// </summary>

	[Header("Ability Info")]
	public string abilityName;
	[TextArea(3, 10)]
	public string abilityDescription;

	public bool isSpell;

	[Header("Damage Type")]
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Spell Info")]
	public int manaCost;			//atm idk if % cost or num cost for spells will work out better. after maxMana % boosts, hard num =
	public float minPercentCost;	//cost to use spells being pointless. % num cost = no point in having player mana ever change from 100
}
