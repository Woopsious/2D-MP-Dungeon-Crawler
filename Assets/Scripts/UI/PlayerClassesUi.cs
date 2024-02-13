using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerClassesUi : MonoBehaviour
{
	public static PlayerClassesUi Instance;

	public GameObject playerClassesPanel;

	[Header("knight Class")]
	public SOPlayerClasses knightClass;
	public GameObject knightClassPanel;
	public Button playAsKnightButton;

	[Header("Warrior Class")]
	public SOPlayerClasses warriorClass;
	public GameObject warriorClassPanel;
	public Button playAsWarriorButton;

	[Header("Rogue Class")]
	public SOPlayerClasses rogueClass;
	public GameObject rogueClassPanel;
	public Button playAsRogueButton;

	[Header("Ranger Class")]
	public SOPlayerClasses rangerClass;
	public GameObject rangerClassPanel;
	public Button playAsRangerButton;

	[Header("Mage Class")]
	public SOPlayerClasses mageClass;
	public GameObject MageClassPanel;
	public Button playAsMageButton;

	public static event Action<SOPlayerClasses> OnClassChange;

	/// <summary> (NEW IDEA)
	/// have generic ClassSkillItem Scriptable Object that every class uses, make classSkillUI Item that i either build from ui or have premade
	/// have an enum to link them to specific classes, only apply bonuses from class the player current is using on class switch
	/// have script PlayerClassManager that links a player to events of other scripts + PlayerClassesUI that will control ui inputs etc.
	/// PlayerClassManager will work hopefully just like PlayerEquipmentManager and pass on stat boosts to PlayerStats.
	/// through events Player Stats update (correctly hopefully) when new items/stat boosts from class tree are unlocked
	/// as well as unlocking equippable abilities from there respective spells and skills tab in inventory
	/// </summary>

	private void Start()
	{
		Instance = this;
	}

	public void PlayAsKnight()
	{

	}
	public void PlayAsWarrior()
	{

	}
	public void PlayAsRogue()
	{
		
	}
	public void PlayAsRanger()
	{

	}
	public void PlayAsMage()
	{

	}

	public void ShowPlayerClasses()
	{
		playerClassesPanel.SetActive(true);
	}
	public void HidePlayerClasses()
	{
		playerClassesPanel.SetActive(false);
	}
}
