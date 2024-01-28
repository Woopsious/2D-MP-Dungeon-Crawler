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

public class InventorySlot : MonoBehaviour, IDropHandler
{
	public static event Action<InventoryItem, InventorySlot> onItemEquip;

	public SlotType slotType;
	public enum SlotType
	{
		generic, weaponMain, weaponOffhand, helmet, chestpiece, legs, consumables, necklace, ringOne, ringTwo, artifact, ability
	}

	public int slotIndex;
	public InventoryItem itemInSlot;

	public void Start()
	{
		slotIndex = transform.GetSiblingIndex();
	}

	public void OnDrop(PointerEventData eventData)
	{
		GameObject droppeditem = eventData.pointerDrag;
		InventoryItem item = droppeditem.GetComponent<InventoryItem>();
		InventorySlot oldInventorySlot = item.parentAfterDrag.GetComponent<InventorySlot>();

		if (!IsCorrectSlotType(item)) return;

		if (!IsSlotEmpty()) //swap slot data
		{
			if (IsItemInSlotStackable() && IsItemInSlotSameAs(item))
			{
				PlayerInventoryManager.Instance.AddToStackCount(this, item);
				if (item.currentStackCount > 0) return;
			}
			else //swapping items
			{
				if (!oldInventorySlot.IsCorrectSlotType(itemInSlot)) return; //stops swapping of equipped items when they are wrong type

				itemInSlot.transform.SetParent(item.parentAfterDrag, false);
				itemInSlot.inventorySlotIndex = item.inventorySlotIndex;
				oldInventorySlot.itemInSlot = itemInSlot;
				oldInventorySlot.UpdateSlotSize();
				oldInventorySlot.CheckIfItemInEquipmentSlot();
			}
		}
		else //set ref of old parent slot to null
		{
			oldInventorySlot.itemInSlot = null;
			oldInventorySlot.CheckIfItemInEquipmentSlot();
		}

		//set new slot data
		item.parentAfterDrag = transform;
		item.inventorySlotIndex = slotIndex;
		itemInSlot = item;
		UpdateSlotSize();
		CheckIfItemInEquipmentSlot();
	}
	public void CheckIfItemInEquipmentSlot()
	{
		if (slotType == SlotType.generic) return;
		onItemEquip?.Invoke(itemInSlot, this);
	}
	public void UpdateSlotSize()
	{
		if (itemInSlot.itemType == InventoryItem.ItemType.isWeapon)
			itemInSlot.uiItemImage.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 100);
		else
			itemInSlot.uiItemImage.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
	}

	public bool IsSlotEmpty()
	{
		if (GetComponentInChildren<InventoryItem>() == null)
			return true;
		else return false;
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
		if (slotType == SlotType.generic)
			return true;
		if (item.itemType == InventoryItem.ItemType.isConsumable && slotType == SlotType.consumables)
			return true;
		else if (item.itemType == InventoryItem.ItemType.isWeapon)
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
		else if (item.itemType == InventoryItem.ItemType.isArmor)
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
}

