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

public class InventorySlotUi : MonoBehaviour, IDropHandler
{
	public static event Action<InventoryItem, InventorySlotUi> OnItemEquip;
	public static event Action<InventoryItem, InventorySlotUi> OnHotbarItemEquip;

	public static event Action<InventoryItem> OnItemBuyEvent;
	public static event Action<InventoryItem> OnItemSellEvent;

	public SlotType slotType;
	public enum SlotType
	{
		generic, weaponMain, weaponOffhand, helmet, chestpiece, legs, consumables, 
		necklace, ringOne, ringTwo, artifact, ability, equippedAbilities, shopSlot, shopSellSlot, shopBuySlot
	}

	public int slotIndex;
	public InventoryItem itemInSlot;

	public void SetSlotIndex()
	{
		slotIndex = transform.GetSiblingIndex();
	}

	//item equipping for on drop and via events/code
	public void OnDrop(PointerEventData eventData)
	{
		GameObject droppeditem = eventData.pointerDrag;
		InventoryItem item = droppeditem.GetComponent<InventoryItem>();

		DragEquipItemToSlot(item);
	}
	public void DragEquipItemToSlot(InventoryItem item) //dragging items
	{
		InventorySlotUi oldInventorySlot = item.parentAfterDrag.GetComponent<InventorySlotUi>();

		if (!IsCorrectSlotType(item)) return;
		if (IsNewSlotSameAsOldSlot(item)) return;

		if (slotType == SlotType.equippedAbilities)
		{
			if (PlayerHotbarUi.Instance.equippedAbilities.Contains(item.abilityBaseRef)) //only 1 copy of ability equipable
				return;

			CheckIfItemInEquipmentSlot(item);
			return;
		}

		if (!IsSlotEmpty()) //swap slot data
		{
			if (IsShopSlot() || item.parentAfterDrag.GetComponent<InventorySlotUi>().IsShopSlot()) return; //disable swapping of shop items

			if (IsItemInSlotStackable() && IsItemInSlotSameAs(item)) //stacking items
			{
				PlayerInventoryUi.Instance.AddToStackCount(this, item);
				if (item.currentStackCount > 0) return;
			}
			else //swapping items
			{
				if (!oldInventorySlot.IsCorrectSlotType(itemInSlot)) return;
				SwapItemInSlot(item, oldInventorySlot);
			}
		}
		else if (IsShopSlot() || oldInventorySlot.IsShopSlot())
		{
			if (IsShopSlot() && oldInventorySlot.slotType == SlotType.generic)
				OnItemSellEvent?.Invoke(item);
			else if (oldInventorySlot.IsShopSlot() && slotType == SlotType.generic)
				OnItemBuyEvent?.Invoke(item);
		}

		AddItemToSlot(item);
		oldInventorySlot.RemoveItemFromSlot();
	}
	public void EquipItemToSlot(InventoryItem item) //ui context menu
	{
		AddItemToSlot(item);
	}

	//types of item changes
	public void AddItemToSlot(InventoryItem item)
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
	private void SwapItemInSlot(InventoryItem item, InventorySlotUi oldInventorySlot)
	{
		itemInSlot.transform.SetParent(item.parentAfterDrag, false);
		itemInSlot.inventorySlotIndex = item.inventorySlotIndex;
		oldInventorySlot.itemInSlot = itemInSlot;
		oldInventorySlot.UpdateSlotSize();
		oldInventorySlot.CheckIfItemInEquipmentSlot(oldInventorySlot.itemInSlot);
	}

	public void CheckIfItemInEquipmentSlot(InventoryItem item)
	{
		if (slotType == SlotType.generic || IsShopSlot()) return;

		if (slotType == SlotType.consumables || slotType == SlotType.equippedAbilities)
			OnHotbarItemEquip?.Invoke(item, this);
		else
			OnItemEquip?.Invoke(item, this);
	}
	public void UpdateSlotSize()
	{
		if (itemInSlot.itemType == InventoryItem.ItemType.isWeapon)
			itemInSlot.uiItemImage.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 100);
		else
			itemInSlot.uiItemImage.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
	}

	//bool checks
	public bool IsNewSlotSameAsOldSlot(InventoryItem item)
	{
		if (item.parentAfterDrag == this)
			return true;
		else
			return false;
	}
	public bool IsSlotEmpty()
	{
		if (GetComponentInChildren<InventoryItem>() == null)
			return true;
		else
			return false;
	}
	public bool IsItemInSlotStackable()
	{
		InventoryItem itemInSlot = GetComponentInChildren<InventoryItem>();
		if (itemInSlot.isStackable)
			return true;
		else return false;
	}
	public bool IsItemInSlotSameAs(InventoryItem Item)
	{
		InventoryItem itemInSlot = GetComponentInChildren<InventoryItem>();
		if (itemInSlot.itemName == Item.itemName)
			return true;
		else return false;
	}
	public bool IsCorrectSlotType(InventoryItem item)
	{
		//ability checks
		if (item.itemType == InventoryItem.ItemType.isAbility)
		{
			if (item.itemType == InventoryItem.ItemType.isAbility && slotType == SlotType.ability)
				return false;
			if (item.itemType == InventoryItem.ItemType.isAbility && slotType == SlotType.equippedAbilities)
				return true;
		}

		//shop checks
		if (IsShopSlot() || item.parentAfterDrag.GetComponent<InventorySlotUi>().slotType == SlotType.generic)
			return true;
		if (item.parentAfterDrag.GetComponent<InventorySlotUi>().IsShopSlot() || slotType == SlotType.generic)
			return true;

		//equipment/class restriction checks
		if (item.itemType == InventoryItem.ItemType.isConsumable && slotType == SlotType.consumables)
			return true;
		else if (item.itemType != InventoryItem.ItemType.isAbility && slotType == SlotType.generic)
			return true;
		else if (item.itemType == InventoryItem.ItemType.isWeapon && CheckClassRestriction((int)item.classRestriction))
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
		else if (item.itemType == InventoryItem.ItemType.isArmor && CheckClassRestriction((int)item.classRestriction))
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
		else if (item.itemType == InventoryItem.ItemType.isAccessory)
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
		int classRestrictionNum = (int)ClassesUi.Instance.currentPlayerClass.classRestriction;
		if (classRestrictionNum >= itemClassRestrictionNum)
			return true;
		else
			return false;
	}
}

