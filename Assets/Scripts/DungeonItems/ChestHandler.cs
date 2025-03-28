using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static PortalHandler;

public class ChestHandler : MonoBehaviour, IInteractables
{
	public Sprite chestClosedSprite;
	public Sprite chestOpenedSprite;
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;
	private LootSpawnHandler lootSpawnHandler;
	private Interactables interactable;

	private ChestState chestState;
	public enum ChestState
	{
		disabled, enabled, opened
	}

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
		interactable = GetComponent<Interactables>();
	}
	private void Start()
	{
		Initilize();
	}

	private void Initilize()
	{
		if (!isPlayerStorageChest)
		{
			if (!DungeonHandler.Instance.dungeonLootChestsList.Contains(this))
				Debug.LogError("Loot chest not added to dungeon loot chest list, ensure of this type are added");

			spriteRenderer.sprite = chestClosedSprite;
			lootSpawnHandler.Initilize(maxDroppedGoldAmount, minDroppedGoldAmount, lootPool, 0);
		}
		else
		{
			if (DungeonHandler.Instance.dungeonLootChestsList.Contains(this))
				Debug.LogError("player storage chest added to dungeon loot chest list, ensure non of this type are added");
		}
	}

	//loot chest states
	public ChestState GetChestState()
	{
		return chestState;
	}
	public void DisableChest()
	{
		gameObject.SetActive(false);
		chestState = ChestState.disabled;
	}
	public void EnableChest()
	{
		gameObject.SetActive(true);
		chestState = ChestState.enabled;
	}
	public void OpenChest(bool isPlayerInteraction)
	{
		if (chestState == ChestState.opened) return;

		gameObject.SetActive(true);
		chestState = ChestState.opened;
		spriteRenderer.sprite = chestOpenedSprite;
		PlayerEventManager.DetectNewInteractedObject(interactable, false, "Interact");

		if (isPlayerInteraction)
		{
			lootSpawnHandler.SpawnLoot();
			lootSpawnHandler.AddGold();
		}

		if (MultiplayerManager.IsMultiplayer())
			DungeonHandler.Instance.TrySyncChestState(this);
	}

	//player interactions
	public void Interact(PlayerController player)
	{
		if (!isPlayerStorageChest)
		{
			if (chestState == ChestState.opened) return;
			audioHandler.PlayAudio(chestOpenSfx);
			OpenChest(true);
		}
		else
		{
			audioHandler.PlayAudio(chestOpenSfx);
			PlayerInventoryUi.Instance.ShowPlayerStorageChest(0);
			player.isInteractingWithInteractable = true;
		}
	}
	public void UnInteract(PlayerController player)
	{
		if (!isPlayerStorageChest) return;
		audioHandler.PlayAudio(chestCloseSfx);
		PlayerInventoryUi.Instance.HidePlayerStorageChest();
		player.isInteractingWithInteractable = false;
	}
}
