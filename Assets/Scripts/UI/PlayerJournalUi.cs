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
	public List<QuestDataUi> activeQuests = new List<QuestDataUi>();

	[Header("Interacted Npc Ui")]
	public GameObject npcJournalPanalUi;
	public GameObject avalableQuestContainer;
	private NpcHandler interactedQuestNpc;

	public static event Action<QuestDataUi> OnNewQuestAccepted;
	public static event Action<QuestDataUi> OnQuestComplete;
	public static event Action<QuestDataUi> OnQuestAbandon;

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
		PlayerEventManager.OnShowPlayerDeathUiEvent += HidePlayerJournal;

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
		PlayerEventManager.OnShowPlayerDeathUiEvent += HidePlayerJournal;

		DungeonHandler.OnEntityDeathEvent -= OnEntityDeathUpdateKillQuests;
		PlayerEventManager.OnShowNpcJournal -= ShowAvailableNpcQuests;
		PlayerEventManager.OnHideNpcJournal -= HideAvailableNpcQuests;
	}

	//quest actions
	public void AcceptQuest(QuestDataUi quest)
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
	public void CompleteQuest(QuestDataUi quest)
	{
		activeQuests.Remove(quest);
		Destroy(quest.gameObject);

		OnQuestComplete?.Invoke(quest);
		UpdateActiveQuestTracker();
	}
	public void AbandonQuest(QuestDataUi quest)
	{
		activeQuests.Remove(quest);
		Destroy(quest.gameObject);

		OnQuestAbandon?.Invoke(quest);
		UpdateActiveQuestTracker();
	}

	//restore player quests data
	private void ReloadActiveQuests()
	{
		List<QuestItemData> questData = SaveManager.Instance.GameData.activePlayerQuests;
		for (int i = 0; i < questData.Count; i++)
		{
			GameObject go = Instantiate(questPrefab, activeQuestContainer.transform);
			QuestDataUi quest = go.GetComponent<QuestDataUi>();

			quest.isCurrentlyActiveQuest = questData[i].isCurrentlyActiveQuest;
			quest.questType = (QuestDataUi.QuestType)questData[i].questType;
			quest.amount = questData[i].amount;
			quest.currentAmount = questData[i].currentAmount;
			quest.entityToKill = questData[i].entityToKill;
			quest.weaponToHandIn = questData[i].weaponToHandIn;
			quest.armorToHandIn = questData[i].armorToHandIn;
			quest.accessoryToHandIn = questData[i].accessoryToHandIn;
			quest.consumableToHandIn = questData[i].consumableToHandIn;
			quest.itemTypeToHandIn = (QuestDataUi.ItemType)questData[i].itemTypeToHandIn;
			quest.questRewardType = (QuestDataUi.RewardType)questData[i].questRewardType;
			quest.rewardToAdd = questData[i].rewardToAdd;

			quest.InitilizeText();
			quest.AcceptThisQuest();
		}
		UpdateActiveQuestTracker();
	}
	private void OnEntityDeathUpdateKillQuests(GameObject obj)
	{
		foreach (QuestDataUi quest in activeQuests)
		{
			if (quest.questType == QuestDataUi.QuestType.isItemHandInQuest) continue;

			if (quest.entityToKill = obj.GetComponent<EntityStats>().statsRef)
				quest.currentAmount++;

			quest.questTrackerUi.text = $"{quest.currentAmount} / {quest.amount} Killed";

			if (quest.currentAmount >= quest.amount)
				quest.CompleteThisQuest();
		}
	}
	public void HandInItemQuests(InventorySlotDataUi slot)
	{
		foreach (QuestDataUi quest in activeQuests)
		{
			if (quest.questType != QuestDataUi.QuestType.isItemHandInQuest) continue;

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
	//player Journal ui
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

	//npc Journal ui
	private void ShowAvailableNpcQuests(NpcHandler npc)
	{
		foreach (QuestDataUi quest in npc.avalableQuestList)
			quest.transform.SetParent(avalableQuestContainer.transform);

		interactedQuestNpc = npc;
		UpdateActiveQuestTracker();
		npcJournalPanalUi.SetActive(true);
	}
	private void HideAvailableNpcQuests(NpcHandler npc)
	{
		foreach (QuestDataUi quest in npc.avalableQuestList)
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
