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
		SaveManager.OnGameLoad += ReloadPlayerBounties;

		EventManagerUi.OnShowPlayerInventoryEvent += HidePlayerJournal;
		EventManagerUi.OnShowPlayerClassSelectionEvent += HidePlayerJournal;
		EventManagerUi.OnShowPlayerSkillTreeEvent += HidePlayerJournal;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent += HidePlayerJournal;
		EventManagerUi.OnShowPlayerJournalEvent += ShowPlayerJournal;
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerBounties;

		EventManagerUi.OnShowPlayerInventoryEvent -= HidePlayerJournal;
		EventManagerUi.OnShowPlayerClassSelectionEvent -= HidePlayerJournal;
		EventManagerUi.OnShowPlayerSkillTreeEvent -= HidePlayerJournal;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent -= HidePlayerJournal;
		EventManagerUi.OnShowPlayerJournalEvent -= ShowPlayerJournal;
	}

	private void ReloadPlayerBounties()
	{
		UpdateActiveQuestTracker();
	}

	public void AcceptQuest(QuestSlotsUi quest)
	{
		if (activeQuests.Count >= 5)
		{
			Debug.Log("max of 5 active quests reached");
			return;
		}

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

	//UI CHANGES
	//player Journal
	public void ShowHidePlayerJournal()
	{
		if (playerJournalPanalUi.activeInHierarchy)
			HidePlayerJournal();
		else
			ShowPlayerJournal();
	}
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
	public void ShowNpcJournal()
	{
		npcJournalPanalUi.SetActive(true);
	}
	public void HideNpcJournal()
	{
		npcJournalPanalUi.SetActive(false);
	}
}
