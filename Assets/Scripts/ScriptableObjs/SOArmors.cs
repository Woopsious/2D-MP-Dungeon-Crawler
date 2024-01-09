using UnityEngine;

[CreateAssetMenu(fileName = "ArmorsScriptableObject", menuName = "Items/Armors")]
public class SOArmors : SOItems
{
	[Header("Armor Info")]
	public int baseBonusHealth;
	public int baseBonusMana;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}

	[Header("Armor Slot")]
	public ArmorSlot armorSlot;
	public enum ArmorSlot
	{
		helmet, chest, legs
	}

	[Header("Resistances")]
	public int bonusPhysicalResistance;
	public int bonusPoisonResistance;
	public int bonusFireResistance;
	public int bonusIceResistance;
}
