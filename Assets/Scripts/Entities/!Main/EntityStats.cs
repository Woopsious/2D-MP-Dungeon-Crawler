using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntityStats : NetworkBehaviour
{
	[Header("Entity Info")]
	public SOEntityStats statsRef;
	[HideInInspector] public PlayerController playerRef;
	[HideInInspector] public EntityBehaviour entityBehaviour;
	[HideInInspector] public EntityAbilityHandler abilityHandler;
	[HideInInspector] public EntityClassHandler classHandler;
	[HideInInspector] public EntityEquipmentHandler equipmentHandler;
	private LootSpawnHandler lootSpawnHandler;
	private BoxCollider2D boxCollider2D;
	private Animator animator;
	public SpriteRenderer SpriteRenderer {get; private set; }
	public SpriteRenderer IdleWeaponSprite { get; private set; }
	private AudioHandler audioHandler;
	public int entityLevel;
	public float levelModifier;

	[Header("Health")]
	private bool entityDead;
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
	public event Action<AbilityStatusEffect> OnStatusEffectAppliedEvent;
	public event Action<int, int> OnHealthChangeEvent;
	public event Action<int, int> OnManaChangeEvent;

	[Header("Idle Sound Settings")]
	private readonly float idleSoundCooldown = 5f;
	private float idleSoundTimer;
	private readonly int chanceOfIdleSound = 25;

	protected virtual void Awake()
	{
		playerRef = GetComponent<PlayerController>();
		entityBehaviour = GetComponent<EntityBehaviour>();
		abilityHandler = GetComponent<EntityAbilityHandler>();
		classHandler = GetComponent<EntityClassHandler>();
		equipmentHandler = GetComponent<EntityEquipmentHandler>();
		lootSpawnHandler = GetComponent<LootSpawnHandler>();
		boxCollider2D = GetComponent<BoxCollider2D>();
		animator = GetComponent<Animator>();
		SpriteRenderer = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
		IdleWeaponSprite = transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
	}
	protected virtual void Start()
	{
		Initilize();
	}

	protected virtual void OnEnable()
	{
		SceneManager.sceneLoaded += UpdateDungeonModifiersAppliedToPlayer;

		GetComponent<Damageable>().OnHit += RecieveDamage;

		classHandler.OnStatUnlock += OnStatUnlock;
		classHandler.OnStatRefund += OnStatRefund;

		equipmentHandler.OnEquipmentChanges += OnEquipmentChanges;
	}
	protected virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= UpdateDungeonModifiersAppliedToPlayer;

		GetComponent<Damageable>().OnHit -= RecieveDamage;

		classHandler.OnStatUnlock -= OnStatUnlock;
		classHandler.OnStatRefund -= OnStatRefund;

		equipmentHandler.OnEquipmentChanges -= OnEquipmentChanges;
	}

	protected virtual void Update()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		PassiveManaRegen();
		PlayIdleSound();
	}

	[Rpc(SendTo.Everyone)]
	public void SyncEntitySORefsRPC(int index, bool isBoss)
	{
		SetEntitySoRefs(index, isBoss);
	}
	public void SetEntitySoRefs(int index, bool isBoss)
	{
		SOEntityStats statsRef;
		if (isBoss)
			statsRef = AssetDatabase.Database.bossEntities[index];
		else
			statsRef = AssetDatabase.Database.entities[index];

		this.statsRef = statsRef;
		entityBehaviour.behaviourRef = statsRef.entityBehaviour;
		entityLevel = GameManager.Localplayer.playerStats.entityLevel;
	}

	//set entity data
	public void Initilize()
	{
		SpriteRenderer.sprite = statsRef.sprite;
		name = statsRef.entityName;
		entityDead = false;
		CalculateBaseStats();

		if (playerRef == null)
		{
			classHandler.AssignEntityRandomClass();
			equipmentHandler.AssignEntityRandomEquipment();
			abilityHandler.AssignEntityRandomAbilities();
			lootSpawnHandler.Initilize(statsRef.maxDroppedGoldAmount, statsRef.minDroppedGoldAmount,
				statsRef.lootPool, statsRef.itemRarityChanceModifier);
		}

		if (GameManager.Instance.currentDungeonData.dungeonStatModifiers != null) //apply dungeon modifiers
			ApplyDungeonModifiersToEntity(GameManager.Instance.currentDungeonData.dungeonStatModifiers);
	}
	public void ResetEntityStats()
	{
		StopAllCoroutines();
		boxCollider2D.enabled = true;
		SpriteRenderer.color = Color.white;
		entityDead = false;
		CalculateBaseStats();

		if (currentStatusEffects.Count != 0)//clear all status effects after death
		{
			for (int i = currentStatusEffects.Count - 1; i >= 0; i--)
				currentStatusEffects[i].ClearStatusEffect();
		}

		if (IsPlayerEntity())
		{
			animator.SetTrigger("DeathTrigger"); //no idea why this works for player and not entities??
		}
		else
		{
			animator.ResetTrigger("DeathTrigger");//+ this vis versa??
			abilityHandler.RerollEquippedAbilities();
		}
	}

	private void PlayIdleSound()
	{
		idleSoundTimer -= Time.deltaTime;

		if (idleSoundTimer <= 0)
		{
			idleSoundTimer = idleSoundCooldown;
			if (chanceOfIdleSound < Utilities.GetRandomNumber(100))
				audioHandler.PlayAudio(statsRef.idleSfx);
		}
	}

	/// <summary>
	/// for MP when recieving damage as levels will not be synced, ill need to convert damage and healing recieved to a precentage 
	/// when it comes to resistances ignoring the difference shouldnt matter as everything scales at the same rate
	/// </summary>

	//HEALTH EVENTS
	//healing recieve event
	public void RecieveHealing(float value, bool isPercentageValue, float healingModifierPercentage)
	{
		float healingPercentage;
		if (isPercentageValue)
			healingPercentage = value;
		else
			healingPercentage = value / maxHealth.finalValue;

		healingPercentage = (float)currentHealth / maxHealth.finalValue + healingPercentage * healingModifierPercentage;

		if (MultiplayerManager.IsMultiplayer())
			ApplyHealingRpc(healingPercentage);
		else
			ApplyHealing(healingPercentage);
	}
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void ApplyHealingRpc(float newHealthPercentage)
	{
		ApplyHealing(newHealthPercentage);
	}
	private void ApplyHealing(float newHealthPercentage)
	{
		UpdateCurrentHealth(newHealthPercentage);
	}

	//damage recieve event
	public void RecieveDamage(DamageSourceInfo damageSourceInfo, bool isDestroyedInOneHit)
	{
		damageSourceInfo = NegateEntityResistances(damageSourceInfo);

		if (!IsPlayerEntity() && damageSourceInfo.hitBye == IDamagable.HitBye.player)
			entityBehaviour.AddToAggroRating(damageSourceInfo.entity.playerRef, (int)damageSourceInfo.damage);

		float newHealthPercentage = (float)currentHealth / maxHealth.finalValue - damageSourceInfo.damage / maxHealth.finalValue;

		//only debug heal when health gets below 0% + damage isnt coming from debug kill player
		newHealthPercentage = DebugForceHealPlayerOnLowHealth(damageSourceInfo, newHealthPercentage);

		if (MultiplayerManager.IsMultiplayer())
			ApplyDamageRpc(newHealthPercentage, damageSourceInfo.deathMessage);
		else
			ApplyDamage(newHealthPercentage, damageSourceInfo.deathMessage);
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void ApplyDamageRpc(float newHealthPercentage, string deathMessage)
	{
		ApplyDamage(newHealthPercentage, deathMessage);
	}
	private void ApplyDamage(float newHealthPercentage, string deathMessage)
	{
		UpdateCurrentHealth(newHealthPercentage);

		StartCoroutine(FlashRedOnRecieveDamage());
		audioHandler.PlayAudio(statsRef.hurtSfx);

		if (IsEntityDead())
			EntityDeath(deathMessage);
	}

	//update health
	private void UpdateCurrentHealth(float newHealthPercentage)
	{
		if (newHealthPercentage > 1)
			currentHealth = maxHealth.finalValue;
		else
			currentHealth = (int)(maxHealth.finalValue * newHealthPercentage);

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);

		if (!EntityIsLocalPlayer()) return;
		PlayerEventManager.PlayerHealthChange(maxHealth.finalValue, currentHealth);
		UpdatePlayerStatInfoUi();
	}

	//helpers
	private DamageSourceInfo NegateEntityResistances(DamageSourceInfo damageSourceInfo)
	{
		//Debug.Log(gameObject.name + " recieved: " + damageSourceInfo.damage);
		if (damageSourceInfo.damageType == IDamagable.DamageType.isPoisonDamage)
		{
			//Debug.Log("Poison Dmg res: " + poisonResistance.finalValue);
			if (damageSourceInfo.isPercentage)
				damageSourceInfo.damage = (maxHealth.finalValue * damageSourceInfo.damage) - poisonResistance.finalValue;
			else
				damageSourceInfo.damage -= poisonResistance.finalValue;
		}
		else if (damageSourceInfo.damageType == IDamagable.DamageType.isFireDamage)
		{
			//Debug.Log("Fire Dmg res: " + fireResistance.finalValue);
			if (damageSourceInfo.isPercentage)
				damageSourceInfo.damage = (maxHealth.finalValue * damageSourceInfo.damage) - fireResistance.finalValue;
			else
				damageSourceInfo.damage -= fireResistance.finalValue;
		}
		else if (damageSourceInfo.damageType == IDamagable.DamageType.isIceDamage)
		{
			//Debug.Log("Ice Dmg res: " + iceResistance.finalValue);
			if (damageSourceInfo.isPercentage)
				damageSourceInfo.damage = (maxHealth.finalValue * damageSourceInfo.damage) - iceResistance.finalValue;
			else
				damageSourceInfo.damage -= iceResistance.finalValue;
		}
		else
		{
			//Debug.Log("Physical Dmg res: " + physicalResistance.finalValue);
			if (damageSourceInfo.isPercentage)
				damageSourceInfo.damage = (maxHealth.finalValue * damageSourceInfo.damage) - physicalResistance.finalValue;
			else
				damageSourceInfo.damage -= physicalResistance.finalValue;
		}

		if (damageSourceInfo.damage < 3) //always deal 3 damage
			damageSourceInfo.damage = 3;

		//Debug.Log("FinalDmg: " + damageSourceInfo.damage);
		return damageSourceInfo;
	}
	private IEnumerator FlashRedOnRecieveDamage()
	{
		SpriteRenderer.color = Color.red;

		yield return new WaitForSeconds(0.1f);
		if (IsEntityDead()) yield break;
		SpriteRenderer.color = Color.white;
	}
	private float DebugForceHealPlayerOnLowHealth(DamageSourceInfo damageSourceInfo, float newHealthPercentage)
	{
		if (IsPlayerEntity() && playerRef.debugNoDeath && newHealthPercentage < 0 &&
			damageSourceInfo.deathMessageType != DamageSourceInfo.DeathMessageType.debug) //force health regen when health below 10
		{
			currentHealth = maxHealth.finalValue;
			newHealthPercentage = 1;
			return newHealthPercentage;
		}
		else return newHealthPercentage;
	}

	//death event
	private void EntityDeath(string optionalDeathMessage)
	{
		if (entityDead) return;

		entityDead = true;
		audioHandler.PlayAudio(statsRef.deathSfx);
		StartCoroutine(EntityDeathFinish(optionalDeathMessage));
		animator.SetTrigger("DeathTrigger");

		if (IsPlayerEntity()) return;
		boxCollider2D.enabled = false;
		entityBehaviour.navMeshAgent.isStopped = true;
	}
	private IEnumerator EntityDeathFinish(string deathMessage)
	{
		if (audioHandler.audioSource.clip != null)
			yield return new WaitForSeconds(audioHandler.audioSource.clip.length);

		if (IsPlayerEntity())
			PlayerEventManager.PlayerDeath(playerRef, deathMessage); //player death
		else
			ObjectPoolingManager.EntityDeathEvent(gameObject); //entity death
	}

	//MANA EVENTS
	private void PassiveManaRegen()
	{
		if (currentMana > maxMana.finalValue)
			DecreaseMana(currentMana - maxMana.finalValue, false);
		if (currentMana >= maxMana.finalValue) return;

		manaRegenTimer -= Time.deltaTime;

		if (manaRegenTimer <= 0)
		{
			manaRegenTimer = manaRegenCooldown;
			IncreaseMana(manaRegenPercentage.finalPercentageValue, true);
		}
	}
	public void IncreaseMana(float value, bool isPercentageValue)
	{
		float newManaPercentage;
		if (isPercentageValue)
			newManaPercentage = value;
		else
			newManaPercentage = value / maxMana.finalValue;

		newManaPercentage = (float)currentMana / maxMana.finalValue + newManaPercentage;

		if (MultiplayerManager.IsMultiplayer())
			UpdateCurrentManaRpc(newManaPercentage);
		else
			UpdateCurrentMana(newManaPercentage);
	}
	public void DecreaseMana(float value, bool isPercentageValue)
	{
		float newManaPercentage;
		if (isPercentageValue)
			newManaPercentage = value;
		else
			newManaPercentage = value / maxMana.finalValue;

		newManaPercentage = (float)currentMana / maxMana.finalValue - newManaPercentage;

		if (MultiplayerManager.IsMultiplayer())
			UpdateCurrentManaRpc(newManaPercentage);
		else
			UpdateCurrentMana(newManaPercentage);
	}

	//update mana
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void UpdateCurrentManaRpc(float newManaPercentage)
	{
		UpdateCurrentMana(newManaPercentage);
	}
	private void UpdateCurrentMana(float newManaPercentage)
	{
		if (newManaPercentage > 1)
			currentMana = maxMana.finalValue;
		else
			currentMana = (int)(maxMana.finalValue * newManaPercentage);

		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);

		if (!EntityIsLocalPlayer()) return;
		PlayerEventManager.PlayerManaChange(maxMana.finalValue, currentMana);
		UpdatePlayerStatInfoUi();
	}

	//status effect functions
	public void ApplyNewStatusEffects(List<SOStatusEffects> effectsToApply, EntityStats casterStats)
	{
		if (!MultiplayerManager.IsClientHost()) return;

		if (MultiplayerManager.IsMultiplayer())
			SyncSetUpStatusEffectRpc(GetStatusEffectsIndexes(effectsToApply), casterStats.NetworkObjectId);
		else
			SetUpStatusEffect(effectsToApply, casterStats);
	}
	[Rpc(SendTo.Server, RequireOwnership = false)]
	private void SyncSetUpStatusEffectRpc(int[] statusEffectsIndexes, ulong casterId)
	{
		EntityStats casterStats = NetworkManager.SpawnManager.SpawnedObjects[casterId].GetComponent<EntityStats>();
		List<SOStatusEffects> effectsToApply = new();
		
		foreach (int effectIndex in statusEffectsIndexes)
			effectsToApply.Add(AssetDatabase.Database.statusEffects[effectIndex]);

		SetUpStatusEffect(effectsToApply, casterStats);
	}
	private void SetUpStatusEffect(List<SOStatusEffects> effectsToApply, EntityStats casterInfo)
	{
		foreach (SOStatusEffects effect in effectsToApply)
		{
			AbilityStatusEffect duplicateStatusEffect = IsStatusEffectAlreadyApplied(effect);
			if (duplicateStatusEffect != null)
			{
				duplicateStatusEffect.ResetAbilityTimer();

				if (MultiplayerManager.IsMultiplayer())
					ResetStatusEffectTimerForUiRpc(duplicateStatusEffect.NetworkObjectId);
				else
					ResetStatusEffectTimerForUi(duplicateStatusEffect);
				continue;
			}
			else
			{
				GameObject go = Instantiate(statusEffectsPrefab);
				if (MultiplayerManager.IsMultiplayer())
					go.GetComponent<NetworkObject>().Spawn();

				AbilityStatusEffect statusEffect = go.GetComponent<AbilityStatusEffect>();
				statusEffect.Initilize(casterInfo, this, effect);
			}
		}
	}

	public void AddStatusEffectValues(AbilityStatusEffect statusEffect)
	{
		SOStatusEffects effect = statusEffect.GetBaseStatusEffect();

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
				playerRef.UpdateMovementSpeed(effect.effectValue, false);
			else
				entityBehaviour.UpdateMovementSpeed(effect.effectValue, false);
		}

		OnStatusEffectAppliedEvent?.Invoke(statusEffect);
		currentStatusEffects.Add(statusEffect);

		if (!IsPlayerEntity()) return;

		if (statusEffect.GetBaseStatusEffect().isMarkedByBossEffect)
			playerRef.MarkPlayer();

		if (EntityIsLocalPlayer())
			PlayerEventManager.PlayerStatusEffectChange(statusEffect);
	}
	public void RemoveStatusEffectValues(AbilityStatusEffect statusEffect)
	{
		SOStatusEffects effect = statusEffect.GetBaseStatusEffect();

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
				playerRef.UpdateMovementSpeed(effect.effectValue, true);
			else
				entityBehaviour.UpdateMovementSpeed(effect.effectValue, true);
		}

		currentStatusEffects.Remove(statusEffect);
		TileMapHazardsManager.Instance.TryReApplyEffect(this); //re apply effects if standing in lava pool etc

		if (IsPlayerEntity() && effect.isMarkedByBossEffect)
			playerRef.UnMarkPlayer();
	}

	//timer ui resets for reapplied status effects
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void ResetStatusEffectTimerForUiRpc(ulong statusEffectid)
	{
		ResetStatusEffectTimerForUi(NetworkManager.SpawnManager.SpawnedObjects[statusEffectid].GetComponent<AbilityStatusEffect>());
	}
	private void ResetStatusEffectTimerForUi(AbilityStatusEffect statusEffect)
	{
		OnStatusEffectAppliedEvent?.Invoke(statusEffect);

		if (!IsPlayerEntity()) return;

		if (statusEffect.GetBaseStatusEffect().isMarkedByBossEffect)
			playerRef.MarkPlayer();

		if (EntityIsLocalPlayer())
			PlayerEventManager.PlayerStatusEffectChange(statusEffect);
	}

	//status effects helpers
	private AbilityStatusEffect IsStatusEffectAlreadyApplied(SOStatusEffects newStatusEffect)
	{
		foreach (AbilityStatusEffect statusEffect in currentStatusEffects)
		{
			if (statusEffect.GetBaseStatusEffect() == newStatusEffect)
				return statusEffect;
		}
		return null;
	}
	private int[] GetStatusEffectsIndexes(List<SOStatusEffects> effectsToApply)
	{
		int[] statusEffectIndexes = new int[effectsToApply.Count];
		for (int i = 0; i < effectsToApply.Count; i++)
		{
			for (int effectIndex = 0; effectIndex < AssetDatabase.Database.statusEffects.Count; effectIndex++)
			{
				if (effectsToApply[i] == AssetDatabase.Database.statusEffects[effectIndex])
					statusEffectIndexes[i] = effectIndex;
			}
		}
		return statusEffectIndexes;
	}

	//set base stats
	public void CalculateBaseStats()
	{
		if (entityLevel == 1)  //get level modifier
			levelModifier = 1;
		else
			levelModifier = 1 + (entityLevel / 10f);

		maxHealth.SetBaseValue((int)(statsRef.maxHealth * levelModifier));
		maxMana.SetBaseValue((int)(statsRef.maxMana * levelModifier));
		manaRegenPercentage.SetBaseValue(statsRef.manaRegenPercentage);
		manaRegenCooldown = statsRef.manaRegenCooldown;
		physicalResistance.SetBaseValue((int)(statsRef.physicalDamageResistance * levelModifier));
		poisonResistance.SetBaseValue((int)(statsRef.poisonDamageResistance * levelModifier));
		fireResistance.SetBaseValue((int)(statsRef.fireDamageResistance * levelModifier));
		iceResistance.SetBaseValue((int)(statsRef.iceDamageResistance * levelModifier));

		damageDealtModifier.SetBaseValue(statsRef.damageDealtBaseModifier);

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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(IdleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}

	//STAT CHANGE EVENTS
	private void OnEquipmentChanges(EntityEquipmentHandler equipmentHandler)
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(IdleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}
	private void OnStatUnlock(SOClassStatBonuses statBoost)
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(IdleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}
	private void OnStatRefund(SOClassStatBonuses statBoost)
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(IdleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}

	//DUNGEON MODIFIERS
	public void UpdateDungeonModifiersAppliedToPlayer(Scene newSceneLoaded, LoadSceneMode mode)
	{
		if (!IsPlayerEntity()) return;

		if (newSceneLoaded.name == GameManager.Instance.hubScene)
			RemoveDungeonModifiersFromEntity(GameManager.Instance.currentDungeonData.dungeonStatModifiers);
		else
			ApplyDungeonModifiersToEntity(GameManager.Instance.currentDungeonData.dungeonStatModifiers);
	}
	private void RemoveDungeonModifiersFromEntity(DungeonStatModifier dungeonModifiers)
	{
		bool oldCurrentHealthEqualToOldMaxHealth = false;
		if (currentHealth == maxHealth.finalValue)
			oldCurrentHealthEqualToOldMaxHealth = true;

		if (!IsPlayerEntity())
		{
			maxHealth.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			maxMana.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			physicalResistance.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			poisonResistance.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			fireResistance.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			iceResistance.RemovePercentageValue(dungeonModifiers.difficultyModifier);

			healingPercentageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			physicalDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			poisonDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			fireDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			iceDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			mainWeaponDamageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			dualWeaponDamageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
			rangedWeaponDamageModifier.RemovePercentageValue(dungeonModifiers.difficultyModifier);
		}

		maxHealth.RemovePercentageValue(dungeonModifiers.healthModifier);
		maxMana.RemovePercentageValue(dungeonModifiers.manaModifier);
		physicalResistance.RemovePercentageValue(dungeonModifiers.physicalResistanceModifier);
		poisonResistance.RemovePercentageValue(dungeonModifiers.poisonResistanceModifier);
		fireResistance.RemovePercentageValue(dungeonModifiers.fireResistanceModifier);
		iceResistance.RemovePercentageValue(dungeonModifiers.iceResistanceModifier);

		//healingPercentageModifier.AddPercentageValue();
		physicalDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.physicalDamageModifier);
		poisonDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.poisonDamageModifier);
		fireDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.fireDamageModifier);
		iceDamagePercentageModifier.RemovePercentageValue(dungeonModifiers.iceDamageModifier);
		mainWeaponDamageModifier.RemovePercentageValue(dungeonModifiers.mainWeaponDamageModifier);
		dualWeaponDamageModifier.RemovePercentageValue(dungeonModifiers.dualWeaponDamageModifier);
		rangedWeaponDamageModifier.RemovePercentageValue(dungeonModifiers.rangedWeaponDamageModifier);

		FullHealOnStatChange(oldCurrentHealthEqualToOldMaxHealth);
		UpdatePlayerStatInfoUi();

		if (equipmentHandler == null || equipmentHandler.equippedWeapon == null) return;
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(IdleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}
	private void ApplyDungeonModifiersToEntity(DungeonStatModifier dungeonModifiers)
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
		equipmentHandler.equippedWeapon.UpdateWeaponDamage(IdleWeaponSprite, this, equipmentHandler.equippedOffhandWeapon);
		UpdatePlayerStatInfoUi();
	}

	//full heal entity on stat changes
	public void FullHealOnStatChange(bool oldCurrentHealthEqualToOldMaxHealth)
	{
		if (oldCurrentHealthEqualToOldMaxHealth)
		{
			currentHealth = maxHealth.finalValue;
			currentMana = maxMana.finalValue;
		}

		OnHealthChangeEvent?.Invoke(maxHealth.finalValue, currentHealth);
		OnManaChangeEvent?.Invoke(maxMana.finalValue, currentMana);
	}

	//update ui info if player
	private void UpdatePlayerStatInfoUi()
	{
		if (!EntityIsLocalPlayer()) return;
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
		if (playerRef != null && statsRef.humanoidType == SOEntityStats.HumanoidTypes.isPlayer)
			return true;
		else return false;
	}
	public bool EntityIsLocalPlayer()
	{
		if (playerRef == null) return false;
		return playerRef.PlayerIsLocalPlayer();
	}
}
