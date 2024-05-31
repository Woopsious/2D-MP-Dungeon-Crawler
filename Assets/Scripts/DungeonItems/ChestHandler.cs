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
	public bool chestStateOpened;

	[Header("Ui Notif")]
	public TMP_Text interactWithText;

	private void Awake()
	{
		Initilize();
	}
	private void OnEnable()
	{

	}
	private void OnDisable()
	{

	}
	private void Update()
	{
		UpdateInteractText();
	}

	private void Initilize()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = chestClosedSprite;
		lootSpawnHandler = GetComponent<LootSpawnHandler>();
		lootSpawnHandler.lootSpawnBounds.min = new Vector3(transform.position.x - 1, transform.position.y - 1, 0);
		lootSpawnHandler.lootSpawnBounds.max = new Vector3(transform.position.x + 1, transform.position.y + 1, 0);
		chestStateOpened = false;

		interactWithText.transform.SetParent(FindObjectOfType<Canvas>().transform);
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

	private void ChangeChestStateToOpen()
	{
		chestStateOpened = true;
		lootSpawnHandler.SpawnChestLoot();
		spriteRenderer.sprite = chestOpenedSprite;
	}

	public void Interact(PlayerController player)
	{
		if (chestStateOpened ==  true) return;

		ChangeChestStateToOpen();
	}
	public void UnInteract(PlayerController player)
	{
		return; //no uninteract required
	}
}
