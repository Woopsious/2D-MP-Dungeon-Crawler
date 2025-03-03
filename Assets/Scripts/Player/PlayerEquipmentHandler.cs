using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;

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

	//sync pre-existing player obj info to newly joined clients
	[Rpc(SendTo.Server, RequireOwnership = false)]
	public void SyncInfoToNewlyJoinedClientRpc(ulong clientId)
	{
		int[] weapon = GetWeaponIndexData(equippedWeapon);
		int[] offhandWeapon = GetWeaponIndexData(equippedOffhandWeapon);
		int[] helmet = GetArmourIndexData(equippedHelmet);
		int[] chestpiece = GetArmourIndexData(equippedChestpiece);
		int[] legs = GetArmourIndexData(equippedLegs);
		int[] necklace = GetAccessoryIndexData(equippedNecklace);
		int[] ringOne = GetAccessoryIndexData(equippedRingOne);
		int[] ringTwo = GetAccessoryIndexData(equippedRingTwo);

		SyncInfoToNewlyJoinedClientRpc(weapon, offhandWeapon, helmet, chestpiece, legs, necklace, ringOne, ringTwo
			, RpcTarget.Single(clientId, RpcTargetUse.Temp));
	}
	[Rpc(SendTo.SpecifiedInParams)]
	private void SyncInfoToNewlyJoinedClientRpc(int[] weapon, int[] offhandWeapon, 
		int[] helmet, int[] chestpiece, int[] legs, int[] necklace, int[] ringOne, int[] ringTwo, RpcParams rpcParams)
	{
		if (weapon[0] != -1)
			EquipItem(0, AssetDatabase.Database.weapons[weapon[0]], weapon[1], weapon[2], weapon[3]);
		if (offhandWeapon[0] != -1)
			EquipItem(1, AssetDatabase.Database.weapons[offhandWeapon[0]], offhandWeapon[1], offhandWeapon[2], offhandWeapon[3]);
		if (helmet[0] != -1)
			EquipItem(2, AssetDatabase.Database.armours[helmet[0]], helmet[1], helmet[2], helmet[3]);
		if (chestpiece[0] != -1)
			EquipItem(3, AssetDatabase.Database.armours[chestpiece[0]], chestpiece[1], chestpiece[2], chestpiece[3]);
		if (legs[0] != -1)
			EquipItem(4, AssetDatabase.Database.armours[legs[0]], legs[1], legs[2], legs[3]);
		if (necklace[0] != -1)
			EquipItem(5, AssetDatabase.Database.accessories[necklace[0]], necklace[1], necklace[2], necklace[3]);
		if (ringOne[0] != -1)
			EquipItem(6, AssetDatabase.Database.accessories[ringOne[0]], ringOne[1], ringOne[2], ringOne[3]);
		if (ringTwo[0] != -1)
			EquipItem(7, AssetDatabase.Database.accessories[ringTwo[0]], ringTwo[1], ringTwo[2], ringTwo[3]);
	}

	//EQUIP PLAYER ITEMS EVENT LISTNER
	private void EquipItem(InventorySlotDataUi slot, InventoryItemUi item)
	{
		if (entityStats.playerRef != GameManager.Localplayer) return;

		if (MultiplayerManager.IsMultiplayer())
		{
			if (item == null)
				SyncHandleEmptySlotsRpc(slot.GetEquipmentSlotIndex());
			else
				SyncEquipItemsRpc(slot.GetEquipmentSlotIndex(), 
					GetItemIndex(item.GetClassType()), (int)item.rarity, item.level, item.enchantmentLevel);
		}
		else
		{
			if (item == null)
				HandleEmptySlots(slot.GetEquipmentSlotIndex());
			else
				EquipItem(slot.GetEquipmentSlotIndex(), item.GetClassType(), (int)item.rarity, item.level, item.enchantmentLevel);
		}
	}
	//equipping items based on set args
	private void EquipItem<T>(int slotIndex, T itemTemplate, int rarity, int level, int enchantmentLevel)
	{
		SOWeapons weaponTemplate = itemTemplate as SOWeapons;
		SOArmors armourTemplate = itemTemplate as SOArmors;
		SOAccessories accessoryTemplate = itemTemplate as SOAccessories;

		if (slotIndex == 0)
			EquipWeapon(weaponTemplate, rarity, level, enchantmentLevel, equippedWeapon, weaponSlotContainer);
		else if (slotIndex == 1)
			EquipWeapon(weaponTemplate, rarity, level, enchantmentLevel, equippedOffhandWeapon, offhandWeaponSlotContainer);
		else if (slotIndex == 2)
			EquipArmor(armourTemplate, rarity, level, enchantmentLevel, equippedHelmet, helmetSlotContainer);
		else if (slotIndex == 3)
			EquipArmor(armourTemplate, rarity, level, enchantmentLevel, equippedChestpiece, chestpieceSlotContainer);
		else if (slotIndex == 4)
			EquipArmor(armourTemplate, rarity, level, enchantmentLevel, equippedLegs, legsSlotContainer);
		else if (slotIndex == 5)
			EquipAccessory(accessoryTemplate, rarity, level, enchantmentLevel, equippedNecklace, necklaceSlotContainer);
		else if (slotIndex == 6)
			EquipAccessory(accessoryTemplate, rarity, level, enchantmentLevel, equippedRingOne, ringOneSlotContainer);
		else if (slotIndex == 7)
			EquipAccessory(accessoryTemplate, rarity, level, enchantmentLevel, equippedRingTwo, ringTwoSlotContainer);
		else
		{
			Debug.LogError("No slot index matches");
			return;
		}
	}

	//sync equip in mp
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncEquipItemsRpc(int slotIndex, int itemIndex, int rarity, int level, int enchantmentLevel)
	{
		SyncEquipItems(slotIndex, itemIndex, rarity, level, enchantmentLevel);
	}
	private void SyncEquipItems(int slotIndex, int itemIndex, int rarity, int level, int enchantmentLevel)
	{
		if (slotIndex >= 0 && slotIndex < 2)
			EquipItem(slotIndex, AssetDatabase.Database.weapons[itemIndex], rarity, level, enchantmentLevel);
		else if (slotIndex >= 2 && slotIndex < 5)
			EquipItem(slotIndex, AssetDatabase.Database.armours[itemIndex], rarity, level, enchantmentLevel);
		else if (slotIndex >= 5 && slotIndex < 8)
			EquipItem(slotIndex, AssetDatabase.Database.accessories[itemIndex], rarity, level, enchantmentLevel);
	}

	//sync empty in mp
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncHandleEmptySlotsRpc(int slotIndex)
	{
		SyncHandleEmptySlots(slotIndex);
	}
	private void SyncHandleEmptySlots(int slotIndex)
	{
		HandleEmptySlots(slotIndex);
	}

	//EQUIP PLAYER ENTITY ITEMS
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

	//handle player equipment being unequipped
	private void HandleEmptySlots(int slotIndex)
	{
		if (slotIndex == 0)
			OnWeaponUnequip(equippedWeapon);
		else if (slotIndex == 1)
			OnWeaponUnequip(equippedOffhandWeapon);
		else if (slotIndex == 2)
			OnArmorUnequip(equippedHelmet);
		else if (slotIndex == 3)
			OnArmorUnequip(equippedChestpiece);
		else if (slotIndex == 4)
			OnArmorUnequip(equippedLegs);
		else if (slotIndex == 5)
			OnAccessoryUnequip(equippedNecklace);
		else if (slotIndex == 6)
			OnAccessoryUnequip(equippedRingOne);
		else if (slotIndex == 7)
			OnAccessoryUnequip(equippedRingTwo);
	}

	//helpers
	private int GetItemIndex(SOItems itemRef)
	{
		if (itemRef is SOWeapons)
		{
			for (int i = 0; i < AssetDatabase.Database.weapons.Count; i++)
			{
				if (itemRef == AssetDatabase.Database.weapons[i])
					return i;
			}
		}
		else if (itemRef is SOArmors)
		{
			for (int i = 0; i < AssetDatabase.Database.armours.Count; i++)
			{
				if (itemRef == AssetDatabase.Database.armours[i])
					return i;
			}
		}
		else if (itemRef is SOAccessories)
		{
			for (int i = 0; i < AssetDatabase.Database.accessories.Count; i++)
			{
				if (itemRef == AssetDatabase.Database.accessories[i])
					return i;
			}
		}
		else if (itemRef is SOConsumables)
		{
			for (int i = 0; i < AssetDatabase.Database.consumables.Count; i++)
			{
				if (itemRef == AssetDatabase.Database.consumables[i])
					return i;
			}
		}

		Debug.LogError("Failed to get item index");
		return 0;
	}
	private int[] GetWeaponIndexData(Weapons weapon)
	{
		int[] itemIndexData = new int[4];

		if (weapon == null)
			itemIndexData[0] = -1;
		else
		{
			itemIndexData[0] = GetItemIndex(weapon.weaponBaseRef);
			itemIndexData[1] = (int)weapon.rarity;
			itemIndexData[2] = weapon.level;
			itemIndexData[3] = weapon.enchantmentLevel;
		}
		return itemIndexData;
	}
	private int[] GetArmourIndexData(Armors armors)
	{
		int[] itemIndexData = new int[4];

		if (armors == null)
			itemIndexData[0] = -1;
		else
		{
			itemIndexData[0] = GetItemIndex(armors.armorBaseRef);
			itemIndexData[1] = (int)armors.rarity;
			itemIndexData[2] = armors.level;
			itemIndexData[3] = armors.enchantmentLevel;
		}
		return itemIndexData;
	}
	private int[] GetAccessoryIndexData(Accessories accessory)
	{
		int[] itemIndexData = new int[4];

		if (accessory == null)
			itemIndexData[0] = -1;
		else
		{
			itemIndexData[0] = GetItemIndex(accessory.accessoryBaseRef);
			itemIndexData[1] = (int)accessory.rarity;
			itemIndexData[2] = accessory.level;
			itemIndexData[3] = accessory.enchantmentLevel;
		}
		return itemIndexData;
	}
}
