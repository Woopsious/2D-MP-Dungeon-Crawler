using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonPortalUi : MonoBehaviour
{
	public static DungeonPortalUi instance;

	[Header("Dungeon Panel")]
	public GameObject portalPanelUi;
	public GameObject dungeonInfoSlotPrefab;
	public TMP_Text dungeonListInfoText;

	[Header("Dungeon lists")]
	public List<DungeonDataUi> activeDungeonLists = new List<DungeonDataUi>();
	public List<DungeonDataUi> savedDungeonLists = new List<DungeonDataUi>();
	public List<DungeonDataUi> bossDungeonLists = new List<DungeonDataUi>();

	[Header("Dungeon Bosses")]
	public List<SOEntityStats> bossesInGame = new List<SOEntityStats>();

	[Header("Shared Dungeon list Ui")]
	public GameObject dungeonListContent;
	public GameObject hiddenDungeonsParentObj;

	public DungeonListTypeToShow dungeonListTypeToShow;
	public enum DungeonListTypeToShow
	{
		activeDungeons, savedDungeons, bossDungeons
	}

	[Header("Dungeon Enterence Ui")]
	public GameObject dungeonEnterenceUi;

	[Header("Dungeon Exit Ui")]
	public GameObject dungeonExitUi;

	private void Awake()
	{
		instance = this;
		GenerateBossDungeonsOnAwake();
	}
	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadSavedDungeons;
		PlayerEventManager.OnShowPortalUi += ShowPortalUi;
		PlayerEventManager.OnHidePortalUi += HidePortalUi;

		SceneManager.sceneLoaded += HidePortalUiOnSceneChange;

		DungeonDataUi.OnDungeonSave += OnSaveDungeon;
		DungeonDataUi.OnDungeonDelete += OnDeleteDungeon;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadSavedDungeons;
		PlayerEventManager.OnShowPortalUi -= ShowPortalUi;
		PlayerEventManager.OnHidePortalUi -= HidePortalUi;

		SceneManager.sceneLoaded -= HidePortalUiOnSceneChange;

		DungeonDataUi.OnDungeonSave -= OnSaveDungeon;
		DungeonDataUi.OnDungeonDelete -= OnDeleteDungeon;
	}

	private void GenerateBossDungeonsOnAwake()
	{
		//if (GameManager.Instance == null) return; //disables for test scene
		if (!Utilities.SceneIsActive("HubArea")) return;

		for (int i = 0; i < bossesInGame.Count; i++) //generate dungond for each boss
		{
			Transform parentTransform;
			if (dungeonListTypeToShow == DungeonListTypeToShow.bossDungeons)
				parentTransform = dungeonListContent.transform;
			else parentTransform = hiddenDungeonsParentObj.transform;

			for (int j = 0; j < 3; j++) //generate dungeon for every difficulty per boss
			{
				GameObject go = Instantiate(dungeonInfoSlotPrefab, parentTransform);
				DungeonDataUi dungeonData = go.GetComponent<DungeonDataUi>();
				dungeonData.Initilize(i, j, bossesInGame[i]);
				bossDungeonLists.Add(dungeonData);
			}
		}
	}
	private void GenerateNewDungeons()
	{
		//if (GameManager.Instance == null) return; //disables for test scene
		if (!Utilities.SceneIsActive(GameManager.Instance.previouslyLoadedScene.name)) return; //if not hub area return

		activeDungeonLists.Clear();

		for (int i = 0; i < 5; i++)
		{
			Transform parentTransform;
			if (dungeonListTypeToShow == DungeonListTypeToShow.activeDungeons)
				parentTransform = dungeonListContent.transform;
			else parentTransform = hiddenDungeonsParentObj.transform;

			GameObject go = Instantiate(dungeonInfoSlotPrefab, parentTransform);
			DungeonDataUi dungeonData = go.GetComponent<DungeonDataUi>();
			dungeonData.Initilize(i);
			activeDungeonLists.Add(dungeonData);
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

		foreach (ChestHandler chest in DungeonHandler.Instance.dungeonLootChestsList)
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
			GameObject go = Instantiate(dungeonInfoSlotPrefab, hiddenDungeonsParentObj.transform);
			DungeonDataUi dungeonData = go.GetComponent<DungeonDataUi>();
			dungeonData.Initilize(SaveManager.Instance.GameData.savedDungeonsList[i], i);
			activeDungeonLists.Add(dungeonData);
			OnSaveDungeon(dungeonData);
		}

		for (int i = 0; i < SaveManager.Instance.GameData.activeDungeonsList.Count; i++)
		{
			GameObject go = Instantiate(dungeonInfoSlotPrefab, hiddenDungeonsParentObj.transform);
			DungeonDataUi dungeonData = go.GetComponent<DungeonDataUi>();
			dungeonData.Initilize(SaveManager.Instance.GameData.activeDungeonsList[i], i);
			activeDungeonLists.Add(dungeonData);
		}
	}
	private void OnSaveDungeon(DungeonDataUi dungeonData)
	{
		dungeonData.transform.SetParent(hiddenDungeonsParentObj.transform);
		dungeonData.saveDungeonButtonObj.SetActive(false);
		dungeonData.deleteDungeonButtonObj.SetActive(true);
		dungeonData.dungeonIndex = savedDungeonLists.Count;

		activeDungeonLists.Remove(dungeonData);
		savedDungeonLists.Add(dungeonData);
	}
	private void OnDeleteDungeon(DungeonDataUi dungeonData)
	{
		activeDungeonLists.Remove(dungeonData);
		savedDungeonLists.Remove(dungeonData);

		Destroy(dungeonData.gameObject);
	}
	private void HidePortalUiOnSceneChange(Scene newSceneLoaded, LoadSceneMode mode)
	{
		portalPanelUi.SetActive(false);
	}

	//UI CHANGES
	public void ShowPortalUi(PortalHandler portal)
	{
		portalPanelUi.SetActive(true);

		if (portal.portalType == PortalHandler.PortalType.isDungeonEnterencePortal)
		{
			dungeonEnterenceUi.SetActive(true);
			dungeonExitUi.SetActive(false);
			ShowActiveDungeonListUi();
		}
		else
		{
			dungeonEnterenceUi.SetActive(false);
			dungeonExitUi.SetActive(true);
		}
	}
	public void HidePortalUi()
	{
		GameManager.Localplayer.isInteractingWithInteractable = false;
		portalPanelUi.SetActive(false);
		dungeonEnterenceUi.SetActive(false);
		dungeonExitUi.SetActive(false);
	}

	public void ShowActiveDungeonListUi() //button click
	{
		if (dungeonListTypeToShow == DungeonListTypeToShow.activeDungeons) return; //ignore if already showing

		dungeonListInfoText.text = "Currently Showing Active Dungeons";
		dungeonListTypeToShow = DungeonListTypeToShow.activeDungeons;
		RemoveContentFromDungeonList();
		AddContentToDungeonList();
	}
	public void ShowSavedDungeonListUi() //button click
	{
		if (dungeonListTypeToShow == DungeonListTypeToShow.savedDungeons) return; //ignore if already showing

		dungeonListInfoText.text = "Currently Showing Saved Dungeons";
		dungeonListTypeToShow = DungeonListTypeToShow.savedDungeons;
		RemoveContentFromDungeonList();
		AddContentToDungeonList();
	}
	public void ShowBossDungeonsListUi() //button click
	{
		if (dungeonListTypeToShow == DungeonListTypeToShow.bossDungeons) return; //ignore if already showing

		dungeonListInfoText.text = "Currently Showing Boss Dungeons";
		dungeonListTypeToShow = DungeonListTypeToShow.bossDungeons;
		RemoveContentFromDungeonList();
		AddContentToDungeonList();
	}

	//hide dungeondata slots currently shown, then show dungeondata slots based on enum type
	private void RemoveContentFromDungeonList()
	{
		for (int i = dungeonListContent.transform.childCount - 1; i >= 0; i--)
			dungeonListContent.transform.GetChild(i).SetParent(hiddenDungeonsParentObj.transform);
	}
	private void AddContentToDungeonList()
	{
		if (dungeonListTypeToShow == DungeonListTypeToShow.activeDungeons)
		{
			foreach (DungeonDataUi dungeonData in activeDungeonLists)
				dungeonData.transform.SetParent(dungeonListContent.transform);
		}
		else if (dungeonListTypeToShow == DungeonListTypeToShow.savedDungeons)
		{
			foreach (DungeonDataUi dungeonData in savedDungeonLists)
				dungeonData.transform.SetParent(dungeonListContent.transform);
		}
		else if (dungeonListTypeToShow == DungeonListTypeToShow.bossDungeons)
		{
			foreach (DungeonDataUi dungeonData in bossDungeonLists)
				dungeonData.transform.SetParent(dungeonListContent.transform);
		}
	}
}
