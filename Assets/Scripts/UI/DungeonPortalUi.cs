using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonPortalUi : MonoBehaviour
{
	public static DungeonPortalUi instance;

	public GameObject portalPanelUi;
	public GameObject dungeonInfoSlotPrefab;

	public TMP_Text dungeonListInfoText;
	public GameObject portalEnterenceUi;
	public GameObject activeDungeonListPanel;
	public GameObject activeDungeonListContent;
	public GameObject savedDungeonListPanel;
	public GameObject savedDungeonListContent;
	public GameObject portalExitUi;

	public List<DungeonSlotUi> activeDungeonLists = new List<DungeonSlotUi>();
	public List<DungeonSlotUi> savedDungeonLists = new List<DungeonSlotUi>();

	private void Awake()
	{
		instance = this;
		Initilize();
	}

	private void OnEnable()
	{
		EventManager.OnShowPortalUi += ShowPortalUi;
		EventManager.OnHidePortalUi += HidePortalUi;

		DungeonSlotUi.OnDungeonSave += OnSaveDungeon;
		DungeonSlotUi.OnDungeonDelete += OnDeleteDungeon;
	}
	private void OnDisable()
	{
		EventManager.OnShowPortalUi -= ShowPortalUi;
		EventManager.OnHidePortalUi -= HidePortalUi;

		DungeonSlotUi.OnDungeonSave -= OnSaveDungeon;
		DungeonSlotUi.OnDungeonDelete -= OnDeleteDungeon;
	}

	private void Initilize()
	{
		GenerateNewDungeons();
	}
	private void GenerateNewDungeons()
	{
		if (!Utilities.GetCurrentlyActiveScene(GameManager.Instance.hubAreaName)) return; //if not hub area return

		activeDungeonLists.Clear();

		for (int i = 0; i < 5; i++)
		{
			GameObject go = Instantiate(dungeonInfoSlotPrefab, activeDungeonListContent.transform);
			DungeonSlotUi dungeonSlot = go.GetComponent<DungeonSlotUi>();
			dungeonSlot.Initilize();
			activeDungeonLists.Add(dungeonSlot);
		}
	}	
	public void GenerateNewRandomDungeonsButton()
	{
		for (int i = activeDungeonLists.Count - 1; i >= 0; i--)
		{
			activeDungeonLists[i].DeleteDungeon();
		}

		GenerateNewDungeons();
	}

	//Events
	public void OnSaveDungeon(DungeonSlotUi dungeonSlot)
	{
		dungeonSlot.transform.SetParent(savedDungeonListContent.transform);
		dungeonSlot.saveDungeonButtonObj.SetActive(false);
		dungeonSlot.deleteDungeonButtonObj.SetActive(true);

		activeDungeonLists.Remove(dungeonSlot);
		savedDungeonLists.Add(dungeonSlot);
	}
	public void OnDeleteDungeon(DungeonSlotUi dungeonSlot)
	{
		activeDungeonLists.Remove(dungeonSlot);
		savedDungeonLists.Remove(dungeonSlot);

		Destroy(dungeonSlot.gameObject);
	}

	//UI CHANGES
	public void ShowPortalUi(PortalHandler portal)
	{
		if (portalPanelUi.activeInHierarchy)
		{
			HidePortalUi();
		}
		else
		{
			portalPanelUi.SetActive(true);

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
		dungeonListInfoText.text = "Current Saved Dungeons";
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
