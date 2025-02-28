using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerEquipmentHandler : EntityEquipmentHandler
{
	private PlayerController player;

	private void Awake()
	{
		player = GetComponent<PlayerController>();
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

	//player equip item event listener
	private void EquipItem(InventorySlotDataUi slot, InventoryItemUi item)
	{
		if (entityStats.playerRef != GameManager.Localplayer) return;

		if (MultiplayerManager.IsMultiplayer())
		{
			if (item == null)
				SyncHandleEmptySlotsRpc(GetSlotIndex(slot));
			else
				SyncEquipItemsRpc((int)item.type, GetSlotIndex(slot), GetItemIndex(item), (int)item.rarity, item.level, item.enchantmentLevel);
		}
		else
		{
			if (item == null)
				HandleEmptySlots(slot);
			else
				EquipItem(slot, item.GetBaseItemClass(), (int)item.rarity, item.level, item.enchantmentLevel);
		}
	}
	//equipping items based on set args
	private void EquipItem<T>(InventorySlotDataUi slot, T itemTemplate, int rarity, int level, int enchantmentLevel)
	{
		if (itemTemplate is SOWeapons)
		{
			SOWeapons weaponTemplate = itemTemplate as SOWeapons;

            if (slot.slotType == InventorySlotDataUi.SlotType.weaponMain)
				EquipWeapon(weaponTemplate, rarity, level, enchantmentLevel, equippedWeapon, weaponSlotContainer);
			else
				EquipWeapon(weaponTemplate, rarity, level, enchantmentLevel, equippedOffhandWeapon, offhandWeaponSlotContainer);
		}
		else if (itemTemplate is SOArmors)
		{
			SOArmors armourTemplate = itemTemplate as SOArmors;

			if (slot.slotType == InventorySlotDataUi.SlotType.helmet)
				EquipArmor(armourTemplate, rarity, level, enchantmentLevel, equippedHelmet, helmetSlotContainer);
			else if (slot.slotType == InventorySlotDataUi.SlotType.chestpiece)
				EquipArmor(armourTemplate, rarity, level, enchantmentLevel, equippedChestpiece, chestpieceSlotContainer);
			else if (slot.slotType == InventorySlotDataUi.SlotType.legs)
				EquipArmor(armourTemplate, rarity, level, enchantmentLevel, equippedLegs, legsSlotContainer);
		}
		else if (itemTemplate is SOAccessories)
		{
			SOAccessories accessoryTemplate = itemTemplate as SOAccessories;

			if (slot.slotType == InventorySlotDataUi.SlotType.necklace)
				EquipAccessory(accessoryTemplate, rarity, level, enchantmentLevel, equippedNecklace, necklaceSlotContainer);
			else if (slot.slotType == InventorySlotDataUi.SlotType.ringOne)
				EquipAccessory(accessoryTemplate, rarity, level, enchantmentLevel, equippedRingOne, ringOneSlotContainer);
			else if (slot.slotType == InventorySlotDataUi.SlotType.ringTwo)
				EquipAccessory(accessoryTemplate, rarity, level, enchantmentLevel, equippedRingTwo, ringTwoSlotContainer);
		}
		else if (itemTemplate is SOConsumables)
		{
			return;
		}
		else
		{
			Debug.LogError("No item type match, type: " + itemTemplate.GetType());
			return;
		}
	}

	//sync equip in mp
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncEquipItemsRpc(int slotIndex, int itemType, int itemIndex, int rarity, int level, int enchantmentLevel)
	{
		SyncEquipItems(itemType, slotIndex, itemIndex, rarity, level, enchantmentLevel);
	}
	private void SyncEquipItems(int slotIndex, int itemType, int itemIndex, int rarity, int level, int enchantmentLevel)
	{
		if ((SOItems.ItemType)itemType == SOItems.ItemType.isWeapon)
			EquipItem(equipmentSlots[slotIndex], AssetDatabase.Database.weapons[itemIndex], rarity, level, enchantmentLevel);
		else if ((SOItems.ItemType)itemType == SOItems.ItemType.isArmor)
			EquipItem(equipmentSlots[slotIndex], AssetDatabase.Database.armours[itemIndex], rarity, level, enchantmentLevel);
		else if ((SOItems.ItemType)itemType == SOItems.ItemType.isAccessory)
			EquipItem(equipmentSlots[slotIndex], AssetDatabase.Database.accessories[itemIndex], rarity, level, enchantmentLevel);
		else if ((SOItems.ItemType)itemType == SOItems.ItemType.isConsumable)
			EquipItem(equipmentSlots[slotIndex], AssetDatabase.Database.consumables[itemIndex], rarity, level, enchantmentLevel);
	}

	//sync empty in mp
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncHandleEmptySlotsRpc(int slotIndex)
	{
		SyncHandleEmptySlots(slotIndex);
	}
	private void SyncHandleEmptySlots(int slotIndex)
	{
		HandleEmptySlots(equipmentSlots[slotIndex]);
	}

	//EQUIP ENTITY ITEMS
	private void EquipWeapon(SOWeapons weaponTemplate, int rarity, int level, int enchantLevel,
		Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = weaponTemplate;
		equippedWeaponRef.Initilize((SOItems.Rarity)rarity, level, enchantLevel);

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
		OnWeaponEquip(equippedWeaponRef, slotToSpawnIn);
	}
	private void EquipArmor(SOArmors armorTemplate, int rarity, int level, int enchantLevel, 
		Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnArmorUnequip(equippedArmorRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = armorTemplate;
		equippedArmorRef.Initilize((SOItems.Rarity)rarity, level, enchantLevel);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
		OnArmorEquip(equippedArmorRef, slotToSpawnIn);
	}
	private void EquipAccessory(SOAccessories accessoryTemplate, int rarity, int level, int enchantLevel, 
		Accessories equippedAccessoryRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnAccessoryUnequip(equippedAccessoryRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedAccessoryRef = go.AddComponent<Accessories>();

		equippedAccessoryRef.accessoryBaseRef = accessoryTemplate;
		equippedAccessoryRef.Initilize((SOItems.Rarity)rarity, level, enchantLevel);

		equippedAccessoryRef.GetComponent<SpriteRenderer>().enabled = false;
		OnAccessoryEquip(equippedAccessoryRef, slotToSpawnIn);
	}
}
