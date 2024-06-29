using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestHandler : MonoBehaviour, IInteractable
{
	public Sprite chestClosedSprite;
	public Sprite chestOpenedSprite;
	private SpriteRenderer spriteRenderer;
	private LootSpawnHandler lootSpawnHandler;
	public bool chestActive;
	public bool chestStateOpened;

	[Header("Player Chest Info")]
	public bool isPlayerStorageChest;
	public GameObject ItemPrefab;
	public List<InventoryItemUi> itemList = new List<InventoryItemUi>();

	[Header("Container Ref")]
	public GameObject itemContainer;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		lootSpawnHandler = GetComponent<LootSpawnHandler>();
	}
	private void Start()
	{
		Initilize();
	}

	private void Initilize()
	{
		spriteRenderer.sprite = chestClosedSprite;
		chestStateOpened = false;
	}

	public void ActivateChest()
	{
		chestActive = true;
	}
	public void DeactivateChest()
	{
		chestActive = false;
		gameObject.SetActive(false);
	}
	public void ChangeChestStateToOpen(bool spawnLoot)
	{
		EventManager.DetectNewInteractedObject(gameObject, false);
		chestStateOpened = true;
		spriteRenderer.sprite = chestOpenedSprite;
		if (spawnLoot)
			lootSpawnHandler.SpawnLoot();
	}

	public void Interact(PlayerController player)
	{
		if (!isPlayerStorageChest)
		{
			if (chestStateOpened == true) return;
			ChangeChestStateToOpen(true);
		}
		else
			PlayerInventoryUi.Instance.ShowPlayerStorageChest(this, 0);
	}
	public void UnInteract(PlayerController player)
	{
		if (!isPlayerStorageChest) return;
		PlayerInventoryUi.Instance.HidePlayerStorageChest(this);
	}
}
