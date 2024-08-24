using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestHandler : MonoBehaviour, IInteractable
{
	public Sprite chestClosedSprite;
	public Sprite chestOpenedSprite;
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;
	private LootSpawnHandler lootSpawnHandler;
	[HideInInspector] public bool chestActive;
	[HideInInspector] public bool chestStateOpened;

	[Header("Player Chest Info")]
	public bool isPlayerStorageChest;
	public GameObject ItemPrefab;
	[HideInInspector] public List<InventoryItemUi> itemList = new List<InventoryItemUi>();

	[Header("Chest Loot Pool Settings")]
	public int maxDroppedGoldAmount;
	public int minDroppedGoldAmount;
	public SOLootPools lootPool;

	[Header("Container Ref")]
	public GameObject itemContainer;

	[Header("Chest Sound Settings")]
	public AudioClip chestOpenSfx;
	public AudioClip chestCloseSfx;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
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
		lootSpawnHandler.Initilize(maxDroppedGoldAmount, minDroppedGoldAmount, lootPool);
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
	public void ChangeChestStateToOpen(bool isPlayerInteraction)
	{
		PlayerEventManager.DetectNewInteractedObject(gameObject, false);
		chestStateOpened = true;
		spriteRenderer.sprite = chestOpenedSprite;
		if (isPlayerInteraction)
		{
			lootSpawnHandler.SpawnLoot();
			lootSpawnHandler.AddGold();
		}
	}

	public void Interact(PlayerController player)
	{
		if (!isPlayerStorageChest)
		{
			if (chestStateOpened == true) return;
			audioHandler.PlayAudio(chestOpenSfx);
			ChangeChestStateToOpen(true);
		}
		else
		{
			audioHandler.PlayAudio(chestOpenSfx);
			PlayerInventoryUi.Instance.ShowPlayerStorageChest(this, 0);
		}
	}
	public void UnInteract(PlayerController player)
	{
		if (!isPlayerStorageChest) return;
		audioHandler.PlayAudio(chestCloseSfx);
		PlayerInventoryUi.Instance.HidePlayerStorageChest(this);
	}
}
