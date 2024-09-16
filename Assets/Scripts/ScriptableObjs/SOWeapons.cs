using UnityEngine;

[CreateAssetMenu(fileName = "WeaponsScriptableObject", menuName = "Items/Weapons")]
public class SOWeapons : SOItems
{
	[Header("Weapon Info")]
	public bool isBareHands;
	public bool isShield;
	public float baseAttackSpeed;
	public float baseKnockback;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}
	[Header("Weapon Type")]
	public WeaponGripType weaponGripType;
	public enum WeaponGripType
	{
		isMainHand, isOffhand, isBoth
	}

	public WeaponType weaponType;
	public enum WeaponType
	{
		isAxe, isBow, isDagger, isMace, isShield, isStaff, isSword
	}

	[Header("Damage")]
	public int baseDamage;
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Ranged Weapon Toggles")]
	[Tooltip("Changes Weapon Attack Behaviour")]
	public bool isRangedWeapon;
	[Tooltip("Max attack range till projectile disapears")]
	public float maxAttackRange;
	[Tooltip("Min attack range, 0 for melee, 2 for ranged (based on longest reaching melee weapon)")]
	public float minAttackRange;

	[Header("Projectile Settings")]
	public Sprite projectileSprite;
	[Range(20, 50)]
	public float projectileSpeed;

	[Header("Magic Weapon Toggles")]
	public int baseBonusMana;

	[Header("Weapon Audio")]
	public AudioClip attackSfx;
}
