using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	private bool blockNewDungeonGeneration;

	private void Awake()
	{
		instance = this;
		blockNewDungeonGeneration = false;
		GenerateBossDungeonsOnAwake();
	}
	private void OnEnable()
	{
		SaveManager.ReloadSaveGameData += ReloadSavedDungeons;
		PlayerEventManager.OnShowPortalUi += ShowPortalUi;
		PlayerEventManager.OnHidePortalUi += HidePortalUi;

		SceneManager.sceneLoaded += HidePortalUiOnSceneChange;

		DungeonDataUi.OnDungeonSave += OnSaveDungeon;
		DungeonDataUi.OnDungeonDelete += OnDeleteDungeon;
	}
	private void OnDisable()
	{
		SaveManager.ReloadSaveGameData -= ReloadSavedDungeons;
		PlayerEventManager.OnShowPortalUi -= ShowPortalUi;
		PlayerEventManager.OnHidePortalUi -= HidePortalUi;

		SceneManager.sceneLoaded -= HidePortalUiOnSceneChange;

		DungeonDataUi.OnDungeonSave -= OnSaveDungeon;
		DungeonDataUi.OnDungeonDelete -= OnDeleteDungeon;
	}

	//button actions
	public void GenerateNewRandomDungeonsButton()
	{
		GenerateNewDungeons();
	}
	public void ReturnToHubButton()
	{
		List<DungeonChestData> chestData = new List<DungeonChestData>();

		foreach (ChestHandler chest in DungeonHandler.Instance.dungeonLootChestsList)
		{
			DungeonChestData data = new DungeonChestData()
			{
				chestState = chest.GetChestState(),
			};
			chestData.Add(data);
		}

		GameManager.Instance.currentDungeonData.dungeonChestData = chestData;

		//save data to corrisponding DungeonSlotUi, where SaveManager will then save/reload all DungeonSlotUi's
		if (savedDungeonLists.Count == 0 && activeDungeonLists.Count == 0)
		{
			Debug.LogError("returning to hub scene whilst testing dungeon scene not supported");
			return;
		}

		if (GameManager.Instance.currentDungeonData.isDungeonSaved)
			savedDungeonLists[GameManager.Instance.currentDungeonData.dungeonIndex].dungeonChestData = chestData;
		else
			activeDungeonLists[GameManager.Instance.currentDungeonData.dungeonIndex].dungeonChestData = chestData;

		GameManager.Instance.LoadHubArea(false, GameManager.GameDataReloadMode.noReload);
	}

	//dungeon generation
	private async void GenerateBossDungeonsOnAwake()
	{
		for (int i = 0; i < AssetDatabase.Database.bossEntities.Count; i++) //generate dungeons for each boss
		{
			Transform parentTransform;
			if (dungeonListTypeToShow == DungeonListTypeToShow.bossDungeons)
				parentTransform = dungeonListContent.transform;
			else parentTransform = hiddenDungeonsParentObj.transform;

			for (int j = 0; j < 3; j++) //generate dungeons for every difficulty per boss
			{
				GameObject go = Instantiate(dungeonInfoSlotPrefab, parentTransform);
				DungeonDataUi dungeonData = go.GetComponent<DungeonDataUi>();
				bossDungeonLists.Add(dungeonData);
				await dungeonData.Initilize(i, j, AssetDatabase.Database.bossEntities[i]);
			}
		}
	}
	private async void GenerateNewDungeons()
	{
		if (!Utilities.SceneIsActive(GameManager.Instance.hubScene)) return; //if not hub area return
		if (blockNewDungeonGeneration) return;
		blockNewDungeonGeneration = true;

		for (int i = activeDungeonLists.Count - 1; i >= 0; i--)
			activeDungeonLists[i].DeleteDungeon();

		activeDungeonLists.Clear();

		for (int i = 0; i < 5; i++)
		{
			Transform parentTransform;
			if (dungeonListTypeToShow == DungeonListTypeToShow.activeDungeons)
				parentTransform = dungeonListContent.transform;
			else parentTransform = hiddenDungeonsParentObj.transform;

			GameObject go = Instantiate(dungeonInfoSlotPrefab, parentTransform);
			DungeonDataUi dungeonData = go.GetComponent<DungeonDataUi>();
			activeDungeonLists.Add(dungeonData);
			await dungeonData.Initilize(i);
		}

		blockNewDungeonGeneration = false;
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
		UpdateDungeonIndexes();
	}
	private void OnDeleteDungeon(DungeonDataUi dungeonData)
	{
		activeDungeonLists.Remove(dungeonData);
		savedDungeonLists.Remove(dungeonData);

		Destroy(dungeonData.gameObject);
		UpdateDungeonIndexes();
	}
	private void UpdateDungeonIndexes()
	{
		for (int i = 0; i < activeDungeonLists.Count; i++)
			activeDungeonLists[i].dungeonIndex = i;
		for (int i = 0; i < savedDungeonLists.Count; i++)
			savedDungeonLists[i].dungeonIndex = i;
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

			if (dungeonListTypeToShow == DungeonListTypeToShow.activeDungeons)
				ShowActiveDungeonListUi();
			else
				ShowSavedDungeonListUi();
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
		dungeonListInfoText.text = "Currently Showing Active Dungeons";
		dungeonListTypeToShow = DungeonListTypeToShow.activeDungeons;
		RemoveContentFromDungeonList();
		AddContentToDungeonList();
	}
	public void ShowSavedDungeonListUi() //button click
	{
		dungeonListInfoText.text = "Currently Showing Saved Dungeons";
		dungeonListTypeToShow = DungeonListTypeToShow.savedDungeons;
		RemoveContentFromDungeonList();
		AddContentToDungeonList();
	}
	public void ShowBossDungeonsListUi() //button click
	{
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
			{
				dungeonData.UpdateDynamicUi();
				dungeonData.transform.SetParent(dungeonListContent.transform);
			}
		}
		else if (dungeonListTypeToShow == DungeonListTypeToShow.savedDungeons)
		{
			foreach (DungeonDataUi dungeonData in savedDungeonLists)
			{
				dungeonData.UpdateDynamicUi();
				dungeonData.transform.SetParent(dungeonListContent.transform);
			}
		}
		else if (dungeonListTypeToShow == DungeonListTypeToShow.bossDungeons)
		{
			foreach (DungeonDataUi dungeonData in bossDungeonLists)
			{
				dungeonData.UpdateDynamicUi();
				dungeonData.transform.SetParent(dungeonListContent.transform);
			}
		}
	}
}
