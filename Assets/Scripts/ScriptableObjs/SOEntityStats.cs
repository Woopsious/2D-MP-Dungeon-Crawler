using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityStatsScriptableObject", menuName = "Entities/Stats")]
public class SOEntityStats : ScriptableObject
{
	[Header("Entity Type Settings")]
	public string entityName;
	public Sprite sprite;

	public bool isBossVersion;
	public HumanoidTypes humanoidType;
	public enum HumanoidTypes
	{
		isPlayer, isGoblinBoss, isEyeBoss, isGoblin, isSkeleton, isBat, isSlime, isZombie
	}
	[Range(0f, 1f)]
	public float enemySpawnChance;

	[Header("Behaviour Ref")]
	public SOEntityBehaviour entityBehaviour;

	[Header("STATS")]
	[Header("Health")]
	[Tooltip("standard value is 75, 100 for player")]
	public int maxHealth;

	[Header("Damage")]
	[Tooltip("standard percentage value is 0.75, 100 for player")]
	public float damageDealtBaseModifier;

	[Header("Resistances")]
	[Tooltip("standard value is 5 | 8 for resistance | 2 for weakness")]
	public int physicalDamageResistance;
	[Tooltip("standard value is 5 | 8 for resistance | 2 for weakness")]
	public int poisonDamageResistance;
	[Tooltip("standard value is 5 | 8 for resistance | 2 for weakness")]
	public int fireDamageResistance;
	[Tooltip("standard value is 5 | 8 for resistance | 2 for weakness")]
	public int iceDamageResistance;

	[Header("Mana")]
	[Tooltip("standard value is 50, 100 for player")]
	public int maxMana;
	[Tooltip("standard value is 0.05")]
	public float manaRegenPercentage;
	[Tooltip("standard value is 3")]
	public float manaRegenCooldown;

	[Header("Classes")]
	public List<SOClasses> entityClasses = new List<SOClasses>();

	[Header("Equipment")]
	[Tooltip("CANNOT BE BLANK")]
	public List<SOWeapons> entityWeapons = new List<SOWeapons>();

	[Tooltip("can be left blank")]
	public List<SOArmors> entityHelmetArmours = new List<SOArmors>();
	[Tooltip("can be left blank")]
	public List<SOArmors> entityChestArmours = new List<SOArmors>();
	[Tooltip("can be left blank")]
	public List<SOArmors> entityLegArmours = new List<SOArmors>();

	[Header("Loot Settings")]
	public int expOnDeath;
	public int maxDroppedGoldAmount;
	public int minDroppedGoldAmount;
	[Range(0f, 100f)]
	public float itemRarityChanceModifier;
	public SOLootPools lootPool;

	[Header("Entity Audio")]
	public AudioClip deathSfx;
	public AudioClip hurtSfx;
	public AudioClip idleSfx;
}
