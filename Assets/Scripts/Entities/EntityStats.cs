using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
	[Header("Entity Info")]
	public SOEntityStats entityBaseStats;
	public int entityLevel;
	public float statModifier;

	[Header("Health")]
	public int maxHealth;
	public int currentHealth;
	[Header("Total")]
	public int totalMaxHealth;
	public int totalCurrentHealth;

	[Header("Mana")]
	public int maxMana;
	public int currentMana;
	[Header("Total")]
	public int totalMaxMana;
	public int totalCurrentMana;

	[Header("Resistances")]
	public int physicalResistance;
	public int poisonResistance;
	public int fireResistance;
	public int iceResistance;
	[Header("Total")]
	public int totalPhyicalResistance;
	public int totalPoisonResistance;
	public int totalFireResistance;
	public int totalIceResistance;

	public event Action<int, int> onRecieveDamageEvent;
	public event Action<GameObject> onDeathEvent;

	private void Start()
	{
		GetStatModifier(entityLevel, IGetStatModifier.Rarity.isCommon);
	}
	private void OnEnable()
	{
		GetComponent<Damageable>().OnHit += RecieveDamage;
	}
	private void OnDisable()
	{
		GetComponent<Damageable>().OnHit -= RecieveDamage;
	}
	public void GetStatModifier(int level, IGetStatModifier.Rarity rarity)
	{
		float modifier = (level - 1f) / 20;  //get level modifier
		SetStats(modifier += 1);
	}

	public void SetStats(float modifier)
	{
		//maxHealth = (int)(entityBaseStats.maxHealth * modifier);
		//currentHealth = (int)(entityBaseStats.maxHealth * modifier);
		//physicalResistance = (int)(entityBaseStats.physicalDamageResistance * modifier);
		//poisonResistance = (int)(entityBaseStats.poisonDamageResistance * modifier);
		//fireResistance = (int)(entityBaseStats.fireDamageResistance * modifier);
		//iceResistance = (int)(entityBaseStats.iceDamageResistance * modifier);
	}

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
		onRecieveDamageEvent?.Invoke(maxHealth, currentHealth);

		///
		/// invoke onRecieveDamage like onEntityDeath that calls hit animations/sounds/knockback/ui health bar update
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
}
