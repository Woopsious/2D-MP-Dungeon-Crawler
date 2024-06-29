using System.Collections;
using System.Collections.Generic;
using TMPro;
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

	[Header("Ui Notif")]
	public TMP_Text interactWithText;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		lootSpawnHandler = GetComponent<LootSpawnHandler>();
	}
	private void Start()
	{
		Initilize();
	}
	private void Update()
	{
		UpdateInteractText();
	}

	private void Initilize()
	{
		spriteRenderer.sprite = chestClosedSprite;
		chestStateOpened = false;

		interactWithText.transform.SetParent(MainMenuManager.Instance.runtimeUiContainer.transform);
		interactWithText.transform.SetAsFirstSibling();
		interactWithText.text = $"F to interact";
	}
	private void UpdateInteractText()
	{
		if (chestStateOpened == true)
		{
			interactWithText.gameObject.SetActive(false);
			return;
		}
		if (!interactWithText.gameObject.activeInHierarchy) return;
		interactWithText.transform.position =
			Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + 0.65f, 0));
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
