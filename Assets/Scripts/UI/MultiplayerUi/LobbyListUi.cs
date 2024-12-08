using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListUi : MonoBehaviour
{
	public static LobbyListUi Instance;

	[Header("Lobby List Panel")]
	public GameObject LobbyListUiPanel;

	[Header("Lobby List")]
	public TMP_InputField lobbyNameSearchInput;
	public List<LobbyCardInfoHandler> LobbyCardInfoList = new List<LobbyCardInfoHandler>();
	public List<Lobby> lobbySearchResultsList = new List<Lobby>();

	private string LobbySearchContinuationToken;
	private int currentPageIndex;

	[Header("Fetching Lobbies Panel")]
	public GameObject fetchingLobbiesPanel;

	private void Awake()
	{
		Instance = this;
	}

	//search for lobbies button
	public void SearchForLobbiesButton()
	{
		SearchForLobbies();
	}
	private async void SearchForLobbies()
	{
		ShowFetchingLobbiesUi();
		await NewSearchForLobbies(lobbyNameSearchInput.text);
		HideFetchingLobbiesUi();
	}

	//search for lobbies via lobby name
	private async Task NewSearchForLobbies(string searchInput)
	{
		try
		{
			QueryLobbiesOptions queryLobbiesOptions = new()
			{
				SampleResults = false,
				Filters = new List<QueryFilter> {
					new QueryFilter(QueryFilter.FieldOptions.Name, searchInput, QueryFilter.OpOptions.CONTAINS),
					new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
				}
			};

			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
			LobbySearchContinuationToken = queryResponse.ContinuationToken;
			lobbySearchResultsList = queryResponse.Results;
			UpdateLobbyListUi(1);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	private async Task ContinueSearchForLobbies(string searchInput, int newPageIndex)
	{
		try
		{
			QueryLobbiesOptions queryLobbiesOptions = new()
			{
				ContinuationToken = LobbySearchContinuationToken,
				SampleResults = false,
				Filters = new List<QueryFilter> {
					new QueryFilter(QueryFilter.FieldOptions.Name, searchInput, QueryFilter.OpOptions.CONTAINS),
					new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
				}
			};

			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
			lobbySearchResultsList = queryResponse.Results;
			UpdateLobbyListUi(newPageIndex);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	//update lobbyUiCardInfos
	public void UpdateLobbyListUi(int pageIndex)
	{
		currentPageIndex = pageIndex;
		int lobbyIndex = GetStartingLobbyIndex(pageIndex);
		foreach (LobbyCardInfoHandler lobbyCardInfo in LobbyCardInfoList)
		{
			if (lobbyIndex >= lobbySearchResultsList.Count)
				lobbyCardInfo.UpdateInfo(null);
			else
				lobbyCardInfo.UpdateInfo(lobbySearchResultsList[lobbyIndex]);

			lobbyIndex++;
		}
	}
	//eg: page 1 * 5 = 5 | 5 - 5 = lobby 0 | page 4 * 5 = 20 | 20 - 5 = lobby 15
	private int GetStartingLobbyIndex(int pageIndex)
	{
		return pageIndex = (pageIndex * 5) - 5;
	}

	//unused atm
	public void NextPage()
	{

	}
	public void PreviousPage()
	{

	}

	//back button
	public void BackToMultiplayerMainMenu()
	{
		MultiplayerMenuUi.Instance.ShowMpMenuUi();
	}

	//UI PANEL CHANGES
	public void ShowLobbyListUi()
	{
		LobbyListUiPanel.SetActive(true);
		MultiplayerMenuUi.Instance.HideMpMenuUi();
		LobbyUi.Instance.HideLobbyUi();
		LobbyUi.Instance.HideLobbySettingsUi();
	}
	public void HideLobbyListUi()
	{
		LobbyListUiPanel.SetActive(false);
	}
	public void ShowFetchingLobbiesUi()
	{
		fetchingLobbiesPanel.SetActive(true);
	}
	public void HideFetchingLobbiesUi()
	{
		fetchingLobbiesPanel.SetActive(false);
	}
}
