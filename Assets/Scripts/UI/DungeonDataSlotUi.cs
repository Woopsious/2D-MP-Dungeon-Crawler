using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DungeonDataSlotUi;

public class DungeonDataSlotUi : MonoBehaviour
{
	public GameObject saveDungeonButtonObj;
	public GameObject deleteDungeonButtonObj;

	public TMP_Text dungeonUnExploredText;
	public TMP_Text dungeonDifficultyText;
	public TMP_Text dungeonModifiersText;

	public bool hasExploredDungeon;
	public bool isDungeonSaved;
	public int dungeonIndex;
	public int dungeonNumber;

	public int maxDungeonModifiers;
	public DungeonStatModifier dungeonStatModifiers;
	public List<DungeonChestData> dungeonChestData = new List<DungeonChestData>();

	public ModifierType possibleModifiers;
	public enum ModifierType
	{
		healthMod, manaMod, physicalResistanceMod, poisonResistanceMod, fireResistanceMod, iceResistanceMod,
		physicalDamageMod, poisonDamageMod, fireDamageMod, iceDamageMod, mainWeaponDamageMod, dualWeaponDamageMod, rangedWeaponDamageMod
	}

	public static event Action<DungeonDataSlotUi> OnDungeonSave;
	public static event Action<DungeonDataSlotUi> OnDungeonDelete;

