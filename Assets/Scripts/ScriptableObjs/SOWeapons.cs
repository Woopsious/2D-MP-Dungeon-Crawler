using UnityEngine;

[CreateAssetMenu(fileName = "WeaponsScriptableObject", menuName = "Items/Weapons")]
public class SOWeapons : SOItems
{
	[Header("Weapon Info")]
	public bool isBareHands;
	public bool isShield;
	public int baseDamage;
	public float baseAttackSpeed;
	public float baseKnockback;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}
	[Header("Weapon Type")]
	public WeaponType weaponType;
	public enum WeaponType
	{
		isMainHand, isOffhand, isBoth
	}

	[Header("Damage Type")]
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Ranged Weapon Toggles")]
	[Tooltip("Changes Weapon Attack Behaviour")]
	public bool isRangedWeapon;
	[Tooltip("Max attack range")]
	public float baseMaxAttackRange;
	[Tooltip("Min attack range, 0 for melee weapons")]
	public float baseMinAttackRange;

	[Header("Projectile Settings")]
	public Sprite projectileSprite;
	[Range(20, 50)]
	public float projectileSpeed;

	[Header("Magic Weapon Toggles")]
	public int baseBonusMana;
}
