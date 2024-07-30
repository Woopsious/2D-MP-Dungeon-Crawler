using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ClassesScriptableObject", menuName = "Classes")]
public class SOClasses : ScriptableObject
{
	/// <summary>
	/// CLASSES can be changed for free, when player changes class, remove all abilities from hotbar and either
	/// unequip equipment back to inventory, or if not possible leave it on but dont let player leave hub area until its fixed and
	/// check all player equipment is valid before leaving (for mp check all players)
	/// </summary>

	/// <summary>
	/// have stat boost for every class every 3, 8, 12, 18 levels etc... (if multiple fore same level only allow picking of 1)
	/// have spells/skills for every class every 1, 5, 10, 15, 20 levels etc...
	/// each class will have spell/skill slots starting at 1 or 2. and gain more slots on certian up level ups based on class
	/// knight, warrior, ranger gets +1 spell/skill slot every 5 levels or etc...
	/// rogue, mage gets +2 spell/skill slot every 5 levels or etc... (set up SObj in a way levels can be customizable)
	/// </summary>

	//total Knight Abilities:	5		Total Ability Slots At Max Lvl:		4
	//total Warrior Abilities:	5		Total Ability Slots At Max Lvl:		4
	//total Ranger Abilities:	8		Total Ability Slots At Max Lvl:		5
	//total Rogue Abilities:	8		Total Ability Slots At Max Lvl:		5
	//total Mage Abilities:		16		Total Ability Slots At Max Lvl:		10


	[Header("Class Info")]
	public string className;
	[TextArea(3, 10)]
	public string classDescription;
	public Image classImageUi;
	public int BaseClassAbilityUnlocks;

	public List<SOClassStatBonuses> statBonusLists = new List<SOClassStatBonuses>();
	public List<SOClassAbilities> abilityLists = new List<SOClassAbilities>();

	[Header("Available Class Stat Bonuses")]
	/// <summary>
	/// loop through all of these when spawning any type of entity to apply stat bonuses.
	/// for player also loop through these when opening class tree ui to update it, + once player selects a stat bonus
	/// </summary>
	public List<ClassStatUnlocks> classStatBonusList = new List<ClassStatUnlocks>();

	[Header("Available Class Abilities")]
	/// <summary>
	/// loop through all of these when spawning any type of entity to apply stat bonuses.
	/// for player also loop through these when opening class tree ui to update it, + once player selects a stat bonus
	/// </summary>
	public List<SpellSlots> spellSlotsPerLevel = new List<SpellSlots>();
	public List<ClassAbilityUnlocks> classAbilitiesOffensiveList = new List<ClassAbilityUnlocks>();
	public List<ClassAbilityUnlocks> classAbilitiesHealingList = new List<ClassAbilityUnlocks>();
	public List<ClassAbilityUnlocks> classAbilitiesEffectsList = new List<ClassAbilityUnlocks>();

	[Header("Spells")]
	public bool canUseSpells;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}

	[Header("Starting Items")]
	public List<SOItems> startingItems = new List<SOItems>();
}

[System.Serializable]
public class SpellSlots
{
	public int LevelRequirement;
	public int SpellSlotsPerLevel;
}

[System.Serializable]
public class ClassStatUnlocks
{
	public SOClassStatBonuses unlock;
	public int LevelRequirement;
}

[System.Serializable]
public class ClassAbilityUnlocks
{
	public SOClassAbilities unlock;
	public int LevelRequirement;
}
