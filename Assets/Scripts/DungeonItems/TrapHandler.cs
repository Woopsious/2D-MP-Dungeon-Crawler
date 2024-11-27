using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapHandler : MonoBehaviour, IInteractables
{
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;

	[Header("Trap Info")]
	public List<SOTraps> trapTypes = new List<SOTraps>();
	public bool debugOverrideTrapType;
	public SOTraps trapBaseRef;
	public LayerMask layerMask;
	public LayerMask projectileObstaclesMaskCheck;
	public bool trapDetected;
	public bool trapDisabled;
	public bool trapActivated;

	private int trapLevel;
	private float levelModifier;

	public GameObject playerDetectionCollider;
	public GameObject projectilePrefab;

	[Header("Trap Damage")]
	private int trapDamage;

	[Header("Players")] //track players already trying to detect trap
	private List<PlayerController> playersCheckedTrap = new List<PlayerController>();

	[Header("Projectile spawn point")]
	public Vector2 projectileSpawnPoint;

	[Header("Shared audio")]
	public AudioClip trapDetectedSfx;
	public AudioClip trapDeactivatedSfx;

	[Header("Trap spawn chances")]
	public float totalTrapSpawnChance;
	public List<float> trapSpawnChanceTable = new List<float>();

	private void Awake()
	{
		Initilize();
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent += UpdateTrapLevel;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateTrapLevel;
		StopAllCoroutines();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.GetComponent<PlayerController>() != null)
		{
			TryDetectTrap(collision.GetComponent<PlayerController>());
		}
	}

	private void Initilize()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
		playerDetectionCollider.GetComponent<TrapActivationCollider>().trapHandler = this;

		if (!debugOverrideTrapType)
		{
			CreateTrapSpawnTable();
			trapBaseRef = trapTypes[GetIndexOfTrapToSpawn()];
		}

		spriteRenderer.sprite = null;
		name = trapBaseRef.name;
		trapDetected = false;
		trapDisabled = false;
		trapActivated = false;

		if (trapBaseRef.hasProjectile)
			FindSpawnPointForProjectiles();
	}
	private void CreateTrapSpawnTable()
	{
		trapSpawnChanceTable.Clear();
		totalTrapSpawnChance = 0;

		foreach (SOTraps trap in trapTypes)
			trapSpawnChanceTable.Add(trap.trapSpawnChance);

		foreach (float num in trapSpawnChanceTable)
			totalTrapSpawnChance += num;
	}
	private int GetIndexOfTrapToSpawn()
	{
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
	private void UpdateTrapLevel(EntityStats playerStats)
	{
		trapLevel = playerStats.entityLevel;
		levelModifier = Utilities.GetLevelModifier(trapLevel);

		trapDamage = (int)(trapBaseRef.baseDamage * levelModifier);
	}

	//trap actions
	public IEnumerator ActivateTrapDelay(Collider2D coll)
	{
		yield return new WaitForSeconds(trapBaseRef.trapActivationDelay);
		ActivateTrap(coll);
	}
	private void ActivateTrap(Collider2D coll)
	{
		if (trapDisabled || trapActivated) return;

		TryDamageThingsInsideAoe(coll); //apply damage + effects
		trapActivated = true;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapBaseRef.trapActivatedSfx);
	}
	private void TryDamageThingsInsideAoe(Collider2D coll)
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
				entityStats.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo); //apply damage

				if (trapBaseRef.hasEffects) //apply effects
					entityStats.ApplyNewStatusEffects(trapBaseRef.statusEffects, entityStats);
			}
		}
	}
	private void ShootProjectiles(EntityStats entity)
	{
		Projectiles projectile = DungeonHandler.GetProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
		}

		projectile.transform.SetParent(null);
		projectile.SetPositionAndAttackDirection(projectileSpawnPoint, entity.transform.position);
		projectile.Initilize(trapBaseRef, trapDamage);
	}

	private void TryDetectTrap(PlayerController newPlayer)
	{
		if (trapDisabled || trapActivated) return;

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

		//detect trap
		trapDetected = true;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapDetectedSfx);
	}
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
			return true;
		else return false;
	}

	private void DeactivateTrap()
	{
		if (trapDisabled || trapActivated) return;

		trapDisabled = true;
		audioHandler.PlayAudio(trapDeactivatedSfx);
		StartCoroutine(DisableObject(trapDeactivatedSfx.length));
	}

	//set projectile spawn point, avoiding positions too close or in LoS of obstacles
	private void FindSpawnPointForProjectiles()
	{
		projectileSpawnPoint = FindPointWithinDonutShape();
	}
	private Vector2 FindPointWithinDonutShape()
	{
		for (var i = 0; i < 100; i++)
		{
			Vector2 pos = (Vector2)transform.position + (Random.insideUnitCircle * 7);
			if (Vector3.Distance(pos, transform.position) > 4 && ProjectileSpawnPointInsideWall(pos))
				return pos;
		}
		return Vector2.zero;
	}
	private bool ProjectileSpawnPointInsideWall(Vector2 pos)
	{
		if (Physics2D.Linecast(transform.position, pos, projectileObstaclesMaskCheck))
			return false;
		return true;
	}

	private IEnumerator DisableObject(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		gameObject.SetActive(false);
	}

	//interacts
	public void Interact(PlayerController player)
	{
		if (trapDetected && !trapActivated)
			DeactivateTrap();
	}
	public void UnInteract(PlayerController player)
	{

	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, 3);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 1 * 5);
	}
}
