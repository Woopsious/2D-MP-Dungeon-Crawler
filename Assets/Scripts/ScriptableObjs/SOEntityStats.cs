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
		isPlayer, isGoblin, isSkeleton, isBat, isSlime, isZombie
	}
	[Range(0f, 1f)]
	public float enemySpawnChance;

	[Header("Equipment Status")]
	public bool canUseEquipment;
	[Tooltip("leave blank unless entity cant use equipment")]
	public SOWeapons UniqueAttackWeapon;

	[Header("Loot Info")]
	public int expOnDeath;
	public int maxDroppedGoldAmount;
	public int minDroppedGoldAmount;
	public SOLootPools lootPool;

	[Header("Behaviour")]
	public SOEntityBehaviour entityBehaviour;

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

	[Header("Possible Classes")]
	public List<SOClasses> possibleClassesList = new List<SOClasses>();

	[Header("Entity Audio")]
	public AudioClip deathSfx;
	public AudioClip hurtSfx;
	public AudioClip idleSfx;
}
