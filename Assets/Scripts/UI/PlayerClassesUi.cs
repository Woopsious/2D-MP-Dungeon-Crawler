using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerClassesUi : MonoBehaviour
{
	public static PlayerClassesUi Instance;

	public GameObject playerClassSelectionPanel;

	[Header("knight Class")]
	public SOClasses knightClass;
	public GameObject knightClassPanel;
	public Button playAsKnightButton;

	[Header("Warrior Class")]
	public SOClasses warriorClass;
	public GameObject warriorClassPanel;
	public Button playAsWarriorButton;

	[Header("Rogue Class")]
	public SOClasses rogueClass;
	public GameObject rogueClassPanel;
	public Button playAsRogueButton;

	[Header("Ranger Class")]
	public SOClasses rangerClass;
	public GameObject rangerClassPanel;
	public Button playAsRangerButton;

	[Header("Mage Class")]
	public SOClasses mageClass;
	public GameObject MageClassPanel;
	public Button playAsMageButton;

	public static event Action<SOClasses> OnClassChange;

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
		OnClassChange?.Invoke(knightClass);
		ShowClassSkillTree(knightClass);
		HidePlayerClassSelection();
	}
	public void PlayAsWarrior()
	{
		OnClassChange?.Invoke(warriorClass);
		ShowClassSkillTree(warriorClass);
		HidePlayerClassSelection();
	}
	public void PlayAsRogue()
	{
		OnClassChange?.Invoke(rogueClass);
		ShowClassSkillTree(rogueClass);
		HidePlayerClassSelection();
	}
	public void PlayAsRanger()
	{
		OnClassChange?.Invoke(rangerClass);
		ShowClassSkillTree(rangerClass);
		HidePlayerClassSelection();
	}
	public void PlayAsMage()
	{
		OnClassChange?.Invoke(mageClass);
		ShowClassSkillTree(mageClass);
		HidePlayerClassSelection();
	}

	private void ShowClassSkillTree(SOClasses thisClass)
	{
		if (thisClass == knightClass)
			knightClassPanel.SetActive(true);
		if (thisClass == warriorClass)
			warriorClassPanel.SetActive(true);
		if (thisClass == rogueClass)
			rogueClassPanel.SetActive(true);
		if (thisClass == rangerClass)
			rangerClassPanel.SetActive(true);
		if (thisClass == mageClass)
			MageClassPanel.SetActive(true);
	}
	private void HideClassSkillTree()
	{
		knightClassPanel.SetActive(false);
		warriorClassPanel.SetActive(false);
		rogueClassPanel.SetActive(false);
		rangerClassPanel.SetActive(false);
		MageClassPanel.SetActive(false);
	}

	public void ShowPlayerClassSelection()
	{
		playerClassSelectionPanel.SetActive(true);
	}
	public void HidePlayerClassSelection()
	{
		playerClassSelectionPanel.SetActive(false);
	}
}
