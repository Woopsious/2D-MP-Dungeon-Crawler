using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonSlotUi : MonoBehaviour
{
	public GameObject saveDungeonButtonObj;
	public GameObject deleteDungeonButtonObj;

	public TMP_Text dungeonDifficultyText;
	public TMP_Text dungeonModifiersText;

	public bool isDungeonSaved;
	public int dungeonNumber;

	public int maxDungeonModifiers;
	public DungeonStatModifier dungeonStatModifiers;

	public static event Action<DungeonSlotUi> OnDungeonSave;
	public static event Action<DungeonSlotUi> OnDungeonDelete;

	public void Initilize()
	{
		dungeonNumber = Utilities.GetRandomNumber(SceneManager.sceneCountInBuildSettings - 2); //(not including hub and main menu scene)
		int modifier = Utilities.GetRandomNumber(3);

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
			dungeonDifficultyText.text = "Difficulty: Hard \n(10% bonus to all enemy stats)";
		}
		else
		{
			maxDungeonModifiers = 5;
			dungeonStatModifiers.difficultyModifier = 0.25f;
			dungeonDifficultyText.text = "Difficulty: Hell \n(25% bonus to all enemy stats)";
		}

		for (int i = 0; i < maxDungeonModifiers; i++)
		{
			int chanceOfModifier = Utilities.GetRandomNumber(101);
			if (chanceOfModifier <= 50) continue;

			int modifierType = Utilities.GetRandomNumber(Enum.GetNames(typeof(DungeonStatModifier.ModifierType)).Length);
			SetModifierForDungeon(modifierType);
		}

		saveDungeonButtonObj.SetActive(true);
		deleteDungeonButtonObj.SetActive(false);
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
		GameManager.dungeonStatModifiers = dungeonStatModifiers;

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
