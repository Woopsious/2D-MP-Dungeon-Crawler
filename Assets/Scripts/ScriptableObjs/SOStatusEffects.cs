using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectsScriptableObject", menuName = "StatusEffects")]
public class SOStatusEffects : ScriptableObject
{
	[Header("Special Marked By Boss Setting")]
	public bool isMarkedByBossEffect;

	[Header("Status Effects Settings")]
	public string Name;
	public Sprite effectSprite;
	[Tooltip("only for status effects, leave as noEffect for anything else")]
	public StatusEffectType statusEffectType;
	public enum StatusEffectType
	{
		noEffect, isHealthEffect, isResistanceEffect, isDamageEffect, isDamageRecievedEffect, isMovementEffect
	}

	[Header("Damage Settings")]
	[Tooltip("if isDoT, value should be int, else percentage based 0.0 - 1.0")]
	public bool isDOT;
	public float effectValue;
	public IDamagable.DamageType damageType;

	[Header("Duration Settings")]
	[Tooltip("for status effects and DoT effects")]
	public bool hasDuration;
	public float abilityDuration;

	[Header("Particle Effect Settings")]
	public bool hasParticleEffect;
	public Color particleColour;
}
