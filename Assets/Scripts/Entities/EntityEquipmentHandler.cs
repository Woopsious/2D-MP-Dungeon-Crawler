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

	[Header("Armor")]
	public GameObject helmetSlotContainer;
	public GameObject chestpieceSlotContainer;
	public GameObject legsSlotContainer;

	public Armors equippedHelmet;
	public Armors equippedChestpiece;
	public Armors equippedLegs;

	[Header("Bonuses Provided By Equipment")]
	public int bonusEquipmentHealth;
	public int bonusEquipmentMana;

	public int bonusEquipmentPhysicalResistance;
	public int bonusEquipmentPoisonResistance;
	public int bonusEquipmentFireResistance;
	public int bonusEquipmentIceResistance;

	public virtual void Start()
	{
		entityStats = gameObject.GetComponentInParent<EntityStats>();
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
			equippedWeapon.entityEquipmentHandler = this;
			equippedWeapon.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel);
		}
	}
	public void OnWeaponUnequip(Weapons weapon)
	{
		if (equippedWeapon != null)
		{
			bonusEquipmentMana -= weapon.bonusMana;

			entityStats.maxMana -= weapon.bonusMana;
			entityStats.currentMana -= weapon.bonusMana;
		}
	}
	public void OnWeaponEquip(Weapons weapon, bool equippedByPlayer, bool equippedByOther)
	{
		weapon.isEquippedByPlayer = equippedByPlayer;
		weapon.isEquippedByOther = equippedByOther;
		bonusEquipmentMana += weapon.bonusMana;

		entityStats.maxMana -= weapon.bonusMana;
		entityStats.currentMana -= weapon.bonusMana;
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
				equippedHelmet.entityEquipmentHandler = this;
				equippedHelmet.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel);
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
				equippedChestpiece.entityEquipmentHandler = this;
				equippedChestpiece.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel);
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
				equippedLegs.entityEquipmentHandler = this;
				equippedLegs.SetItemStats(Items.Rarity.isCommon, entityStats.entityLevel);
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
	}
}
