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

	public string Name;
	public string Level;
	public string Date;

	public void Initilize(string directory)
	{
		folderDirectory = directory;

		saveSlotInfoText.text = $"Player Name: {Name} \n Player Level: {Level} \n Date: {Date}";
		saveSlotCountText.text = GrabSaveSlotNumber(directory);
	}
	private string GrabSaveSlotNumber(string directory)
	{
		string[] strings = directory.Split("/");

		Debug.Log(strings[^1]);

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
		Destroy(gameObject);
	}
}
