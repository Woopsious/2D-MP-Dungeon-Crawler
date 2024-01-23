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
	[HideInInspector] public PlayerEquipmentHandler playerEquipment;
	private SpriteRenderer spriteRenderer;
	public int entityLevel;
	public float statModifier;

	[Header("Health")]
	public int maxHealth;
	public int currentHealth;

	[Header("Mana")]
	public int maxMana;
	public int currentMana;

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

	public event Action<int, int> onRecieveDamageEvent;
	public event Action<GameObject, PlayerController> onDeathEvent;

	private void Start()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.sprite = GetComponent<EntityStats>().entityBaseStats.sprite;

		SetStats();
	}
	private void OnEnable()
	{
		GetComponent<Damageable>().OnHit += RecieveDamage;

		PlayerExperienceHandler playerExperienceHandler = FindObjectOfType<PlayerExperienceHandler>();

		onDeathEvent += playerExperienceHandler.AddExperience;
		playerExperienceHandler.onPlayerLevelUpEvent += OnPlayerLevelUp;
	}
	private void OnDisable()
	{
		GetComponent<Damageable>().OnHit -= RecieveDamage;

		PlayerExperienceHandler playerExperienceHandler = FindObjectOfType<PlayerExperienceHandler>();

		onDeathEvent -= playerExperienceHandler.AddExperience;
		playerExperienceHandler.onPlayerLevelUpEvent -= OnPlayerLevelUp;
	}

	public void SetStats()
	{
		float modifier = (entityLevel - 1f) / 20;  //get level modifier
		statModifier = modifier += 1;

		maxHealth = (int)(entityBaseStats.maxHealth * statModifier);
		currentHealth = (int)(entityBaseStats.maxHealth * statModifier);
		physicalResistance = (int)(entityBaseStats.physicalDamageResistance * statModifier);
		poisonResistance = (int)(entityBaseStats.poisonDamageResistance * statModifier);
		fireResistance = (int)(entityBaseStats.fireDamageResistance * statModifier);
		iceResistance = (int)(entityBaseStats.iceDamageResistance * statModifier);

		int numOfTries = 0;
		StartCoroutine(SpawnEntityEquipment(numOfTries));
	}
	public IEnumerator SpawnEntityEquipment(int numOfTries)
	{
		if (numOfTries >= 5)
			Debug.LogWarning("Unable to Spawn Entity Equipment");

		else if (entityEquipment == null)
		{
			numOfTries += 1;
			yield return new WaitForSeconds(0.1f);
			StartCoroutine(SpawnEntityEquipment(numOfTries));
		}
		else
		{
			entityEquipment.EquipRandomWeapon();
			entityEquipment.EquipRandomArmor();
		}
	}

	/// <summary>
	/// for MP when recieving damage as levels will no be synced, ill need to convert damage and healing recieved to a precentage 
	/// when it comes to resistances ignoring the difference shouldnt matter as everything scales at the same rate
	/// </summary>
	//health functions
	public virtual void RecieveHealing(int health)
	{
		currentHealth += health;
		if (currentHealth > maxHealth)
			currentHealth = maxHealth;
	}
	public void RecieveDamage(int damage, IDamagable.DamageType damageType, bool isDestroyedInOneHit)
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

		onRecieveDamageEvent?.Invoke(maxHealth, currentHealth);

		///
		/// invoke onRecieveDamage like onEntityDeath that calls hit animations/sounds/ui health bar update
		/// also could invoke a onEntityDeath that instead calls functions in scripts to disable them then and play death sounds/animations
		/// this way if an entity does have a death sound but no death animation i dont need to run checks or hard code a reference
		/// and a box for instance can just have a death sound and instead of a death animation has a death partical effect explosion
		///

		if (currentHealth <= 0)
		{
			onDeathEvent?.Invoke(gameObject, GetComponent<EntityBehaviour>().player);
			Destroy(gameObject);
		}
		//healthUi.UpdateHealthBar(currentHealth, maxHealth);	//ui not made atm
		Debug.Log("health lost after resistance: " + damage + " | current health: " + currentHealth);
	}
	public void RedFlashOnRecieveDamage()
	{
		spriteRenderer.color = Color.red;
		StartCoroutine(ResetRedFlashOnRecieveDamage());
	}
	IEnumerator ResetRedFlashOnRecieveDamage()
	{
		yield return new WaitForSeconds(0.1f);
		spriteRenderer.color = Color.white;
	}

	public void OnPlayerLevelUp(int newPlayerLevel)
	{
		entityLevel = newPlayerLevel;
		float modifier = (entityLevel - 1f) / 1;  //get level modifier / 20
		statModifier = modifier += 1;

		maxHealth = (int)(entityBaseStats.maxHealth * statModifier);
		physicalResistance = (int)(entityBaseStats.physicalDamageResistance * statModifier);
		poisonResistance = (int)(entityBaseStats.poisonDamageResistance * statModifier);
		fireResistance = (int)(entityBaseStats.fireDamageResistance * statModifier);
		iceResistance = (int)(entityBaseStats.iceDamageResistance * statModifier);

		if (currentHealth <= 0) return;
		try
		{
			entityEquipment.OnPlayerLevelUp(newPlayerLevel);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
}
