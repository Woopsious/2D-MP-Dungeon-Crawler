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
	public List<QuestSlotsUi> activeQuests = new List<QuestSlotsUi>();

	[Header("Interacted Npc Ui")]
	public GameObject npcJournalPanalUi;
	public GameObject avalableQuestContainer;
	public Button refreshQuestsButton;
	public Button closeQuestsButton;

	public static event Action<QuestSlotsUi> OnNewQuestAccepted;
	public static event Action<QuestSlotsUi> OnQuestComplete;
	public static event Action<QuestSlotsUi> OnQuestAbandon;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadActiveQuests;

		EventManager.OnShowPlayerInventoryEvent += HidePlayerJournal;
		EventManager.OnShowPlayerClassSelectionEvent += HidePlayerJournal;
		EventManager.OnShowPlayerSkillTreeEvent += HidePlayerJournal;
		EventManager.OnShowPlayerLearntAbilitiesEvent += HidePlayerJournal;
		EventManager.OnShowPlayerJournalEvent += ShowPlayerJournal;

		EventManager.OnDeathEvent += OnEntityDeathUpdateKillQuests;
		EventManager.OnShowNpcJournal += ShowNpcJournal;
		EventManager.OnHideNpcJournal += HideNpcJournal;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadActiveQuests;

		EventManager.OnShowPlayerInventoryEvent -= HidePlayerJournal;
		EventManager.OnShowPlayerClassSelectionEvent -= HidePlayerJournal;
		EventManager.OnShowPlayerSkillTreeEvent -= HidePlayerJournal;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= HidePlayerJournal;
		EventManager.OnShowPlayerJournalEvent -= ShowPlayerJournal;

		EventManager.OnDeathEvent += OnEntityDeathUpdateKillQuests;
		EventManager.OnShowNpcJournal -= ShowNpcJournal;
		EventManager.OnHideNpcJournal -= HideNpcJournal;
	}

	public void AcceptQuest(QuestSlotsUi quest)
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
	public void CompleteQuest(QuestSlotsUi quest)
	{
		activeQuests.Remove(quest);
		Destroy(quest.gameObject);

		OnQuestComplete?.Invoke(quest);
		UpdateActiveQuestTracker();
	}
	public void AbandonQuest(QuestSlotsUi quest)
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
			QuestSlotsUi quest = go.GetComponent<QuestSlotsUi>();

			quest.isCurrentlyActiveQuest = questData[i].isCurrentlyActiveQuest;
			quest.questType = (QuestSlotsUi.QuestType)questData[i].questType;
			quest.amount = questData[i].amount;
			quest.currentAmount = questData[i].currentAmount;
			quest.entityToKill = questData[i].entityToKill;
			quest.weaponToHandIn = questData[i].weaponToHandIn;
			quest.armorToHandIn = questData[i].armorToHandIn;
			quest.accessoryToHandIn = questData[i].accessoryToHandIn;
			quest.consumableToHandIn = questData[i].consumableToHandIn;
			quest.itemTypeToHandIn = (QuestSlotsUi.ItemType)questData[i].itemTypeToHandIn;
			quest.questRewardType = (QuestSlotsUi.RewardType)questData[i].questRewardType;
			quest.rewardToAdd = questData[i].rewardToAdd;

			quest.InitilizeText();
			quest.AcceptThisQuest();
		}
		UpdateActiveQuestTracker();
	}
	private void OnEntityDeathUpdateKillQuests(GameObject obj)
	{
		foreach (QuestSlotsUi quest in activeQuests)
		{
			if (quest.questType == QuestSlotsUi.QuestType.isItemHandInQuest) continue;

			if (quest.entityToKill = obj.GetComponent<EntityStats>().entityBaseStats)
				quest.currentAmount++;

			quest.questTrackerUi.text = $"{quest.currentAmount} / {quest.amount} Killed";

			if (quest.currentAmount >= quest.amount)
				quest.CompleteThisQuest();
		}
	}
	public void HandInItemQuests(InventorySlotUi slot)
	{
		foreach (QuestSlotsUi quest in activeQuests)
		{
			if (quest.questType != QuestSlotsUi.QuestType.isItemHandInQuest) continue;

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
	public void ShowPlayerJournal()
	{
		if (playerJournalPanalUi.activeInHierarchy)
			HidePlayerJournal();
		else
			playerJournalPanalUi.SetActive(true);
	}
	public void HidePlayerJournal()
	{
		playerJournalPanalUi.SetActive(false);
	}
	private void UpdateActiveQuestTracker()
	{
		activeQuestsTrackerUi.text = $"{activeQuests.Count} / 5 Active Quests";
	}

	//npc Journal
	public void ShowNpcJournal(NpcHandler npc)
	{
		foreach (QuestSlotsUi quest in npc.avalableQuestList)
			quest.transform.SetParent(avalableQuestContainer.transform);

		npcJournalPanalUi.SetActive(true);
	}
	public void HideNpcJournal(NpcHandler npc)
	{
		foreach (QuestSlotsUi quest in npc.avalableQuestList)
			quest.transform.SetParent(npc.npcContainer.transform);

		npcJournalPanalUi.SetActive(false);
		HidePlayerJournal();
	}
}
