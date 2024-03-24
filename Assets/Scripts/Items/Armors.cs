using System.Collections;
using System.Collections.Generic;
using TMPro;
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

	public override void Initilize(Rarity setRarity, int setLevel)
	{
		base.Initilize(setRarity, setLevel);

		bonusHealth = (int)(armorBaseRef.baseBonusHealth * levelModifier);
		bonusMana = (int)(armorBaseRef.baseBonusMana * levelModifier);
		isStackable = armorBaseRef.isStackable;

		armorSlot = (ArmorSlot)armorBaseRef.armorSlot; // may not need this depending on what happens when i make proper inventroy

		bonusPhysicalResistance = (int)(armorBaseRef.bonusPhysicalResistance * levelModifier);
		bonusPoisonResistance = (int)(armorBaseRef.bonusPoisonResistance * levelModifier);
		bonusFireResistance = (int)(armorBaseRef.bonusFireResistance * levelModifier);
		bonusIceResistance = (int)(armorBaseRef.bonusIceResistance * levelModifier);

		SetToolTip();
	}
	protected override void SetToolTip()
	{
		toolTip = GetComponent<ToolTipUi>();

		string rarity;
		if (this.rarity == Rarity.isLegendary)
			rarity = "Legendary";
		else if (this.rarity == Rarity.isEpic)
			rarity = "Epic";
		else if (this.rarity == Rarity.isRare)
			rarity = "Rare";
		else
			rarity = "Common";
		string info = $"{rarity} Level {itemLevel} {itemName} \n {itemPrice} Price";

		string resInfo = $"{bonusHealth} Extra Health \n {bonusMana} Extra Mana \n {bonusPhysicalResistance} Physical Res \n" +
			$"{bonusPoisonResistance} Poison Res \n {bonusFireResistance} Fire Res \n {bonusIceResistance} Ice Res";

		toolTip.tipToShow = $"{info} \n {resInfo}";
	}
}
