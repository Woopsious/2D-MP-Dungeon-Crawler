using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics.Internal;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotDataUi : MonoBehaviour, IDropHandler
{
	public static event Action<InventoryItemUi, InventorySlotDataUi> OnItemEquip;
	public static event Action<InventoryItemUi, InventorySlotDataUi> OnHotbarItemEquip;

	public SlotType slotType;
	public enum SlotType
	{
		playerInventory, weaponMain, weaponOffhand, helmet, chestpiece, legs, consumables, 
		necklace, ringOne, ringTwo, artifact, ability, equippedAbilities, shopSlot,
		weaponStorage, armourStorage, accessoryStorage, consumablesStorage
	}

	public int slotIndex;
	public InventoryItemUi itemInSlot;

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

		if (slotType == SlotType.equippedAbilities)
		{
			if (PlayerHotbarUi.Instance.equippedAbilities.Contains(item.abilityBaseRef)) //only 1 copy of ability equipable
				return;

			CheckIfItemInEquipmentSlot(item);
			return;
		}

		if (!IsSlotEmpty()) //swap slot data
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
		else if (IsShopSlot() || oldInventorySlot.IsShopSlot())
		{
			if (IsShopSlot() && oldInventorySlot.IsPlayerInventorySlot())
				PlayerInventoryUi.Instance.OnItemSell(item, this);
			else if (oldInventorySlot.IsShopSlot() && IsPlayerInventorySlot())
				PlayerInventoryUi.Instance.OnItemTryBuy(item, this, oldInventorySlot);
		}
		else
		{
			AddItemToSlot(item);
			oldInventorySlot.RemoveItemFromSlot();
		}
	}

	//types of item changes
	public void AddItemToSlot(InventoryItemUi item)
	{
		item.parentAfterDrag = transform;
		item.inventorySlotIndex = slotIndex;

		itemInSlot = item;
		UpdateSlotSize();
		CheckIfItemInEquipmentSlot(item);
		item.transform.SetParent(transform);
	}
	public void RemoveItemFromSlot()
	{
		UpdateSlotSize();
		itemInSlot = null;
		CheckIfItemInEquipmentSlot(itemInSlot);
	}
	public void CheckIfItemInEquipmentSlot(InventoryItemUi item)
	{
		if (!IsPlayerEquipmentSlot()) return;

		if (slotType == SlotType.consumables || slotType == SlotType.equippedAbilities)
			OnHotbarItemEquip?.Invoke(item, this);
		else
			OnItemEquip?.Invoke(item, this);
	}
	public void UpdateSlotSize()
	{
		if (itemInSlot.itemType == InventoryItemUi.ItemType.isWeapon)
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
	public bool IsNewSlotSameAsOldSlot(InventorySlotDataUi oldInventorySlot)
	{
		if (oldInventorySlot == this)
			return true;
		else
			return false;
	}
	public bool IsNewSlotTypeSameAsOldSlotType(InventoryItemUi item)
	{
		if (slotType == item.parentAfterDrag.GetComponent<InventorySlotDataUi>().slotType)
			return true;
		else return false;
	}
	public bool IsItemInSlotStackable()
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
	public bool IsCorrectSlotType(InventoryItemUi item)
	{
		//ability checks
		if (item.itemType == InventoryItemUi.ItemType.isAbility)
		{
			if (item.itemType == InventoryItemUi.ItemType.isAbility && slotType == SlotType.equippedAbilities)
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
		if (IsPlayerInventorySlot() && item.itemType != InventoryItemUi.ItemType.isAbility)
			return true;
		if (IsNewSlotTypeSameAsOldSlotType(item))
			return true;
		else if (item.itemType == InventoryItemUi.ItemType.isConsumable && slotType == SlotType.consumables)
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
		{
			Debug.Log("Player Level Too Low");
			return false;
		}

		if (item.itemType == InventoryItemUi.ItemType.isAccessory)
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
		{
			Debug.Log("Equipment weight too heavy");
			return false;
		}

		if (item.itemType == InventoryItemUi.ItemType.isWeapon)
		{
			SOWeapons SOweapon = item.GetComponent<Weapons>().weaponBaseRef;

			if (SOweapon.weaponType == SOWeapons.WeaponType.isMainHand && slotType == SlotType.weaponMain)
				return true;
			else if (SOweapon.weaponType == SOWeapons.WeaponType.isOffhand && slotType == SlotType.weaponOffhand)
				return true;
			else if (SOweapon.weaponType == SOWeapons.WeaponType.isBoth)
				return true;
			else
				return false;
		}
		else if (item.itemType == InventoryItemUi.ItemType.isArmor)
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
		else
		{
			Debug.Log("slot checks failed");
			return false;
		}
	}
	public bool IsCorrectLevel(InventoryItemUi item)
	{
		if (PlayerInfoUi.playerInstance.GetComponent<EntityStats>().entityLevel >= item.itemLevel)
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
		if (slotType != SlotType.playerInventory && slotType != SlotType.shopSlot && slotType != SlotType.ability && 
			slotType != SlotType.weaponStorage && slotType != SlotType.armourStorage && 
			slotType != SlotType.accessoryStorage && slotType != SlotType.consumablesStorage)
			return true;
		else return false;
	}
	public bool IsCorrectStorageSlot(InventoryItemUi item)
	{
		if (item.itemType == InventoryItemUi.ItemType.isWeapon &&
			slotType == SlotType.weaponStorage && item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
			return true;
		else if (item.itemType == InventoryItemUi.ItemType.isArmor &&
			slotType == SlotType.armourStorage && item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
			return true;
		else if (item.itemType == InventoryItemUi.ItemType.isAccessory &&
			slotType == SlotType.accessoryStorage && item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
			return true;
		else if (item.itemType == InventoryItemUi.ItemType.isConsumable &&
			slotType == SlotType.consumablesStorage && item.parentAfterDrag.GetComponent<InventorySlotDataUi>().IsPlayerInventorySlot())
			return true;
		else return false;
	}
	public bool IsShopSlot()
	{
		if (slotType == SlotType.shopSlot)
			return true;
		else return false;
	}
	public bool CheckClassRestriction(int itemClassRestrictionNum)
	{
		int classRestrictionNum = (int)PlayerClassesUi.Instance.currentPlayerClass.classRestriction;
		if (classRestrictionNum >= itemClassRestrictionNum)
			return true;
		else
			return false;
	}
}
