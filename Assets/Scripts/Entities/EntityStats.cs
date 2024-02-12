using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
	[Header("Entity Info")]
	public SOEntityStats entityBaseStats;
	[HideInInspector] public EntityClassHandler classHandler;
	[HideInInspector] public EntityEquipmentHandler equipmentHandler;
	private SpriteRenderer spriteRenderer;
	public int entityLevel;
	public float levelModifier;

	[Header("Health")]
	private int entityMaxHealth;
	public int maxHealth;
	public int currentHealth;

	[Header("Mana")]
	private int entityMaxMana;
	public int maxMana;
	public int currentMana;
	private int manaRegenPercentage;
	private float manaRegenCooldown;
	private float manaRegenTimer;

	[Header("Resistances")]
	private int entityPhysicalResistance;
	private int entityPoisonResistance;
	private int entityFireResistance;
	private int entityIceResistance;
	public int physicalResistance;
	public int poisonResistance;
	public int fireResistance;
	public int iceResistance;

	[Header("Basic Stat Modifiers")]
	public int maxHealthModifier;
	public int maxManaModifier;
	public int manaRegenPercentageModifier;

	[Header("Percentage Resistance Modifiers")]
	public int physicalResistanceModifier;
	public int poisonResistanceModifier;
	public int fireResistanceModifier;
	public int iceResistanceModifier;

	[Header("Percentage Damage Modifiers")]
	public int physicalDamagePercentageModifier;
	public int poisonDamagePercentageModifier;
	public int fireDamagePercentageModifier;
	public int iceDamagePercentageModifier;

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

		CalculateStats();
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
			healthValue = maxHealth / 100 * healthValue;

		currentHealth += healthValue;

		if (currentHealth > maxHealth)
			currentHealth = maxHealth;

		OnHealthChangeEvent?.Invoke(maxHealth, currentHealth);
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
			damage -= poisonResistance;
		}
		if (damageType == IDamagable.DamageType.isFireDamageType)
		{
			Debug.Log("Fire Dmg res: " + fireResistance);
			damage -= fireResistance;
		}
		if (damageType == IDamagable.DamageType.isIceDamageType)
		{
			Debug.Log("Ice Dmg res: " + iceResistance);
			damage -= iceResistance;
		}
		else
		{
			Debug.Log("Physical Dmg res: " + physicalResistance);
			damage -= physicalResistance;
		}

		if (damage < 2) //always deal 2 damage
			damage = 2;

		currentHealth -= damage;
		RedFlashOnRecieveDamage();

		OnHealthChangeEvent?.Invoke(maxHealth, currentHealth);

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
		if (currentMana >= maxMana) return;

		manaRegenTimer -= Time.deltaTime;

		if (manaRegenTimer <= 0)
		{
			manaRegenTimer = manaRegenCooldown;
			IncreaseMana(manaRegenPercentage, true);
		}
	}
	public void IncreaseMana(int manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana / 100 * manaValue;

		currentMana += manaValue;
		if (currentMana > maxMana)
			currentMana = maxMana;

		OnManaChangeEvent?.Invoke(maxMana, currentMana);
	}
	public void DecreaseMana(int manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana / 100 * manaValue;

		currentMana -= manaValue;
		OnManaChangeEvent?.Invoke(maxMana, currentMana);
	}

	private void OnPlayerLevelUp(int newPlayerLevel)
	{
		if (currentHealth <= 0) return;

		entityLevel = newPlayerLevel;
		bool wasDamaged = true;

		if (maxHealth == currentHealth)
			wasDamaged = false;

		int oldMaxHP = entityMaxHealth;
		int oldCurrentHP = currentHealth;
		int oldcurrentMana = currentMana;
		CalculateStats();

		if (GetComponent<PlayerController>() == null && !wasDamaged) //if not player or taken damage full heal entity
			currentHealth = maxHealth;
		else
			currentHealth = oldCurrentHP + (entityMaxHealth - oldMaxHP); //else return health + extra health from new level/modifiers

		OnHealthChangeEvent?.Invoke(maxHealth, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana, currentMana);
	}

	/// <summary>
	/// recalculate stats when ever needed as hard bonuses and percentage bonuses need to be recalculated, if entity equipment for example
	/// changes a piece of armor etc, so stats are consistant in how they are applied, especially when percentage modifiers are included
	/// hard numbers added so far: equipment values, 
	/// percentage numbers added so far: level modifier, 
	/// </summary>
	public void CalculateStats()
	{
		if (entityLevel == 1)  //get level modifier
			levelModifier = 1;
		else
			levelModifier = 1 + (entityLevel / 10f);

		float currenthealthPercentage = 100;
		if (currentHealth != 0 && maxHealth != 0)//remove divide by 0 error
			currenthealthPercentage = (float)currentHealth / maxHealth * 100;

		float currentmanaPercentage = 100;
		if (currentHealth != 0 && maxHealth != 0)//remove divide by 0 error
			currentmanaPercentage = (float)currentMana / maxMana * 100;

		entityMaxHealth = (int)(entityBaseStats.maxHealth * levelModifier);
		entityMaxMana = (int)(entityBaseStats.maxMana * levelModifier);
		entityPhysicalResistance = (int)(entityBaseStats.physicalDamageResistance * levelModifier);
		entityPoisonResistance = (int)(entityBaseStats.poisonDamageResistance * levelModifier);
		entityFireResistance = (int)(entityBaseStats.fireDamageResistance * levelModifier);
		entityIceResistance = (int)(entityBaseStats.iceDamageResistance * levelModifier);

		maxHealth = (int)(entityBaseStats.maxHealth * levelModifier + equipmentHandler.equipmentHealth);
		maxMana = (int)(entityBaseStats.maxMana * levelModifier + equipmentHandler.equipmentMana);
		physicalResistance = (int)(entityBaseStats.physicalDamageResistance * levelModifier + equipmentHandler.equipmentPhysicalResistance);
		poisonResistance = (int)(entityBaseStats.poisonDamageResistance * levelModifier + equipmentHandler.equipmentPoisonResistance);
		fireResistance = (int)(entityBaseStats.fireDamageResistance * levelModifier + equipmentHandler.equipmentFireResistance);
		iceResistance = (int)(entityBaseStats.iceDamageResistance * levelModifier + equipmentHandler.equipmentIceResistance);

		if (equipmentHandler.equippedWeapon != null)
			equipmentHandler.equippedWeapon.UpdateWeaponDamage(this, equipmentHandler.equippedOffhandWeapon);

		OnHealthChangeEvent?.Invoke(maxHealth, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana, currentMana);
	}

	public int GetStatNum(int baseNum, int equipmentNum, float modifierNum)
	{
		baseNum = (int)(baseNum + equipmentNum * modifierNum);
		return baseNum;
	}
}
