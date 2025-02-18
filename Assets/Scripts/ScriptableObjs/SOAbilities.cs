using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/Abilities")]
public class SOAbilities : SOClassUnlocks
{
	/// <summary>
	///  check if status effect first, if true: apply said status effect to self/target
	///  else check if AoE, if true: have player select valid placable area for ability
	///  else check if damageType == healing, if true: heal self/target
	///  else treat as active ability and if its a projectile, if true: fire projectile in mouse direction
	///  else treat as active ability (Rogue backstab skill) and apply ability damage to next player attack
	/// </summary>

	[Header("Ability Info")]
	public bool isBossAbility;
	public float abilityCastingTimer;
	public float abilityCooldown;
	public bool isOffensiveAbility;
	[Tooltip("forces ability to need specific target to be applied to. EG: healing spells/skills")]
	public bool requiresTarget;
	[Tooltip("EG: Knight/Warrior healing skills")]
	public bool canOnlyTargetSelf;

	[Header("Status Effects Settings")]
	public bool hasStatusEffects;
	public List<SOStatusEffects> statusEffects;

	[Header("AoE Settings")]
	[Tooltip("AOE's cannot also be a projectile")]
	public bool isAOE;
	public AoeType aoeType;
	public enum AoeType
	{
		isCircleAoe, isConeAoe, isBoxAoe
	}
	public bool isDamageSplitBetweenHits;
	public bool hasAoeDuration;
	public float aoeDuration;

	[Header("Circle Aoe Settings")]
	[Range(20, 50)]
	public float circleAoeRadius;

	[Header("Cone Aoe Settings")]
	[Range(30, 150)]
	public float angle;
	[Range(20, 250)]
	public float coneAoeRadius;

	[Header("Box Aoe Settings")]
	[Range(10, 50)]
	public float boxAoeSizeX;
	[Range(20, 250)]

	public float boxAoeSizeY;

	[Header("Projectile Settings")]
	public bool isProjectile;
	public Sprite projectileSprite;
	[Range(20,50)]
	public float projectileSpeed;

	[Header("Damage Settings")]
	public bool isDamagePercentageBased;
	public IDamagable.DamageType damageType;
	public int damageValue;
	public float damageValuePercentage;

	[Header("Spell Settings")]
	public bool isSpell;
	public int manaCost;            //atm idk if % cost or num cost for spells will work out better. after maxMana % boosts, hard num =
	public float minPercentCost;    //cost to use spells being pointless. % num cost = no point in having player mana ever change from 100

	[Header("Particle Effect Settings")]
	public bool hasParticleEffect;
	public Color particleColour;
}
