using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerClassesScriptableObject", menuName = "PlayerClasses")]
public class SOPlayerClasses : ScriptableObject
{
	/// <summary>
	/// CLASSES can be changed for free, when player changes class, remove all abilities from hotbar and either
	/// unequip equipment back to inventory, or if not possible leave it on but dont let player leave hub area until its fixed and
	/// check check all player equipment is valid before leaving (for mp check all players)
	/// </summary>

	[Header("Class Info")]
	public string className;
	[TextArea(3, 10)]
	public string classDescription;

	public bool canUseSpells;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}
}
