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
	public static event Action<InventoryItemUi, InventorySlotUi> OnItemEquip;
	public static event Action<InventoryItemUi, InventorySlotUi> OnHotbarItemEquip;

	public static event Action<InventoryItemUi> OnItemBuyEvent;
	public static event Action<InventoryItemUi> OnItemSellEvent;

	public SlotType slotType;
	public enum SlotType
	{
		playerInventory, weaponMain, weaponOffhand, helmet, chestpiece, legs, consumables, 
		necklace, ringOne, ringTwo, artifact, ability, equippedAbilities, shopSlot
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

		DragEquipItemToSlot(item);
	}
	public void DragEquipItemToSlot(InventoryItemUi item) //dragging items
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
			if (IsShopSlot() && oldInventorySlot.IsPlayerInventorySlot())
				OnItemSellEvent?.Invoke(item);
			else if (oldInventorySlot.IsShopSlot() && IsPlayerInventorySlot())
			{
				int itemCost = item.itemPrice * item.currentStackCount; //check if player can afford
				if (itemCost > PlayerInfoUi.playerInstance.GetComponent<PlayerInventoryManager>().playerGoldAmount) return;
				OnItemBuyEvent?.Invoke(item);
			}
		}

		AddItemToSlot(item);
		oldInventorySlot.RemoveItemFromSlot();
	}
	public void EquipItemToSlot(InventoryItemUi item) //ui context menu
	{
		AddItemToSlot(item);
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

		item.GetComponent<ToolTipUi>().UpdatePlayerToolTip();
	}
	public void RemoveItemFromSlot()
	{
		UpdateSlotSize();
		itemInSlot = null;
		CheckIfItemInEquipmentSlot(itemInSlot);
	}
	private void SwapItemInSlot(InventoryItemUi item, InventorySlotUi oldInventorySlot)
	{
		itemInSlot.transform.SetParent(item.parentAfterDrag, false);
		itemInSlot.inventorySlotIndex = item.inventorySlotIndex;
		oldInventorySlot.itemInSlot = itemInSlot;
		oldInventorySlot.UpdateSlotSize();
		oldInventorySlot.CheckIfItemInEquipmentSlot(oldInventorySlot.itemInSlot);
	}
	public void CheckIfItemInEquipmentSlot(InventoryItemUi item)
	{
		if (IsPlayerInventorySlot() || IsShopSlot()) return;

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
	public bool IsNewSlotSameAsOldSlot(InventoryItemUi item)
	{
		if (item.parentAfterDrag == this)
			return true;
		else
			return false;
	}
	public bool IsSlotEmpty()
	{
		if (GetComponentInChildren<InventoryItemUi>() == null)
			return true;
		else
			return false;
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
			//if (item.itemType == InventoryItemUi.ItemType.isAbility && slotType == SlotType.ability)
				//return false;
			if (item.itemType == InventoryItemUi.ItemType.isAbility && slotType == SlotType.equippedAbilities)
				return true;
			else
				return false;
		}

		//shop checks
		if (IsShopSlot() || item.parentAfterDrag.GetComponent<InventorySlotUi>().IsShopSlot())
		{
			if (IsShopSlot() && item.parentAfterDrag.GetComponent<InventorySlotUi>().IsPlayerInventorySlot())
				return true;
			if (IsPlayerInventorySlot() && item.parentAfterDrag.GetComponent<InventorySlotUi>().IsShopSlot())
				return true;
			else return false;
		}

		//equipment class restriction/level checks
		if (item.parentAfterDrag.GetComponent<InventorySlotUi>().IsPlayerEquipmentSlot() && IsPlayerInventorySlot())
			return true;
		else if (item.itemType == InventoryItemUi.ItemType.isConsumable && slotType == SlotType.consumables)
			return true;
		else if (item.itemType == InventoryItemUi.ItemType.isWeapon && CheckClassRestriction((int)item.classRestriction))
		{
			if (!IsCorrectLevel(item)) return false;
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
		else if (item.itemType == InventoryItemUi.ItemType.isArmor && CheckClassRestriction((int)item.classRestriction))
		{
			if (!IsCorrectLevel(item)) return false;
			Armors armor = item.GetComponent<Armors>();

			if (armor.armorSlot == Armors.ArmorSlot.helmet && slotType == SlotType.helmet)
				return true;
			if (armor.armorSlot == Armors.ArmorSlot.chestpiece && slotType == SlotType.chestpiece)
				return true;
			if (armor.armorSlot == Armors.ArmorSlot.legs && slotType == SlotType.legs)
				return true;
			else return false;
		}
		else if (item.itemType == InventoryItemUi.ItemType.isAccessory)
		{
			if (!IsCorrectLevel(item)) return false;
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
		if (slotType != SlotType.playerInventory && slotType != SlotType.shopSlot && slotType != SlotType.ability)
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
		int classRestrictionNum = (int)ClassesUi.Instance.currentPlayerClass.classRestriction;
		if (classRestrictionNum >= itemClassRestrictionNum)
			return true;
		else
			return false;
	}
}

