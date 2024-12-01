using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class InventoryItemUi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[Header("Debug settings")]
	public bool generateStatsOnStart;

	[Header("Ui settings")]
	public TMP_Text uiItemName;
	public Image uiItemImage;
	public TMP_Text uiItemLevel;
	public TMP_Text uiItemStackCount;
	public TMP_Text uiCantUtilizeItemNotif;

	[Header("Parent Slot")]
	[HideInInspector] public InventorySlotDataUi inventorySlot;
	[HideInInspector] public Transform parentAfterDrag;

	[Header("Item Base Ref")]
	public SOAbilities abilityBaseRef;
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;
	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}

	[Header("Item Info")]
	public string itemName;
	public Sprite itemSprite;
	private AudioHandler audioHandler;
	public int itemPrice;
	public int itemLevel;
	public int itemEnchantmentLevel;
	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}

	[Header("Item Dynamic Info")]
	public int inventorySlotIndex;
	public bool isStackable;
	public int maxStackCount;
	public int currentStackCount;

	private void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}
	private void OnDisable()
	{
		//subbing to events happens when abilities/consumables are equipped
		PlayerInfoUi.playerInstance.playerStats.OnHealthChangeEvent -= CheckIfCanUseConsumables;
		PlayerInfoUi.playerInstance.playerStats.OnManaChangeEvent -= CheckIfCanUseConsumables;
		PlayerInfoUi.playerInstance.playerStats.OnManaChangeEvent -= UpdateCanCastAbility;
	}

	//set item data
	public void Initilize()
	{
		audioHandler = GetComponent<AudioHandler>();

		if (GetComponent<Items>() != null)
			SetUpItems(GetComponent<Items>());
		else if (GetComponent<Abilities>() != null)
			SetUpAbilities(GetComponent<Abilities>());

		SetUpUi();
	}
	private void SetUpItems(Items item)
	{
		name = item.itemName;
		itemName = item.itemName;
		itemSprite = item.itemSprite;
		itemPrice = item.itemPrice;
		itemLevel = item.itemLevel;
		itemEnchantmentLevel = item.itemEnchantmentLevel;
		rarity = (Rarity)item.rarity;
		itemType = (ItemType)item.itemType;
		classRestriction = GetClassRestriction();

		isStackable = IsStackable();
		maxStackCount = GetMaxStackCount();
		currentStackCount = item.currentStackCount;

		if (transform.parent == null || transform.parent.GetComponent<InventorySlotDataUi>() == null) return;
		InventorySlotDataUi slotDataUi = transform.parent.GetComponent<InventorySlotDataUi>();

		if (slotDataUi.slotType == InventorySlotDataUi.SlotType.consumables)
		{
			PlayerInfoUi.playerInstance.playerStats.OnHealthChangeEvent += CheckIfCanUseConsumables;
			PlayerInfoUi.playerInstance.playerStats.OnManaChangeEvent += CheckIfCanUseConsumables;
		}
	}
	private void SetUpAbilities(Abilities ability)
	{
		name = ability.abilityName;
		itemName = ability.abilityName;
		itemSprite = ability.abilitySprite;
		itemType = ItemType.isAbility;

		isStackable = false;
		maxStackCount = 1;
		currentStackCount = 1;

		ability.abilityImage = uiItemImage;
		uiItemImage.type = Image.Type.Filled;
		uiItemImage.fillMethod = Image.FillMethod.Radial360;

		if (transform.parent == null) return;
		InventorySlotDataUi slotDataUi = transform.parent.GetComponent<InventorySlotDataUi>();

		if (slotDataUi.slotType == InventorySlotDataUi.SlotType.equippedAbilities)
			PlayerInfoUi.playerInstance.playerStats.OnManaChangeEvent += UpdateCanCastAbility;
	}
	private ClassRestriction GetClassRestriction()
	{
		if (weaponBaseRef != null) return (ClassRestriction)weaponBaseRef.classRestriction;
		else if (armorBaseRef != null) return (ClassRestriction)armorBaseRef.classRestriction;
		else if (accessoryBaseRef != null) return ClassRestriction.light;
		else return ClassRestriction.light;
	}
	private bool IsStackable()
	{
		if (weaponBaseRef != null) return weaponBaseRef.isStackable;
		else if (armorBaseRef != null) return armorBaseRef.isStackable;
		else if (accessoryBaseRef != null) return accessoryBaseRef.isStackable;
		else return consumableBaseRef.isStackable;
	}
	private int GetMaxStackCount()
	{
		if (weaponBaseRef != null) return weaponBaseRef.MaxStackCount;
		else if (armorBaseRef != null) return armorBaseRef.MaxStackCount;
		else if (accessoryBaseRef != null) return accessoryBaseRef.MaxStackCount;
		else return consumableBaseRef.MaxStackCount;
	}
	private void SetUpUi()
	{
		gameObject.name = itemName;
		uiItemName.text = itemName;
		if (itemEnchantmentLevel == 0 || itemType == ItemType.isConsumable)
		{
			uiItemName.text = itemName;
			uiItemLevel.text = $"LVL: {itemLevel}";
		}
		else
		{
			uiItemName.text = $"Enchanted {itemName}";
			uiItemLevel.text = $"LVL: {itemLevel} +{itemEnchantmentLevel}";
		}

		uiItemImage.sprite = itemSprite;
		uiItemStackCount.text = currentStackCount.ToString();

		if (rarity == Rarity.isRare)
			SetColour(Color.blue);
		else if (rarity == Rarity.isEpic)
			SetColour(Color.magenta);
		else if (rarity == Rarity.isLegendary)
			SetColour(new Color(1f, 0.4f, 0f));
		else
			SetColour(Color.white);
	}
	private void SetColour(Color colour)
	{
		uiItemName.color = colour;
		uiItemLevel.color = colour;
		uiItemStackCount.color = colour;
	}
	public void UpdateToolTip(bool isShopSlot)
	{

	}

	//functions for dragging
	public void OnBeginDrag(PointerEventData eventData)
	{
		parentAfterDrag = transform.parent;
		transform.SetParent(transform.root);
		transform.SetAsLastSibling();
		uiItemImage.raycastTarget = false;
	}
	public void OnDrag(PointerEventData eventData)
	{
		transform.position = Input.mousePosition;
	}
	public void OnEndDrag(PointerEventData eventData)
	{
		transform.SetParent(parentAfterDrag);
		uiItemImage.raycastTarget = true;
	}

	public void PlayItemEquipSound()
	{
		if (weaponBaseRef != null)
			audioHandler.PlayAudio(weaponBaseRef.equipItemSfx);
		if (armorBaseRef != null)
			audioHandler.PlayAudio(armorBaseRef.equipItemSfx);
		if (accessoryBaseRef != null)
			audioHandler.PlayAudio(accessoryBaseRef.equipItemSfx);
		if (consumableBaseRef != null)
			audioHandler.PlayAudio(consumableBaseRef.equipItemSfx);
	}

	//update data + event listners
	public void CheckIfCanEquipItem()
	{
		if (itemType == ItemType.isAbility || itemType == ItemType.isConsumable) return;
		uiCantUtilizeItemNotif.gameObject.SetActive(true);

		if (PlayerInfoUi.playerInstance.GetComponent<EntityStats>().entityLevel < itemLevel)
			uiCantUtilizeItemNotif.text = "Cant Equip \n low level";
		else if (itemType == ItemType.isWeapon)
		{
			if ((int)PlayerClassesUi.Instance.currentPlayerClass.classRestriction < (int)weaponBaseRef.classRestriction)
				uiCantUtilizeItemNotif.text = "Cant Equip \n too heavy";
			else
				uiCantUtilizeItemNotif.gameObject.SetActive(false);
		}
		else if (itemType == ItemType.isArmor)
		{
			if ((int)PlayerClassesUi.Instance.currentPlayerClass.classRestriction < (int)armorBaseRef.classRestriction)
				uiCantUtilizeItemNotif.text = "Cant Equip \n too heavy";
			else
				uiCantUtilizeItemNotif.gameObject.SetActive(false);
		}
		else
			uiCantUtilizeItemNotif.gameObject.SetActive(false);
	}
	public void CheckIfCanUseConsumables(int maxValue, int currentValue)
	{
		if (consumableBaseRef == null) return;

		Consumables consumable = GetComponent<Consumables>();

		if (consumable.consumableBaseRef.consumableType == SOConsumables.ConsumableType.healthRestoration)
		{
			if (consumable.EntityHealthFull(PlayerInfoUi.playerInstance.playerStats))
			{
				uiCantUtilizeItemNotif.text = "max health";
				uiCantUtilizeItemNotif.gameObject.SetActive(true);
			}
			else
				uiCantUtilizeItemNotif.gameObject.SetActive(false);
		}
		else if (consumable.consumableBaseRef.consumableType == SOConsumables.ConsumableType.manaRestoration)
		{
			if (consumable.EntityManaFull(PlayerInfoUi.playerInstance.playerStats))
			{
				uiCantUtilizeItemNotif.text = "max mana";
				uiCantUtilizeItemNotif.gameObject.SetActive(true);
			}
			else
				uiCantUtilizeItemNotif.gameObject.SetActive(false);
		}
	}
	public void UpdateCanCastAbility(int maxMana, int currentMana)
	{
		if (abilityBaseRef == null) return;

		Abilities equippedAbility = GetComponent<Abilities>();

		if (!equippedAbility.CanAffordManaCost(PlayerInfoUi.playerInstance.playerStats))
		{
			uiCantUtilizeItemNotif.text = "Not enough mana";
			uiCantUtilizeItemNotif.gameObject.SetActive(true);
		}
		else
			uiCantUtilizeItemNotif.gameObject.SetActive(false);
	}
	public void SetStackCounter(int newCount)
	{
		currentStackCount = newCount;
		uiItemStackCount.text = currentStackCount.ToString();
		GetComponent<Items>().currentStackCount = currentStackCount;
	}
	public void IncreaseStackCounter()
	{
		currentStackCount++;
		uiItemStackCount.text = currentStackCount.ToString();
		GetComponent<Items>().currentStackCount = currentStackCount;
	}
	public void DecreaseStackCounter()
	{
		currentStackCount--;
		uiItemStackCount.text = currentStackCount.ToString();
		GetComponent<Items>().currentStackCount = currentStackCount;

		if (currentStackCount <= 0)
			Destroy(gameObject);
	}

	//Debug functions
	private void GenerateStatsOnStart()
	{
		if (weaponBaseRef != null)
		{
			Weapons weapon = gameObject.AddComponent<Weapons>();
			weapon.weaponBaseRef = weaponBaseRef;
			weapon.Initilize(Utilities.SetRarity(0), Utilities.GetRandomNumber(20), 0);
		}
		if (armorBaseRef != null)
		{
			Armors armor = gameObject.AddComponent<Armors>();
			armor.armorBaseRef = armorBaseRef;
			armor.Initilize(Utilities.SetRarity(0), Utilities.GetRandomNumber(20), 0);
		}
		if (accessoryBaseRef != null)
		{
			Accessories accessory = gameObject.AddComponent<Accessories>();
			accessory.accessoryBaseRef = accessoryBaseRef;
			accessory.Initilize(Utilities.SetRarity(0), Utilities.GetRandomNumber(20), 0);
		}
		if (consumableBaseRef != null)
		{
			Consumables consumable = gameObject.AddComponent<Consumables>();
			consumable.consumableBaseRef = consumableBaseRef;
			consumable.Initilize(Utilities.SetRarity(0), Utilities.GetRandomNumber(20), 0);
		}
		if (abilityBaseRef != null)
		{
			Abilities ability = gameObject.AddComponent<Abilities>();
			ability.abilityBaseRef = abilityBaseRef;
			ability.Initilize();
		}
		Initilize();
	}
}
