using System.Collections;
using System.Collections.Generic;
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
	}
}
