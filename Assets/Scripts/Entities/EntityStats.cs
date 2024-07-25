using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntityStats : MonoBehaviour
{
	[Header("Entity Info")]
	public SOEntityStats entityBaseStats;
	private EntityBehaviour entityBehaviour;
	private EntityClassHandler classHandler;
	[HideInInspector] public EntityEquipmentHandler equipmentHandler;
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;
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
	public Stat rangedWeaponDamageModifier;

	[Header("Status Effects")]
	public GameObject statusEffectsPrefab;
	public GameObject statusEffectsParentObj;
	public List<AbilityStatusEffect> currentStatusEffects;

	public event Action<float, bool> OnRecieveHealingEvent;
	public event Action<PlayerController, float, IDamagable.DamageType, bool> OnRecieveDamageEvent;

	public event Action<int, int> OnHealthChangeEvent;
	public event Action<int, int> OnManaChangeEvent;

	[Header("Idle Sound Settings")]
	private readonly float idleSoundCooldown = 5f;
	private float idleSoundTimer;
	private readonly int chanceOfIdleSound = 25;

	private void Awake()
	{
		entityBehaviour = GetComponent<EntityBehaviour>();
		classHandler = GetComponent<EntityClassHandler>();
		equipmentHandler = GetComponent<EntityEquipmentHandler>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
	}
	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		OnRecieveHealingEvent += RecieveHealing;
		GetComponent<Damageable>().OnHit += OnHit;
		OnRecieveDamageEvent += RecieveDamage;

		classHandler.OnClassChange += OnClassChanges;
		classHandler.OnStatUnlock += OnStatUnlock;

		equipmentHandler.OnEquipmentChanges += OnEquipmentChanges;
	}
	private void OnDisable()
	{
		OnRecieveHealingEvent -= RecieveHealing;
		GetComponent<Damageable>().OnHit -= OnHit;
		OnRecieveDamageEvent -= RecieveDamage;

		classHandler.OnClassChange -= OnClassChanges;
		classHandler.OnStatUnlock -= OnStatUnlock;

		equipmentHandler.OnEquipmentChanges -= OnEquipmentChanges;
	}

	private void Update()
	{
		PassiveManaRegen();
		PlayIdleSound();
	}
	private void Initilize()
	{
		spriteRenderer.sprite = entityBaseStats.sprite;

		int numOfTries = 0;
		if (GetComponent<PlayerController>() == null)
		{
			classHandler.SetEntityClass();
			equipmentHandler.StartCoroutine(equipmentHandler.SpawnEntityEquipment(numOfTries));
		}

		CalculateBaseStats();
		if (GameManager.Instance == null) return; //for now leave this line in
		if (SceneManager.GetActiveScene().name != "TestingScene" && GameManager.Instance.currentDungeonData.dungeonStatModifiers != null)
			ApplyDungeonModifiers(GameManager.Instance.currentDungeonData.dungeonStatModifiers); //apply dungeon mods outside of testing
	}
	public void ResetEntityStats()
	{
		spriteRenderer.color = Color.white;
		CalculateBaseStats();
	}
	public void ResetEntityBehaviour(SpawnHandler spawner)
	{
		entityBehaviour.UpdateBounds(spawner.transform.position);
		entityBehaviour.ResetBehaviour();
	}

	private void PlayIdleSound()
	{
		idleSoundTimer -= Time.deltaTime;

		if (idleSoundTimer <= 0)
		{
			idleSoundTimer = idleSoundCooldown;
			if (chanceOfIdleSound < Utilities.GetRandomNumber(100))
				audioHandler.PlayAudio(entityBaseStats.idleSfx);
		}
	}

	/// <summary>
	/// for MP when recieving damage as levels will no be synced, ill need to convert damage and healing recieved to a precentage 
	/// when it comes to resistances ignoring the difference shouldnt matter as everything scales at the same rate
	/// </summary>
	//health functions
	public void OnHeal(float healthValue, bool isPercentageValue)
	{
		OnRecieveHealingEvent?.Invoke(healthValue, isPercentageValue);
	}
	private void RecieveHealing(float healthValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			healthValue = maxHealth.finalValue * healthValue;

		currentHealth = (int)(currentHealth + healthValue);

		if (currentHealth > maxHealth.finalValue)
			currentHealth = maxHealth.finalValue;

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		if (!IsPlayerEntity()) return;
		EventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		UpdatePlayerStatInfoUi();
	}
	public void OnHit(PlayerController player, float damage, IDamagable.DamageType damageType, bool isPercentageValue, bool isDestroyedInOneHit)
	{
        if (isDestroyedInOneHit)
        {
			EventManager.DeathEvent(gameObject);
			return;
        }

		OnRecieveDamageEvent?.Invoke(player, damage, damageType, isPercentageValue);
	}
	private void RecieveDamage(PlayerController player, float damage, IDamagable.DamageType damageType, bool isPercentageValue)
	{
		//Debug.Log(gameObject.name + " recieved: " + damage);
		if (damageType == IDamagable.DamageType.isPoisonDamageType)
		{
			//Debug.Log("Poison Dmg res: " + poisonResistance.finalValue);
			if (isPercentageValue)
				damage = (maxHealth.finalValue * damage) - poisonResistance.finalValue;
			else
				damage -= poisonResistance.finalValue;
		}
		if (damageType == IDamagable.DamageType.isFireDamageType)
		{
			//Debug.Log("Fire Dmg res: " + fireResistance.finalValue);
			if (isPercentageValue)
				damage = (maxHealth.finalValue * damage) - fireResistance.finalValue;
			else
				damage -= fireResistance.finalValue;
		}
		if (damageType == IDamagable.DamageType.isIceDamageType)
		{
			//Debug.Log("Ice Dmg res: " + iceResistance.finalValue);
			if (isPercentageValue)
				damage = (maxHealth.finalValue * damage) - iceResistance.finalValue;
			else
				damage -= iceResistance.finalValue;
		}
		else
		{
			//Debug.Log("Physical Dmg res: " + physicalResistance.finalValue);
			if (isPercentageValue)
				damage = (maxHealth.finalValue * damage) - physicalResistance.finalValue;
			else
				damage -= physicalResistance.finalValue;
		}

		if (damage < 3) //always deal 3 damage
			damage = 3;

		//Debug.Log("FinalDmg: " + damage);
		currentHealth = (int)(currentHealth - damage);
		RedFlashOnRecieveDamage();
		audioHandler.PlayAudio(entityBaseStats.hurtSfx);
		if (!IsPlayerEntity())
			entityBehaviour.AddToAggroRating(player, (int)damage);

		///
		/// invoke onRecieveDamage like onEntityDeath that calls hit animations/sounds/ui health bar update
		/// also could invoke a onEntityDeath that instead calls functions in scripts to disable them then and play death sounds/animations
		/// this way if an entity does have a death sound but no death animation i dont need to run checks or hard code a reference
		/// and a box for instance can just have a death sound and instead of a death animation has a death partical effect explosion
		///

		if (currentHealth <= 0)
		{
			audioHandler.PlayAudio(entityBaseStats.deathSfx);
			EventManager.DeathEvent(gameObject);
		}

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);

		Debug.Log(gameObject.name);
		if (!IsPlayerEntity()) return;
		EventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		UpdatePlayerStatInfoUi();
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
		if (currentMana > maxMana.finalValue)
			DecreaseMana(currentMana - maxMana.finalValue, false);
		if (currentMana >= maxMana.finalValue) return;

		manaRegenTimer -= Time.deltaTime;

		if (manaRegenTimer <= 0)
		{
			manaRegenTimer = manaRegenCooldown;
			IncreaseMana(manaRegenPercentage.finalPercentageValue, true);
			if (!IsPlayerEntity()) return;
			EventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		}
	}
	public void IncreaseMana(float manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana.finalValue * manaValue;

		currentMana = (int)(currentMana + manaValue);
		if (currentMana > maxMana.finalValue)
			currentMana = maxMana.finalValue;

		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
		if (!IsPlayerEntity()) return;
		EventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		UpdatePlayerStatInfoUi();
	}
	public void DecreaseMana(float manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana.finalValue * manaValue;

		currentMana = (int)(currentMana - manaValue);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
		if (!IsPlayerEntity()) return;
		EventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		UpdatePlayerStatInfoUi();
	}

	//status effect functions
	public void ApplyStatusEffect(SOClassAbilities newStatusEffect)
	{
		GameObject go = Instantiate(statusEffectsPrefab, statusEffectsParentObj.transform);
		AbilityStatusEffect statusEffect = go.GetComponent<AbilityStatusEffect>();
		statusEffect.Initilize(newStatusEffect, this);

		if (newStatusEffect.statusEffectType == SOClassAbilities.StatusEffectType.isResistanceEffect)
		{
			physicalResistance.AddPercentageValue(newStatusEffect.damageValuePercentage);
			poisonResistance.AddPercentageValue(newStatusEffect.damageValuePercentage);
			fireResistance.AddPercentageValue(newStatusEffect.damageValuePercentage);
			iceResistance.AddPercentageValue(newStatusEffect.damageValuePercentage);
		}
		if (newStatusEffect.statusEffectType == SOClassAbilities.StatusEffectType.isDamageEffect)
		{
			physicalDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
			poisonDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
			fireDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
			iceDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
		}

		if (newStatusEffect.statusEffectType == SOClassAbilities.StatusEffectType.isMagicDamageEffect)
		{
			physicalDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
			poisonDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
			fireDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
			iceDamagePercentageModifier.AddPercentageValue(newStatusEffect.damageValuePercentage);
		}

		currentStatusEffects.Add(statusEffect);
	}
	public void UnApplyStatusEffect(AbilityStatusEffect statusEffect, SOClassAbilities abilityBaseRef)
	{
		if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isResistanceEffect)
		{
			physicalResistance.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			poisonResistance.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			fireResistance.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			iceResistance.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
		}
		if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isDamageEffect)
		{
			physicalDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			poisonDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			fireDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			iceDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
		}

		if (abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.isMagicDamageEffect)
		{
			physicalDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			poisonDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			fireDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
			iceDamagePercentageModifier.RemovePercentageValue(abilityBaseRef.damageValuePercentage);
		}

		currentStatusEffects.Remove(statusEffect);
	}

	/// <summary>
	/// recalculate stats when ever needed as hard bonuses and percentage bonuses need to be recalculated, if entity equipment for example
	/// changes a piece of armor etc, so stats are consistant in how they are applied, especially when percentage modifiers are included
	/// hard numbers added so far: equipment values, 
	/// percentage numbers added so far: level modifier
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
		rangedWeaponDamageModifier.SetBaseValue(1);

		currentHealth = maxHealth.finalValue - maxHealth.equipmentValue;
		currentMana = maxMana.finalValue - maxMana.equipmentValue;
		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);

		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
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
			poisonDamagePercentageModifier.RemovePercentageValue(statBoost.poisionDamageBoostValue);
			fireDamagePercentageModifier.RemovePercentageValue(statBoost.fireDamageBoostValue);
			iceDamagePercentageModifier.RemovePercentageValue(statBoost.iceDamageBoostValue);
			mainWeaponDamageModifier.RemovePercentageValue(statBoost.mainWeaponDamageBoostValue);
			dualWeaponDamageModifier.RemovePercentageValue(statBoost.duelWeaponDamageBoostValue);
			rangedWeaponDamageModifier.RemovePercentageValue(statBoost.rangedWeaponDamageBoostValue);
		}

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);
		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
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
		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
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
		poisonDamagePercentageModifier.AddPercentageValue(statBoost.poisionDamageBoostValue);
		fireDamagePercentageModifier.AddPercentageValue(statBoost.fireDamageBoostValue);
		iceDamagePercentageModifier.AddPercentageValue(statBoost.iceDamageBoostValue);
		mainWeaponDamageModifier.AddPercentageValue(statBoost.mainWeaponDamageBoostValue);
		dualWeaponDamageModifier.AddPercentageValue(statBoost.duelWeaponDamageBoostValue);
		rangedWeaponDamageModifier.AddPercentageValue(statBoost.rangedWeaponDamageBoostValue);

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);
		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}
	public void ApplyDungeonModifiers(DungeonStatModifier dungeonModifiers)
	{
		bool oldCurrentHealthEqualToOldMaxHealth = false;
		if (currentHealth == maxHealth.finalValue)
			oldCurrentHealthEqualToOldMaxHealth = true;

		if (!IsPlayerEntity())
		{
			maxHealth.AddPercentageValue(dungeonModifiers.difficultyModifier);
			maxMana.AddPercentageValue(dungeonModifiers.difficultyModifier);
			physicalResistance.AddPercentageValue(dungeonModifiers.difficultyModifier);
			poisonResistance.AddPercentageValue(dungeonModifiers.difficultyModifier);
			fireResistance.AddPercentageValue(dungeonModifiers.difficultyModifier);
			iceResistance.AddPercentageValue(dungeonModifiers.difficultyModifier);

			physicalDamagePercentageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
			poisonDamagePercentageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
			fireDamagePercentageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
			iceDamagePercentageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
			mainWeaponDamageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
			dualWeaponDamageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
			rangedWeaponDamageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
		}

		maxHealth.AddPercentageValue(dungeonModifiers.healthModifier);
		maxMana.AddPercentageValue(dungeonModifiers.manaModifier);
		physicalResistance.AddPercentageValue(dungeonModifiers.physicalResistanceModifier);
		poisonResistance.AddPercentageValue(dungeonModifiers.poisonResistanceModifier);
		fireResistance.AddPercentageValue(dungeonModifiers.fireResistanceModifier);
		iceResistance.AddPercentageValue(dungeonModifiers.iceResistanceModifier);

		physicalDamagePercentageModifier.AddPercentageValue(dungeonModifiers.physicalDamageModifier);
		poisonDamagePercentageModifier.AddPercentageValue(dungeonModifiers.poisonDamageModifier);
		fireDamagePercentageModifier.AddPercentageValue(dungeonModifiers.fireDamageModifier);
		iceDamagePercentageModifier.AddPercentageValue(dungeonModifiers.iceDamageModifier);
		mainWeaponDamageModifier.AddPercentageValue(dungeonModifiers.mainWeaponDamageModifier);
		dualWeaponDamageModifier.AddPercentageValue(dungeonModifiers.dualWeaponDamageModifier);
		rangedWeaponDamageModifier.AddPercentageValue(dungeonModifiers.rangedWeaponDamageModifier);

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);
		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
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
	public void UpdatePlayerStatInfoUi()
	{
		if (!IsPlayerEntity()) return;
		EventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		EventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		EventManager.PlayerStatChange(this);
	}

	//Checks
	public bool IsPlayerEntity()
	{
		if (entityBaseStats.humanoidType == SOEntityStats.HumanoidTypes.isPlayer)
			return true;
		else return false;
	}
}
