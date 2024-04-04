using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NpcHandler : MonoBehaviour, IInteractable
{
	public SONpcs npc;

	private Animator animator;
	private SpriteRenderer spriteRenderer;

	[Header("Quests")]
	public GameObject questPrefab;
	public List<QuestSlotsUi> avalableQuestList = new List<QuestSlotsUi>();

	[Header("Shop Items")]
	public GameObject ItemPrefab;
	public List<InventoryItemUi> avalableShopItemsList = new List<InventoryItemUi>();

	[Header("Container Ref")]
	public GameObject npcContainer;

	[Header("Ui Notif")]
	public TMP_Text NpcTypeText;
	public TMP_Text interactWithText;

	private void Start()
	{
		Initilize();
	}
	private void OnDisable()
	{
		PlayerJournalUi.OnNewQuestAccepted -= OnQuestAccepted;
	}
	private void Update()
	{
		UpdateInteractText();
		UpdateNpcTypeText();
	}

	private void Initilize()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("isIdle", true);
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.sprite = npc.sprite;
		name = npc.entityName;

		NpcTypeText.transform.SetParent(FindObjectOfType<Canvas>().transform); //NpcTypeText.text set inside functions below
		NpcTypeText.transform.SetAsFirstSibling();
		if (npc.npcType == SONpcs.NPCType.isQuestNpc)
			GenerateNewQuests();
		else if (npc.npcType == SONpcs.NPCType.isShopNpc)
			GenerateShopItems();

		interactWithText.transform.SetParent(FindObjectOfType<Canvas>().transform);
		interactWithText.transform.SetAsFirstSibling();
		interactWithText.text = $"F to interact";
	}

	public void Interact(PlayerController player)
	{
		if (npc.npcType == SONpcs.NPCType.isQuestNpc)
		{
			player.isInteractingWithNpc = true;
			PlayerJournalUi.Instance.ShowPlayerJournal();
			PlayerJournalUi.Instance.ShowNpcJournal();
			PlayerJournalUi.Instance.refreshQuestsButton.onClick.AddListener(delegate { RefreshThisNpcsQuests(); });
			PlayerJournalUi.Instance.closeQuestsButton.onClick.AddListener(delegate { UnInteract(player); });
			MoveQuestsToUi();
		}
		else if (npc.npcType == SONpcs.NPCType.isShopNpc)
		{
			player.isInteractingWithNpc = true;
			PlayerInventoryUi.Instance.ShowInventory();
			PlayerInventoryUi.Instance.ShowNpcShop();
			PlayerInventoryUi.Instance.closeShopButton.onClick.AddListener(delegate { UnInteract(player); });
			MoveShopItemsToUi();
		}
	}
	public void UnInteract(PlayerController player)
	{
		if (npc.npcType == SONpcs.NPCType.isQuestNpc)
		{
			player.isInteractingWithNpc = false;
			PlayerJournalUi.Instance.HidePlayerJournal();
			PlayerJournalUi.Instance.HideNpcJournal();
			PlayerJournalUi.Instance.refreshQuestsButton.onClick.RemoveListener(delegate { RefreshThisNpcsQuests(); });
			PlayerJournalUi.Instance.closeQuestsButton.onClick.RemoveListener(delegate { UnInteract(player); });
			MoveQuestsToContainer();
		}
		else if (npc.npcType == SONpcs.NPCType.isShopNpc)
		{
			player.isInteractingWithNpc = false;
			PlayerInventoryUi.Instance.HideInventory();
			PlayerInventoryUi.Instance.HideNpcShop();
			PlayerInventoryUi.Instance.closeShopButton.onClick.RemoveListener(delegate { UnInteract(player); });
			MoveShopItemsToContainer();
		}
	}
	private void UpdateNpcTypeText()
	{
		if (!NpcTypeText.gameObject.activeInHierarchy) return;
		NpcTypeText.transform.position =
			Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + 1f, 0));
	}
	private void UpdateInteractText()
	{
		if (!interactWithText.gameObject.activeInHierarchy) return;
		interactWithText.transform.position = 
			Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + 0.65f, 0));
	}

	//shop npc functions
	public void GenerateShopItems()
	{
		for (int i = avalableShopItemsList.Count; i > 0; i--)
			Destroy(avalableShopItemsList[i - 1].gameObject);
		avalableShopItemsList.Clear();

		//for now grab player level, later need a better way to do this if possible
		int playerLevel = FindObjectOfType<PlayerController>().GetComponent<EntityStats>().entityLevel;

		for (int i = 0; i < Utilities.GetRandomNumberBetween(npc.minNumOfShopItems, npc.maxNumOfShopItems + 1); i++)
			GenerateItem(playerLevel);
	}
	public void GenerateItem(int playerLevel)
	{
		GameObject go = Instantiate(ItemPrefab, npcContainer.transform);
		InventoryItemUi item = go.GetComponent<InventoryItemUi>();

		if (npc.shopType == SONpcs.ShopType.isWeaponSmith)
		{
			NpcTypeText.text = "Weapon Smith Npc";
			Weapons weapon = go.AddComponent<Weapons>();
			weapon.weaponBaseRef = (SOWeapons)npc.weaponSmithShopItems[Utilities.GetRandomNumber(npc.weaponSmithShopItems.Count)];
			item.weaponBaseRef = weapon.weaponBaseRef;
			weapon.currentStackCount = 1;
			weapon.Initilize(Utilities.SetRarity(), playerLevel);
		}
		if (npc.shopType == SONpcs.ShopType.isArmorer)
		{
			NpcTypeText.text = "Armor Smith Npc";
			Armors armor = go.AddComponent<Armors>();
			armor.armorBaseRef = (SOArmors)npc.armorerShopItems[Utilities.GetRandomNumber(npc.armorerShopItems.Count)];
			item.armorBaseRef = armor.armorBaseRef;
			armor.currentStackCount = 1;
			armor.Initilize(Utilities.SetRarity(), playerLevel);
		}
		if (npc.shopType == SONpcs.ShopType.isGoldSmith)
		{
			NpcTypeText.text = "Gold Smith Npc";
			Accessories accessory = go.AddComponent<Accessories>();
			accessory.accessoryBaseRef = (SOAccessories)npc.goldSmithShopItems[Utilities.GetRandomNumber(npc.goldSmithShopItems.Count)];
			item.accessoryBaseRef = accessory.accessoryBaseRef;
			accessory.currentStackCount = 1;
			accessory.Initilize(Utilities.SetRarity(), playerLevel);
		}
		if (npc.shopType == SONpcs.ShopType.isGeneralStore)
		{
			NpcTypeText.text = "General Store Npc";
			Consumables consumable = go.AddComponent<Consumables>();
			consumable.consumableBaseRef = (SOConsumables)npc.generalStoreItems[Utilities.GetRandomNumber(npc.generalStoreItems.Count)];
			item.consumableBaseRef = consumable.consumableBaseRef;
			consumable.currentStackCount = 3;
			consumable.Initilize(Utilities.SetRarity(), playerLevel);
		}

		item.Initilize();
		avalableShopItemsList.Add(item);
	}

	//move shops items between NPC container/ui container
	private void MoveShopItemsToUi()
	{
		for (int i = 0; i < avalableShopItemsList.Count; i++)
		{
			InventorySlotUi slot = PlayerInventoryUi.Instance.shopSlots[i].GetComponent<InventorySlotUi>();
			slot.AddItemToSlot(avalableShopItemsList[i]);
		}
	}
	private void MoveShopItemsToContainer()
	{
		avalableShopItemsList.Clear();

		foreach (GameObject obj in PlayerInventoryUi.Instance.shopSlots)
		{
			InventorySlotUi slot = obj.GetComponent<InventorySlotUi>();
			if (slot.IsSlotEmpty()) continue;

			avalableShopItemsList.Add(slot.itemInSlot); //add new items
			slot.itemInSlot.transform.SetParent(npcContainer.transform);
			slot.RemoveItemFromSlot();
		}
	}

	//quest npc functions
	public void GenerateNewQuests()
	{
		NpcTypeText.text = "Quest Npc";
		for (int i = avalableQuestList.Count; i > 0; i--)
		{
			if (!avalableQuestList[i - 1].isCurrentlyActiveQuest)
				Destroy(avalableQuestList[i - 1].gameObject);
		}
		avalableQuestList.Clear();

		for (int i = 0; i < 5; i++)
			GenerateQuest();
	}
	public void GenerateQuest()
	{
		GameObject go = Instantiate(questPrefab, npcContainer.transform);
		QuestSlotsUi quest = go.GetComponent<QuestSlotsUi>();

		int percentage = Utilities.GetRandomNumber(101);
		if (percentage >= 86)
			quest.InitilizeBossKillQuest();
		else if (percentage >= 36 && percentage < 86)
			quest.InitilizeKillQuest();
		else
			quest.InitilizeItemHandInQuest();

		PlayerJournalUi.OnNewQuestAccepted += OnQuestAccepted;
		PlayerJournalUi.OnNewQuestAccepted += quest.OnQuestAccepted;

		avalableQuestList.Add(quest);
	}
	public void RefreshThisNpcsQuests() //function delegated to button in ui
	{
		GenerateNewQuests();
		MoveQuestsToUi();
	}

	//move quests between NPC container/ui container
	private void MoveQuestsToUi()
	{
		foreach (QuestSlotsUi quest in avalableQuestList)
			quest.transform.SetParent(PlayerJournalUi.Instance.avalableQuestContainer.transform);
	}
	private void MoveQuestsToContainer()
	{
		foreach (QuestSlotsUi quest in avalableQuestList)
			quest.transform.SetParent(npcContainer.transform);
	}

	public void OnQuestAccepted(QuestSlotsUi quest)
	{
		avalableQuestList.Remove(quest);
		PlayerJournalUi.OnNewQuestAccepted -= quest.OnQuestAccepted;
	}
}
