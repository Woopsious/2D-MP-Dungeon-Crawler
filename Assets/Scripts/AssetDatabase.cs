using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetDatabase : MonoBehaviour
{
	public static AssetDatabase Database;

	[Header("Entities Database")]
	public List<SOEntityStats> bossEntities = new List<SOEntityStats>();
	public List<SOEntityStats> entities = new List<SOEntityStats>();

	[Header("Classes/Stat Boosts Database")]
	public List<SOClasses> classes = new List<SOClasses>();
	public List<SOClassStatBonuses> classStatBoosts = new List<SOClassStatBonuses>();

	[Header("Abilities/Effects Database")]
	public List<SOAbilities> abilities = new List<SOAbilities>();
	public List<SOStatusEffects> statusEffects = new List<SOStatusEffects>();

	[Header("Items Database")]
	public List<SOWeapons> weapons = new List<SOWeapons>();
	public List<SOArmors> armours = new List<SOArmors>();
	public List<SOAccessories> accessories = new List<SOAccessories>();
	public List<SOConsumables> consumables = new List<SOConsumables>();

	[Header("Traps Database")]
	public List<SOTraps> traps = new List<SOTraps>();

	private void Awake()
	{
		Database = GetComponent<AssetDatabase>();
	}
}
