using System;
using System.Collections;
using UnityEngine;

public class Armors : Items
{
	[Header("Armor Info")]
	public int bonusHealth;
	public int bonusMana;

	[Header("Armor Slot")]
	public ArmorSlot armorSlot;
	public enum ArmorSlot
	{
		helmet, chestpiece, legs, robe
	}

	[Header("Resistances")]
	public int bonusPhysicalResistance;
	public int bonusPoisonResistance;
	public int bonusFireResistance;
	public int bonusIceResistance;

	private void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}

	//set armor data
	public override void Initilize(Rarity setRarity, int setLevel, int setEnchantmentLevel)
	{
		base.Initilize(setRarity, setLevel, setEnchantmentLevel);

		bonusHealth = (int)(armorBaseRef.baseBonusHealth * levelModifier);
		bonusMana = (int)(armorBaseRef.baseBonusMana * levelModifier);
		isStackable = armorBaseRef.isStackable;

		armorSlot = (ArmorSlot)armorBaseRef.armorSlot; // may not need this depending on what happens when i make proper inventroy

		bonusPhysicalResistance = (int)(armorBaseRef.bonusPhysicalResistance * levelModifier);
		bonusPoisonResistance = (int)(armorBaseRef.bonusPoisonResistance * levelModifier);
		bonusFireResistance = (int)(armorBaseRef.bonusFireResistance * levelModifier);
		bonusIceResistance = (int)(armorBaseRef.bonusIceResistance * levelModifier);
	}
	public override void UpdateToolTip(EntityStats playerStats, bool itemInShopSlot)
	{
		string rarity;
		if (this.rarity == Rarity.isLegendary)
			rarity = "<color=orange>Legendary</color>";
		else if (this.rarity == Rarity.isEpic)
			rarity = "<color=purple>Epic</color>";
		else if (this.rarity == Rarity.isRare)
			rarity = "<color=blue>Rare</color>";
		else
			rarity = "Common";

		string weightClass;
		if (armorBaseRef.classRestriction == SOArmors.ClassRestriction.heavy)
			weightClass = "Heavy Weight Restriction";
		else if (armorBaseRef.classRestriction == SOArmors.ClassRestriction.medium)
			weightClass = "Medium Weight Restriction";
		else
			weightClass = "Light Weight Restriction";

		string info;
		if (itemEnchantmentLevel == 0)
			info = $"{rarity} Level {itemLevel} {itemName}\n{AdjustItemPriceDisplay(itemInShopSlot)} Price \n{weightClass}";
		else
			info = $"{rarity} Level {itemLevel} Enchanted {itemName} +{itemEnchantmentLevel}\n{itemPrice} Price \n{weightClass}";

		string resInfo = $"{(int)(bonusHealth * playerStats.maxHealth.GetPercentageModifiers())} Extra Health\n" +
			$"{(int)(bonusMana * playerStats.maxMana.GetPercentageModifiers())} Extra Mana\n" +
			$"{(int)(bonusPhysicalResistance * playerStats.physicalResistance.GetPercentageModifiers())} Physical Res\n" +
			$"{(int)(bonusPoisonResistance * playerStats.poisonResistance.GetPercentageModifiers())} Poison Res\n" +
			$"{(int)(bonusFireResistance * playerStats.fireResistance.GetPercentageModifiers())} Fire Res\n" +
			$"{(int)(bonusIceResistance * playerStats.iceResistance.GetPercentageModifiers())} Ice Res";

		string equipInfo;
		if (playerStats.entityLevel < itemLevel)
			equipInfo = "<color=red>Cant Equip Armor \n Level Too High</color>";
		else if (PlayerClassesUi.Instance.currentPlayerClass == null)
		{
			equipInfo = "<color=yellow>no class equipped</color>";
		}
		else if ((int)PlayerClassesUi.Instance.currentPlayerClass.classRestriction < (int)armorBaseRef.classRestriction)
			equipInfo = "<color=red>Cant Equip Armor \n Weight too heavy</color>";
		else
			equipInfo = "<color=green>Can Equip Armor</color>";

		toolTip.tipToShow = $"{info}\n{resInfo}\n{equipInfo}";

	}
}
