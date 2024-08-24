using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/Abilities")]
public class SOClassAbilities : SOClassUnlocks
{
	/// <summary>
	///  check if status effect first, if true: apply said status effect to self/target
	///  else check if AoE, if true: have player select valid placable area for ability
	///  else check if damageType == healing, if true: heal self/target
	///  else treat as active ability and if its a projectile, if true: fire projectile in mouse direction
	///  else treat as active ability (Rogue backstab skill) and apply ability damage to next player attack
	/// </summary>

	[Header("Ability Info")]
	public float abilityCooldown;
	public bool isOffensiveAbility;
	[Tooltip("forces ability to need specific target to be applied to. EG: healing spells/skills")]
	public bool requiresTarget;
	[Tooltip("EG: Knight/Warrior healing skills")]
	public bool canOnlyTargetSelf;

	[Header("Status Effects Settings")]
	[Tooltip("only for status effects, leave as noEffect for anything else")]
	public StatusEffectType statusEffectType;
	public enum StatusEffectType
	{
		noEffect, isHealthEffect, isResistanceEffect, isDamageEffect, isDamageRecievedEffect
	}
	public float statusEffectPercentageModifier;

	[Header("AoE Settings")]
	[Tooltip("AOE's cannot also be a projectile")]
	public bool isAOE;
	[Range(30, 60)]
	public float aoeSize;

	[Header("DoT Settings")]
	[Tooltip("Mainly for status effects and some abilities that are AoE. EG: stances and wellOfRes spell")]
	public bool isDOT;
	public float abilityDuration;

	[Header("Projectile Settings")]
	public bool isProjectile;
	public Sprite projectileSprite;
	[Range(20,50)]
	public float projectileSpeed;

	[Header("Damage Settings")]
	[Tooltip("Status effects are always percentage based, spells and skill")]
	public bool isDamagePercentageBased;
	public int damageValue;
	public float damageValuePercentage;

	public DamageType damageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType, isHealing, isMana
	}

	[Header("Spell Settings")]
	public bool isSpell;
	public int manaCost;            //atm idk if % cost or num cost for spells will work out better. after maxMana % boosts, hard num =
	public float minPercentCost;    //cost to use spells being pointless. % num cost = no point in having player mana ever change from 100

	[Header("Particle Effect Settings")]
	public bool hasParticleEffect;
	public Color particleColour;
}
