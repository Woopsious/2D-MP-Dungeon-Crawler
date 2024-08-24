using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class NpcHandler : MonoBehaviour, IInteractable
{
	public SONpcs npc;

	private Animator animator;
	private SpriteRenderer spriteRenderer;

	[Header("Quests")]
	public GameObject questPrefab;
	public List<QuestDataSlotUi> avalableQuestList = new List<QuestDataSlotUi>();

	[Header("Shop Items")]
	public GameObject ItemPrefab;
	public List<InventoryItemUi> avalableShopItemsList = new List<InventoryItemUi>();

	[Header("Container Ref")]
	public GameObject npcContainer;

	[Header("Ui Notif")]
	public TMP_Text NpcTypeText;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}
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
		UpdateNpcTypeText();
	}

	private void Initilize()
	{
		animator.SetBool("isIdle", true);
		spriteRenderer.sprite = npc.sprite;
		name = npc.entityName;

		NpcTypeText.transform.SetParent(MainMenuManager.Instance.runtimeUiContainer.transform);
		NpcTypeText.transform.SetAsFirstSibling();
		if (npc.npcType == SONpcs.NPCType.isQuestNpc)
			GenerateNewQuests();
		else if (npc.npcType == SONpcs.NPCType.isShopNpc)
			GenerateShopItems();
	}

	public void Interact(PlayerController player)
	{
		player.isInteractingWithNpc = true;

		if (npc.npcType == SONpcs.NPCType.isQuestNpc)
		{
			PlayerEventManager.ShowPlayerJournal();
			PlayerEventManager.ShowNpcJournal(this);
		}
		else if (npc.npcType == SONpcs.NPCType.isShopNpc)
		{
			PlayerEventManager.ShowPlayerInventory();
			PlayerEventManager.ShowNpcShopInventory(this);
		}
	}
	public void UnInteract(PlayerController player)
	{
		player.isInteractingWithNpc = false;

		if (npc.npcType == SONpcs.NPCType.isQuestNpc)
			PlayerEventManager.HideNpcJournal(this);
		else if (npc.npcType == SONpcs.NPCType.isShopNpc)
			PlayerEventManager.HideNpcShopInventory(this);
	}
	private void UpdateNpcTypeText()
	{
		if (!NpcTypeText.gameObject.activeInHierarchy) return;
		NpcTypeText.transform.position =
			Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + 1f, 0));
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
		QuestDataSlotUi quest = go.GetComponent<QuestDataSlotUi>();

		int percentage = Utilities.GetRandomNumber(100);
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

	//shop npc functions
	public void GenerateShopItems()
	{
		for (int i = avalableShopItemsList.Count; i > 0; i--)
			Destroy(avalableShopItemsList[i - 1].gameObject);
		avalableShopItemsList.Clear();

		//for now grab player level, later need a better way to do this if possible
		int playerLevel = FindObjectOfType<PlayerController>().GetComponent<EntityStats>().entityLevel;

		for (int i = 0; i < Utilities.GetRandomNumberBetween(npc.minNumOfShopItems, npc.maxNumOfShopItems); i++)
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
			weapon.weaponBaseRef = (SOWeapons)npc.weaponSmithShopItems[Utilities.GetRandomNumber(npc.weaponSmithShopItems.Count - 1)];
			item.weaponBaseRef = weapon.weaponBaseRef;
			weapon.currentStackCount = 1;
			weapon.Initilize(Utilities.SetRarity(), playerLevel);
		}
		if (npc.shopType == SONpcs.ShopType.isArmorer)
		{
			NpcTypeText.text = "Armor Smith Npc";
			Armors armor = go.AddComponent<Armors>();
			armor.armorBaseRef = (SOArmors)npc.armorerShopItems[Utilities.GetRandomNumber(npc.armorerShopItems.Count - 1)];
			item.armorBaseRef = armor.armorBaseRef;
			armor.currentStackCount = 1;
			armor.Initilize(Utilities.SetRarity(), playerLevel);
		}
		if (npc.shopType == SONpcs.ShopType.isGoldSmith)
		{
			NpcTypeText.text = "Gold Smith Npc";
			Accessories accessory = go.AddComponent<Accessories>();
			accessory.accessoryBaseRef = (SOAccessories)npc.goldSmithShopItems[Utilities.GetRandomNumber(npc.goldSmithShopItems.Count - 1)];
			item.accessoryBaseRef = accessory.accessoryBaseRef;
			accessory.currentStackCount = 1;
			accessory.Initilize(Utilities.SetRarity(), playerLevel);
		}
		if (npc.shopType == SONpcs.ShopType.isGeneralStore)
		{
			NpcTypeText.text = "General Store Npc";
			Consumables consumable = go.AddComponent<Consumables>();
			consumable.consumableBaseRef = (SOConsumables)npc.generalStoreItems[Utilities.GetRandomNumber(npc.generalStoreItems.Count - 1)];
			item.consumableBaseRef = consumable.consumableBaseRef;
			consumable.currentStackCount = 3;
			consumable.Initilize(Utilities.SetRarity(), playerLevel);
		}

		item.Initilize();
		avalableShopItemsList.Add(item);
	}

	public void OnQuestAccepted(QuestDataSlotUi quest)
	{
		avalableQuestList.Remove(quest);
		PlayerJournalUi.OnNewQuestAccepted -= quest.OnQuestAccepted;
	}
}
