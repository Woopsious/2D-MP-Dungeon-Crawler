using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCodexUi : MonoBehaviour
{
	public static PlayerCodexUi Instance;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		PlayerEventManager.OnShowPlayerInventoryEvent += HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerJournalEvent += HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerCodexEvent += ShowPlayerCodexUi;
		PlayerEventManager.OnShowPlayerDeathUiEvent += HidePlayerCodexUi;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnShowPlayerInventoryEvent -= HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerJournalEvent -= HidePlayerCodexUi;
		PlayerEventManager.OnShowPlayerCodexEvent -= ShowPlayerCodexUi;
		PlayerEventManager.OnShowPlayerDeathUiEvent -= HidePlayerCodexUi;
	}
	private void Start()
	{
		Initilize();
	}

	private void Initilize()
	{
		//grab asset data from asset data base, filter and sort them into better lists eg: abilities need to be sorted into better catagories
		//use ui templates to fill out data

		List<SOEntityStats> entityStats = AssetDatabase.Database.entities;
	}

	//update ui
	private void ShowPlayerCodexUi()
	{

	}
	private void HidePlayerCodexUi()
	{

	}
}
