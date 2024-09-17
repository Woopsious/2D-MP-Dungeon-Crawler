using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TrapHandler : MonoBehaviour, IInteractables
{
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;

	[Header("Trap Info")]
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
		playerDetectionCollider.GetComponent<EntityDetection>().trapHandler = this;

		spriteRenderer.sprite = null;
		name = trapBaseRef.name;
		trapDetected = false;
		trapDisabled = false;
		trapActivated = false;

		if (trapBaseRef.hasProjectile)
			FindSpawnPointForProjectiles();
	}
	private void UpdateTrapLevel(EntityStats playerStats)
	{
		trapLevel = playerStats.entityLevel;
		levelModifier = Utilities.GetLevelModifier(trapLevel);

		trapDamage = (int)(trapBaseRef.baseDamage * levelModifier);
	}

	//trap actions
	public IEnumerator ActivateTrapDelay(PlayerController player, Collider2D coll)
	{
		yield return new WaitForSeconds(trapBaseRef.trapActivationDelay);
		ActivateTrap(player, coll);
	}
	private void ActivateTrap(PlayerController player, Collider2D coll)
	{
		if (trapDisabled || trapActivated) return;

		DamageEverythingInsideAoe(coll); //apply damage + effects
		Debug.Log("trap activated");

		trapActivated = true;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapBaseRef.trapActivatedSfx);
	}
	private void DamageEverythingInsideAoe(Collider2D coll)
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, trapBaseRef.aoeSize, Vector2.up, 0, layerMask);

		foreach (RaycastHit2D hit in hits)
		{
			EntityStats entityStats = hit.transform.GetComponent<EntityStats>();

			if (trapBaseRef.hasProjectile) //no need to apply effects as projectiles do that already
				ShootProjectiles(entityStats);
			else
			{
				entityStats.GetComponent<Damageable>().OnHitFromDamageSource(null, coll, trapDamage, 
					(IDamagable.DamageType)trapBaseRef.baseDamageType, 0, false, false, true); //apply damage

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
		projectile.transform.position = projectileSpawnPoint;
		projectile.Initilize(trapBaseRef, trapDamage, projectileSpawnPoint);

		SetProjectileDirection(projectile, GetAttackRotation(entity.transform.position));
	}
	private float GetAttackRotation(Vector3 positionOfThingToAttack)
	{
		Vector3 rotation = positionOfThingToAttack - (Vector3)projectileSpawnPoint;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		return rotz;
	}
	private void SetProjectileDirection(Projectiles projectile, float rotz)
	{
		projectile.transform.rotation = Quaternion.Euler(0, 0, rotz - 90);
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
				Debug.Log("same player detected breaking loop");
				break;
			}
		}
		if (playerAlreadyTried) return;

		if (!RollPlayerDetectChance(newPlayer)) //roll for detect
		{
			Debug.Log("trap not detected");
			return; //failed detect
		}

		//detect trap
		Debug.Log("trap detected");

		trapDetected = true;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapDetectedSfx);
	}
	private bool RollPlayerDetectChance(PlayerController newPlayer)
	{
		playersCheckedTrap.Add(newPlayer);

		if (newPlayer.playerClassHandler.currentEntityClass.trapDetectionChance >= trapBaseRef.trapDetectionDifficulty)
			return true;
		else return false;
	}
	private void DeactivateTrap()
	{
		if (trapDisabled || trapActivated) return;

		Debug.Log("trap deactivated");

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
