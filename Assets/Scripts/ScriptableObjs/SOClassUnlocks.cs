using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks")]
public class SOClassUnlocks : ScriptableObject
{
	/// <summary>
	/// ABILITIES spells and skills are both under abilities, only difference being spells requite mana
	/// they are both unlocked from classes skill tree, spell book and abilities tab for inventory so player can equip them to hotbar
	/// abilities once unlocked are added to respective tabs in inventory, reset and rebuild unlocked abilities on class change
	/// some spells may just have a hard % cost if they are more unique like reviving spell (atm dont know what to do on death esp for MP)
	/// </summary>
	/// 
	[Header("Tree Unlock Info")]
	public string Name;
	[TextArea(3, 10)]
	public string Description;

	public int levelRequirementForNonPlayerEntities;
}
