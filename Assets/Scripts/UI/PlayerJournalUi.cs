using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJournalUi : MonoBehaviour
{
	public static PlayerJournalUi Instance;

	public GameObject PlayerJournalPanalUi;
	public GameObject activeQuestContainer;

	public GameObject NPCJournalPanalUi;
	public GameObject avalableQuestContainer;

	public GameObject questPrefab;

	public List<QuestSlotsUi> activeQuests = new List<QuestSlotsUi>();

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerBounties;
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerBounties;
	}

	private void ReloadPlayerBounties()
	{

	}

	//UI CHANGES
	//player Journal
	public void ShowHidePlayerJournal()
	{
		if (PlayerJournalPanalUi.activeInHierarchy)
			HidePlayerJournal();
		else
			ShowPlayerJournal();
	}
	public void ShowPlayerJournal()
	{
		PlayerJournalPanalUi.SetActive(true);

		PlayerInventoryUi.Instance.HideInventory();
		PlayerInventoryUi.Instance.HideLearntAbilities();
		ClassesUi.Instance.HidePlayerClassSelection();
		ClassesUi.Instance.HideClassSkillTree();
	}
	public void HidePlayerJournal()
	{
		PlayerJournalPanalUi.SetActive(false);
	}
}
