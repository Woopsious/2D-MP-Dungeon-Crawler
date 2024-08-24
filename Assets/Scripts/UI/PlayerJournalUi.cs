using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerJournalUi : MonoBehaviour
{
	public static PlayerJournalUi Instance;

	public GameObject questPrefab;

	[Header("Player Journal Ui")]
	public TMP_Text activeQuestsTrackerUi;
	public GameObject playerJournalPanalUi;
	public GameObject activeQuestContainer;
	public List<QuestDataSlotUi> activeQuests = new List<QuestDataSlotUi>();

	[Header("Interacted Npc Ui")]
	public GameObject npcJournalPanalUi;
	public GameObject avalableQuestContainer;
	private NpcHandler interactedQuestNpc;

	public static event Action<QuestDataSlotUi> OnNewQuestAccepted;
	public static event Action<QuestDataSlotUi> OnQuestComplete;
	public static event Action<QuestDataSlotUi> OnQuestAbandon;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadActiveQuests;

		PlayerEventManager.OnShowPlayerInventoryEvent += HidePlayerJournal;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HidePlayerJournal;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HidePlayerJournal;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HidePlayerJournal;
		PlayerEventManager.OnShowPlayerJournalEvent += ShowPlayerJournal;

		DungeonHandler.OnEntityDeathEvent += OnEntityDeathUpdateKillQuests;
		PlayerEventManager.OnShowNpcJournal += ShowAvailableNpcQuests;
		PlayerEventManager.OnHideNpcJournal += HideAvailableNpcQuests;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadActiveQuests;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HidePlayerJournal;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HidePlayerJournal;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HidePlayerJournal;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HidePlayerJournal;
		PlayerEventManager.OnShowPlayerJournalEvent -= ShowPlayerJournal;

		DungeonHandler.OnEntityDeathEvent -= OnEntityDeathUpdateKillQuests;
		PlayerEventManager.OnShowNpcJournal -= ShowAvailableNpcQuests;
		PlayerEventManager.OnHideNpcJournal -= HideAvailableNpcQuests;
	}

	public void AcceptQuest(QuestDataSlotUi quest)
	{
		if (activeQuests.Count >= 5)
		{
			Debug.Log("max of 5 active quests reached");
			return;
		}
		quest.isCurrentlyActiveQuest = true;
		quest.acceptQuestButtonObj.SetActive(false);
		quest.transform.SetParent(activeQuestContainer.transform);
		activeQuests.Add(quest);

		OnNewQuestAccepted?.Invoke(quest);
		UpdateActiveQuestTracker();
	}
	public void CompleteQuest(QuestDataSlotUi quest)
	{
		activeQuests.Remove(quest);
		Destroy(quest.gameObject);

		OnQuestComplete?.Invoke(quest);
		UpdateActiveQuestTracker();
	}
	public void AbandonQuest(QuestDataSlotUi quest)
	{
		activeQuests.Remove(quest);
		Destroy(quest.gameObject);

		OnQuestAbandon?.Invoke(quest);
		UpdateActiveQuestTracker();
	}

	//player quests
	private void ReloadActiveQuests()
	{
		List<QuestItemData> questData = SaveManager.Instance.GameData.activePlayerQuests;
		for (int i = 0; i < questData.Count; i++)
		{
			GameObject go = Instantiate(questPrefab, activeQuestContainer.transform);
			QuestDataSlotUi quest = go.GetComponent<QuestDataSlotUi>();

			quest.isCurrentlyActiveQuest = questData[i].isCurrentlyActiveQuest;
			quest.questType = (QuestDataSlotUi.QuestType)questData[i].questType;
			quest.amount = questData[i].amount;
			quest.currentAmount = questData[i].currentAmount;
			quest.entityToKill = questData[i].entityToKill;
			quest.weaponToHandIn = questData[i].weaponToHandIn;
			quest.armorToHandIn = questData[i].armorToHandIn;
			quest.accessoryToHandIn = questData[i].accessoryToHandIn;
			quest.consumableToHandIn = questData[i].consumableToHandIn;
			quest.itemTypeToHandIn = (QuestDataSlotUi.ItemType)questData[i].itemTypeToHandIn;
			quest.questRewardType = (QuestDataSlotUi.RewardType)questData[i].questRewardType;
			quest.rewardToAdd = questData[i].rewardToAdd;

			quest.InitilizeText();
			quest.AcceptThisQuest();
		}
		UpdateActiveQuestTracker();
	}
	private void OnEntityDeathUpdateKillQuests(GameObject obj)
	{
		foreach (QuestDataSlotUi quest in activeQuests)
		{
			if (quest.questType == QuestDataSlotUi.QuestType.isItemHandInQuest) continue;

			if (quest.entityToKill = obj.GetComponent<EntityStats>().entityBaseStats)
				quest.currentAmount++;

			quest.questTrackerUi.text = $"{quest.currentAmount} / {quest.amount} Killed";

			if (quest.currentAmount >= quest.amount)
				quest.CompleteThisQuest();
		}
	}
	public void HandInItemQuests(InventorySlotDataUi slot)
	{
		foreach (QuestDataSlotUi quest in activeQuests)
		{
			if (quest.questType != QuestDataSlotUi.QuestType.isItemHandInQuest) continue;

			if (quest.DoesHandInItemMatch(slot.itemInSlot))
			{
				quest.currentAmount += 1;
				slot.itemInSlot.DecreaseStackCounter();
			}

			quest.questTrackerUi.text = $"{quest.currentAmount} / {quest.amount} Handed In";

			if (quest.currentAmount >= quest.amount)
				quest.CompleteThisQuest();

			break;
		}
	}

	//UI CHANGES
	//player Journal
	private void ShowPlayerJournal()
	{
		if (playerJournalPanalUi.activeInHierarchy)
			HidePlayerJournal();
		else
			playerJournalPanalUi.SetActive(true);
	}
	private void HidePlayerJournal()
	{
		playerJournalPanalUi.SetActive(false);
	}
	private void UpdateActiveQuestTracker()
	{
		activeQuestsTrackerUi.text = $"{activeQuests.Count} / 5 Active Quests";
	}

	//npc Journal
	private void ShowAvailableNpcQuests(NpcHandler npc)
	{
		foreach (QuestDataSlotUi quest in npc.avalableQuestList)
			quest.transform.SetParent(avalableQuestContainer.transform);

		interactedQuestNpc = npc;
		UpdateActiveQuestTracker();
		npcJournalPanalUi.SetActive(true);
	}
	private void HideAvailableNpcQuests(NpcHandler npc)
	{
		foreach (QuestDataSlotUi quest in npc.avalableQuestList)
			quest.transform.SetParent(npc.npcContainer.transform);

		interactedQuestNpc = null;
		npcJournalPanalUi.SetActive(false);
		HidePlayerJournal();
	}
	public void RefreshAvailableNpcQuestsButton()
	{
		interactedQuestNpc.GenerateNewQuests();
		ShowAvailableNpcQuests(interactedQuestNpc);
	}
	public void HideAvailableNpcQuestsButton()
	{
		HideAvailableNpcQuests(interactedQuestNpc);
	}
}
