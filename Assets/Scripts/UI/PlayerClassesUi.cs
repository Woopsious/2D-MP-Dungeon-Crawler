using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerClassesUi : MonoBehaviour
{
	public static PlayerClassesUi Instance;

	[Header("Class Selection")]
	public GameObject playerClassSelectionPanel;
	public SOClasses currentPlayerClass;
	public List<ClassTreeNodeSlotUi> currentUnlockedClassNodes = new List<ClassTreeNodeSlotUi>();

	[Header("knight Class")]
	public SOClasses knightClass;
	public GameObject knightClassPanel;
	public GameObject knightStatBonusContent;
	public GameObject knightAbilityContent;
	public Button playAsKnightButton;

	[Header("Warrior Class")]
	public SOClasses warriorClass;
	public GameObject warriorClassPanel;
	public GameObject warriorStatBonusContent;
	public GameObject warriorAbilityContent;
	public Button playAsWarriorButton;

	[Header("Rogue Class")]
	public SOClasses rogueClass;
	public GameObject rogueClassPanel;
	public GameObject rogueStatBonusContent;
	public GameObject rogueAbilityContent;
	public Button playAsRogueButton;

	[Header("Ranger Class")]
	public SOClasses rangerClass;
	public GameObject rangerClassPanel;
	public GameObject rangerStatBonusContent;
	public GameObject rangerAbilityContent;
	public Button playAsRangerButton;

	[Header("Mage Class")]
	public SOClasses mageClass;
	public GameObject MageClassPanel;
	public GameObject mageStatBonusContent;
	public GameObject mageAbilityContent;
	public Button playAsMageButton;

	[Header("Shared Class ui elements")]
	public GameObject closeClassTreeButtonObj;
	public GameObject resetPlayerClassButtonObj;
	public int maxAbilitySlots;
	public int abilitySlotsUsed;

	public static event Action<SOClasses> OnClassChanges;
	public static event Action<EntityStats> OnClassNodeUnlocks;

	public static event Action<SOClassStatBonuses> OnNewStatBonusUnlock;
	public static event Action<SOClassAbilities> OnNewAbilityUnlock;

	public static event Action<SOClassStatBonuses> OnRefundStatBonusUnlock;
	public static event Action<SOClassAbilities> OnRefundAbilityUnlock;

	public GameObject classNodeSlotsPrefab;
	private List<ClassTreeNodeSlotUi> nodeSlotUiList = new List<ClassTreeNodeSlotUi>();

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
		SaveManager.RestoreData += ReloadPlayerClass;

		PlayerEventManager.OnShowPlayerInventoryEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += ShowPlayerClassSelection;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerJournalEvent += HidePlayerClassSelection;

		PlayerEventManager.OnShowPlayerInventoryEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += ShowClassSkillTree;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HideClassSkillTree;
		PlayerEventManager.OnShowPlayerJournalEvent += HideClassSkillTree;

		PlayerEventManager.OnPlayerLevelUpEvent += UpdateMaxAbilitySlots;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerClass;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= ShowPlayerClassSelection;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HidePlayerClassSelection;
		PlayerEventManager.OnShowPlayerJournalEvent -= HidePlayerClassSelection;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= ShowClassSkillTree;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HideClassSkillTree;
		PlayerEventManager.OnShowPlayerJournalEvent -= HideClassSkillTree;

		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateMaxAbilitySlots;

		nodeSlotUiList.Clear();
	}

	private void Initilize()
	{
		SetUpKnightClassTree();
		SetUpWarriorClassTree();
		SetUpRogueClassTree();
		SetUpRangerClassTree();
		SetUpMageClassTree();
	}

	/// <summary>
	/// instantiate ClassTreeNodeSlotUi template as children of specific ui elements based on if its a stat bonus/ability and
	/// the level requirment to unlock it, then assign that stat buns/ability to ClassTreeNodeSlotUi ref, and Initilize node.
	/// </summary>
	
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
		ClassTreeNodeSlotUi nodeSlotUi = go.GetComponent<ClassTreeNodeSlotUi>();
		nodeSlotUi.statUnlock = statUnlock;
		nodeSlotUi.abilityUnlock = abilityUnlock;
		nodeSlotUi.Initilize(verticalParentIndex);
		nodeSlotUiList.Add(nodeSlotUi);
	}
	private Transform GrabParentTransformFromUi(Transform parent, int verticalParentIndex)
	{
		return parent.GetChild(0).transform.GetChild(verticalParentIndex).transform.GetChild(0).transform;
	}

	//reload player class
	/// <summary>
	/// for updating reloading of player class once rest of the code is finished.
	/// have parent transform eg: MageStatBonusContent, save indexes of vertical child objs (what level/grade stat bonuses/abilities are)
	/// then horizontal indexs for ClassTreeNodeSlotUi , based on those indexes force unlock these nodes
	/// similar to how i set up Class Tree nodes.
	/// </summary>
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
		ClassTreeNodeSlotUi nodeSlotUi = parent.GetChild(0).transform.GetChild(verticalParentIndex).transform.GetChild(0).
			transform.GetChild(nodeHorizontalIndex).GetComponent<ClassTreeNodeSlotUi>();

		nodeSlotUi.UnlockThisNode();
	}

	//change of classes calls reset class 
	private void SetPlayerClass(SOClasses newClass, bool displayClassSkillTree)
	{
		if (GameManager.isNewGame && currentPlayerClass == null)
			SaveManager.Instance.GameData.hasRecievedStartingItems = false;

		currentPlayerClass = newClass;
		UpdatePlayerClass();

		if (displayClassSkillTree)
			PlayerEventManager.ShowPlayerSkillTree();
	}
	public void UpdatePlayerClass()
	{
		for (int i = currentUnlockedClassNodes.Count - 1; i >= 0; i--)
			currentUnlockedClassNodes[i].RefundThisNode();

		UpdateMaxAbilitySlots(PlayerInfoUi.playerInstance.playerStats);
		currentUnlockedClassNodes.Clear();

		OnClassChanges?.Invoke(currentPlayerClass);
	}

	//skill tree node event calls
	public void UnlockStatBonus(ClassTreeNodeSlotUi classTreeSlot, SOClassStatBonuses statBonus)
	{
		classTreeSlot.isAlreadyUnlocked = true;

		currentUnlockedClassNodes.Add(classTreeSlot);
		OnNewStatBonusUnlock?.Invoke(statBonus);
	}
	public void UnlockAbility(ClassTreeNodeSlotUi classTreeSlot, SOClassAbilities ability)
	{
		if (!DoesPlayerHaveFreeAbilitySlot())
		{
			Debug.Log("player has no spare ability Slots");
			return;
		}

		abilitySlotsUsed++;
		classTreeSlot.isAlreadyUnlocked = true;
		UpdateMaxAbilitySlots(PlayerInfoUi.playerInstance.playerStats);

		currentUnlockedClassNodes.Add(classTreeSlot);
		OnNewAbilityUnlock?.Invoke(ability);
	}
	public void RefundStatBonus(ClassTreeNodeSlotUi classTreeSlot, SOClassStatBonuses statBonus)
	{
		classTreeSlot.isAlreadyUnlocked = false;

		currentUnlockedClassNodes.Remove(classTreeSlot);
		OnRefundStatBonusUnlock?.Invoke(statBonus);
	}
	public void RefundAbility(ClassTreeNodeSlotUi classTreeSlot, SOClassAbilities ability)
	{
		abilitySlotsUsed--;
		classTreeSlot.isAlreadyUnlocked = false;
		UpdateMaxAbilitySlots(PlayerInfoUi.playerInstance.playerStats);

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

		Debug.Log("updating abiloty slots");

		maxAbilitySlots = currentPlayerClass.baseClassAbilitySlots;

		foreach (SpellSlots spellSlot in currentPlayerClass.spellSlotsPerLevel)
		{
			if (playerStats.entityLevel >= spellSlot.LevelRequirement)
				maxAbilitySlots += spellSlot.SpellSlotsPerLevel;
		}
	}
	public bool DoesPlayerHaveFreeAbilitySlot()
	{
		if (maxAbilitySlots == abilitySlotsUsed)
			return false;
		else return true;
	}

	//UI CHANGES
	//classes
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
	public void ShowHidePlayerClassSelection()
	{
		if (playerClassSelectionPanel.activeInHierarchy)
			HidePlayerClassSelection();
		else
			ShowPlayerClassSelection();
	}
	public void ShowPlayerClassSelection()
	{
		if (playerClassSelectionPanel.activeInHierarchy)
			HidePlayerClassSelection();
		else
			playerClassSelectionPanel.SetActive(true);
	}
	public void HidePlayerClassSelection()
	{
		playerClassSelectionPanel.SetActive(false);
	}

	//class skill trees
	public void ShowHideClassSkillTree()
	{
		if (closeClassTreeButtonObj.activeInHierarchy)
			HideClassSkillTree();
		else
			ShowClassSkillTree();
	}
	public void ShowClassSkillTree()
	{
		if (currentPlayerClass == null) return;

		if (closeClassTreeButtonObj.activeInHierarchy)
			HideClassSkillTree();
		else
		{
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
			UpdateNodesInClassTree(PlayerInfoUi.playerInstance.playerStats);

			closeClassTreeButtonObj.SetActive(true);
			resetPlayerClassButtonObj.SetActive(true);
		}
	}
	public void HideClassSkillTree()
	{
		closeClassTreeButtonObj.SetActive(false);
		resetPlayerClassButtonObj.SetActive(false);
		knightClassPanel.SetActive(false);
		warriorClassPanel.SetActive(false);
		rogueClassPanel.SetActive(false);
		rangerClassPanel.SetActive(false);
		MageClassPanel.SetActive(false);
	}

	//tool tips
	private void UpdateToolTipsForClassNodes()
	{
		foreach (ClassTreeNodeSlotUi node in nodeSlotUiList)
		{
			if (node.GetComponent<ToolTipUi>() == null) continue;
			ToolTipUi tip = node.GetComponent<ToolTipUi>();
			tip.UpdatePlayerToolTip();
		}
	}
}
