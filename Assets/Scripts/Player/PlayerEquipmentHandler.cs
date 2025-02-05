using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class PlayerEquipmentHandler : EntityEquipmentHandler
{
	[HideInInspector] public PlayerController player;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		isPlayerEquipment = true;
	}
	private void OnEnable()
	{
		InventorySlotDataUi.OnItemEquip += EquipItem;
	}
	private void OnDisable()
	{
		InventorySlotDataUi.OnItemEquip -= EquipItem;
	}

	//player equip item event listner
	private void EquipItem(InventoryItemUi item, InventorySlotDataUi slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);
		else if (item.itemType == InventoryItemUi.ItemType.isWeapon) //when player first equips/swaps equipment
		{
			Weapons weapon = item.GetComponent<Weapons>();
			if (slot.slotType == InventorySlotDataUi.SlotType.weaponMain)
				EquipWeapon(weapon, equippedWeapon, weaponSlotContainer);
			else
				EquipWeapon(weapon, equippedOffhandWeapon, offhandWeaponSlotContainer);
		}
		else if (item.itemType == InventoryItemUi.ItemType.isArmor)
		{
			Armors armor = item.GetComponent<Armors>();
			if (slot.slotType == InventorySlotDataUi.SlotType.helmet)
				EquipArmor(armor, equippedHelmet, helmetSlotContainer);

			else if (slot.slotType == InventorySlotDataUi.SlotType.chestpiece)
				EquipArmor(armor, equippedChestpiece, chestpieceSlotContainer);

			else if (slot.slotType == InventorySlotDataUi.SlotType.legs)
				EquipArmor(armor, equippedLegs, legsSlotContainer);
		}
		else if (item.itemType == InventoryItemUi.ItemType.isAccessory)
		{
			Accessories accessories = item.GetComponent<Accessories>();
			if (slot.slotType == InventorySlotDataUi.SlotType.necklace)
				EquipAccessory(accessories, equippedNecklace, necklaceSlotContainer);

			else if (slot.slotType == InventorySlotDataUi.SlotType.ringOne)
				EquipAccessory(accessories, equippedRingOne, ringOneSlotContainer);

			else if (slot.slotType == InventorySlotDataUi.SlotType.ringTwo)
				EquipAccessory(accessories, equippedRingTwo, ringTwoSlotContainer);
		}
	}
	private void EquipWeapon(Weapons weaponToEquip, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		OnWeaponUnequip(equippedWeaponRef);

		GameObject go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.Initilize(weaponToEquip.rarity, weaponToEquip.itemLevel, weaponToEquip.itemEnchantmentLevel);

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
		OnWeaponEquip(equippedWeaponRef, slotToSpawnIn);
	}
	private void EquipArmor(Armors armorToEquip, Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		OnArmorUnequip(equippedArmorRef);

		GameObject go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = armorToEquip.armorBaseRef;
		equippedArmorRef.Initilize(armorToEquip.rarity, armorToEquip.itemLevel, armorToEquip.itemEnchantmentLevel);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
		OnArmorEquip(equippedArmorRef, slotToSpawnIn);
	}
	private void EquipAccessory(Accessories accessoryToEquip, Accessories equippedAccessoryRef, GameObject slotToSpawnIn)
	{
		OnAccessoryUnequip(equippedAccessoryRef);

		GameObject go = SpawnItemPrefab(slotToSpawnIn);
		equippedAccessoryRef = go.AddComponent<Accessories>();

		equippedAccessoryRef.accessoryBaseRef = accessoryToEquip.accessoryBaseRef;
		equippedAccessoryRef.Initilize(accessoryToEquip.rarity, accessoryToEquip.itemLevel, accessoryToEquip.itemEnchantmentLevel);

		equippedAccessoryRef.GetComponent<SpriteRenderer>().enabled = false;
		OnAccessoryEquip(equippedAccessoryRef, slotToSpawnIn);
	}

	private void HandleEmptySlots(InventorySlotDataUi slot)
	{
		if (slot.slotType == InventorySlotDataUi.SlotType.weaponMain)
			OnWeaponUnequip(equippedWeapon);
		if (slot.slotType == InventorySlotDataUi.SlotType.weaponOffhand)
			OnWeaponUnequip(equippedOffhandWeapon);
		if (slot.slotType == InventorySlotDataUi.SlotType.helmet)
			OnArmorUnequip(equippedHelmet);
		if (slot.slotType == InventorySlotDataUi.SlotType.chestpiece)
			OnArmorUnequip(equippedChestpiece);
		if (slot.slotType == InventorySlotDataUi.SlotType.legs)
			OnArmorUnequip(equippedLegs);
		if (slot.slotType == InventorySlotDataUi.SlotType.necklace)
			OnAccessoryUnequip(equippedNecklace);
		if (slot.slotType == InventorySlotDataUi.SlotType.ringOne)
			OnAccessoryUnequip(equippedRingOne);
		if (slot.slotType == InventorySlotDataUi.SlotType.ringTwo)
			OnAccessoryUnequip(equippedRingTwo);
	}
}
