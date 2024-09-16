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

	[Range(0f, 1f)]
	public float trapDetectionDifficulty;

	[Header("Trap Type")]
	public TrapType trapType;
	public enum TrapType
	{
		isSpike, isBomb, isArrow, isMagic
	}

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

	[Header("Trap Aoe")]
	public bool hasAoe;
	public float aoeSize;

	[Header("Audio")]
	public AudioClip trapActivatedSfx;
}
