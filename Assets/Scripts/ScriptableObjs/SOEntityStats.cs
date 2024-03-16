using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityStatsScriptableObject", menuName = "Entities/Stats")]
public class SOEntityStats : ScriptableObject
{
	public string entityName;
	public Sprite sprite;

	public HumanoidTypes humanoidType;
	public enum HumanoidTypes
	{
		isPlayer, isGoblin
	}

	public int expOnDeath;

	[Header("health")]
	public int maxHealth;

	[Header("Resistances")]
	public int physicalDamageResistance;
	public int poisonDamageResistance;
	public int fireDamageResistance;
	public int iceDamageResistance;

	[Header("Mana")]
	public int maxMana;
	public float manaRegenPercentage;
	public float manaRegenCooldown;

	[Header("Possible Weapons")]
	public List<SOWeapons> possibleWeaponsList = new List<SOWeapons>();

	[Header("Possible Armors")]
	public List<SOArmors> possibleHelmetsList = new List<SOArmors>();
	public List<SOArmors> possibleChestpiecesList = new List<SOArmors>();
	public List<SOArmors> possibleLegsList = new List<SOArmors>();
}
