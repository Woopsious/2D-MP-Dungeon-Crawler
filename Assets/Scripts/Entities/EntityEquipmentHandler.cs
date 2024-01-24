using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEquipmentHandler : MonoBehaviour
{
	public GameObject itemPrefab;

	[HideInInspector] public EntityStats entityStats;

	[Header("Weapon")]
	public GameObject weaponSlotContainer;
	public Weapons equippedWeapon;

	[Header("Offhand Weapon")]
	public GameObject offhandWeaponSlotContainer;
	public Weapons equippedOffhandWeapon;

	[Header("Armor")]
	public GameObject helmetSlotContainer;
	public GameObject chestpieceSlotContainer;
	public GameObject legsSlotContainer;

	public Armors equippedHelmet;
	public Armors equippedChestpiece;
	public Armors equippedLegs;

	[Header("Accessories")]
	public GameObject necklaceSlotContainer;
	public GameObject ringOneSlotContainer;
	public GameObject ringTwoSlotContainer;

	public Accessories equippedNecklace;
	public Accessories equippedRingOne;
	public Accessories equippedRingTwo;

	[Header("Bonuses Provided By Equipment")]
	public int bonusEquipmentHealth;
	public int bonusEquipmentMana;

	public int bonusEquipmentPhysicalResistance;
	public int bonusEquipmentPoisonResistance;
	public int bonusEquipmentFireResistance;
	public int bonusEquipmentIceResistance;

	public int bonusPhysicalDamagePercentage;
	public int bonusPoisonDamagePercentage;
	public int bonusFireDamagePercentage;
	public int bonusIceDamagePercentage;

	public virtual void Start()
	{
		entityStats = GetComponentInParent<EntityStats>();
		entityStats.entityEquipment = this;
	}

	//for entitys other then player to randomly equip items when they spawn in
	public void EquipRandomItems()
	{
		EquipRandomWeapon(entityStats.entityBaseStats.possibleWeaponsList, equippedWeapon, weaponSlotContainer);			//main weapon
		//EquipRandomWeapon( NO LIST FOR OFFHAND WEAPONS ATM, equippedOffhandWeapon, offhandWeaponSlotContainer);			//offhand weapon

		EquipRandomArmor(entityStats.entityBaseStats.possibleHelmetsList, equippedHelmet, helmetSlotContainer);				//helmet
		EquipRandomArmor(entityStats.entityBaseStats.possibleChestpiecesList, equippedChestpiece, chestpieceSlotContainer); //chestpiece
		EquipRandomArmor(entityStats.entityBaseStats.possibleLegsList, equippedLegs, legsSlotContainer);                    //legs

		//Accessory functions here if/when i decide to add it
	}
	public void EquipRandomWeapon(List<SOWeapons> listOfPossibleWeapons, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		int index;
		index = Utilities.GetRandomNumber(listOfPossibleWeapons.Count);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = listOfPossibleWeapons[index];
		equippedWeaponRef.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
		equippedWeaponRef.isEquippedByOther = true;

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
	}
	public void EquipRandomArmor(List<SOArmors> listOfPossibleArmors, Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		int index;
		index = Utilities.GetRandomNumber(listOfPossibleArmors.Count);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = listOfPossibleArmors[index];
		equippedArmorRef.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
	}

	//weapons
	public void OnWeaponUnequip(Weapons weapon)
	{
		if (weapon == null) return;

		if (weapon.isShield)	//shield is a unique so i use damage value to store bonus health and resists it adds
		{
			bonusEquipmentHealth -= weapon.damage;

			bonusEquipmentPhysicalResistance -= weapon.damage;
			bonusEquipmentPoisonResistance -= weapon.damage;
			bonusEquipmentFireResistance -= weapon.damage;
			bonusEquipmentIceResistance -= weapon.damage;

			entityStats.physicalResistance -= weapon.damage;
			entityStats.poisonResistance -= weapon.damage;
			entityStats.fireResistance -= weapon.damage;
			entityStats.iceResistance -= weapon.damage;

			entityStats.maxHealth -= weapon.damage;
			entityStats.currentHealth -= weapon.damage;
		}
		else
		{
			bonusEquipmentMana -= weapon.bonusMana;

			entityStats.maxMana -= weapon.bonusMana;
			entityStats.currentMana -= weapon.bonusMana;
		}
	}
	public void OnWeaponEquip(Weapons weapon, GameObject slotItemIsIn)
	{
		if (weapon.isShield)	//shield is a unique so i use damage value to store bonus health and resists it adds
		{
			bonusEquipmentHealth += weapon.damage;

			bonusEquipmentPhysicalResistance += weapon.damage;
			bonusEquipmentPoisonResistance += weapon.damage;
			bonusEquipmentFireResistance += weapon.damage;
			bonusEquipmentIceResistance += weapon.damage;

			entityStats.physicalResistance += weapon.damage;
			entityStats.poisonResistance += weapon.damage;
			entityStats.fireResistance += weapon.damage;
			entityStats.iceResistance += weapon.damage;

			entityStats.maxHealth += weapon.damage;
			entityStats.currentHealth += weapon.damage;
		}
		else
		{
			bonusEquipmentMana += weapon.bonusMana;

			entityStats.maxMana += weapon.bonusMana;
			entityStats.currentMana += weapon.bonusMana;
		}

		AssignItemRefOnEquip(weapon, slotItemIsIn);
	}

	//armors
	public void OnArmorUnequip(Armors armor)
	{
		if (armor == null) return;

		bonusEquipmentHealth -= armor.bonusHealth;
		bonusEquipmentMana -= armor.bonusMana;
		bonusEquipmentPhysicalResistance -= armor.bonusPhysicalResistance;
		bonusEquipmentPoisonResistance -= armor.bonusPoisonResistance;
		bonusEquipmentFireResistance -= armor.bonusFireResistance;
		bonusEquipmentIceResistance -= armor.bonusIceResistance;

		entityStats.maxHealth -= armor.bonusHealth;
		entityStats.currentHealth -= armor.bonusHealth;
		entityStats.maxMana -= armor.bonusMana;
		entityStats.currentMana -= armor.bonusMana;

		entityStats.physicalResistance -= armor.bonusPhysicalResistance;
		entityStats.poisonResistance -= armor.bonusPoisonResistance;
		entityStats.fireResistance -= armor.bonusFireResistance;
		entityStats.iceResistance -= armor.bonusIceResistance;
	}
	public void OnArmorEquip(Armors armor, GameObject slotItemIsIn)
	{
		bonusEquipmentHealth += armor.bonusHealth;
		bonusEquipmentMana += armor.bonusMana;
		bonusEquipmentPhysicalResistance += armor.bonusPhysicalResistance;
		bonusEquipmentPoisonResistance += armor.bonusPoisonResistance;
		bonusEquipmentFireResistance += armor.bonusFireResistance;
		bonusEquipmentIceResistance += armor.bonusIceResistance;

		entityStats.maxHealth += armor.bonusHealth;
		entityStats.currentHealth += armor.bonusHealth;
		entityStats.maxMana += armor.bonusMana;
		entityStats.currentMana += armor.bonusMana;

		entityStats.physicalResistance += armor.bonusPhysicalResistance;
		entityStats.poisonResistance += armor.bonusPoisonResistance;
		entityStats.fireResistance += armor.bonusFireResistance;
		entityStats.iceResistance += armor.bonusIceResistance;

		AssignItemRefOnEquip(armor, slotItemIsIn);
	}

	//accessories
	public void OnAccessoryUnequip(Accessories accessory)
	{
		if (accessory == null) return;

		bonusEquipmentHealth -= accessory.bonusHealth;
		bonusEquipmentMana -= accessory.bonusMana;
		bonusEquipmentPhysicalResistance -= accessory.bonusPhysicalResistance;
		bonusEquipmentPoisonResistance -= accessory.bonusPoisonResistance;
		bonusEquipmentFireResistance -= accessory.bonusFireResistance;
		bonusEquipmentIceResistance -= accessory.bonusIceResistance;

		entityStats.maxHealth -= accessory.bonusHealth;
		entityStats.currentHealth -= accessory.bonusHealth;
		entityStats.maxMana -= accessory.bonusMana;
		entityStats.currentHealth -= accessory.bonusMana;
		entityStats.physicalResistance -= accessory.bonusPhysicalResistance;
		entityStats.poisonResistance -= accessory.bonusPoisonResistance;
		entityStats.fireResistance -= accessory.bonusFireResistance;
		entityStats.iceResistance -= accessory.bonusIceResistance;

		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPhysicalDamageType)
		{
			bonusPhysicalDamagePercentage -= accessory.bonusPercentageValue;
			entityStats.bonusPhysicalDamagePercentage -= accessory.bonusPercentageValue;
		}
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPoisonDamageType)
		{
			bonusPhysicalDamagePercentage -= accessory.bonusPercentageValue;
			entityStats.bonusPoisonDamagePercentage -= accessory.bonusPercentageValue;
		}
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isFireDamageType)
		{
			bonusFireDamagePercentage -= accessory.bonusPercentageValue;
			entityStats.bonusFireDamagePercentage -= accessory.bonusPercentageValue;
		}
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isIceDamageType)
		{
			bonusIceDamagePercentage -= accessory.bonusPercentageValue;
			entityStats.bonusIceDamagePercentage -= accessory.bonusPercentageValue;
		}
	}
	public void OnAccessoryEquip(Accessories accessory, GameObject slotItemIsIn)
	{
		bonusEquipmentHealth += accessory.bonusHealth;
		bonusEquipmentMana += accessory.bonusMana;
		bonusEquipmentPhysicalResistance += accessory.bonusPhysicalResistance;
		bonusEquipmentPoisonResistance += accessory.bonusPoisonResistance;
		bonusEquipmentFireResistance += accessory.bonusFireResistance;
		bonusEquipmentIceResistance += accessory.bonusIceResistance;

		entityStats.maxHealth += accessory.bonusHealth;
		entityStats.currentHealth += accessory.bonusHealth;
		entityStats.maxMana += accessory.bonusMana;
		entityStats.currentHealth += accessory.bonusMana;
		entityStats.physicalResistance += accessory.bonusPhysicalResistance;
		entityStats.poisonResistance += accessory.bonusPoisonResistance;
		entityStats.fireResistance += accessory.bonusFireResistance;
		entityStats.iceResistance += accessory.bonusIceResistance;

		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPhysicalDamageType)
		{
			bonusPhysicalDamagePercentage += accessory.bonusPercentageValue;
			entityStats.bonusPhysicalDamagePercentage += accessory.bonusPercentageValue;
		}
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPoisonDamageType)
		{
			bonusPhysicalDamagePercentage += accessory.bonusPercentageValue;
			entityStats.bonusPoisonDamagePercentage += accessory.bonusPercentageValue;
		}
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isFireDamageType)
		{
			bonusFireDamagePercentage += accessory.bonusPercentageValue;
			entityStats.bonusFireDamagePercentage += accessory.bonusPercentageValue;
		}
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isIceDamageType)
		{
			bonusIceDamagePercentage += accessory.bonusPercentageValue;
			entityStats.bonusIceDamagePercentage += accessory.bonusPercentageValue;
		}

		AssignItemRefOnEquip(accessory, slotItemIsIn);
	}

	public void OnPlayerLevelUp(int newPlayerLevel)
	{
		if (entityStats.playerEquipment != null && entityStats.currentHealth <= 0) return; //only level up stats for non player entities
		equippedWeapon.itemLevel = newPlayerLevel;
		equippedWeapon.SetItemStats(equippedWeapon.rarity, equippedWeapon.itemLevel, this);

		equippedHelmet.itemLevel = newPlayerLevel;
		equippedHelmet.SetItemStats(equippedHelmet.rarity, equippedHelmet.itemLevel, this);

		equippedChestpiece.itemLevel = newPlayerLevel;
		equippedChestpiece.SetItemStats(equippedChestpiece.rarity, equippedChestpiece.itemLevel, this);

		equippedLegs.itemLevel = newPlayerLevel;
		equippedLegs.SetItemStats(equippedLegs.rarity, equippedLegs.itemLevel, this);
	}

	public GameObject SpawnItemPrefab(GameObject slotToSpawnIn)
	{
		GameObject go;
		if (slotToSpawnIn.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, slotToSpawnIn.transform);
			return go;
		}
		else return slotToSpawnIn.transform.GetChild(0).gameObject;
	}
	public Items AssignItemRefOnEquip(Items itemToAssign, GameObject SlotItemIsIn)
	{
		if (SlotItemIsIn == weaponSlotContainer)
			equippedWeapon = (Weapons)itemToAssign;
		else if (SlotItemIsIn == offhandWeaponSlotContainer)
			equippedOffhandWeapon = (Weapons)itemToAssign;
		else if (SlotItemIsIn == helmetSlotContainer)
			equippedHelmet = (Armors)itemToAssign;
		else if (SlotItemIsIn == chestpieceSlotContainer)
			equippedChestpiece = (Armors)itemToAssign;
		else if (SlotItemIsIn == legsSlotContainer)
			equippedLegs = (Armors)itemToAssign;
		else if (SlotItemIsIn == necklaceSlotContainer)
			equippedNecklace = (Accessories)itemToAssign;
		else if (SlotItemIsIn == ringOneSlotContainer)
			equippedRingOne = (Accessories)itemToAssign;
		else if (SlotItemIsIn == ringTwoSlotContainer)
			equippedRingTwo = (Accessories)itemToAssign;
		else
			Debug.LogError("item doesnt match any equipment slot"); return null;
	}
}
