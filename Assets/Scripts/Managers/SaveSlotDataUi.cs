using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.UI;

public class SaveSlotDataUi : MonoBehaviour
{
	public string folderDirectory;

	public TMP_Text saveSlotCountText;
	public TMP_Text saveSlotInfoText;

	public GameObject saveButtonObj;
	public GameObject loadButtonObj;
	public GameObject deleteButtonObj;

	public string slotNumber;
	public string Name;
	public string Level;
	public string Date;

	public void Initilize(string directory, bool isAutoSaveSlot , bool isEmpty)
	{
		folderDirectory = directory;

		saveSlotInfoText.text = $"Player Name: {Name}\nPlayer Level: {Level}\nDate: {Date}";
		saveSlotCountText.text = "Save Slot: " + GrabSaveSlotNumber(directory);

		if (isAutoSaveSlot)
			saveSlotCountText.text = "Save Slot:\nAuto Save";

		if (isEmpty)
		{
			loadButtonObj.SetActive(false);
			deleteButtonObj.SetActive(false);
		}
		else
		{
			loadButtonObj.SetActive(true);
			deleteButtonObj.SetActive(true);
		}
		if (Utilities.GetCurrentlyActiveScene(GameManager.Instance.mainMenuName))
			saveButtonObj.SetActive(false);
		else
			saveButtonObj.SetActive(true);
	}
	private string GrabSaveSlotNumber(string directory)
	{
		string[] strings = directory.Split("/");
		string saveSlotNum = "";

		foreach (char c in strings[^1])
		{
			if (char.IsDigit(c))
				saveSlotNum += c;
		}
		slotNumber = saveSlotNum;
		return saveSlotNum;
	}

	//button actions
	public void SaveGame()
	{
		Button button = MainMenuManager.Instance.confirmActionButton.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ConfirmSaveGame);

		if (Date == "Empty")
			SaveManager.Instance.SaveGameData(folderDirectory);
		else
			MainMenuManager.Instance.ShowConfirmActionPanel(this, 0);
	}
	public void ConfirmSaveGame()
	{
		SaveManager.Instance.SaveGameData(folderDirectory);
		MainMenuManager.Instance.HideConfirmActionPanel();
	}

	public void LoadGame()
	{
		Button button = MainMenuManager.Instance.confirmActionButton.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ConfirmLoadGame);

		if (Utilities.GetCurrentlyActiveScene(GameManager.Instance.mainMenuName))
		{
			SaveManager.Instance.LoadGameData(folderDirectory);
			GameManager.Instance.LoadHubArea(false);
		}
		else
			MainMenuManager.Instance.ShowConfirmActionPanel(this, 1);
	}
	public void ConfirmLoadGame()
	{
		SaveManager.Instance.LoadGameData(folderDirectory);
		GameManager.Instance.LoadHubArea(false);
		MainMenuManager.Instance.HideConfirmActionPanel();
	}

	public void DeleteGame()
	{
		Button button = MainMenuManager.Instance.confirmActionButton.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ConfirmDeleteGame);

		MainMenuManager.Instance.ShowConfirmActionPanel(this, 2);
	}
	public void ConfirmDeleteGame()
	{
		SaveManager.Instance.DeleteGameData(folderDirectory);
		MainMenuManager.Instance.HideConfirmActionPanel();
	}
}
