using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrapsScriptableObject", menuName = "Traps")]
public class SOTraps : ScriptableObject
{
	[Header("Trap Info")]
	public string trapName;
	public Sprite trapSpriteUnDetected;
	public Sprite trapSpriteDetected;
	public Sprite trapSpriteActivated;

	[Header("Trap detection")]
	[Range(0.2f, 0.5f)]
	public float trapDetectionDifficulty;

	[Header("Trap Type")]
	public TrapType trapType;
	public enum TrapType
	{
		isSpike, isBomb, isArrow, isMagic, isGas
	}
	public float trapActivationDelay;

	[Header("Trap Damage")]
	public int baseDamage;
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Trap effects")]
	public bool hasEffects;
	public List<SOStatusEffects> statusEffects;

	[Header("Trap Aoe size")]
	public float aoeSize;

	[Header("Trap is Projectile based")]
	public bool hasProjectile;
	public Sprite projectileSprite;
	public float projectileSpeed;

	[Header("Trap spawn chance")]
	[Range(0f, 1f)]
	public float trapSpawnChance;

	[Header("Audio")]
	public AudioClip trapActivatedSfx;
}
