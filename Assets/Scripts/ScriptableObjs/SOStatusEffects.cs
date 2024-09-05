using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectsScriptableObject", menuName = "StatusEffects")]
public class SOStatusEffects : ScriptableObject
{
	[Header("Status Effects Settings")]
	public string Name;
	[Tooltip("only for status effects, leave as noEffect for anything else")]
	public StatusEffectType statusEffectType;
	public enum StatusEffectType
	{
		noEffect, isHealthEffect, isResistanceEffect, isDamageEffect, isDamageRecievedEffect
	}

	[Header("Damage Settings")]
	[Tooltip("if isDoT, value should be int, else percentage based 0.0 - 1.0")]
	public bool isDOT;
	public float effectValue;

	public DamageType damageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType, isHealing, isMana
	}

	[Header("Duration Settings")]
	[Tooltip("for status effects and DoT effects")]
	public bool hasDuration;
	public float abilityDuration;

	[Header("Particle Effect Settings")]
	public bool hasParticleEffect;
	public Color particleColour;
}

[System.Serializable]
public class StatusEffectSettings
{
	[Tooltip("add effect name eg: poisoned/burning for ui (if left blank will use name)")]
	public string statusEffectName;
	[Tooltip("only for status effects, leave as noEffect for anything else")]
	public StatusEffectType statusEffectType;
	public enum StatusEffectType
	{
		noEffect, isHealthEffect, isResistanceEffect, isDamageEffect, isDamageRecievedEffect
	}

	[Header("Damage Settings")]
	[Tooltip("if isDoT, value should be int, else percentage based 0.0 - 1.0")]
	public bool isDOT;
	public float effectValue;

	public DamageType damageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType, isHealing, isMana
	}

	[Header("Duration Settings")]
	[Tooltip("for status effects and DoT effects")]
	public bool hasDuration;
	public float abilityDuration;
}
