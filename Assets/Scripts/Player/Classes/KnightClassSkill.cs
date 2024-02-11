using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightClassSkill : MonoBehaviour
{
	public int playerLevelRequirement;

	[Tooltip("previous skill tree items needed to unlock this one")]
	public List<KnightClassSkill> preRequisites = new List<KnightClassSkill>();
	[Tooltip("skill tree items that will lock this one")]
	public List<KnightClassSkill> exclusions = new List<KnightClassSkill>();

	/// <summary>
	/// each class will have x amount of these to fill out there class tree
	/// </summary>

	/// <summary> (NEW IDEA)
	/// have generic ClassSkillItem Scriptable Object that every class uses, make classSkillUI Item that i either build from ui or have premade
	/// have an enum to link them to specific classes, only apply bonuses from class the player current is using on class switch
	/// have script PlayerClassManager that links a player to events of other scripts + PlayerClassesUI that will control ui inputs etc.
	/// PlayerClassManager will work hopefully just like PlayerEquipmentManager and pass on stat boosts to PlayerStats.
	/// through events Player Stats update (correctly hopefully) when new items/stat boosts from class tree are unlocked
	/// as well as unlocking equippable abilities from there respective spells and skills tab in inventory
	/// </summary>

	public void Start()
	{
		
	}

	private void OnEnable()
	{

	}
	private void OnDisable()
	{
		
	}

	public void UnlockThisClassSkill()
	{
		//do one final check if level requirement is passed, check for any exclusions, check prerequisites are owned
		//if all checks pass continue
	}

	public void ApplyClassSkillEffect()
	{
		//apply effect of what ever it is, weather its percentage boosts to stats or unlocking abilities
	}


	public void CheckIfClassSkillShouldBeAvalable(int playerLevel)
	{
		//auto check if level requirement is passed, check for any exclusions, check prerequisites are owned
		//if all are passed allow the player to click this
	}

	public bool CheckExclusions()
	{
		return false;
	}
	public bool CheckPreRequisites()
	{
		return false;
	}
}
