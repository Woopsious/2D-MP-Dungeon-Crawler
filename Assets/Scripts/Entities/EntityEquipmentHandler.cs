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
	public void OnWeaponEquip(Weapons weapon)
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
	}

	//armors
	public void EquipRandomArmor()
	{
		GameObject go;
		int index;

		if (entityStats.entityBaseStats.possibleHelmetsList.Count != 0)
		{
			if (equippedHelmet == null)
			{
				go = Instantiate(itemPrefab, helmetSlotContainer.transform);
				go.AddComponent<Armors>();
				equippedHelmet = go.GetComponent<Armors>();
			}

			index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleHelmetsList.Count);

			equippedHelmet.armorBaseRef = entityStats.entityBaseStats.possibleHelmetsList[index];
			equippedHelmet.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
			equippedHelmet.GetComponent<SpriteRenderer>().enabled = false;
		}

		if (entityStats.entityBaseStats.possibleChestpiecesList.Count != 0)
		{
			if (equippedChestpiece == null)
			{
				go = Instantiate(itemPrefab, chestpieceSlotContainer.transform);
				go.AddComponent<Armors>();
				equippedChestpiece = go.GetComponent<Armors>();
			}

			index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleChestpiecesList.Count);

			equippedChestpiece.armorBaseRef = entityStats.entityBaseStats.possibleChestpiecesList[index];
			equippedChestpiece.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
			equippedChestpiece.GetComponent<SpriteRenderer>().enabled = false;
		}

		if (entityStats.entityBaseStats.possibleLegsList.Count != 0)
		{
			if (equippedLegs == null)
			{
				go = Instantiate(itemPrefab, legsSlotContainer.transform);
				go.AddComponent<Armors>();
				equippedLegs = go.GetComponent<Armors>();
			}
			index = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleLegsList.Count);

			equippedLegs.armorBaseRef = entityStats.entityBaseStats.possibleLegsList[index];
			equippedLegs.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel, this);
			equippedLegs.GetComponent<SpriteRenderer>().enabled = false;
		}
	}
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
	public void OnAccessoryEquip(Accessories accessory)
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
