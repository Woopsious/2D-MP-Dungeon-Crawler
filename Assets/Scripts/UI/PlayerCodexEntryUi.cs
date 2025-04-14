using TMPro;
using UnityEngine;

public class PlayerCodexEntryUi : MonoBehaviour
{
	private CodexEntryType codexEntryType;
	public enum CodexEntryType
	{
		none, bossEntityCodex, entityCodex, weaponCodex, armourCodex, accessoryCodex, consumableCodex, abilitiesCodex
	}

	//all codex entries use this
	public TMP_Text codexTitleInfo;

	//unique to entities
	public TMP_Text codexEntityInfo;

	//unique to items
	public TMP_Text codexItemInfo;

	[Header("Buttons/Inputs to edit info")]
	public GameObject levelInputObj;
	public TMP_InputField levelInputFieldText;
	public GameObject setRarityLevelObjs;
	public GameObject setEnchantmentLevelObjs;

	///<summary> weapons
	/// items level (based on players level + updates with them)
	/// rarity + level + name + enchant level
	/// class restriction
	/// weapon grip type
	/// ranged or melee weapon
	/// if shield display extra health + extra resistance instead of damage stats
	/// damage info {damage per hit + attack speed + DPS + knockback + bonus mana} for shield {health + resistance bonus}
	///	price 
	/// button to set rarity of said item
	/// button to set enchantment level of said item
	///<summary>

	///<summary> armour
	/// items level (based on players level + updates with them)
	/// rarity + level + name + enchant level
	/// class restriction
	/// if shield display extra health + extra resistance instead of damage stats
	/// resistance info {health + mana + resistances}
	///	price 
	/// button to set rarity of said item
	/// button to set enchantment level of said item
	///<summary>

	///<summary> accessories
	/// items level (based on players level + updates with them)
	/// rarity + level + name + enchant level
	/// class restriction
	/// accessory slot
	/// accessory type
	/// warding info {health + resistances}
	/// damage info {mana + damage boost}
	/// healing info {mana + healing boost}
	///	price 
	/// button to set rarity of said item
	/// button to set enchantment level of said item
	///<summary>

	///<summary> consumables
	/// name
	/// consumable type
	/// how much it restores
	///	price 
	/// button to set rarity of said item
	/// button to set enchantment level of said item
	///<summary>


	public void DisplayWeaponCodexEntry()
	{
		codexEntryType = CodexEntryType.weaponCodex;

		//generate text etc...
		//show said text etc...
	}

	public void DisplayNoCodexEntry()
	{
		codexEntryType = CodexEntryType.none;

		//hide text fields then clear them
	}

	public void UpdateCodexEntryLevel()
	{

	}
	public void UpdateCodexEntryRarity()
	{

	}
	public void UpdateCodexEntryEnchantment()
	{

	}

}
