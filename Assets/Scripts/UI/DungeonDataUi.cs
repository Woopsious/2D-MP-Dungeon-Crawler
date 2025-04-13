using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DungeonDataUi : MonoBehaviour
{
	public GameObject DungeonInfoUi;

	public GameObject saveDungeonButtonObj;
	public GameObject deleteDungeonButtonObj;

	public TMP_Text dungeonExploredText;
	public TMP_Text dungeonDifficultyText;
	public TMP_Text dungeonModifiersText;

	public bool hasExploredDungeon;
	public bool isDungeonSaved;
	public int dungeonIndex;
	public int dungeonNumber;

	private int minDungeonModifiers;
	private int maxDungeonModifiers;
	private SOEntityStats bossToSpawn;
	private List<int> dungeonModifiersInUse = new List<int>();
	public DungeonStatModifier dungeonStatModifiers;
	public List<DungeonChestData> dungeonChestData = new List<DungeonChestData>();

	public ModifierType possibleModifiers;
	public enum ModifierType
	{
		healthMod, manaMod, physicalResistanceMod, poisonResistanceMod, fireResistanceMod, iceResistanceMod,
		physicalDamageMod, poisonDamageMod, fireDamageMod, iceDamageMod, mainWeaponDamageMod, dualWeaponDamageMod, rangedWeaponDamageMod
	}

	public static event Action<DungeonDataUi> OnDungeonSave;
	public static event Action<DungeonDataUi> OnDungeonDelete;

	//set dungeon data
	public async Task Initilize(int index, int dungeonDifficulty, SOEntityStats bossToSpawn) //initilize boss dungeon
	{
		hasExploredDungeon = false;
		isDungeonSaved = false;
		dungeonIndex = index;
		dungeonNumber = -1; //-1 to indicate its boss dungeon and scene it loads is randomized on enter
		this.bossToSpawn = bossToSpawn;
		await SetModifiersAndUi(dungeonDifficulty);
	}
	public async Task Initilize(int index) //initilize new dungeon
	{
		DungeonInfoUi.SetActive(false);
		hasExploredDungeon = false;
		isDungeonSaved = false;
		dungeonIndex = index;
		int choice = Utilities.GetRandomNumber(GameManager.Instance.dungeonSceneNamesList.Count - 1);
		dungeonNumber = choice + 4; //+4 for other scenes in build
		await SetModifiersAndUi(Utilities.GetRandomNumber(2));
	}
	public void Initilize(DungeonData dungeonData, int index) //initilize dungeon from saved data
	{
		hasExploredDungeon = dungeonData.hasExploredDungeon;
		isDungeonSaved = dungeonData.isDungeonSaved;
		dungeonIndex = index;
		dungeonNumber = dungeonData.dungeonNumber;
		dungeonStatModifiers = dungeonData.dungeonStatModifiers;
		dungeonChestData = dungeonData.dungeonChestData;

		int modifier;
		if (dungeonStatModifiers.difficultyModifier == 0)
			modifier = 0;
		else if (dungeonStatModifiers.difficultyModifier == 0.1f)
			modifier = 1;
		else
			modifier = 2;

		SetDifficultyModifierAndUI(modifier);
		UpdateDynamicUi();

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

	//set modifiers + ui
	private async Task SetModifiersAndUi(int modifier)
	{
		SetDifficultyModifierAndUI(modifier);
		UpdateDynamicUi();

		if (dungeonNumber != -1)
		{
			for (int i = 0; i < maxDungeonModifiers; i++)
			{
				int chanceOfModifierAndDelayTime = Utilities.GetRandomNumber(100);
				await Task.Delay(chanceOfModifierAndDelayTime);

				if (dungeonModifiersInUse.Count < minDungeonModifiers) //ensure min modifiers
					chanceOfModifierAndDelayTime = 100;

				if (chanceOfModifierAndDelayTime <= 66) continue;
				SetDungeonModifiersAndUi(GetNonDuplicateModifier());
			}
		}
		DungeonInfoUi.SetActive(true);
	}
	private void SetDifficultyModifierAndUI(int modifier)
	{
		if (modifier == 0)
		{
			minDungeonModifiers = 0;
			maxDungeonModifiers = 1;
			dungeonStatModifiers.difficultyModifier = 0;
			dungeonDifficultyText.text = "Difficulty: Normal \n(No Bonuses to enemy stats)";
		}
		else if (modifier == 1)
		{
			minDungeonModifiers = 1;
			maxDungeonModifiers = 3;
			dungeonStatModifiers.difficultyModifier = 0.1f;
			dungeonDifficultyText.text = "Difficulty: <color=orange>Hard</color> \n(<color=orange>10%</color> bonus to all enemy stats)";
		}
		else
		{
			minDungeonModifiers = 3;
			maxDungeonModifiers = 5;
			dungeonStatModifiers.difficultyModifier = 0.25f;
			dungeonDifficultyText.text = "Difficulty: <color=red>Hell</color> \n(<color=red>25%</color> bonus to all enemy stats)";
		}
	}
	private void SetDungeonModifiersAndUi(int modifierType)
	{
		float modifierValue = 0.25f;
		dungeonModifiersInUse.Add(modifierType);

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
	private int GetNonDuplicateModifier()
	{
		for (int i = 0; i < 10; i++) //try max 10 times
		{
			int modifierType = Utilities.GetRandomNumber(Enum.GetNames(typeof(ModifierType)).Length - 1);
			if (!DungeonStatModifierAlreadyExists(modifierType))
				return modifierType;
			else
				continue;
		}
		return 0; //return health mod as fail safe
	}
	private bool DungeonStatModifierAlreadyExists(int newModifier)
	{
		foreach (int modifier in dungeonModifiersInUse)
		{
			if (modifier == newModifier)
				return true;
		}
		return false;
	}

	//ui that changes
	public void UpdateDynamicUi()
	{
		if (dungeonNumber == -1) //boss dungeon
		{
			dungeonExploredText.gameObject.SetActive(true);
			dungeonExploredText.text = bossToSpawn.entityName;
			dungeonModifiersText.text = "No Modifiers for boss Dungeons";

			saveDungeonButtonObj.SetActive(false);
			deleteDungeonButtonObj.SetActive(false);
			return;
		}

		if (hasExploredDungeon)
			dungeonExploredText.text = "<color=yellow>Explored</color>";
		else
			dungeonExploredText.text = "<color=yellow>!!!Unexplored!!!</color>";

		if (isDungeonSaved)
		{
			saveDungeonButtonObj.SetActive(false);
			deleteDungeonButtonObj.SetActive(true);
		}
		else
		{
			saveDungeonButtonObj.SetActive(true);
			deleteDungeonButtonObj.SetActive(false);
		}
	}

	//actions
	public void EnterDungeon() //button click
	{
		hasExploredDungeon = true;

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncDungeonStatModifiersRpc(dungeonStatModifiers.difficultyModifier, GetListOfDungeonStatModifiers());

		GameManager.Instance.currentDungeonData.hasExploredDungeon = hasExploredDungeon;
		GameManager.Instance.currentDungeonData.isDungeonSaved = isDungeonSaved;
		GameManager.Instance.currentDungeonData.dungeonIndex = dungeonIndex;
		GameManager.Instance.currentDungeonData.dungeonNumber = dungeonNumber;
		GameManager.Instance.currentDungeonData.bossToSpawn = bossToSpawn;
		GameManager.Instance.currentDungeonData.dungeonStatModifiers = dungeonStatModifiers;
		GameManager.Instance.currentDungeonData.dungeonChestData = dungeonChestData;

		if (dungeonNumber == -1)
			EnterBossDungeon();
		else	
			EnterNormalDungeon();
	}
	private void EnterNormalDungeon()
	{
		if (dungeonNumber == 4)
			GameManager.Instance.LoadDungeonOne();
		else if (dungeonNumber == 5)
			GameManager.Instance.LoadDungeonTwo();
	}
	private void EnterBossDungeon()
	{
		GameManager.Instance.LoadRandomBossDungeon();
	}
	public void SaveDungeon() //button click
	{
		isDungeonSaved = true;
		OnDungeonSave.Invoke(this);
	}
	public void DeleteDungeon() //button click
	{
		OnDungeonDelete.Invoke(this);
	}

	private float[] GetListOfDungeonStatModifiers()
	{
		float[] modifiers = new float[]
		{
			dungeonStatModifiers.healthModifier, dungeonStatModifiers.manaModifier, 
			dungeonStatModifiers.physicalResistanceModifier,dungeonStatModifiers.poisonResistanceModifier, 
			dungeonStatModifiers.fireResistanceModifier, dungeonStatModifiers.iceResistanceModifier,
			dungeonStatModifiers.physicalDamageModifier, dungeonStatModifiers.poisonDamageModifier, 
			dungeonStatModifiers.fireDamageModifier, dungeonStatModifiers.iceDamageModifier, 
			dungeonStatModifiers.mainWeaponDamageModifier, dungeonStatModifiers.dualWeaponDamageModifier,
			dungeonStatModifiers.rangedWeaponDamageModifier,
		};

		return modifiers;
	}
}
