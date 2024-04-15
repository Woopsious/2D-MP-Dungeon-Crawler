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
	public float dungeonDifficultyModifier;
	public List<DungeonStatModifier> dungeonModifiers;

	public static event Action<DungeonSlotUi> OnDungeonSave;
	public static event Action<DungeonSlotUi> OnDungeonDelete;

	public void Initilize()
	{
		dungeonNumber = Utilities.GetRandomNumber(SceneManager.sceneCountInBuildSettings - 2); //(not including hub and main menu scene)
		int modifier = Utilities.GetRandomNumber(3);

		if (modifier == 0)
		{
			maxDungeonModifiers = 1;
			dungeonDifficultyModifier = 0;
			dungeonDifficultyText.text = "Difficulty: Normal \n(No Bonuses to enemy stats)";
		}
		else if (modifier == 1)
		{
			maxDungeonModifiers = 3;
			dungeonDifficultyModifier = 0.1f;
			dungeonDifficultyText.text = "Difficulty: Hard \n(10% bonus to all enemy stats)";
		}
		else
		{
			maxDungeonModifiers = 5;
			dungeonDifficultyModifier = 0.25f;
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
		DungeonStatModifier modifier = new()
		{ percentageValue = 0.25f };

		if (modifierType == 0)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.healthMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Health";
		}
		else if (modifierType == 1)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.manaMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Mana";
		}

		else if (modifierType == 2)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.physicalResistanceMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Physical Resistance";
		}
		else if (modifierType == 3)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.poisonResistanceMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Poison Resistance";
		}
		else if (modifierType == 4)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.fireResistanceMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Fire Resistance";
		}
		else if (modifierType == 5)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.iceResistanceMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Ice Resistance";
		}

		else if (modifierType == 6)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.physicalDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Physical Damage";
		}
		else if (modifierType == 7)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.poisonDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Poison Damage";
		}
		else if (modifierType == 8)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.fireDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Fire Damage";
		}
		else if (modifierType == 9)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.iceDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Ice Damage";
		}

		else if (modifierType == 10)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.mainWeaponDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Main Weapon Damage";
		}
		else if (modifierType == 11)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.dualWeaponDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Dual Weapon Damage";
		}
		else if (modifierType == 12)
		{
			modifier.modifierType = DungeonStatModifier.ModifierType.rangedWeaponDamageMod;
			dungeonModifiers.Add(modifier);
			dungeonModifiersText.text += $"\n{Utilities.ConvertFloatToUiPercentage(modifier.percentageValue)}% more Ranged Weapon Damage";
		}
		else
			Debug.LogError("modifer type out of range");
	}

	public void EnterDungeon()
	{
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
