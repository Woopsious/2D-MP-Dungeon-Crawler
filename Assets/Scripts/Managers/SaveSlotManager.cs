using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using JetBrains.Annotations;

public class SaveSlotManager : MonoBehaviour
{
	[SerializeField] private string folderDirectory;

	public TMP_Text saveSlotCountText;
	public TMP_Text saveSlotInfoText;

	public GameObject saveButtonObj;
	public GameObject loadButtonObj;
	public GameObject deleteButtonObj;

	public string Name;
	public string Level;
	public string Date;

	public void Initilize(string directory, bool isEmpty)
	{
		folderDirectory = directory;

		saveSlotInfoText.text = $"Player Name: {Name} \n Player Level: {Level} \n Date: {Date}";
		saveSlotCountText.text = GrabSaveSlotNumber(directory);

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
	}
	private string GrabSaveSlotNumber(string directory)
	{
		string[] strings = directory.Split("/");
		string saveSlotNum = "Save Slot: ";

		foreach (char c in strings[^1])
		{
			if (char.IsDigit(c))
				saveSlotNum += c;
		}
		return saveSlotNum;
	}

	//button actions
	public void SaveGame()
	{
		SaveManager.Instance.SaveGameData(folderDirectory);
	}
	public void LoadGame()
	{
		SaveManager.Instance.LoadGameData(folderDirectory);
	}
	public void DeleteGame()
	{
		SaveManager.Instance.DeleteGameData(folderDirectory);
	}
}
