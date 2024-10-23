using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
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

	//set accessory data
	public override void Initilize(Rarity setRarity, int setLevel, int setEnchantmentLevel)
	{
		base.Initilize(setRarity, setLevel, setEnchantmentLevel);

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
	public override void SetToolTip(EntityStats playerStats, bool itemInShopSlot)
	{
		toolTip = GetComponent<ToolTipUi>();

		string rarity;
		if (this.rarity == Rarity.isLegendary)
			rarity = "<color=orange>Legendary</color>";
		else if (this.rarity == Rarity.isEpic)
			rarity = "<color=purple>Epic</color>";
		else if (this.rarity == Rarity.isRare)
			rarity = "<color=blue>Rare</color>";
		else
			rarity = "Common";

		string info;
		if (itemEnchantmentLevel == 0)
			info = $"{rarity} Level {itemLevel} {itemName}\n{AdjustItemPriceDisplay(itemInShopSlot)} Price";
		else
			info = $"{rarity} Level {itemLevel} Enchanted {itemName} +{itemEnchantmentLevel}\n{itemPrice} Price";

		string extraInfo;

		if (accessoryType == AccessoryType.isWarding)
		{
			extraInfo = $"{(int)(bonusHealth * playerStats.maxHealth.GetPercentageModifiers())} Extra Health\n" +
				$"{(int)(bonusPhysicalResistance * playerStats.physicalResistance.GetPercentageModifiers())} Physical Res\n" +
				$"{(int)(bonusPoisonResistance * playerStats.poisonResistance.GetPercentageModifiers())} Poison Res\n" +
				$"{(int)(bonusFireResistance * playerStats.fireResistance.GetPercentageModifiers())} Fire Res\n" +
				$"{(int)(bonusIceResistance * playerStats.iceResistance.GetPercentageModifiers())} Ice Res";
		}
		else if (accessoryType == AccessoryType.isDamaging)
		{
			string damageType;
			if (damageTypeToBoost == DamageTypeToBoost.isPhysicalDamageType)
				damageType = $"{Utilities.ConvertFloatToUiPercentage(bonusPercentageValue)}% Physical Damage Boost";
			else if (damageTypeToBoost == DamageTypeToBoost.isPoisonDamageType)
				damageType = $"{Utilities.ConvertFloatToUiPercentage(bonusPercentageValue)}% Posion Damage Boost";
			else if (damageTypeToBoost == DamageTypeToBoost.isFireDamageType)
				damageType = $"{Utilities.ConvertFloatToUiPercentage(bonusPercentageValue)}% Fire Damage Boost";
			else
				damageType = $"{Utilities.ConvertFloatToUiPercentage(bonusPercentageValue)}% Ice Damage Boost";

			extraInfo = $"{(int)(bonusMana * playerStats.maxMana.GetPercentageModifiers())} Extra Mana\n{damageType}";
		}
		else
			extraInfo = $"{(int)(bonusMana * playerStats.maxMana.GetPercentageModifiers())} Extra Mana" +
				$"\n{Utilities.ConvertFloatToUiPercentage(bonusPercentageValue)}% Extra Healing";

		string equipInfo;
		if (playerStats.entityLevel < itemLevel)
			equipInfo = "<color=red>Cant Equip Accessory \n Level Too High</color>";
		else
			equipInfo = "<color=green>Can Equip Accessory</color>";

		toolTip.tipToShow = $"{info}\n{extraInfo}\n{equipInfo}";
	}

	//only called when loot drop or from shop
	public void SetRandomDamageTypeOnDrop()
	{
		int num = Utilities.GetRandomNumber(3);
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
