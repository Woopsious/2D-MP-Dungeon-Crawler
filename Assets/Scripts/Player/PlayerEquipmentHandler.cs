using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class PlayerEquipmentHandler : EntityEquipmentHandler
{
	PlayerController playerController;

	public override void Start()
	{
		entityStats = gameObject.transform.parent.GetComponentInParent<EntityStats>();
		entityStats.playerEquipment = this;
		playerController = gameObject.transform.parent.GetComponentInParent<PlayerController>();
		playerController.playerEquipmentHandler = this;
		//when loading/saving game, once inventory is loaded then load/instantiate equipped items based on loaded inventory
	}
	private void OnEnable()
	{
		InventorySlot.onItemEquip += EquipItem;
	}
	private void OnDisable()
	{
		InventorySlot.onItemEquip -= EquipItem;
	}

	public virtual void EquipItem(InventoryItem item, InventorySlot slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);

		else if (item.itemType == InventoryItem.ItemType.isWeapon) //when player first equips/swaps equipment
		{
			if (item.weaponType == InventoryItem.WeaponType.isMainHand)
			{
				EquipWeapon(item, equippedWeapon, true);
				equippedWeapon = weaponSlotContainer.GetComponentInChildren<Weapons>();
			}
			else
			{
				EquipWeapon(item, equippedOffhandWeapon, false);
				equippedOffhandWeapon = offhandWeaponSlotContainer.GetComponentInChildren<Weapons>();
			}
		}
		else if (item.itemType == InventoryItem.ItemType.isArmor)
		{
			if (item.armorSlot == InventoryItem.ArmorSlot.helmet)
			{
				EquipArmor(item, equippedHelmet, helmetSlotContainer);
				equippedHelmet = helmetSlotContainer.GetComponentInChildren<Armors>();
			}

			if (item.armorSlot == InventoryItem.ArmorSlot.chestpiece)
			{
				EquipArmor(item, equippedChestpiece, chestpieceSlotContainer);
				equippedChestpiece = chestpieceSlotContainer.GetComponentInChildren<Armors>();
			}

			if (item.armorSlot == InventoryItem.ArmorSlot.legs)
			{
				EquipArmor(item, equippedLegs, legsSlotContainer);
				equippedLegs = legsSlotContainer.GetComponentInChildren<Armors>();
			}
		}
		else if (item.itemType == InventoryItem.ItemType.isAccessory)
		{
			if (item.accessorySlot == InventoryItem.AccessorySlot.necklace)
			{
				EquipAccessory(item, equippedNecklace, necklaceSlotContainer);
				equippedNecklace = necklaceSlotContainer.GetComponentInChildren<Accessories>();
			}

			if (item.accessorySlot == InventoryItem.AccessorySlot.ring && slot.slotType == InventorySlot.SlotType.ringOne)
			{
				EquipAccessory(item, equippedRingOne, ringOneSlotContainer);
				equippedRingOne = ringOneSlotContainer.GetComponentInChildren<Accessories>();
			}

			if (item.accessorySlot == InventoryItem.AccessorySlot.ring && slot.slotType == InventorySlot.SlotType.ringTwo)
			{
				EquipAccessory(item, equippedRingTwo, ringTwoSlotContainer);
				equippedRingTwo = ringTwoSlotContainer.GetComponentInChildren<Accessories>();
			}
		}
	}

	public void EquipWeapon(InventoryItem weaponToEquip, Weapons equippedWeaponRef, bool isMainHand)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		if (isMainHand && weaponSlotContainer.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, weaponSlotContainer.transform);
			go.AddComponent<Weapons>();
			equippedWeaponRef = go.GetComponent<Weapons>();
		}
		else if (!isMainHand && offhandWeaponSlotContainer.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, offhandWeaponSlotContainer.transform);
			go.AddComponent<Weapons>();
			equippedWeaponRef = go.GetComponent<Weapons>();
		}

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.SetItemStats((Items.Rarity)weaponToEquip.rarity, weaponToEquip.itemLevel, this);
		equippedWeaponRef.isEquippedByPlayer = true;

		if (isMainHand)
			equippedWeapon = equippedWeaponRef;
		else
			equippedOffhandWeapon = equippedWeaponRef;

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
	}
	public void EquipArmor(InventoryItem armorToEquip, Armors equippedArmorRef, GameObject slot)
	{
		GameObject go;
		OnArmorUnequip(equippedArmorRef);

		if (slot.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, slot.transform);
			go.AddComponent<Armors>();
			equippedArmorRef = go.GetComponent<Armors>();
		}
		equippedArmorRef.armorBaseRef = armorToEquip.armorBaseRef;
		equippedArmorRef.SetItemStats((Items.Rarity)armorToEquip.rarity, armorToEquip.itemLevel, this);

		if (equippedArmorRef.armorBaseRef.armorSlot == SOArmors.ArmorSlot.helmet)
			equippedHelmet = equippedArmorRef;
		if (equippedArmorRef.armorBaseRef.armorSlot == SOArmors.ArmorSlot.chest)
			equippedChestpiece = equippedArmorRef;
		if (equippedArmorRef.armorBaseRef.armorSlot == SOArmors.ArmorSlot.legs)
			equippedLegs = equippedArmorRef;

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
	}
	public void EquipAccessory(InventoryItem accessoryToEquip, Accessories equippedAccessoryRef, GameObject slot)
	{
		GameObject go;
		OnAccessoryUnequip(equippedAccessoryRef);

		if (slot.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, slot.transform);
			go.AddComponent<Accessories>();
			equippedAccessoryRef = go.GetComponent<Accessories>();
		}
		equippedAccessoryRef.accessoryBaseRef = accessoryToEquip.accessoryBaseRef;
		equippedAccessoryRef.SetItemStats((Items.Rarity)accessoryToEquip.rarity, accessoryToEquip.itemLevel, this);

		if (equippedAccessoryRef.accessoryBaseRef.accessorySlot == SOAccessories.AccessorySlot.necklace)
			equippedNecklace = equippedAccessoryRef;
		if (equippedAccessoryRef.accessoryBaseRef.accessorySlot == SOAccessories.AccessorySlot.ring)
			equippedRingOne = equippedAccessoryRef;

		equippedAccessoryRef.GetComponent<SpriteRenderer>().enabled = false;
	}

	public void HandleEmptySlots(InventorySlot slot)
	{
		if (slot.slotType == InventorySlot.SlotType.weaponMain)
		{
			OnWeaponUnequip(equippedWeapon);
			Destroy(equippedWeapon.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.weaponOffhand)
		{
			OnWeaponUnequip(equippedOffhandWeapon);
			Destroy(equippedOffhandWeapon.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.helmet)
		{
			OnArmorUnequip(equippedHelmet);
			Destroy(equippedHelmet.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.chestpiece)
		{
			OnArmorUnequip(equippedChestpiece);
			Destroy(equippedChestpiece.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.legs)
		{
			OnArmorUnequip(equippedLegs);
			Destroy(equippedLegs.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.necklace)
		{
			OnAccessoryUnequip(equippedNecklace);
			Destroy(equippedNecklace.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.ringOne)
		{
			OnAccessoryUnequip(equippedRingOne);
			Destroy(equippedRingOne.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.ringTwo)
		{
			OnAccessoryUnequip(equippedRingTwo);
			Destroy(equippedRingTwo.gameObject);
		}
	}
}
