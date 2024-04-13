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
	public GameObject savedDungeonListPanel;
	public GameObject portalExitUi;

	public List<DungeonSlotUi> activeDungeonLists = new List<DungeonSlotUi>();
	public List<DungeonSlotUi> savedDungeonLists = new List<DungeonSlotUi>();

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		EventManager.OnShowPortalUi += ShowPortalUi;
		EventManager.OnHidePortalUi += HidePortalUi;
	}
	private void OnDisable()
	{
		EventManager.OnShowPortalUi -= ShowPortalUi;
		EventManager.OnHidePortalUi -= HidePortalUi;
	}

	//UI CHANGES
	public void ShowPortalUi(PortalHandler portal)
	{
		if (portal.isDungeonEnterencePortal)
		{
			portalEnterenceUi.SetActive(true);
			ShowActiveDungeonListUi();
		}
		else
			portalExitUi.SetActive(true);
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
	public void HidePortalUi(PortalHandler portal)
	{
		portalEnterenceUi.SetActive(false);
		portalExitUi.SetActive(false);
	}
}
