using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accessories : Items
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
	public int bonusHealth;

	public int bonusPhysicalResistance;
	public int bonusPoisonResistance;
	public int bonusFireResistance;
	public int bonusIceResistance;

	[Header("Accessory Damaging/healing")]
	public int bonusMana;
	public float bonusPercentageValue;

	public DamageTypeToBoost damageTypeToBoost;
	public enum DamageTypeToBoost
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	private void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}

	public override void Initilize(Rarity setRarity, int setLevel)
	{
		base.Initilize(setRarity, setLevel);

		if (rarity == Rarity.isCommon)
			bonusPercentageValue = accessoryBaseRef.bonusPercentageValue[0];
		else if (rarity == Rarity.isRare)
			bonusPercentageValue = accessoryBaseRef.bonusPercentageValue[1];
		else if (rarity == Rarity.isEpic)
			bonusPercentageValue = accessoryBaseRef.bonusPercentageValue[2];
		else if (rarity == Rarity.isLegendary)
			bonusPercentageValue = accessoryBaseRef.bonusPercentageValue[3];

		bonusHealth = (int)(accessoryBaseRef.baseBonusHealth * levelModifier);
		bonusMana = (int)(accessoryBaseRef.baseBonusMana * levelModifier);
		isStackable = accessoryBaseRef.isStackable;

		accessorySlot = (AccessorySlot)accessoryBaseRef.accessorySlot;
		accessoryType = (AccessoryType)accessoryBaseRef.accessoryType;

		bonusPhysicalResistance = (int)(accessoryBaseRef.bonusPhysicalResistance * levelModifier);
		bonusPoisonResistance = (int)(accessoryBaseRef.bonusPoisonResistance * levelModifier);
		bonusFireResistance = (int)(accessoryBaseRef.bonusFireResistance * levelModifier);
		bonusIceResistance = (int)(accessoryBaseRef.bonusIceResistance * levelModifier);
	}

	public void SetRandomDamageTypeOnDrop()
	{
		int num = Utilities.GetRandomNumber(4);
		if (num == 0)
			damageTypeToBoost = DamageTypeToBoost.isPhysicalDamageType;
		else if (num == 1)
			damageTypeToBoost = DamageTypeToBoost.isPoisonDamageType;
		else if (num == 2)
			damageTypeToBoost = DamageTypeToBoost.isFireDamageType;
		else if (num == 3)
			damageTypeToBoost = DamageTypeToBoost.isIceDamageType;
		else
			Debug.LogError("No value match");
	}
}
