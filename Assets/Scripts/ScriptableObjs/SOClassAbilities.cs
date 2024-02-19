using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/Abilities")]
public class SOClassAbilities : SOClassUnlocks
{
	/// <summary>
	///  check if status effect first, then check if AoE, else treat it as instant use ability
	///  status effects require a target to be used on unless aoe, then anyone in aoe gets that effect - enemies (idk how thatll work atm)
	///  aoes can be placed anywhere valid in world and can be duration based
	///  instant use spells firebolt or heal can )
	///  a way to cancel abilities requiring a target via right click once casted??
	/// </summary>

	[Header("Ability Info")]
	public float abilityCooldown;
	public bool requiresTarget;

	[Header("Status Effects Settings")]
	public StatusEffectType statusEffectType;
	public enum StatusEffectType
	{
		noEffect, isHealthEffect, isResistanceEffect, isDamageEffect, isMagicDamageEffect
	}
	public bool canOnlyTargetSelf;

	[Header("AoE Settings")]
	public bool isAOE;

	[Header("DoT Settings")]
	public bool isDOT;
	public float abilityDuration;

	[Header("Damage Settings")]
	public bool percentageBased;
	public int value;
	public float valuePercentage;

	public DamageType damageType;
	public enum DamageType
	{
		isHealing, isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Spell Settings")]
	public bool isSpell;
	public int manaCost;            //atm idk if % cost or num cost for spells will work out better. after maxMana % boosts, hard num =
	public float minPercentCost;    //cost to use spells being pointless. % num cost = no point in having player mana ever change from 100
}
