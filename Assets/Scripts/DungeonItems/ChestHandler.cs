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

	private int chestListIndex;
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

		SetChestListIndex();
	}
	private void SetChestListIndex()
	{
		for (int i = 0; i < DungeonHandler.Instance.dungeonLootChestsList.Count; i++)
		{
			if (DungeonHandler.Instance.dungeonLootChestsList[i] != this) continue;
			chestListIndex = i;
		}
	}

	//CHEST STATE CHANGES
	//disable chest
	public void DisableChestState()
	{
		chestState = ChestState.disabled;
		gameObject.SetActive(false);
	}

	//enable chest
	public void EnableChestState()
	{
		chestState = ChestState.enabled;
		gameObject.SetActive(true);
	}

	//open chest
	public void OpenChest(bool isPlayerInteraction)
	{
		if (chestState == ChestState.opened) return;
		chestState = ChestState.opened; //call early

		PlayerEventManager.DetectNewInteractedObject(interactable, false, "Interact");

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncChestStateRpc(chestListIndex, chestState, isPlayerInteraction);
		else
			OpenChestState(isPlayerInteraction);
	}
	public void OpenChestState(bool isPlayerInteraction)
	{
		gameObject.SetActive(true);
		chestState = ChestState.opened;
		spriteRenderer.sprite = chestOpenedSprite;

		Debug.LogError("Chest opened");

		if (isPlayerInteraction)
		{
			lootSpawnHandler.SpawnLoot();
			lootSpawnHandler.AddGold();
		}
	}

	//helpers
	public ChestState GetChestState()
	{
		return chestState;
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
