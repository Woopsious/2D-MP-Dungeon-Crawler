using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	public static event Action OnGameLoad;

	[SerializeReference] public InventoryData InventoryData = new InventoryData();

	public void Start()
	{
		Instance = this;
	}

	public void SaveDataToJson()
	{
		InventoryData.hasRecievedStartingItems = PlayerInventoryManager.Instance.hasRecievedStartingItems;

		foreach (GameObject slot in PlayerInventoryUi.Instance.InventorySlots)
		{
			InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
			if (inventorySlot.IsSlotEmpty()) continue;
			else
			{
				InventoryData.InventoryItems.Add(inventorySlot.itemInSlot.gameObject);
			}
		}

		string inventoryData = JsonUtility.ToJson(InventoryData);
		string filePath = Application.persistentDataPath + "/PlayerData/InventoryData.json";
		System.IO.File.WriteAllText(filePath, inventoryData);

		Debug.Log(filePath);
	}
	public void LoadDataToJson()
	{
		string filePath = Application.persistentDataPath + "/PlayerData/InventoryData.json";
		string inventoryData = System.IO.File.ReadAllText(filePath);
		InventoryData = JsonUtility.FromJson<InventoryData>(inventoryData);

		OnGameLoad?.Invoke();
	}
}

public class InventoryData
{
	public bool hasRecievedStartingItems;

	public List<GameObject> InventoryItems = new List<GameObject>();
}

public class EquipmentData
{
	public List<InventoryItem> EquipmentItems = new List<InventoryItem>();
}