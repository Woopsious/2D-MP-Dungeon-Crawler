using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityStatsScriptableObject", menuName = "Entities/Stats")]
public class SOEntityStats : ScriptableObject
{
	[Header("Entity Type")]
	public string entityName;
	public Sprite sprite;

	public HumanoidTypes humanoidType;
	public enum HumanoidTypes
	{
		isPlayer, isGoblin, isSkeleton
	}

	public int expOnDeath;

	[Header("Behaviour")]
	public SOEntityBehaviour entityBehaviour;

	[Header("Health")]
	[Tooltip("standard value is 100")]
	public int maxHealth;

	[Header("Resistances")]
	[Tooltip("standard value is 5")]
	public int physicalDamageResistance;
	[Tooltip("standard value is 5")]
	public int poisonDamageResistance;
	[Tooltip("standard value is 5")]
	public int fireDamageResistance;
	[Tooltip("standard value is 5")]
	public int iceDamageResistance;

	[Header("Mana")]
	[Tooltip("standard value is 100")]
	public int maxMana;
	[Tooltip("standard value is 0.05")]
	public float manaRegenPercentage;
	[Tooltip("standard value is 3")]
	public float manaRegenCooldown;

	[Header("Possible Classes")]
	public List<SOClasses> possibleClassesList = new List<SOClasses>();

	[Header("Possible Weapons")]
	public List<SOWeapons> possibleWeaponsList = new List<SOWeapons>();

	[Header("Possible Armors")]
	public List<SOArmors> possibleHelmetsList = new List<SOArmors>();
	public List<SOArmors> possibleChestpiecesList = new List<SOArmors>();
	public List<SOArmors> possibleLegsList = new List<SOArmors>();

	[Header("Entity Audio")]
	public AudioClip deathSfx;
	public AudioClip hurtSfx;
	public AudioClip idleSfx;
}
