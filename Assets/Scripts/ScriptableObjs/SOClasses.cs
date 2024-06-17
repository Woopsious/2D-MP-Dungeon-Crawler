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

	[Header("Class Info")]
	public string className;
	[TextArea(3, 10)]
	public string classDescription;
	public Image classImageUi;

	public List<SOClassStatBonuses> statBonusLists = new List<SOClassStatBonuses>();
	public List<SOClassAbilities> abilityLists = new List<SOClassAbilities>();

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
