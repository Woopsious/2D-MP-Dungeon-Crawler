using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerClassesUi : MonoBehaviour
{
	public static PlayerClassesUi Instance;

	public GameObject ClassSelectionUiPanel;
	public GameObject ClassTreesUiPanel;

	[Header("Class Selection")]
	public GameObject playerClassSelectionPanel;
	public GameObject closeplayerClassSelectionButton;
	public SOClasses currentPlayerClass;
	public List<ClassTreeNodeUi> currentUnlockedClassNodes = new List<ClassTreeNodeUi>();

	[Header("knight Class")]
	public TMP_Text knightClassInfo;
	public SOClasses knightClass;
	public GameObject knightClassPanel;
	public GameObject knightStatBonusContent;
	public GameObject knightAbilityContent;
	public Button playAsKnightButton;

	[Header("Warrior Class")]
	public TMP_Text warriorClassInfo;
	public SOClasses warriorClass;
	public GameObject warriorClassPanel;
	public GameObject warriorStatBonusContent;
	public GameObject warriorAbilityContent;
	public Button playAsWarriorButton;

	[Header("Rogue Class")]
	public TMP_Text rogueClassInfo;
	public SOClasses rogueClass;
	public GameObject rogueClassPanel;
	public GameObject rogueStatBonusContent;
	public GameObject rogueAbilityContent;
	public Button playAsRogueButton;

	[Header("Ranger Class")]
	public TMP_Text rangerClassInfo;
	public SOClasses rangerClass;
	public GameObject rangerClassPanel;
	public GameObject rangerStatBonusContent;
	public GameObject rangerAbilityContent;
	public Button playAsRangerButton;

	[Header("Mage Class")]
	public TMP_Text mageClassInfo;
	public SOClasses mageClass;
	public GameObject MageClassPanel;
	public GameObject mageStatBonusContent;
	public GameObject mageAbilityContent;
	public Button playAsMageButton;

	[Header("Shared Class ui elements")]
	public GameObject SharedClassUiElements;
	public TMP_Text abilitySlotsCounterText;
	public int maxAbilitySlots;
	public int abilitySlotsUsed;

	public static event Action<SOClasses> OnClassChanges;
	public static event Action<EntityStats> OnClassNodeUnlocks;

	public static event Action<SOClassStatBonuses> OnNewStatBonusUnlock;
	public static event Action<SOAbilities> OnNewAbilityUnlock;

	public static event Action<SOClassStatBonuses> OnRefundStatBonusUnlock;
	public static event Action<SOAbilities> OnRefundAbilityUnlock;

	public GameObject classNodeSlotsPrefab;
	private List<ClassTreeNodeUi> nodeSlotUiList = new List<ClassTreeNodeUi>();

	/// <summary> (NEW IDEA)
	/// player selects class on new game start. if reloading game, indexes of Unlocked ClassTreeSlotUi nodes are saved and loaded to disk,
	/// from a List in PlayerClassHandler, nodes are then auto unlocked based on those indexes when reloading a game.
	/// when player opens or levels up updating ui and check if nodes could be unlocked (check for exclusions, player level, pre requisites).
	/// once player unlocks new node, event sent to PlayerClassHandler that applies stat bonuses, PlayerClassHandler calls another event
	/// </summary>

	private void Awake()
	{
		Instance = this;
		Initilize();
	}
	private void OnEnable()
	{
		//SaveManager.RestoreData += ReloadPlayerClass;
		SceneManager.sceneLoaded += OnSceneChange;
		SaveManager.ReloadSaveGameData += ReloadPlayerClass;

		PlayerEventManager.OnShowPlayerInventoryEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += ShowPlayerClassSelection;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerJournalEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerDeathUiEvent += HidePlayerClassSelection;

		PlayerEventManager.OnShowPlayerInventoryEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += ShowClassSkillTree;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerJournalEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerDeathUiEvent += HideClassSkillTree;

		PlayerEventManager.OnPlayerLevelUpEvent += UpdateMaxAbilitySlots;
	}
	private void OnDisable()
	{
		//SaveManager.RestoreData -= ReloadPlayerClass;
		SceneManager.sceneLoaded -= OnSceneChange;
		SaveManager.ReloadSaveGameData -= ReloadPlayerClass;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= ShowPlayerClassSelection;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerJournalEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerDeathUiEvent -= HidePlayerClassSelection;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= ShowClassSkillTree;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerJournalEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerDeathUiEvent -= HideClassSkillTree;

		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateMaxAbilitySlots;

		nodeSlotUiList.Clear();
	}

	private void Initilize()
	{
		SetUpKnightClassInfo();
		SetUpWarriorClassInfo();
		SetUpRogueClassInfo();
		SetUpRangerClassInfo();
		SetUpMageClassInfo();

		SetUpKnightClassTree();
		SetUpWarriorClassTree();
		SetUpRogueClassTree();
		SetUpRangerClassTree();
		SetUpMageClassTree();
	}

	private void OnSceneChange(Scene newLoadedScene, LoadSceneMode mode)
	{
		if (newLoadedScene.name == GameManager.Instance.hubScene && GameManager.isNewGame)
			ShowPlayerClassSelection();
	}

	//set up text ui for class selection panel
	private void SetUpKnightClassInfo()
	{
		string knightInfoText = "Knight Class Info:\n" +
			$"Heavy focus on damage resistance and health whilst keeping aggro.\n\n" +
			$"Comes with ways of increasing damage resistance and health with some damage increases.\n\n" +
			$"Abilities are mostly defensive with some healing and damage";

		string casterInfo = "Non caster";
		string knightStartingEquipment = "Starting Equipment:\n" + GetStartingEquipmentInfo(knightClass);
		string knightAbilitySlotInfo = "Ability Slots Info:\n" + GetAbilitySlotsInfo(knightClass);

		knightClassInfo.text = knightInfoText + "\n\n" + casterInfo + "\n\n" + knightStartingEquipment + "\n\n" + knightAbilitySlotInfo;
	}
	private void SetUpWarriorClassInfo()
	{
		string warriorInfoText = "Warrior Class Info:\n" +
			$"A balance between damage and survivability.\n\n" +
			$"Comes with ways to spec more into increasing damage or resisting it.\n\n" +
			$"Abilities are defensive leaning with more options to deal damage";

		string casterInfo = "Non caster";
		string warriorStartingEquipment = "Starting Equipment:\n" + GetStartingEquipmentInfo(warriorClass);
		string warriorAbilitySlotInfo = "Ability Slots Info:\n" + GetAbilitySlotsInfo(warriorClass);

		warriorClassInfo.text = warriorInfoText + "\n\n" + casterInfo + "\n\n" + warriorStartingEquipment + "\n\n" + warriorAbilitySlotInfo;
	}
	private void SetUpRogueClassInfo()
	{
		string rogueInfoText = "Rogue Class Info:\n" +
			$"Consistent damage dealer with some survivability.\n\n" +
			$"Comes mostly with ways of increasing damage with duel wield weapons and some survivability.\n\n" +
			$"Abilities are mostly focused on dealing consistent damage with some utility options";

		string casterInfo = "Half caster";
		string rogueStartingEquipment = "Starting Equipment:\n" + GetStartingEquipmentInfo(rogueClass);
		string rogueAbilitySlotInfo = "Ability Slots Info:\n" + GetAbilitySlotsInfo(rogueClass);

		rogueClassInfo.text = rogueInfoText + "\n\n" + casterInfo + "\n\n" + rogueStartingEquipment + "\n\n" + rogueAbilitySlotInfo;
	}
	private void SetUpRangerClassInfo()
	{
		string rangerInfoText = "Ranger Class Info:\n" +
			$"Consistent damage dealer with weak survivability and ranged attacks.\n\n" +
			$"Comes mostly with ways of increasing damage with ranged weapons and little survivability.\n\n" +
			$"Abilities are mostly focused on dealing damage with some utility and support with healing";

		string casterInfo = "Half caster";
		string rangerStartingEquipment = "Starting Equipment:\n" + GetStartingEquipmentInfo(rangerClass);
		string rangerAbilitySlotInfo = "Ability Slots Info:\n" + GetAbilitySlotsInfo(rangerClass);

		rangerClassInfo.text = rangerInfoText + "\n\n" + casterInfo + "\n\n" + rangerStartingEquipment + "\n\n" + rangerAbilitySlotInfo;
	}
	private void SetUpMageClassInfo()
	{
		string mageInfoText = "Mage Class Info:\n" +
			$"Burst damage dealer with weak survivability and a variety of aoe and ranged attacks.\n\n" +
			$"Comes mostly with ways of increasing magic damage or bonus healing, with some mana bonuses and little survivability.\n\n" +
			$"Wide variety of abilities from damage, healing and utility spells, both aoe and single target";

		string casterInfo = "Full caster";
		string mageStartingEquipment = "Starting Equipment:\n" + GetStartingEquipmentInfo(mageClass);
		string mageAbilitySlotInfo = "Ability Slots Info:\n" + GetAbilitySlotsInfo(mageClass);

		mageClassInfo.text = mageInfoText + "\n\n" + casterInfo + "\n\n" + mageStartingEquipment + "\n\n" + mageAbilitySlotInfo;
	}
	private string GetStartingEquipmentInfo(SOClasses soClass)
	{
		string info = "Starts with 500 gold\n";

		if (soClass.startingWeapon.Count == 1)
			info += $"1x {soClass.startingWeapon[0].itemName}";
		else
		{
			info += "1x random ";

			foreach (SOWeapons weapon in soClass.startingWeapon)
				info += $"{weapon.itemName}, ";

		}
		info += "\n";

		foreach (SOArmors armor in soClass.startingArmor)
		{
			if (armor.armorSlot == SOArmors.ArmorSlot.helmet)
				info += $"1x {armor.itemName}, ";
			else if (armor.armorSlot == SOArmors.ArmorSlot.chest)
				info += $"1x {armor.itemName}, ";
			else if (armor.armorSlot == SOArmors.ArmorSlot.legs)
				info += $"1x {armor.itemName}\n";
		}
		foreach (SOConsumables consumables in soClass.startingConsumables)
		{
			if (consumables.consumableType == SOConsumables.ConsumableType.healthRestoration)
				info += $"3x {consumables.itemName}, ";
			else if (consumables.consumableType == SOConsumables.ConsumableType.manaRestoration)
				info += $"3x {consumables.itemName}";
		}

		return info;
	}
	private string GetAbilitySlotsInfo(SOClasses soClass)
	{
		string info = $"Start with {soClass.baseClassAbilitySlots} ability slot\n";

		foreach (AbilitySlots abilitySlot in soClass.spellSlotsPerLevel)
			info += $"Gain {abilitySlot.AbilitySlotsPerLevel} ability slot at level {abilitySlot.LevelRequirement} \n";

		return info;
	}

	//set up class trees ui for every class
	private void SetUpKnightClassTree()
	{
		foreach (ClassStatUnlocks classStatUnlock in knightClass.classStatBonusList)
		{
			if (classStatUnlock.LevelRequirement == 1)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 0);
			if (classStatUnlock.LevelRequirement == 3)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 1);
			if (classStatUnlock.LevelRequirement == 6)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 2);
			if (classStatUnlock.LevelRequirement == 9)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 3);
			if (classStatUnlock.LevelRequirement == 12)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 4);
			if (classStatUnlock.LevelRequirement == 15)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 5);
			if (classStatUnlock.LevelRequirement == 18)
				CreateNewUnlockNode(classStatUnlock, null, knightStatBonusContent.transform, 6);
		}
		foreach (ClassAbilityUnlocks classAbility in knightClass.classAbilitiesOffensiveList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 5)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 10)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 15)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 3);
		}
		foreach (ClassAbilityUnlocks classAbility in knightClass.classAbilitiesEffectsList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 5)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 10)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 15)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 3);
		}
		foreach (ClassAbilityUnlocks classAbility in knightClass.classAbilitiesHealingList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 5)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 10)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 15)
				CreateNewUnlockNode(null, classAbility, knightAbilityContent.transform, 3);
		}
	}
	private void SetUpWarriorClassTree()
	{
		foreach (ClassStatUnlocks classStatUnlock in warriorClass.classStatBonusList)
		{
			if (classStatUnlock.LevelRequirement == 1)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 0);
			if (classStatUnlock.LevelRequirement == 3)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 1);
			if (classStatUnlock.LevelRequirement == 6)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 2);
			if (classStatUnlock.LevelRequirement == 9)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 3);
			if (classStatUnlock.LevelRequirement == 12)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 4);
			if (classStatUnlock.LevelRequirement == 15)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 5);
			if (classStatUnlock.LevelRequirement == 18)
				CreateNewUnlockNode(classStatUnlock, null, warriorStatBonusContent.transform, 6);
		}
		foreach (ClassAbilityUnlocks classAbility in warriorClass.classAbilitiesOffensiveList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 5)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 10)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 15)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 3);
		}
		foreach (ClassAbilityUnlocks classAbility in warriorClass.classAbilitiesEffectsList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 5)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 10)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 15)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 3);
		}
		foreach (ClassAbilityUnlocks classAbility in warriorClass.classAbilitiesHealingList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 5)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 10)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 15)
				CreateNewUnlockNode(null, classAbility, warriorAbilityContent.transform, 3);
		}
	}
	private void SetUpRogueClassTree()
	{
		foreach (ClassStatUnlocks classStatUnlock in rogueClass.classStatBonusList)
		{
			if (classStatUnlock.LevelRequirement == 1)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 0);
			if (classStatUnlock.LevelRequirement == 3)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 1);
			if (classStatUnlock.LevelRequirement == 6)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 2);
			if (classStatUnlock.LevelRequirement == 9)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 3);
			if (classStatUnlock.LevelRequirement == 12)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 4);
			if (classStatUnlock.LevelRequirement == 15)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 5);
			if (classStatUnlock.LevelRequirement == 18)
				CreateNewUnlockNode(classStatUnlock, null, rogueStatBonusContent.transform, 6);
		}
		foreach (ClassAbilityUnlocks classAbility in rogueClass.classAbilitiesOffensiveList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 4);
		}
		foreach (ClassAbilityUnlocks classAbility in rogueClass.classAbilitiesEffectsList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 4);
		}
		foreach (ClassAbilityUnlocks classAbility in rogueClass.classAbilitiesHealingList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, rogueAbilityContent.transform, 4);
		}
	}
	private void SetUpRangerClassTree()
	{
		foreach (ClassStatUnlocks classStatUnlock in rangerClass.classStatBonusList)
		{
			if (classStatUnlock.LevelRequirement == 1)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 0);
			if (classStatUnlock.LevelRequirement == 3)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 1);
			if (classStatUnlock.LevelRequirement == 6)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 2);
			if (classStatUnlock.LevelRequirement == 9)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 3);
			if (classStatUnlock.LevelRequirement == 12)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 4);
			if (classStatUnlock.LevelRequirement == 15)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 5);
			if (classStatUnlock.LevelRequirement == 18)
				CreateNewUnlockNode(classStatUnlock, null, rangerStatBonusContent.transform, 6);
		}
		foreach (ClassAbilityUnlocks classAbility in rangerClass.classAbilitiesOffensiveList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 4);
		}
		foreach (ClassAbilityUnlocks classAbility in rangerClass.classAbilitiesEffectsList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 4);
		}
		foreach (ClassAbilityUnlocks classAbility in rangerClass.classAbilitiesHealingList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, rangerAbilityContent.transform, 4);
		}
	}
	private void SetUpMageClassTree()
	{
		foreach (ClassStatUnlocks classStatUnlock in mageClass.classStatBonusList)
		{
			if (classStatUnlock.LevelRequirement == 1)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 0);
			if (classStatUnlock.LevelRequirement == 3)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 1);
			if (classStatUnlock.LevelRequirement == 6)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 2);
			if (classStatUnlock.LevelRequirement == 9)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 3);
			if (classStatUnlock.LevelRequirement == 12)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 4);
			if (classStatUnlock.LevelRequirement == 15)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 5);
			if (classStatUnlock.LevelRequirement == 18)
				CreateNewUnlockNode(classStatUnlock, null, mageStatBonusContent.transform, 6);
		}
		foreach (ClassAbilityUnlocks classAbility in mageClass.classAbilitiesOffensiveList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 4);
		}
		foreach (ClassAbilityUnlocks classAbility in mageClass.classAbilitiesEffectsList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 4);
		}
		foreach (ClassAbilityUnlocks classAbility in mageClass.classAbilitiesHealingList)
		{
			if (classAbility.LevelRequirement == 1)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 0);
			if (classAbility.LevelRequirement == 4)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 1);
			if (classAbility.LevelRequirement == 8)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 2);
			if (classAbility.LevelRequirement == 12)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 3);
			if (classAbility.LevelRequirement == 16)
				CreateNewUnlockNode(null, classAbility, mageAbilityContent.transform, 4);
		}
	}
	private void CreateNewUnlockNode(ClassStatUnlocks statUnlock, ClassAbilityUnlocks abilityUnlock, 
		Transform parent, int verticalParentIndex)
	{
		GameObject go = Instantiate(classNodeSlotsPrefab, GrabParentTransformFromUi(parent, verticalParentIndex));
		ClassTreeNodeUi nodeSlotUi = go.GetComponent<ClassTreeNodeUi>();
		nodeSlotUi.statUnlock = statUnlock;
		nodeSlotUi.abilityUnlock = abilityUnlock;
		nodeSlotUi.Initilize(verticalParentIndex);
		nodeSlotUiList.Add(nodeSlotUi);
	}
	private Transform GrabParentTransformFromUi(Transform parent, int verticalParentIndex)
	{
		return parent.GetChild(0).transform.GetChild(verticalParentIndex).transform.GetChild(0).transform;
	}

	//reload player class + unlocked nodes
	public void ReloadPlayerClass()
	{
		if (SaveManager.Instance.GameData.currentPlayerClass == null) return;
		SetPlayerClass(SaveManager.Instance.GameData.currentPlayerClass, false);

		if (currentPlayerClass == knightClass)
			ReloadPlayerClassTreeNodes(knightStatBonusContent.transform, knightAbilityContent.transform);
		if (currentPlayerClass == warriorClass)
			ReloadPlayerClassTreeNodes(warriorStatBonusContent.transform, warriorAbilityContent.transform);
		if (currentPlayerClass == rogueClass)
			ReloadPlayerClassTreeNodes(rogueStatBonusContent.transform, rogueAbilityContent.transform);
		if (currentPlayerClass == rangerClass)
			ReloadPlayerClassTreeNodes(rangerStatBonusContent.transform, rangerAbilityContent.transform);
		if (currentPlayerClass == mageClass)
			ReloadPlayerClassTreeNodes(mageStatBonusContent.transform, mageAbilityContent.transform);

		HidePlayerClassSelection();
	}
	private void ReloadPlayerClassTreeNodes(Transform statBonuesTransform, Transform abilitiesTransform)
	{
		List<ClassTreeNodeData> nodeIndexes = SaveManager.Instance.GameData.unlockedClassNodeIndexesList;
		foreach (ClassTreeNodeData nodeData in nodeIndexes)
		{
			if (nodeData.isStatBoost)
				RestorePlayerNodes(statBonuesTransform, nodeData.nodeVerticalParentIndex, nodeData.nodeHorizontalIndex);
			else
				RestorePlayerNodes(abilitiesTransform, nodeData.nodeVerticalParentIndex, nodeData.nodeHorizontalIndex);
		}
	}
	private void RestorePlayerNodes(Transform parent, int verticalParentIndex, int nodeHorizontalIndex)
	{
		ClassTreeNodeUi nodeSlotUi = parent.GetChild(0).transform.GetChild(verticalParentIndex).transform.GetChild(0).
			transform.GetChild(nodeHorizontalIndex).GetComponent<ClassTreeNodeUi>();

		nodeSlotUi.UnlockThisNode();
	}

	//set player class
	private void SetPlayerClass(SOClasses newClass, bool displayClassSkillTree)
	{
		if (GameManager.isNewGame && currentPlayerClass == null)
			SaveManager.Instance.GameData.hasRecievedStartingItems = false;

		currentPlayerClass = newClass;
		UpdatePlayerClass();

		if (displayClassSkillTree)
			PlayerEventManager.ShowPlayerSkillTree();
	}
	//handle resetting/swapping class by refunding unlocked nodes
	private void UpdatePlayerClass()
	{
		for (int i = currentUnlockedClassNodes.Count - 1; i >= 0; i--)
			currentUnlockedClassNodes[i].RefundThisNode();

		UpdateMaxAbilitySlots(GameManager.Localplayer.playerStats);
		currentUnlockedClassNodes.Clear();

		OnClassChanges?.Invoke(currentPlayerClass);
	}

	//skill tree node event calls
	public void UnlockStatBonus(ClassTreeNodeUi classTreeSlot, SOClassStatBonuses statBonus)
	{
		classTreeSlot.isAlreadyUnlocked = true;

		currentUnlockedClassNodes.Add(classTreeSlot);
		OnNewStatBonusUnlock?.Invoke(statBonus);
	}
	public void UnlockAbility(ClassTreeNodeUi classTreeSlot, SOAbilities ability)
	{
		if (!DoesPlayerHaveFreeAbilitySlot())
		{
			Debug.Log("player has no spare ability Slots");
			return;
		}

		abilitySlotsUsed++;
		classTreeSlot.isAlreadyUnlocked = true;
		UpdateMaxAbilitySlots(GameManager.Localplayer.playerStats);

		currentUnlockedClassNodes.Add(classTreeSlot);
		OnNewAbilityUnlock?.Invoke(ability);
	}
	public void RefundStatBonus(ClassTreeNodeUi classTreeSlot, SOClassStatBonuses statBonus)
	{
		classTreeSlot.isAlreadyUnlocked = false;

		currentUnlockedClassNodes.Remove(classTreeSlot);
		OnRefundStatBonusUnlock?.Invoke(statBonus);
	}
	public void RefundAbility(ClassTreeNodeUi classTreeSlot, SOAbilities ability)
	{
		abilitySlotsUsed--;
		classTreeSlot.isAlreadyUnlocked = false;
		UpdateMaxAbilitySlots(GameManager.Localplayer.playerStats);

		currentUnlockedClassNodes.Remove(classTreeSlot);
		OnRefundAbilityUnlock?.Invoke(ability);
	}
	public void UpdateNodesInClassTree(EntityStats playerStats)
	{
		OnClassNodeUnlocks?.Invoke(playerStats);
	}

	//ability slots tracking
	private void UpdateMaxAbilitySlots(EntityStats playerStats)
	{
		if (currentPlayerClass == null) return;

		maxAbilitySlots = currentPlayerClass.baseClassAbilitySlots;

		foreach (AbilitySlots abilitySlot in currentPlayerClass.spellSlotsPerLevel)
		{
			if (playerStats.entityLevel >= abilitySlot.LevelRequirement)
				maxAbilitySlots += abilitySlot.AbilitySlotsPerLevel;
		}
		UpdateAbilitySlotsCounterUi();
	}
	public bool DoesPlayerHaveFreeAbilitySlot()
	{
		if (maxAbilitySlots == abilitySlotsUsed)
			return false;
		else return true;
	}
	private void UpdateAbilitySlotsCounterUi()
	{
		abilitySlotsCounterText.text = $"{abilitySlotsUsed} / {maxAbilitySlots}";
	}

	//UI CHANGES
	//classes changes
	public void PlayAsKnightButton()
	{
		SetPlayerClass(knightClass, true);
	}
	public void PlayAsWarriorButton()
	{
		SetPlayerClass(warriorClass, true);
	}
	public void PlayAsRogueButton()
	{
		SetPlayerClass(rogueClass, true);
	}
	public void PlayAsRangerButton()
	{
		SetPlayerClass(rangerClass, true);
	}
	public void PlayAsMageButton()
	{
		SetPlayerClass(mageClass, true);
	}

	//class selection
	public void ShowPlayerClassSelection()
	{
		if (playerClassSelectionPanel.activeInHierarchy)
			HidePlayerClassSelection();
		else
		{
			ClassSelectionUiPanel.SetActive(true);
			playerClassSelectionPanel.SetActive(true);

			if (GameManager.isNewGame)
				closeplayerClassSelectionButton.SetActive(false);
			else
				closeplayerClassSelectionButton.SetActive(true);
		}
	}
	public void HidePlayerClassSelection()
	{
		ClassSelectionUiPanel.SetActive(false);
		playerClassSelectionPanel.SetActive(false);
	}

	//class skill trees
	public void ShowClassSkillTree()
	{
		if (currentPlayerClass == null) return;

		if (SharedClassUiElements.activeInHierarchy)
			HideClassSkillTree();
		else
		{
			ClassTreesUiPanel.SetActive(true);
			SharedClassUiElements.SetActive(true);

			if (currentPlayerClass == knightClass)
				knightClassPanel.SetActive(true);
			if (currentPlayerClass == warriorClass)
				warriorClassPanel.SetActive(true);
			if (currentPlayerClass == rogueClass)
				rogueClassPanel.SetActive(true);
			if (currentPlayerClass == rangerClass)
				rangerClassPanel.SetActive(true);
			if (currentPlayerClass == mageClass)
				MageClassPanel.SetActive(true);

			UpdateToolTipsForClassNodes();
			UpdateNodesInClassTree(GameManager.Localplayer.playerStats);
		}
	}
	public void HideClassSkillTree()
	{
		ClassTreesUiPanel.SetActive(false);
		SharedClassUiElements.SetActive(false);
		knightClassPanel.SetActive(false);
		warriorClassPanel.SetActive(false);
		rogueClassPanel.SetActive(false);
		rangerClassPanel.SetActive(false);
		MageClassPanel.SetActive(false);
	}

	//tool tips
	private void UpdateToolTipsForClassNodes()
	{
		foreach (ClassTreeNodeUi node in nodeSlotUiList)
		{
			if (node.GetComponent<ToolTipUi>() == null) continue;
			node.SetToolTip(GameManager.Localplayer.playerStats);
		}
	}
}
