using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotDataUi : MonoBehaviour, IDropHandler
{
	public static event Action<InventorySlotDataUi, InventoryItemUi> OnItemEquip;
	public static event Action<InventorySlotDataUi, InventoryItemUi> OnHotbarItemEquip;
	public static event Action<InventoryItemUi> OnNewItemToEnchant;

	public SlotType slotType;
	public enum SlotType
	{
		playerInventory, weaponMain, weaponOffhand, helmet, chestpiece, legs, consumables, 
		necklace, ringOne, ringTwo, artifact, ability, equippedAbilities, shopSlot,
		weaponStorage, armourStorage, accessoryStorage, consumablesStorage, enchantItemSlot
	}

	public int slotIndex;
	public InventoryItemUi itemInSlot;

	//slot indexing
	public void SetSlotIndex()
	{
		slotIndex = transform.GetSiblingIndex();
	}

	//item equipping for on drop and via events/code
	public void OnDrop(PointerEventData eventData)
	{
		GameObject droppeditem = eventData.pointerDrag;
		InventoryItemUi item = droppeditem.GetComponent<InventoryItemUi>();

		EquipItemToSlot(item);
	}
	public void EquipItemToSlot(InventoryItemUi item) //dragging items
	{
		InventorySlotDataUi oldInventorySlot = item.parentAfterDrag.GetComponent<InventorySlotDataUi>();
		if (!IsCorrectSlotType(item)) return;
		if (IsNewSlotSameAsOldSlot(oldInventorySlot)) return;

		//Abilities
		if (slotType == SlotType.equippedAbilities)
		{
			if (PlayerHotbarUi.Instance.IsAbilityAlreadyEquipped(item.abilityBaseRef)) //only 1 copy of ability equipable
				return;

			CheckIfItemInEquipmentSlot(item);
			return;
		}

		//Items
		if (IsSlotEmpty() && IsShopSlot() || IsSlotEmpty() && oldInventorySlot.IsShopSlot()) //buying/selling items in empty slots
		{
			if (IsShopSlot() && oldInventorySlot.IsPlayerInventorySlot())
				PlayerInventoryUi.Instance.OnItemSell(item, this);
			else if (oldInventorySlot.IsShopSlot() && IsPlayerInventorySlot())
				PlayerInventoryUi.Instance.OnItemTryBuy(item, this, oldInventorySlot);
		}
		else if (!IsSlotEmpty()) //logic when slot isnt empty
		{
			if (IsShopSlot() || item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsShopSlot()) //disable swapping of shop items
			{
				PlayerInventoryUi.Instance.OnItemCancelBuy(item, oldInventorySlot, "Slot not empty");
				return;
			}

			if (IsItemInSlotStackable() && IsItemInSlotSameAs(item)) //stacking items
			{
				PlayerInventoryUi.Instance.AddToStackCount(this, item);
				if (item.currentStackCount > 0) return;
			}
			else //swapping items
			{
				if (!oldInventorySlot.IsCorrectSlotType(itemInSlot)) return;
				oldInventorySlot.AddItemToSlot(itemInSlot);
				AddItemToSlot(item);
			}
		}
		else //logic when slot is empty
		{
			AddItemToSlot(item);
			oldInventorySlot.RemoveItemFromSlot();
		}
	}

	//types of item changes
	public void AddItemToSlot(InventoryItemUi item)
	{
		item.parentAfterDrag = transform;
		item.inventorySlot = this;
		item.inventorySlotIndex = slotIndex;

		itemInSlot = item;
		UpdateSlotSize();
		CheckIfItemInEquipmentSlot(item);
		CheckIfItemInEnchantmentSlot(item);
		item.transform.SetParent(transform);

		if (itemInSlot.abilityBaseRef != null)
			itemInSlot.GetComponent<Abilities>().UpdateToolTip(GameManager.Localplayer.playerStats);
		else
			itemInSlot.GetComponent<Items>().UpdateToolTip(GameManager.Localplayer.playerStats, IsShopSlot());
	}
	public void RemoveItemFromSlot()
	{
		UpdateSlotSize();
		itemInSlot = null;
		CheckIfItemInEquipmentSlot(itemInSlot);
		CheckIfItemInEnchantmentSlot(itemInSlot);
	}

	//unique enchant item
	public void EnchantItemInSlot()
	{
		Items item = itemInSlot.GetComponent<Items>();
		item.enchantmentLevel++;

		item.Initilize(itemInSlot.rarity, itemInSlot.level, item.enchantmentLevel);
		itemInSlot.Initilize();

		if (itemInSlot.abilityBaseRef != null)
			itemInSlot.GetComponent<Abilities>().UpdateToolTip(GameManager.Localplayer.playerStats);
		else
			itemInSlot.GetComponent<Items>().UpdateToolTip(GameManager.Localplayer.playerStats, IsShopSlot());
	}

	//other checks
	public void CheckIfItemInEquipmentSlot(InventoryItemUi item)
	{
		if (!IsPlayerEquipmentSlot()) return;

		if (item != null)
			item.PlayItemEquipSound();

		if (IsHotbarSlot())
			OnHotbarItemEquip?.Invoke(this, item);
		else
			OnItemEquip?.Invoke(this, item);
	}
	public void CheckIfItemInEnchantmentSlot(InventoryItemUi item)
	{
		if (slotType == SlotType.enchantItemSlot)
			OnNewItemToEnchant?.Invoke(item);
	}
	public void UpdateSlotSize()
	{
		if (itemInSlot.type == SOItems.ItemType.isWeapon)
			itemInSlot.uiItemImage.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 100);
		else
			itemInSlot.uiItemImage.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
	}

	//bool checks
	public bool IsSlotEmpty()
	{
		if (GetComponentInChildren<InventoryItemUi>() == null)
			return true;
		else
			return false;
	}
	private bool IsItemInSlotStackable()
	{
		InventoryItemUi itemInSlot = GetComponentInChildren<InventoryItemUi>();
		if (itemInSlot.isStackable)
			return true;
		else return false;
	}
	public bool IsItemInSlotSameAs(InventoryItemUi Item)
	{
		InventoryItemUi itemInSlot = GetComponentInChildren<InventoryItemUi>();
		if (itemInSlot.itemName == Item.itemName)
			return true;
		else return false;
	}
	private bool IsCorrectSlotType(InventoryItemUi item)
	{
		//enchanter checks
		if (item.type != SOItems.ItemType.isConsumable && item.type != SOItems.ItemType.isAbility &&
			slotType == SlotType.enchantItemSlot)
				return true;

		//ability checks
		if (item.type == SOItems.ItemType.isAbility)
		{
			if (item.type == SOItems.ItemType.isAbility && slotType == SlotType.equippedAbilities)
				return true;
			else
				return false;
		}

		//shop checks
		if (IsShopSlot() || item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsShopSlot())
		{
			if (IsShopSlot() && item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
				return true;
			if (IsPlayerInventorySlot() && item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsShopSlot())
				return true;
			else return false;
		}

		//unequipping/moving item checks
		if (IsPlayerInventorySlot() && item.type != SOItems.ItemType.isAbility)
			return true;
		if (IsNewSlotTypeSameAsOldSlotType(item))
			return true;
		else if (item.type == SOItems.ItemType.isConsumable && slotType == SlotType.consumables)
			return true;

		//storage checks
		if (slotType == SlotType.weaponStorage || slotType == SlotType.armourStorage || 
			slotType == SlotType.accessoryStorage || slotType == SlotType.consumablesStorage)
		{
			if (IsCorrectStorageSlot(item))
				return true;
		}

		//equipping item checks: level/class restriction
		if (!IsCorrectLevel(item))
			return false;

		if (item.type == SOItems.ItemType.isAccessory)
		{
			Accessories accessory = item.GetComponent<Accessories>();

			if (accessory.accessorySlot == Accessories.AccessorySlot.necklace && slotType == SlotType.necklace)
				return true;
			else if (accessory.accessorySlot == Accessories.AccessorySlot.ring && slotType == SlotType.ringOne ||
			accessory.accessorySlot == Accessories.AccessorySlot.ring && slotType == SlotType.ringTwo)
				return true;
			else
				return false;
		}
		else if (!CheckClassRestriction((int)item.classRestriction))
			return false;

		if (item.type == SOItems.ItemType.isWeapon)
		{
			SOWeapons SOweapon = item.GetComponent<Weapons>().weaponBaseRef;

			if (SOweapon.weaponGripType == SOWeapons.WeaponGripType.isMainHand && slotType == SlotType.weaponMain)
				return true;
			else if (SOweapon.weaponGripType == SOWeapons.WeaponGripType.isOffhand && slotType == SlotType.weaponOffhand)
				return true;
			else if (SOweapon.weaponGripType == SOWeapons.WeaponGripType.isBoth)
				return true;
			else
				return false;
		}
		else if (item.type == SOItems.ItemType.isArmor)
		{
			Armors armor = item.GetComponent<Armors>();

			if (armor.armorSlot == Armors.ArmorSlot.helmet && slotType == SlotType.helmet)
				return true;
			if (armor.armorSlot == Armors.ArmorSlot.chestpiece && slotType == SlotType.chestpiece)
				return true;
			if (armor.armorSlot == Armors.ArmorSlot.legs && slotType == SlotType.legs)
				return true;
			else return false;
		}

		Debug.LogError("slot checks failed");
		return false;
	}
	private bool IsNewSlotSameAsOldSlot(InventorySlotDataUi oldInventorySlot)
	{
		if (oldInventorySlot == this)
			return true;
		else
			return false;
	}
	private bool IsNewSlotTypeSameAsOldSlotType(InventoryItemUi item)
	{
		if (slotType == item.parentAfterDrag.GetComponent<InventorySlotDataUi>().slotType)
			return true;
		else return false;
	}
	private bool IsCorrectLevel(InventoryItemUi item)
	{
		if (GameManager.Localplayer.GetComponent<EntityStats>().entityLevel >= item.level)
			return true;
		else return false;
	}
	private bool IsCorrectStorageSlot(InventoryItemUi item)
	{
		if (item.type == SOItems.ItemType.isWeapon && slotType == SlotType.weaponStorage &&
			item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
		{
			return true;
		}
		else if (item.type == SOItems.ItemType.isArmor && slotType == SlotType.armourStorage &&
			item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
		{
			return true;
		}
		else if (item.type == SOItems.ItemType.isAccessory && slotType == SlotType.accessoryStorage &&
			item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
		{
			return true;
		}
		else if (item.type == SOItems.ItemType.isConsumable && slotType == SlotType.consumablesStorage &&
			item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
		{
			return true;
		}
		else return false;
	}
	private bool CheckClassRestriction(int itemClassRestrictionNum)
	{
		int classRestrictionNum = (int)PlayerClassesUi.Instance.currentPlayerClass.classRestriction;
		if (classRestrictionNum >= itemClassRestrictionNum)
			return true;
		else
			return false;
	}

	//slot type checks
	public bool IsStorageSlot()
	{
		if (slotType == SlotType.weaponStorage || slotType == SlotType.armourStorage ||
			slotType == SlotType.accessoryStorage || slotType == SlotType.consumablesStorage)
			return true;
		else return false;
	}
	public bool IsHotbarSlot()
	{
		if (slotType == SlotType.consumables || slotType == SlotType.equippedAbilities)
			return true;
		else return false;
	}
	public bool IsPlayerInventorySlot()
	{
		if (slotType == SlotType.playerInventory)
			return true;
		else return false;
	}
	public bool IsPlayerEquipmentSlot()
	{
		if (slotType == SlotType.weaponMain || slotType == SlotType.weaponOffhand || 
			slotType == SlotType.helmet || slotType == SlotType.chestpiece || slotType == SlotType.legs ||
			slotType == SlotType.necklace || slotType == SlotType.ringOne || slotType == SlotType.ringTwo ||
			slotType == SlotType.consumables || slotType == SlotType.equippedAbilities)
			return true;
		else return false;
	}
	public bool IsShopSlot()
	{
		if (slotType == SlotType.shopSlot)
			return true;
		else return false;
	}

	public int GetEquipmentSlotIndex()
	{
		if (slotType == SlotType.weaponMain)
			return 0;
		if (slotType == SlotType.weaponOffhand)
			return 1;
		if (slotType == SlotType.helmet)
			return 2;
		if (slotType == SlotType.chestpiece)
			return 3;
		if (slotType == SlotType.legs)
			return 4;
		if (slotType == SlotType.necklace)
			return 5;
		if (slotType == SlotType.ringOne)
			return 6;
		if (slotType == SlotType.ringTwo)
			return 7;
		if (slotType == SlotType.consumables)
			return 8;

		Debug.LogError("failed to find correct equipment slot type to match with index");
		return -1;
	}
}

