using UnityEngine;

[CreateAssetMenu(fileName = "AccessoriesScriptableObject", menuName = "Items/Accessories")]
public class SOAccessories : SOItems
{
	[Header("Accessory Slot")]
	public AccessorySlot accessorySlot;
	public enum AccessorySlot
	{
		necklace, ring
	}

	[Header("Accessory Type")]
	public AccessoryType accessoryType;
	public enum AccessoryType
	{
		isWarding, isDamaging, isHealing
	}

	[Header("Accessory Warding")]
	public int baseBonusHealth;

	public int bonusPhysicalResistance;
	public int bonusPoisonResistance;
	public int bonusFireResistance;
	public int bonusIceResistance;

	[Header("Accessory Damaging/healing")]
	public int baseBonusMana;
	public float[] bonusPercentageValue = {0, 0.02f, 0.04f, 0.08f};

	public DamageTypeToBoost damageTypeToBoost;
	public enum DamageTypeToBoost
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
}
