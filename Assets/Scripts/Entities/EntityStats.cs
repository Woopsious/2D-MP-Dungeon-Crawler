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
	[HideInInspector] public EntityBehaviour entityBehaviour;
	[HideInInspector] public EntityClassHandler classHandler;
	[HideInInspector] public EntityEquipmentHandler equipmentHandler;
	[HideInInspector] public LootSpawnHandler lootSpawnHandler;
	private BoxCollider2D boxCollider2D;
	private Animator animator;
	public SpriteRenderer spriteRenderer {get; private set; }
	public SpriteRenderer idleWeaponSprite { get; private set; }
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

	[Header("Modifiers")]
	public Stat damageDealtModifier;

	public Stat healingPercentageModifier;
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

	//events
	public event Action<AbilityStatusEffect> OnNewStatusEffect;
	public event Action<SOStatusEffects> OnResetStatusEffectTimer;
	public event Action<SOStatusEffects> OnRemoveStatusEffect;


	public event Action<float, bool, float> OnRecieveHealingEvent;
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
		lootSpawnHandler = GetComponent<LootSpawnHandler>();
		boxCollider2D = GetComponent<BoxCollider2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
		idleWeaponSprite = transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
	}
	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		GetComponent<Damageable>().OnHit += OnHit;
		OnRecieveDamageEvent += RecieveDamage;
		OnRecieveHealingEvent += RecieveHealing;

		classHandler.OnStatUnlock += OnStatUnlock;
		classHandler.OnStatRefund += OnStatRefund;

		equipmentHandler.OnEquipmentChanges += OnEquipmentChanges;
	}
	private void OnDisable()
	{
		GetComponent<Damageable>().OnHit -= OnHit;
		OnRecieveDamageEvent -= RecieveDamage;
		OnRecieveHealingEvent -= RecieveHealing;

		classHandler.OnStatUnlock -= OnStatUnlock;
		classHandler.OnStatRefund -= OnStatRefund;

		equipmentHandler.OnEquipmentChanges -= OnEquipmentChanges;
	}

	private void Update()
	{
		PassiveManaRegen();
		PlayIdleSound();
	}

	//set entity data
	private void Initilize()
	{
		spriteRenderer.sprite = entityBaseStats.sprite;
		CalculateBaseStats();

		if (GetComponent<PlayerController>() == null)
		{
			classHandler.SetEntityClass();
			equipmentHandler.SpawnEntityEquipment();
			lootSpawnHandler.Initilize(entityBaseStats.maxDroppedGoldAmount, entityBaseStats.minDroppedGoldAmount, entityBaseStats.lootPool);
		}

		if (GameManager.Instance == null) return; //for now leave this line in
		if (SceneManager.GetActiveScene().name != "TestingScene" && GameManager.Instance.currentDungeonData.dungeonStatModifiers != null)
			ApplyDungeonModifiers(GameManager.Instance.currentDungeonData.dungeonStatModifiers); //apply dungeon mods outside of testing
	}
	public void ResetEntityStats()
	{
		StopAllCoroutines();
		boxCollider2D.enabled = true;
		animator.ResetTrigger("DeathTrigger");
		spriteRenderer.color = Color.white;
		CalculateBaseStats();
		classHandler.RerollEquippedAbilities();
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
	public void OnHeal(float healthValue, bool isPercentageValue, float healingModifierPercentage)
	{
		OnRecieveHealingEvent?.Invoke(healthValue, isPercentageValue, healingModifierPercentage);
	}
	private void RecieveHealing(float healthValue, bool isPercentageValue, float healingModifierPercentage)
	{
		if (isPercentageValue)
			healthValue = maxHealth.finalValue * healthValue;

		healthValue *= healingModifierPercentage * damageDealtModifier.finalPercentageValue;
		currentHealth = (int)(currentHealth + Mathf.Round(healthValue));

		if (currentHealth > maxHealth.finalValue)
			currentHealth = maxHealth.finalValue;

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		if (!IsPlayerEntity()) return;
		PlayerEventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		UpdatePlayerStatInfoUi();
	}
	public void OnHit(PlayerController player, float damage, IDamagable.DamageType damageType, bool isPercentageValue, bool isDestroyedInOneHit)
	{
        if (isDestroyedInOneHit)
        {
			DungeonHandler.EntityDeathEvent(gameObject);
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
		else if (damageType == IDamagable.DamageType.isFireDamageType)
		{
			//Debug.Log("Fire Dmg res: " + fireResistance.finalValue);
			if (isPercentageValue)
				damage = (maxHealth.finalValue * damage) - fireResistance.finalValue;
			else
				damage -= fireResistance.finalValue;
		}
		else if (damageType == IDamagable.DamageType.isIceDamageType)
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
		if (!IsPlayerEntity() && player != null)
			entityBehaviour.AddToAggroRating(player, (int)damage);

		if (currentHealth <= 0)
			OnDeath();

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);

		if (!IsPlayerEntity()) return;
		PlayerEventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		UpdatePlayerStatInfoUi();
	}
	private void OnDeath()
	{
		audioHandler.PlayAudio(entityBaseStats.deathSfx);
		entityBehaviour.navMeshAgent.isStopped = true;
		animator.SetTrigger("DeathTrigger");
		boxCollider2D.enabled = false;
		StartCoroutine(WaitForDeathSound());
	}
	private void RedFlashOnRecieveDamage()
	{
		spriteRenderer.color = Color.red;
		StartCoroutine(ResetRedFlashOnRecieveDamage());
	}
	IEnumerator ResetRedFlashOnRecieveDamage()
	{
		yield return new WaitForSeconds(0.1f);
		if (IsEntityDead()) yield break;
		spriteRenderer.color = Color.white;
	}
	IEnumerator WaitForDeathSound()
	{
		if (audioHandler.audioSource.clip != null)
			yield return new WaitForSeconds(audioHandler.audioSource.clip.length);

		DungeonHandler.EntityDeathEvent(gameObject);
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
			PlayerEventManager.PlayerManaChange(maxMana.finalValue, currentMana);
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
		PlayerEventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		UpdatePlayerStatInfoUi();
	}
	public void DecreaseMana(float manaValue, bool isPercentageValue)
	{
		if (isPercentageValue)
			manaValue = maxMana.finalValue * manaValue;

		currentMana = (int)(currentMana - manaValue);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
		if (!IsPlayerEntity()) return;
		PlayerEventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		UpdatePlayerStatInfoUi();
	}

	//status effect functions
	public void ApplyNewStatusEffects(List<SOStatusEffects> effectsToApply, EntityStats casterInfo)
	{
		foreach (SOStatusEffects effect in effectsToApply)
		{
			AbilityStatusEffect duplicateStatusEffect = CheckIfStatusEffectAlreadyApplied(effect);
			if (duplicateStatusEffect != null)
			{
				duplicateStatusEffect.ResetTimer();
				OnResetStatusEffectTimer?.Invoke(effect);
				continue;
			}

			GameObject go = Instantiate(statusEffectsPrefab, statusEffectsParentObj.transform);
			AbilityStatusEffect statusEffect = go.GetComponent<AbilityStatusEffect>();
			statusEffect.Initilize(effect, casterInfo, this);

			if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageRecievedEffect)
				damageDealtModifier.AddPercentageValue(effect.effectValue);
			if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isResistanceEffect)
			{
				physicalResistance.AddPercentageValue(effect.effectValue);
				poisonResistance.AddPercentageValue(effect.effectValue);
				fireResistance.AddPercentageValue(effect.effectValue);
				iceResistance.AddPercentageValue(effect.effectValue);
			}
			if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageEffect)
			{
				physicalDamagePercentageModifier.AddPercentageValue(effect.effectValue);
				poisonDamagePercentageModifier.AddPercentageValue(effect.effectValue);
				fireDamagePercentageModifier.AddPercentageValue(effect.effectValue);
				iceDamagePercentageModifier.AddPercentageValue(effect.effectValue);
			}
			if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isMovementEffect)
			{
				if (IsPlayerEntity())
					GetComponent<PlayerController>().UpdateMovementSpeed(effect.effectValue, false);
				else
					entityBehaviour.UpdateMovementSpeed(effect.effectValue, false);
			}

			OnNewStatusEffect?.Invoke(statusEffect);
			currentStatusEffects.Add(statusEffect);
		}
	}
	public void UnApplyStatusEffect(AbilityStatusEffect statusEffect)
	{
		SOStatusEffects effect = statusEffect.GrabAbilityBaseRef();

		if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageRecievedEffect)
			damageDealtModifier.RemovePercentageValue(effect.effectValue);

		if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isResistanceEffect)
		{
			physicalResistance.RemovePercentageValue(effect.effectValue);
			poisonResistance.RemovePercentageValue(effect.effectValue);
			fireResistance.RemovePercentageValue(effect.effectValue);
			iceResistance.RemovePercentageValue(effect.effectValue);
		}
		if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageEffect)
		{
			physicalDamagePercentageModifier.RemovePercentageValue(effect.effectValue);
			poisonDamagePercentageModifier.RemovePercentageValue(effect.effectValue);
			fireDamagePercentageModifier.RemovePercentageValue(effect.effectValue);
			iceDamagePercentageModifier.RemovePercentageValue(effect.effectValue);
		}
		if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isMovementEffect)
		{
			if (IsPlayerEntity())
				GetComponent<PlayerController>().UpdateMovementSpeed(effect.effectValue, true);
			else
				entityBehaviour.UpdateMovementSpeed(effect.effectValue, true);
		}

		currentStatusEffects.Remove(statusEffect);
		OnRemoveStatusEffect?.Invoke(effect);
		TileMapHazardsManager.Instance.TryReApplyEffect(this); //re apply effects if standing in lava pool etc
	}
	private AbilityStatusEffect CheckIfStatusEffectAlreadyApplied(SOStatusEffects newStatusEffect)
	{
		foreach (AbilityStatusEffect statusEffect in currentStatusEffects)
		{
			if (statusEffect.GrabAbilityBaseRef() == newStatusEffect)
				return statusEffect;
		}
		return null;
	}

	//set base stats
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

		damageDealtModifier.SetBaseValue(entityBaseStats.damageDealtBaseModifier);

		healingPercentageModifier.SetBaseValue(1);
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(idleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}

	//update stat values/modifiers based on events
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(idleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
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

		healingPercentageModifier.AddPercentageValue(statBoost.healingBoostValue);
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(idleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}
	public void OnStatRefund(SOClassStatBonuses statBoost)
	{
		bool oldCurrentHealthEqualToOldMaxHealth = false;
		if (currentHealth == maxHealth.finalValue)
			oldCurrentHealthEqualToOldMaxHealth = true;

		maxHealth.RemovePercentageValue(statBoost.healthBoostValue);
		maxMana.RemovePercentageValue(statBoost.manaBoostValue);
		physicalResistance.RemovePercentageValue(statBoost.physicalResistanceBoostValue);
		poisonResistance.RemovePercentageValue(statBoost.poisonResistanceBoostValue);
		fireResistance.RemovePercentageValue(statBoost.fireResistanceBoostValue);
		iceResistance.RemovePercentageValue(statBoost.iceResistanceBoostValue);

		healingPercentageModifier.RemovePercentageValue(statBoost.healingBoostValue);
		physicalDamagePercentageModifier.RemovePercentageValue(statBoost.physicalDamageBoostValue);
		poisonDamagePercentageModifier.RemovePercentageValue(statBoost.poisionDamageBoostValue);
		fireDamagePercentageModifier.RemovePercentageValue(statBoost.fireDamageBoostValue);
		iceDamagePercentageModifier.RemovePercentageValue(statBoost.iceDamageBoostValue);
		mainWeaponDamageModifier.RemovePercentageValue(statBoost.mainWeaponDamageBoostValue);
		dualWeaponDamageModifier.RemovePercentageValue(statBoost.duelWeaponDamageBoostValue);
		rangedWeaponDamageModifier.RemovePercentageValue(statBoost.rangedWeaponDamageBoostValue);

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);
		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(idleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
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

			healingPercentageModifier.AddPercentageValue(dungeonModifiers.difficultyModifier);
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

		//healingPercentageModifier.AddPercentageValue();
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(idleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}

	//full heal entity if not player and at full health on stat changes
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
		PlayerEventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		PlayerEventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		PlayerEventManager.PlayerStatChange(this);
	}

	//bool checks
	public bool IsEntityDead()
	{
		if (currentHealth <= 0)
			return true;
		else return false;
	}
	public bool IsPlayerEntity()
	{
		if (entityBaseStats.humanoidType == SOEntityStats.HumanoidTypes.isPlayer)
			return true;
		else return false;
	}
}
