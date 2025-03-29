using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Accessibility;

public class TrapHandler : MonoBehaviour, IInteractables
{
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;

	[Header("Trap Info")]
	public bool debugOverrideTrapType;
	public SOTraps trapBaseRef;
	public LayerMask layerMask;
	public LayerMask projectileObstaclesMaskCheck;

	private int trapLevel;
	private float levelModifier;

	public GameObject playerDetectionCollider;
	public GameObject projectilePrefab;

	//trap states
	public TrapStates trapState;
	public enum TrapStates
	{
		disabled, enabled, detected, activated
	}

	[Header("Trap Damage")]
	private int trapDamage;

	[Header("Players")] //track players already trying to detect trap
	private List<PlayerController> playersCheckedTrap = new List<PlayerController>();

	[Header("Projectile spawn point")]
	public Vector2 projectileSpawnPoint;

	[Header("Shared audio")]
	public AudioClip trapDetectedSfx;
	public AudioClip trapDeactivatedSfx;
	public AudioClip trapActivatedSfx;

	[Header("Trap spawn chances")]
	public float totalTrapSpawnChance;
	public List<float> trapSpawnChanceTable = new List<float>();

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
		playerDetectionCollider.GetComponent<TrapActivationCollider>().trapHandler = this;
		trapState = TrapStates.disabled;
	}
	private void Start()
	{
		if (!DungeonHandler.Instance.dungeonTrapsList.Contains(this))
			Debug.LogError("Trap not added to dungeon trap list, ensure all are added");
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerLevelChangeEvent += UpdateTrapLevel;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerLevelChangeEvent -= UpdateTrapLevel;
		StopAllCoroutines();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.GetComponent<PlayerController>() != null)
			TryDetectTrap(collision.GetComponent<PlayerController>());
	}

	//set up trap + type
	public int SetUpTrap()
	{
		int trapTypeIndex;

		if (debugOverrideTrapType && trapBaseRef != null)
			trapTypeIndex = GetIndexOfTrapTypeToSetUP(trapBaseRef);
		else
		{
			CreateTrapTypeTable();
			trapTypeIndex = GetIndexOfTrapTypeToSetUP(null);
		}

		SetTrapType(trapTypeIndex);

		if (trapBaseRef.hasProjectile)
			FindSpawnPointForProjectiles();

		return trapTypeIndex;
	}
	public void SetTrapType(int trapTypeIndex)
	{
		trapBaseRef = AssetDatabase.Database.traps[trapTypeIndex];
		name = trapBaseRef.name;
		EnableTrapState();
	}

	//trap initilization
	private void CreateTrapTypeTable()
	{
		trapSpawnChanceTable.Clear();
		totalTrapSpawnChance = 0;

		foreach (SOTraps trap in AssetDatabase.Database.traps)
			trapSpawnChanceTable.Add(trap.trapSpawnChance);

		foreach (float num in trapSpawnChanceTable)
			totalTrapSpawnChance += num;
	}
	private int GetIndexOfTrapTypeToSetUP(SOTraps optionalTrap)
	{
		if (optionalTrap != null) //fetch index of trap ref instead of getting random one
		{
			for (int i = 0; i < AssetDatabase.Database.traps.Count; i++)
			{
				if (optionalTrap == AssetDatabase.Database.traps[i])
					return i;
			}
		}

		float rand = Random.Range(0, totalTrapSpawnChance);
		float cumChance = 0;

		for (int i = 0; i < trapSpawnChanceTable.Count; i++)
		{
			cumChance += trapSpawnChanceTable[i];

			if (rand <= cumChance)
				return i;
		}
		return -1;
	}

	//set projectile spawn point
	private void FindSpawnPointForProjectiles()
	{
		projectileSpawnPoint = FindPointWithinDonutShape();
	}
	private Vector2 FindPointWithinDonutShape()
	{
		for (var i = 0; i < 250; i++)
		{
			Vector2 pos = (Vector2)transform.position + (Random.insideUnitCircle * 7);
			if (Vector3.Distance(pos, transform.position) > 4 && PositionVisible(pos))
				return pos;
		}
		return Vector2.zero;
	}
	private bool PositionVisible(Vector2 pos)
	{
		if (Physics2D.Linecast(transform.position, pos, projectileObstaclesMaskCheck))
			return false;
		return true;
	}

	//TRAP STATE CHANGES + SYNCING
	//enable trap
	public void EnableTrapState()
	{
		trapState = TrapStates.enabled;
		spriteRenderer.sprite = null;
	}

	//disable trap
	public void DisableTrap(bool playerInteraction)
	{
		float waitTime;

		if (playerInteraction)
		{
			if (trapState != TrapStates.detected) return; //ignore disabling if player interaction and trap undetected
			waitTime = trapDeactivatedSfx.length;
		}
		else
			waitTime = 0;

		StartCoroutine(DisableTrapState(waitTime));
		//call mp sync state

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncDungeonTrapStateRpc(GetTrapIndex(), TrapStates.disabled, waitTime);
		else
			StartCoroutine(DisableTrapState(waitTime));
	}
	public IEnumerator DisableTrapState(float waitTime)
	{
		trapState = TrapStates.disabled;

		if (waitTime != 0)
			audioHandler.PlayAudio(trapDeactivatedSfx);

		yield return new WaitForSeconds(waitTime);
		gameObject.SetActive(false);
	}

	//detect trap
	private void TryDetectTrap(PlayerController newPlayer)
	{
		if (trapState != TrapStates.enabled) return;

		bool playerAlreadyTried = false; //check if player already rolled for detect
		foreach (PlayerController player in playersCheckedTrap)
		{
			if (newPlayer == player)
			{
				playerAlreadyTried = true;
				break;
			}
		}
		if (playerAlreadyTried) return;

		//roll for detect
		if (!RollPlayerDetectChance(newPlayer))
			return; //failed detect

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncDungeonTrapStateRpc(GetTrapIndex(), TrapStates.detected, 0);
		else
			DetectTrapState();
	}
	public void DetectTrapState()
	{
		trapState = TrapStates.detected;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapDetectedSfx);
	}

	//activate trap from player coll
	public IEnumerator ActivateTrap()
	{
		if (trapState == TrapStates.disabled || trapState == TrapStates.activated) yield return null;
		trapState = TrapStates.activated; //call early

		yield return new WaitForSeconds(trapBaseRef.trapActivationDelay);

		TryDamageThingsInsideAoe(); //apply damage + effects

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncDungeonTrapStateRpc(GetTrapIndex(), TrapStates.activated, 0);
		else
			StartCoroutine(ActivateTrapState());
	}
	public IEnumerator ActivateTrapState()
	{
		trapState = TrapStates.activated;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapActivatedSfx);

		yield return new WaitForSeconds(trapActivatedSfx.length);
		gameObject.SetActive(false);
	}

	//helpers
	private bool RollPlayerDetectChance(PlayerController newPlayer)
	{
		playersCheckedTrap.Add(newPlayer);
		SOClasses playerClass = newPlayer.playerClassHandler.currentEntityClass;
		float trapDetectionChance = 0;

		//magic traps easier to spot for mages but harder for rogues (other classes leave as is since magic traps difficult to spot)
		if (playerClass.classType == SOClasses.ClassType.isMage && trapBaseRef.trapType == SOTraps.TrapType.isMagic)
			trapDetectionChance += 0.05f;
		else if (playerClass.classType == SOClasses.ClassType.isRogue && trapBaseRef.trapType == SOTraps.TrapType.isMagic)
			trapDetectionChance -= 0.05f;

		//randomise chance slighty whilst leaving rogue as best trap spotter
		trapDetectionChance += Random.Range(-0.1f, 0.1f) + playerClass.trapDetectionChance;

		if (trapDetectionChance >= trapBaseRef.trapDetectionDifficulty)
		{
			Debug.LogError("player with id: " + newPlayer.OwnerClientId + " detected trap");
			return true;
		}
		else
		{
			Debug.LogError("player with id: " + newPlayer.OwnerClientId + " failed to detected trap");
			return false;
		}
	}
	private int GetTrapIndex()
	{
		for (int i = 0; i < DungeonHandler.Instance.dungeonTrapsList.Count; i++)
		{
			if (DungeonHandler.Instance.dungeonTrapsList[i] == this)
				return i;
		}

		Debug.LogError("Failed to match this trap to one in DungeonHandler list");
		return 0;
	}

	//apply damage/effects
	private void TryDamageThingsInsideAoe()
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, trapBaseRef.aoeSize, Vector2.up, 0, layerMask);

		foreach (RaycastHit2D hit in hits)
		{
			EntityStats entityStats = hit.transform.GetComponent<EntityStats>();

			if (trapBaseRef.hasProjectile) //no need to apply effects as projectiles do that already
				ShootProjectiles(entityStats);
			else
			{
				DamageSourceInfo damageSourceInfo = new(null, IDamagable.HitBye.enviroment, 
					trapDamage, (IDamagable.DamageType)trapBaseRef.baseDamageType, false);

				damageSourceInfo.SetDeathMessage(trapBaseRef);
				entityStats.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo); //apply damage

				if (trapBaseRef.hasEffects) //apply effects
					entityStats.ApplyNewStatusEffects(trapBaseRef.statusEffects, entityStats);
			}
		}
	}
	private void ShootProjectiles(EntityStats entity)
	{
		Projectiles projectile = ObjectPoolingManager.GetInActiveProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
			ObjectPoolingManager.AddProjectileToObjectPooling(projectile);

			if (MultiplayerManager.IsMultiplayer())
				projectile.GetComponent<NetworkObject>().Spawn();
		}

		projectile.Initilize(trapBaseRef, trapDamage, transform.position, entity.transform.position);
	}

	//player interacts
	public void Interact(PlayerController player)
	{
		DisableTrap(true);
	}
	public void UnInteract(PlayerController player)
	{
		//noop
	}

	//update trap level event
	private void UpdateTrapLevel(PlayerController player)
	{
		trapLevel = player.playerStats.entityLevel;
		levelModifier = Utilities.GetLevelModifier(trapLevel);

		trapDamage = (int)(trapBaseRef.baseDamage * levelModifier);
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, 3);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 1 * 5);
	}
}
