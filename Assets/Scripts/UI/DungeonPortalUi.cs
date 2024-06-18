using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonPortalUi : MonoBehaviour
{
	public static DungeonPortalUi instance;

	[Header("Dungeon Panel")]
	public GameObject portalPanelUi;
	public GameObject dungeonInfoSlotPrefab;
	public TMP_Text dungeonListInfoText;

	[Header("Dungeon Enterence Ui")]
	public GameObject portalEnterenceUi;
	public GameObject activeDungeonListPanel;
	public GameObject activeDungeonListContent;
	public GameObject savedDungeonListPanel;
	public GameObject savedDungeonListContent;

	public List<DungeonDataSlotUi> activeDungeonLists = new List<DungeonDataSlotUi>();
	public List<DungeonDataSlotUi> savedDungeonLists = new List<DungeonDataSlotUi>();

	[Header("Dungeon Exit Ui")]
	public GameObject portalExitUi;

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadSavedDungeons;
		EventManager.OnShowPortalUi += ShowPortalUi;
		EventManager.OnHidePortalUi += HidePortalUi;

		DungeonDataSlotUi.OnDungeonSave += OnSaveDungeon;
		DungeonDataSlotUi.OnDungeonDelete += OnDeleteDungeon;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadSavedDungeons;
		EventManager.OnShowPortalUi -= ShowPortalUi;
		EventManager.OnHidePortalUi -= HidePortalUi;

		DungeonDataSlotUi.OnDungeonSave -= OnSaveDungeon;
		DungeonDataSlotUi.OnDungeonDelete -= OnDeleteDungeon;
	}

	private void GenerateNewDungeons()
	{
		if (GameManager.Instance == null) return; //disables for test scene
		if (!Utilities.GetCurrentlyActiveScene(GameManager.Instance.hubAreaName)) return; //if not hub area return

		activeDungeonLists.Clear();

		for (int i = 0; i < 5; i++)
		{
			GameObject go = Instantiate(dungeonInfoSlotPrefab, activeDungeonListContent.transform);
			DungeonDataSlotUi dungeonSlot = go.GetComponent<DungeonDataSlotUi>();
			dungeonSlot.Initilize(i);
			activeDungeonLists.Add(dungeonSlot);
		}
	}	

	//button actions
	public void GenerateNewRandomDungeonsButton()
	{
		for (int i = activeDungeonLists.Count - 1; i >= 0; i--)
			activeDungeonLists[i].DeleteDungeon();

		GenerateNewDungeons();
	}
	public void ReturnToHubButton()
	{
		List<DungeonChestData> chestData = new List<DungeonChestData>();

		foreach (ChestHandler chest in DungeonHandler.Instance.dungeonChestList)
		{
			DungeonChestData data = new DungeonChestData()
			{
				chestActive = chest.chestActive,
				chestStateOpened = chest.chestStateOpened,
			};
			chestData.Add(data);
		}

		GameManager.Instance.currentDungeonData.dungeonChestData = chestData;
		//save data to corrisponding DungeonSlotUi, where SaveManager will then save/reload all DungeonSlotUi's
		if (GameManager.Instance.currentDungeonData.isDungeonSaved)
			savedDungeonLists[GameManager.Instance.currentDungeonData.dungeonIndex].dungeonChestData = chestData;
		else
			activeDungeonLists[GameManager.Instance.currentDungeonData.dungeonIndex].dungeonChestData = chestData;

		SaveManager.Instance.AutoSaveData();
		GameManager.Instance.LoadHubArea(false);
	}

	//Events
	private void ReloadSavedDungeons()
	{
		for (int i = 0; i < SaveManager.Instance.GameData.savedDungeonsList.Count; i++)
		{
			GameObject go = Instantiate(dungeonInfoSlotPrefab, savedDungeonListContent.transform);
			DungeonDataSlotUi dungeonSlot = go.GetComponent<DungeonDataSlotUi>();
			dungeonSlot.Initilize(SaveManager.Instance.GameData.savedDungeonsList[i], i);
			activeDungeonLists.Add(dungeonSlot);
			OnSaveDungeon(dungeonSlot);
		}

		for (int i = 0; i < SaveManager.Instance.GameData.activeDungeonsList.Count; i++)
		{
			GameObject go = Instantiate(dungeonInfoSlotPrefab, activeDungeonListContent.transform);
			DungeonDataSlotUi dungeonSlot = go.GetComponent<DungeonDataSlotUi>();
			dungeonSlot.Initilize(SaveManager.Instance.GameData.activeDungeonsList[i], i);
			activeDungeonLists.Add(dungeonSlot);
		}
	}
	private void OnSaveDungeon(DungeonDataSlotUi dungeonSlot)
	{
		dungeonSlot.transform.SetParent(savedDungeonListContent.transform);
		dungeonSlot.saveDungeonButtonObj.SetActive(false);
		dungeonSlot.deleteDungeonButtonObj.SetActive(true);
		dungeonSlot.dungeonIndex = savedDungeonLists.Count;

		activeDungeonLists.Remove(dungeonSlot);
		savedDungeonLists.Add(dungeonSlot);
	}
	private void OnDeleteDungeon(DungeonDataSlotUi dungeonSlot)
	{
		activeDungeonLists.Remove(dungeonSlot);
		savedDungeonLists.Remove(dungeonSlot);

		Destroy(dungeonSlot.gameObject);
	}

	//UI CHANGES
	public void ShowPortalUi(PortalHandler portal)
	{
		if (portalPanelUi.activeInHierarchy)
			HidePortalUi();
		else
		{
			portalPanelUi.SetActive(true);
			portalEnterenceUi.SetActive(false);
			portalExitUi.SetActive(false);

			if (portal.isDungeonEnterencePortal)
			{
				portalEnterenceUi.SetActive(true);
				ShowActiveDungeonListUi();
			}
			else
				portalExitUi.SetActive(true);
		}
	}
	public void ShowActiveDungeonListUi() //button click
	{
		dungeonListInfoText.text = "Currently Active Dungeons";
		activeDungeonListPanel.SetActive(true);
		savedDungeonListPanel.SetActive(false);
	}
	public void ShowSavedDungeonListUi() //button click
	{
		dungeonListInfoText.text = "Currently Saved Dungeons";
		activeDungeonListPanel.SetActive(false);
		savedDungeonListPanel.SetActive(true);
	}
	public void HidePortalUi()
	{
		portalPanelUi.SetActive(false);
		portalEnterenceUi.SetActive(false);
		portalExitUi.SetActive(false);
	}
}
