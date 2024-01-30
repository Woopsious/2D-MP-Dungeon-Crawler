using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
	[Header("Entity Info")]
	public SOEntityStats entityBaseStats;
	[HideInInspector] public EntityEquipmentHandler entityEquipment;
	private SpriteRenderer spriteRenderer;
	public int entityLevel;
	public float statModifier;

	[Header("Health")]
	public int maxHealth;
	public int currentHealth;

	[Header("Mana")]
	public int maxMana;
	public int currentMana;
	public float manaRegenCooldown;
	private float manaRegenTimer;

	[Header("Resistances")]
	public int physicalResistance;
	public int poisonResistance;
	public int fireResistance;
	public int iceResistance;

	[Header("Percentage Damage Bonuses")]
	public int bonusPhysicalDamagePercentage;
	public int bonusPoisonDamagePercentage;
	public int bonusFireDamagePercentage;
	public int bonusIceDamagePercentage;

	public event Action<int, bool> onRecieveHealingEvent;
	public event Action<int, IDamagable.DamageType> onRecieveDamageEvent;

	public event Action<int, int> onHealthChangeEvent;
	public event Action<int, int> onManaChangeEvent;

	public event Action<GameObject> onDeathEvent;

	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		onRecieveHealingEvent += RecieveHealing;
		GetComponent<Damageable>().OnHit += OnHit;
		onRecieveDamageEvent += RecieveDamage;

		PlayerExperienceHandler playerExperienceHandler = FindObjectOfType<PlayerExperienceHandler>();

		onDeathEvent += playerExperienceHandler.AddExperience;
		playerExperienceHandler.onPlayerLevelUpEvent += OnPlayerLevelUp;
	}
	private void OnDisable()
	{
		onRecieveHealingEvent -= RecieveHealing;
		GetComponent<Damageable>().OnHit -= OnHit;
		onRecieveDamageEvent -= RecieveDamage;

		PlayerExperienceHandler playerExperienceHandler = FindObjectOfType<PlayerExperienceHandler>();

		onDeathEvent -= playerExperienceHandler.AddExperience;
		playerExperienceHandler.onPlayerLevelUpEvent -= OnPlayerLevelUp;
	}

	private void Update()
	{
		PassiveManaRegen();
	}

	public void Initilize()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.sprite = GetComponent<EntityStats>().entityBaseStats.sprite;

		float modifier = (entityLevel - 1f) / 20;  //get level modifier
		statModifier = modifier += 1;

		maxHealth = (int)(entityBaseStats.maxHealth * statModifier);
		currentHealth = (int)(entityBaseStats.maxHealth * statModifier);
		maxMana = (int)(entityBaseStats.maxMana * statModifier);
		currentMana = (int)(entityBaseStats.maxMana * statModifier);

		physicalResistance = (int)(entityBaseStats.physicalDamageResistance * statModifier);
		poisonResistance = (int)(entityBaseStats.poisonDamageResistance * statModifier);
		fireResistance = (int)(entityBaseStats.fireDamageResistance * statModifier);
		iceResistance = (int)(entityBaseStats.iceDamageResistance * statModifier);

		int numOfTries = 0;
		entityEquipment.StartCoroutine(entityEquipment.SpawnEntityEquipment(numOfTries));
	}

	/// <summary>
	/// for MP when recieving damage as levels will no be synced, ill need to convert damage and healing recieved to a precentage 
	/// when it comes to resistances ignoring the difference shouldnt matter as everything scales at the same rate
	/// </summary>
	//health functions
	public void OnHeal(int healthValue, bool isPercentageValue)
	{
		onRecieveHealingEvent?.Invoke(healthValue, isPercentageValue);
	}
	private void RecieveHealing(int healthValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			healthValue = maxHealth / 100 * healthValue;

		currentHealth += healthValue;
		onHealthChangeEvent?.Invoke(maxHealth, currentHealth);

		if (currentHealth > maxHealth)
			currentHealth = maxHealth;
	}
	public void OnHit(int damage, IDamagable.DamageType damageType, bool isDestroyedInOneHit)
	{
        if (isDestroyedInOneHit)
        {
			onDeathEvent.Invoke(gameObject);
			return;
        }

		onRecieveDamageEvent?.Invoke(damage, damageType);
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

		onHealthChangeEvent?.Invoke(maxHealth, currentHealth);

		///
		/// invoke onRecieveDamage like onEntityDeath that calls hit animations/sounds/ui health bar update
		/// also could invoke a onEntityDeath that instead calls functions in scripts to disable them then and play death sounds/animations
		/// this way if an entity does have a death sound but no death animation i dont need to run checks or hard code a reference
		/// and a box for instance can just have a death sound and instead of a death animation has a death partical effect explosion
		///

		if (currentHealth <= 0)
		{
			onDeathEvent?.Invoke(gameObject);
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
			IncreaseMana(2, true);
		}
	}
	public void IncreaseMana(int manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana / 100 * manaValue;

		currentMana += manaValue;
		onManaChangeEvent?.Invoke(maxMana, currentMana);

		if (currentMana > maxMana)
			currentMana = maxMana;
	}
	public void DecreaseMana(int manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana / 100 * manaValue;

		currentMana -= manaValue;
		onManaChangeEvent?.Invoke(maxMana, currentMana);
	}

	private void OnPlayerLevelUp(int newPlayerLevel)
	{
		if (currentHealth <= 0) return;

		int oldMaxHealth = maxHealth;
		int oldCurrentHealth = currentHealth;
        entityLevel = newPlayerLevel;
		float modifier = (entityLevel - 1f) / 20;  //get level modifier / 20
		statModifier = modifier += 1;

		int newMaxHealth = (int)(entityBaseStats.maxHealth * statModifier) - maxHealth;
		int newMaxMana = (int)(entityBaseStats.maxMana * statModifier) - maxMana;
		int newPhysicalResistance = (int)(entityBaseStats.physicalDamageResistance * statModifier) - physicalResistance;
		int newPoisonResistance = (int)(entityBaseStats.poisonDamageResistance * statModifier) - poisonResistance;
		int newFireResistance = (int)(entityBaseStats.fireDamageResistance * statModifier) - fireResistance;
		int newIceResistance = (int)(entityBaseStats.iceDamageResistance * statModifier) - iceResistance;

		if (GetComponent<PlayerController>()  != null)
		{
			Debug.LogError("new max health: " + newMaxHealth);
			Debug.LogError("new max physical: " + newPhysicalResistance);
		}

		maxHealth += newMaxHealth;
		maxMana += newMaxMana;
		physicalResistance += newPhysicalResistance;
		poisonResistance += newPoisonResistance;
		fireResistance += newFireResistance;
		iceResistance += newIceResistance;


		if (GetComponent<PlayerController>() == null && oldMaxHealth == oldCurrentHealth) //if not player or taken damage full heal entity
			currentHealth = maxHealth;
		else
			currentHealth = oldCurrentHealth;

		/// <summary>
		/// when class trees are made and have additional bonuses ill reapply them here
		/// </summary>
	}
}
