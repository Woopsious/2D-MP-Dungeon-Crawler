using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
	[Header("Entity Info")]
	public SOEntityStats entityBaseStats;
	public EntityClassHandler classHandler;
	public EntityEquipmentHandler equipmentHandler;
	private SpriteRenderer spriteRenderer;
	public int entityLevel;
	public float levelModifier;

	[Header("Health")]
	public Stat maxHealth;
	public int currentHealth;

	[Header("Mana")]
	public Stat maxMana;
	public int currentMana;
	public Stat manaRegenPercentage;
	private float manaRegenCooldown;
	private float manaRegenTimer;

	[Header("Resistances")]
	public Stat physicalResistance;
	public Stat poisonResistance;
	public Stat fireResistance;
	public Stat iceResistance;

	[Header("Damage Modifiers")]
	public Stat physicalDamagePercentageModifier;
	public Stat poisonDamagePercentageModifier;
	public Stat fireDamagePercentageModifier;
	public Stat iceDamagePercentageModifier;
	public Stat mainWeaponDamageModifier;
	public Stat dualWeaponDamageModifier;

	public event Action<int, bool> OnRecieveHealingEvent;
	public event Action<int, IDamagable.DamageType> OnRecieveDamageEvent;

	public event Action<int, int> OnHealthChangeEvent;
	public event Action<int, int> OnManaChangeEvent;

	public event Action<GameObject> OnDeathEvent;

	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		OnRecieveHealingEvent += RecieveHealing;
		GetComponent<Damageable>().OnHit += OnHit;
		OnRecieveDamageEvent += RecieveDamage;

		//for mp needs to be a list of ExpHandlers for each player
		PlayerExperienceHandler playerExperienceHandler = FindObjectOfType<PlayerExperienceHandler>();

		OnDeathEvent += playerExperienceHandler.AddExperience;
		playerExperienceHandler.OnPlayerLevelUpEvent += OnPlayerLevelUp;

		EntityClassHandler entityClassHandler = GetComponent<EntityClassHandler>();
		entityClassHandler.OnClassChange += OnClassChanges;
		entityClassHandler.OnStatUnlock += OnStatUnlock;

		EntityEquipmentHandler entityEquipmentHandler = GetComponent<EntityEquipmentHandler>();
		entityEquipmentHandler.OnEquipmentChanges += OnEquipmentChanges;
	}
	private void OnDisable()
	{
		OnRecieveHealingEvent -= RecieveHealing;
		GetComponent<Damageable>().OnHit -= OnHit;
		OnRecieveDamageEvent -= RecieveDamage;

		//for mp needs to be a list of ExpHandlers for each player
		PlayerExperienceHandler playerExperienceHandler = FindObjectOfType<PlayerExperienceHandler>();

		OnDeathEvent -= playerExperienceHandler.AddExperience;
		playerExperienceHandler.OnPlayerLevelUpEvent -= OnPlayerLevelUp;

		EntityClassHandler entityClassHandler = GetComponent<EntityClassHandler>();
		entityClassHandler.OnClassChange -= OnClassChanges;
		entityClassHandler.OnStatUnlock -= OnStatUnlock;

		EntityEquipmentHandler entityEquipmentHandler = GetComponent<EntityEquipmentHandler>();
		entityEquipmentHandler.OnEquipmentChanges -= OnEquipmentChanges;
	}

	private void Update()
	{
		PassiveManaRegen();
	}

	public void Initilize()
	{
		equipmentHandler = GetComponent<EntityEquipmentHandler>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.sprite = GetComponent<EntityStats>().entityBaseStats.sprite;

		CalculateBaseStats();
		int numOfTries = 0;
		equipmentHandler.StartCoroutine(equipmentHandler.SpawnEntityEquipment(numOfTries));
	}

	/// <summary>
	/// for MP when recieving damage as levels will no be synced, ill need to convert damage and healing recieved to a precentage 
	/// when it comes to resistances ignoring the difference shouldnt matter as everything scales at the same rate
	/// </summary>
	//health functions
	public void OnHeal(int healthValue, bool isPercentageValue)
	{
		OnRecieveHealingEvent?.Invoke(healthValue, isPercentageValue);
	}
	private void RecieveHealing(int healthValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			healthValue = maxHealth.finalValue / 100 * healthValue;

		currentHealth += healthValue;

		if (currentHealth > maxHealth.finalValue)
			currentHealth = maxHealth.finalValue;

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
	}
	public void OnHit(int damage, IDamagable.DamageType damageType, bool isDestroyedInOneHit)
	{
        if (isDestroyedInOneHit)
        {
			OnDeathEvent.Invoke(gameObject);
			return;
        }

		OnRecieveDamageEvent?.Invoke(damage, damageType);
	}
	private void RecieveDamage(int damage, IDamagable.DamageType damageType)
	{
		Debug.Log(gameObject.name + " recieved :" + damage);
		if (damageType == IDamagable.DamageType.isPoisonDamageType)
		{
			Debug.Log("Poison Dmg res: " + poisonResistance);
			damage -= poisonResistance.finalValue;
		}
		if (damageType == IDamagable.DamageType.isFireDamageType)
		{
			Debug.Log("Fire Dmg res: " + fireResistance);
			damage -= fireResistance.finalValue;
		}
		if (damageType == IDamagable.DamageType.isIceDamageType)
		{
			Debug.Log("Ice Dmg res: " + iceResistance);
			damage -= iceResistance.finalValue;
		}
		else
		{
			Debug.Log("Physical Dmg res: " + physicalResistance);
			damage -= physicalResistance.finalValue;
		}

		if (damage < 2) //always deal 2 damage
			damage = 2;

		currentHealth -= damage;
		RedFlashOnRecieveDamage();

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);

		///
		/// invoke onRecieveDamage like onEntityDeath that calls hit animations/sounds/ui health bar update
		/// also could invoke a onEntityDeath that instead calls functions in scripts to disable them then and play death sounds/animations
		/// this way if an entity does have a death sound but no death animation i dont need to run checks or hard code a reference
		/// and a box for instance can just have a death sound and instead of a death animation has a death partical effect explosion
		///

		if (currentHealth <= 0)
		{
			OnDeathEvent?.Invoke(gameObject);
			Destroy(gameObject);
		}
		//healthUi.UpdateHealthBar(currentHealth, maxHealth);	//ui not made atm
		Debug.Log("health lost after resistance: " + damage + " | current health: " + currentHealth);
	}
	private void RedFlashOnRecieveDamage()
	{
		spriteRenderer.color = Color.red;
		StartCoroutine(ResetRedFlashOnRecieveDamage());
	}
	IEnumerator ResetRedFlashOnRecieveDamage()
	{
		yield return new WaitForSeconds(0.1f);
		spriteRenderer.color = Color.white;
	}

	//mana functions
	public void PassiveManaRegen()
	{
		if (currentMana >= maxMana.finalValue) return;

		manaRegenTimer -= Time.deltaTime;

		if (manaRegenTimer <= 0)
		{
			manaRegenTimer = manaRegenCooldown;
			IncreaseMana(manaRegenPercentage.finalValue, true);
		}
	}
	public void IncreaseMana(int manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana.finalValue / 100 * manaValue;

		currentMana += manaValue;
		if (currentMana > maxMana.finalValue)
			currentMana = maxMana.finalValue;

		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
	}
	public void DecreaseMana(int manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana.finalValue / 100 * manaValue;

		currentMana -= manaValue;
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
	}

	private void OnPlayerLevelUp(int newPlayerLevel)
	{
		if (currentHealth <= 0) return;

		entityLevel = newPlayerLevel;
		bool wasDamaged = true;

		if (maxHealth.finalValue == currentHealth)
			wasDamaged = false;

		int oldMaxHP = maxHealth.baseValue;
		int oldCurrentHP = currentHealth;
		int oldcurrentMana = currentMana;
		CalculateBaseStats();

		if (GetComponent<PlayerController>() == null && !wasDamaged) //if not player or taken damage full heal entity
			currentHealth = maxHealth.finalValue;
		else
			currentHealth = oldCurrentHP + (maxHealth.finalValue - oldMaxHP); //else return health + extra health from new level/modifiers

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
	}

	/// <summary>
	/// recalculate stats when ever needed as hard bonuses and percentage bonuses need to be recalculated, if entity equipment for example
	/// changes a piece of armor etc, so stats are consistant in how they are applied, especially when percentage modifiers are included
	/// hard numbers added so far: equipment values, 
	/// percentage numbers added so far: level modifier, 
	/// </summary>
	public void CalculateBaseStats()
	{
		if (entityLevel == 1)  //get level modifier
			levelModifier = 1;
		else
			levelModifier = 1 + (entityLevel / 10f);

		maxHealth.SetBaseValue((int)(entityBaseStats.maxHealth * levelModifier));
		maxMana.SetBaseValue((int)(entityBaseStats.maxMana * levelModifier));
		manaRegenPercentage.SetBaseValue(entityBaseStats.manaRegenPercentage);
		manaRegenCooldown = entityBaseStats.manaRegenCooldown;
		physicalResistance.SetBaseValue((int)(entityBaseStats.physicalDamageResistance * levelModifier));
		poisonResistance.SetBaseValue((int)(entityBaseStats.poisonDamageResistance * levelModifier));
		fireResistance.SetBaseValue((int)(entityBaseStats.fireDamageResistance * levelModifier));
		iceResistance.SetBaseValue((int)(entityBaseStats.iceDamageResistance * levelModifier));

		physicalDamagePercentageModifier.SetBaseValue(1);
		poisonDamagePercentageModifier.SetBaseValue(1);
		fireDamagePercentageModifier.SetBaseValue(1);
		iceDamagePercentageModifier.SetBaseValue(1);
		mainWeaponDamageModifier.SetBaseValue(1);
		dualWeaponDamageModifier.SetBaseValue(1);

		currentHealth = maxHealth.finalValue - maxHealth.equipmentValue;
		currentMana = maxMana.finalValue - maxMana.equipmentValue;
		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(physicalDamagePercentageModifier.finalPercentageValue,
		poisonDamagePercentageModifier.finalPercentageValue, fireDamagePercentageModifier.finalPercentageValue,
		iceDamagePercentageModifier.finalPercentageValue, mainWeaponDamageModifier.finalPercentageValue,
		dualWeaponDamageModifier.finalPercentageValue, equipmentHandler.equippedOffhandWeapon);
	}
	public void OnEquipmentChanges(EntityEquipmentHandler equipmentHandler)
	{
		bool oldCurrentHealthEqualToOldMaxHealth = false;
		if (currentHealth == maxHealth.finalValue)
			oldCurrentHealthEqualToOldMaxHealth = true;

		maxHealth.UpdateEquipmentValue(equipmentHandler.equipmentHealth);
		maxMana.UpdateEquipmentValue(equipmentHandler.equipmentMana);
		physicalResistance.UpdateEquipmentValue(equipmentHandler.equipmentPhysicalResistance);
		poisonResistance.UpdateEquipmentValue(equipmentHandler.equipmentPoisonResistance);
		fireResistance.UpdateEquipmentValue(equipmentHandler.equipmentFireResistance);
		iceResistance.UpdateEquipmentValue(equipmentHandler.equipmentIceResistance);

		physicalDamagePercentageModifier.UpdateEquipmentPercentageValue(equipmentHandler.physicalDamagePercentage);
		poisonDamagePercentageModifier.UpdateEquipmentPercentageValue(equipmentHandler.poisonDamagePercentage);
		fireDamagePercentageModifier.UpdateEquipmentPercentageValue(equipmentHandler.fireDamagePercentage);
		iceDamagePercentageModifier.UpdateEquipmentPercentageValue(equipmentHandler.iceDamagePercentage);

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(physicalDamagePercentageModifier.finalPercentageValue,
		poisonDamagePercentageModifier.finalPercentageValue, fireDamagePercentageModifier.finalPercentageValue,
		iceDamagePercentageModifier.finalPercentageValue, mainWeaponDamageModifier.finalPercentageValue,
		dualWeaponDamageModifier.finalPercentageValue, equipmentHandler.equippedOffhandWeapon);
	}
	public void OnStatUnlock(SOClassStatBonuses statBoost)
	{
		bool oldCurrentHealthEqualToOldMaxHealth = false;
		if (currentHealth == maxHealth.finalValue)
			oldCurrentHealthEqualToOldMaxHealth = true;

		maxHealth.AddPercentageValue(statBoost.healthBoostValue);
		maxMana.AddPercentageValue(statBoost.manaBoostValue);
		physicalResistance.AddPercentageValue(statBoost.physicalResistanceBoostValue);
		poisonResistance.AddPercentageValue(statBoost.poisonResistanceBoostValue);
		fireResistance.AddPercentageValue(statBoost.fireResistanceBoostValue);
		iceResistance.AddPercentageValue(statBoost.iceResistanceBoostValue);

		physicalDamagePercentageModifier.AddPercentageValue(statBoost.physicalDamageBoostValue);
		poisonDamagePercentageModifier.AddPercentageValue(statBoost.posionDamageBoostValue);
		fireDamagePercentageModifier.AddPercentageValue(statBoost.fireDamageBoostValue);
		iceDamagePercentageModifier.AddPercentageValue(statBoost.iceDamageBoostValue);
		mainWeaponDamageModifier.AddPercentageValue(statBoost.mainWeaponDamageBoostValue);
		dualWeaponDamageModifier.AddPercentageValue(statBoost.duelWeaponDamageBoostValue);

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(physicalDamagePercentageModifier.finalPercentageValue,
		poisonDamagePercentageModifier.finalPercentageValue, fireDamagePercentageModifier.finalPercentageValue,
		iceDamagePercentageModifier.finalPercentageValue, mainWeaponDamageModifier.finalPercentageValue,
		dualWeaponDamageModifier.finalPercentageValue, equipmentHandler.equippedOffhandWeapon);
	}
	public void OnClassChanges(EntityClassHandler classHandler) //also called when class is reset
	{
		bool oldCurrentHealthEqualToOldMaxHealth = false;
		if (currentHealth == maxHealth.finalValue)
			oldCurrentHealthEqualToOldMaxHealth = true;

		foreach (SOClassStatBonuses statBoost in classHandler.unlockedStatBoostList)
		{
			maxHealth.RemovePercentageValue(statBoost.healthBoostValue);
			maxMana.RemovePercentageValue(statBoost.manaBoostValue);
			physicalResistance.RemovePercentageValue(statBoost.physicalResistanceBoostValue);
			poisonResistance.RemovePercentageValue(statBoost.poisonResistanceBoostValue);
			fireResistance.RemovePercentageValue(statBoost.fireResistanceBoostValue);
			iceResistance.RemovePercentageValue(statBoost.iceResistanceBoostValue);

			physicalDamagePercentageModifier.RemovePercentageValue(statBoost.physicalDamageBoostValue);
			poisonDamagePercentageModifier.RemovePercentageValue(statBoost.posionDamageBoostValue);
			fireDamagePercentageModifier.RemovePercentageValue(statBoost.fireDamageBoostValue);
			iceDamagePercentageModifier.RemovePercentageValue(statBoost.iceDamageBoostValue);
			mainWeaponDamageModifier.RemovePercentageValue(statBoost.mainWeaponDamageBoostValue);
			dualWeaponDamageModifier.RemovePercentageValue(statBoost.duelWeaponDamageBoostValue);
		}
		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(physicalDamagePercentageModifier.finalPercentageValue,
		poisonDamagePercentageModifier.finalPercentageValue, fireDamagePercentageModifier.finalPercentageValue,
		iceDamagePercentageModifier.finalPercentageValue, mainWeaponDamageModifier.finalPercentageValue,
		dualWeaponDamageModifier.finalPercentageValue, equipmentHandler.equippedOffhandWeapon);
	}
	//function will full heal entity if not player and at full health when stat changes
	public void FullHealOnStatChange(bool oldCurrentHealthEqualToOldMaxHealth)
	{
		if (GetComponent<PlayerController>() == null && oldCurrentHealthEqualToOldMaxHealth)
		{
			currentHealth = maxHealth.finalValue;
			currentMana = maxMana.finalValue;
		}

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
	}
}
