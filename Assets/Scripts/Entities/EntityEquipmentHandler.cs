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
	private InventoryItem equippedOffhandItem;

	[Header("Armor")]
	public GameObject helmetSlotContainer;
	public GameObject chestpieceSlotContainer;
	public GameObject legsSlotContainer;

	public Armors equippedHelmet;
	public Armors equippedChestpiece;
	public Armors equippedLegs;

	[Header("Accessories")]
	private InventoryItem equippedNecklaceItem;
	private InventoryItem equippedRingOneItem;
	private InventoryItem equippedRingTwoItem;

	[Header("Bonuses Provided By Equipment")]
	public int bonusEquipmentHealth;
	public int bonusEquipmentMana;

	public int bonusEquipmentPhysicalResistance;
	public int bonusEquipmentPoisonResistance;
	public int bonusEquipmentFireResistance;
	public int bonusEquipmentIceResistance;

	public virtual void Start()
	{
		entityStats = gameObject.transform.parent.GetComponentInParent<EntityStats>();
		entityStats.entityEquipment = this;
	}

	//weapon
	public void EquipRandomWeapon()
	{
		GameObject go;
		int index;

		if (weaponSlotContainer.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, weaponSlotContainer.transform);
			go.AddComponent<Weapons>();
			equippedWeapon = go.GetComponent<Weapons>();
		}

		index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleWeaponsList.Count);
		if (equippedWeapon != null)
		{
			equippedWeapon.weaponBaseRef = entityStats.entityBaseStats.possibleWeaponsList[index];
			equippedWeapon.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
			equippedWeapon.isEquippedByOther = true;
		}
	}
	public void EquipWeaponTest(InventoryItem weaponToEquip, Weapons equippedWeaponRef)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		if (weaponSlotContainer.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, weaponSlotContainer.transform);
			go.AddComponent<Weapons>();
			equippedWeaponRef = go.GetComponent<Weapons>();
		}

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.SetItemStats((Items.Rarity)weaponToEquip.rarity, weaponToEquip.itemLevel, this);
		equippedWeaponRef.isEquippedByPlayer = true;

		equippedWeapon = equippedWeaponRef;
		OnWeaponEquip(equippedWeaponRef);

		/*
		equippedWeaponRef.itemName = weaponToEquip.itemName;
		equippedWeaponRef.itemImage = weaponToEquip.itemImage.sprite;
		equippedWeaponRef.itemLevel = weaponToEquip.itemLevel;
		equippedWeaponRef.rarity = (Items.Rarity)weaponToEquip.rarity;

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.damage = weaponToEquip.damage;
		equippedWeaponRef.bonusMana = weaponToEquip.bonusWeaponMana;
		*/
	}

	public void OnWeaponUnequip(Weapons weapon)
	{
		if (equippedWeapon != null)
		{
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
	}
	public void OnWeaponEquip(Weapons weapon)
	{
		bonusEquipmentMana += weapon.bonusMana;

		entityStats.maxMana -= weapon.bonusMana;
		entityStats.currentMana -= weapon.bonusMana;

		weapon.GetComponent<SpriteRenderer>().enabled = false;
	}

	//armors
	public virtual void EquipRandomArmor()
	{
		GameObject go;
		int index;

		if (entityStats.entityBaseStats.possibleHelmetsList.Count != 0)
		{
			if (helmetSlotContainer.transform.childCount == 0)
			{
				go = Instantiate(itemPrefab, helmetSlotContainer.transform);
				go.AddComponent<Armors>();
				equippedHelmet = go.GetComponent<Armors>();
			}

			index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleHelmetsList.Count);
			if (equippedHelmet != null)
			{
				equippedHelmet.armorBaseRef = entityStats.entityBaseStats.possibleHelmetsList[index];
				equippedHelmet.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
				equippedHelmet.GetComponent<SpriteRenderer>().enabled = false;
			}
		}

		if (entityStats.entityBaseStats.possibleChestpiecesList.Count != 0)
		{
			if (chestpieceSlotContainer.transform.childCount == 0)
			{
				go = Instantiate(itemPrefab, chestpieceSlotContainer.transform);
				go.AddComponent<Armors>();
				equippedChestpiece = go.GetComponent<Armors>();
			}

			index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleChestpiecesList.Count);
			if (equippedChestpiece != null)
			{
				equippedChestpiece.armorBaseRef = entityStats.entityBaseStats.possibleChestpiecesList[index];
				equippedChestpiece.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
				equippedChestpiece.GetComponent<SpriteRenderer>().enabled = false;
			}
		}

		if (entityStats.entityBaseStats.possibleLegsList.Count != 0)
		{
			if (legsSlotContainer.transform.childCount == 0)
			{
				go = Instantiate(itemPrefab, legsSlotContainer.transform);
				go.AddComponent<Armors>();
				equippedLegs = go.GetComponent<Armors>();
			}

			index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleLegsList.Count);
			if (equippedLegs != null)
			{
				equippedLegs.armorBaseRef = entityStats.entityBaseStats.possibleLegsList[index];
				equippedLegs.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
				equippedLegs.GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}
	public void OnArmorUnequip(Armors armor)
	{
		if (armor != null)
		{
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
	}
	public void OnArmorEquip(Armors armor)
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

		armor.GetComponent<SpriteRenderer>().enabled = false;
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
}
