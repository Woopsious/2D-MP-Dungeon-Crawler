using UnityEngine;

[CreateAssetMenu(fileName = "WeaponsScriptableObject", menuName = "Items/Weapons")]
public class SOWeapons : SOItems
{
	[Header("Weapon Info")]
	public bool isBareHands;
	public int baseDamage;
	public float baseAttackSpeed;
	public float baseKnockback;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}

	[Header("Damage Type")]
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Ranged Weapon Toggles")]
	public bool isRangedWeapon;
	public float baseMaxAttackRange;

	[Header("Magic Weapon Toggles")]
	public int baseBonusMana;
}
