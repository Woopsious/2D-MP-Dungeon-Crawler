using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/Abilities")]
public class SOClassAbilities : SOClassUnlocks
{
	[Header("Ability Info")]
	public bool isSpell;
	public bool isDOT;
	public bool isAOE;
	public float abilityDuration;
	public float abilityCooldown;

	[Header("Damage Type")]
	public bool isHealing;
	public bool percentageBased;
	public int value;
	public float valuePercentage;

	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Spell Info")]
	public int manaCost;            //atm idk if % cost or num cost for spells will work out better. after maxMana % boosts, hard num =
	public float minPercentCost;    //cost to use spells being pointless. % num cost = no point in having player mana ever change from 100
}