	public void Initilize(int index)
	{
		hasExploredDungeon = false;
		isDungeonSaved = false;
		dungeonIndex = index;
		dungeonNumber = Utilities.GetRandomNumber(SceneManager.sceneCountInBuildSettings - 3); //(not including hub and main menu scene)
		int modifier = Utilities.GetRandomNumber(2);

		if (hasExploredDungeon == false)
			dungeonUnExploredText.gameObject.SetActive(true);
		else
			dungeonUnExploredText.gameObject.SetActive(false);

		if (modifier == 0)
		{
			maxDungeonModifiers = 1;
			dungeonStatModifiers.difficultyModifier = 0;
			dungeonDifficultyText.text = "Difficulty: Normal \n(No Bonuses to enemy stats)";
		}
		else if (modifier == 1)
		{
			maxDungeonModifiers = 3;
			dungeonStatModifiers.difficultyModifier = 0.1f;
			dungeonDifficultyText.text = "Difficulty: <color=orange>Hard</color> \n(<color=orange>10%</color> bonus to all enemy stats)";
		}
		else
		{
			maxDungeonModifiers = 5;
			dungeonStatModifiers.difficultyModifier = 0.25f;
			dungeonDifficultyText.text = "Difficulty: <color=red>Hell</color> \n(<color=red>25%</color> bonus to all enemy stats)";
		}

		for (int i = 0; i < maxDungeonModifiers; i++)
		{
			int chanceOfModifier = Utilities.GetRandomNumber(100);
			if (chanceOfModifier <= 50) continue;

			int modifierType = Utilities.GetRandomNumber(Enum.GetNames(typeof(ModifierType)).Length - 1);
			SetModifierForDungeon(modifierType);
		}
		
		saveDungeonButtonObj.SetActive(true);
		deleteDungeonButtonObj.SetActive(false);
	}
	public void Initilize(DungeonData dungeonData, int index)
	{
		hasExploredDungeon = dungeonData.hasExploredDungeon;
		isDungeonSaved = dungeonData.isDungeonSaved;
		dungeonIndex = index;
		dungeonNumber = dungeonData.dungeonNumber;
		dungeonStatModifiers = dungeonData.dungeonStatModifiers;
		dungeonChestData = dungeonData.dungeonChestData;

		if (hasExploredDungeon == false)
			dungeonUnExploredText.gameObject.SetActive(true);
		else
			dungeonUnExploredText.gameObject.SetActive(false);

		if (dungeonStatModifiers.difficultyModifier == 0)
		{
			maxDungeonModifiers = 1;
			dungeonDifficultyText.text = "Difficulty: Normal \n(No Bonuses to enemy stats)";
		}
		else if (dungeonStatModifiers.difficultyModifier == 0.1f)
		{
			maxDungeonModifiers = 3;
			dungeonDifficultyText.text = "Difficulty: <color=orange>Hard</color> \n(<color=orange>10%</color> bonus to all enemy stats)";
		}
		else
		{
			maxDungeonModifiers = 5;
			dungeonDifficultyText.text = "Difficulty: <color=red>Hell</color> \n(<color=red>25%</color> bonus to all enemy stats)";
		}

		if (dungeonStatModifiers.healthModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.healthModifier)}% more Health";
		if (dungeonStatModifiers.manaModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.manaModifier)}% more Mana";

		if (dungeonStatModifiers.physicalResistanceModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.physicalResistanceModifier)}% more Physical Resistance";
		if (dungeonStatModifiers.poisonResistanceModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.poisonResistanceModifier)}% more Poison Resistance";
		if (dungeonStatModifiers.fireResistanceModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.fireResistanceModifier)}% more Fire Resistance";
		if (dungeonStatModifiers.iceResistanceModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.iceResistanceModifier)}% more Ice Resistance";

		if (dungeonStatModifiers.physicalDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.physicalDamageModifier)}% more Physical Damage";
		if (dungeonStatModifiers.poisonDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.poisonDamageModifier)}% more Poison Damage";
		if (dungeonStatModifiers.fireDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.fireDamageModifier)}% more Fire Damage";
		if (dungeonStatModifiers.iceDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.iceDamageModifier)}% more Ice Damage";

		if (dungeonStatModifiers.mainWeaponDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.mainWeaponDamageModifier)}% more Main Weapon Damage";
		if (dungeonStatModifiers.dualWeaponDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.dualWeaponDamageModifier)}% more Dual Weapon Damage";
		if (dungeonStatModifiers.rangedWeaponDamageModifier != 0)
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(dungeonStatModifiers.rangedWeaponDamageModifier)}% more Ranged Weapon Damage";
	}
	private void SetModifierForDungeon(int modifierType)
	{
		float modifierValue = 0.25f;

		if (modifierType == 0)
		{
			dungeonStatModifiers.healthModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Health";
		}
		else if (modifierType == 1)
		{
			dungeonStatModifiers.manaModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Mana";
		}

		else if (modifierType == 2)
		{
			dungeonStatModifiers.physicalResistanceModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Physical Resistance";
		}
		else if (modifierType == 3)
		{
			dungeonStatModifiers.poisonResistanceModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Poison Resistance";
		}
		else if (modifierType == 4)
		{
			dungeonStatModifiers.fireResistanceModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Fire Resistance";
		}
		else if (modifierType == 5)
		{
			dungeonStatModifiers.iceResistanceModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Ice Resistance";
		}

		else if (modifierType == 6)
		{
			dungeonStatModifiers.physicalDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Physical Damage";
		}
		else if (modifierType == 7)
		{
			dungeonStatModifiers.poisonDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Poison Damage";
		}
		else if (modifierType == 8)
		{
			dungeonStatModifiers.fireDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Fire Damage";
		}
		else if (modifierType == 9)
		{
			dungeonStatModifiers.iceDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Ice Damage";
		}

		else if (modifierType == 10)
		{
			dungeonStatModifiers.mainWeaponDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Main Weapon Damage";
		}
		else if (modifierType == 11)
		{
			dungeonStatModifiers.dualWeaponDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Dual Weapon Damage";
		}
		else if (modifierType == 12)
		{
			dungeonStatModifiers.rangedWeaponDamageModifier = modifierValue;
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifierValue)}% more Ranged Weapon Damage";
		}
		else
			Debug.LogError("modifer type out of range");
	}
	public void EnterDungeon()
	{
		hasExploredDungeon = true;

		GameManager.Instance.currentDungeonData.hasExploredDungeon = hasExploredDungeon;
		GameManager.Instance.currentDungeonData.isDungeonSaved = isDungeonSaved;
		GameManager.Instance.currentDungeonData.dungeonIndex = dungeonIndex;
		GameManager.Instance.currentDungeonData.dungeonNumber = dungeonNumber;
		GameManager.Instance.currentDungeonData.dungeonStatModifiers = dungeonStatModifiers;
		GameManager.Instance.currentDungeonData.dungeonChestData = dungeonChestData;

		if (dungeonNumber == 0)
			GameManager.Instance.LoadDungeonOne();
		else if (dungeonNumber == 1)
			GameManager.Instance.LoadDungeonTwo();
	}
	public void SaveDungeon()
	{
		isDungeonSaved = true;
		OnDungeonSave.Invoke(this);
	}
	public void DeleteDungeon()
	{
		OnDungeonDelete.Invoke(this);
	}
}
