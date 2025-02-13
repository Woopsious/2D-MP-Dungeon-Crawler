using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Projectiles : NetworkBehaviour
{
	public SOTraps trapRef;
	public SOWeapons weaponRef;
	public SOAbilities abilityRef;
	public EntityStats projectileOwner;	//only set for abilities

	private BoxCollider2D boxCollider;
	private SpriteRenderer projectileSprite;
	private float projectileSpeed;
	public int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
	private IDamagable.HitBye hitBye;
	private bool isPercentageDamage;

	Vector2 projectileOrigin;
	float distanceTraveled;

	//set trap projectile data
	public void Initilize(SOTraps trap, int trapDamage)
	{
		int trapIndex = 0;

		foreach (SOTraps trapX in AssetDatabase.Database.traps)
		{
			if (trap != trapX)
			{
				trapIndex++;
				continue;
			}

			if (MultiplayerManager.IsMultiplayer())
				SyncTrapRpc(trapIndex, trapDamage);
			else
				SyncTrapRpc(trapIndex, trapDamage);

			break;
		}
	}

	[Rpc(SendTo.Everyone)]
	private void SyncTrapRpc(int trapIndex, int trapDamage)
	{
		SyncTrap(trapIndex, trapDamage);
	}
	private void SyncTrap(int trapIndex, int trapDamage)
	{
		trapRef = AssetDatabase.Database.traps[trapIndex];
		weaponRef = null;
		abilityRef = null;
		gameObject.name = trapRef.name + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = trapRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = trapRef.projectileSpeed;
		projectileDamage = trapDamage;
		damageType = (DamageType)trapRef.baseDamageType;
		UpdateHitByeVariable(null);
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set ability projectile data
	public void Initilize(EntityStats projectileOwner, SOAbilities abilityRef)
	{
		if (!MultiplayerManager.IsMultiplayer()) //fix ability owner ref as Network spawn manager wont exist in sp
		{
			InitilizeSinglePlayer(projectileOwner, abilityRef);
			return;
		}

		ulong ownerId = projectileOwner.GetComponent<NetworkObject>().NetworkObjectId;
		int abilityIndex = 0;

		foreach (SOAbilities ability in AssetDatabase.Database.abilities)
		{
			if (abilityRef != ability)
			{
				abilityIndex++;
				continue;
			}

			SyncProjectileRpc(ownerId, abilityIndex);
			break;
		}
	}

	[Rpc(SendTo.Everyone)]
	private void SyncProjectileRpc(ulong ownerId, int abilityIndex)
	{
		SyncProjectile(ownerId, abilityIndex);
	}
	private void SyncProjectile(ulong ownerId, int abilityIndex)
	{
		trapRef = null;
		weaponRef = null;
		abilityRef = AssetDatabase.Database.abilities[abilityIndex];
		//grab owner of ability via list of spawned objs using its unique id
		projectileOwner = NetworkManager.SpawnManager.SpawnedObjects[ownerId].gameObject.GetComponent<EntityStats>();

		gameObject.name = abilityRef.Name + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = abilityRef.projectileSpeed;
		int newDamage = (int)(abilityRef.damageValue * Utilities.GetLevelModifier(projectileOwner.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.iceDamagePercentageModifier.finalPercentageValue);

		projectileDamage *= (int)projectileOwner.damageDealtModifier.finalPercentageValue;
		damageType = (DamageType)abilityRef.damageType;
		UpdateHitByeVariable(projectileOwner.playerRef);
		isPercentageDamage = abilityRef.isDamagePercentageBased;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	private void InitilizeSinglePlayer(EntityStats projectileOwner, SOAbilities abilityRef)
	{
		trapRef = null;
		weaponRef = null;
		this.abilityRef = abilityRef;
		this.projectileOwner = projectileOwner;

		gameObject.name = abilityRef.Name + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = abilityRef.projectileSpeed;
		int newDamage = (int)(abilityRef.damageValue * Utilities.GetLevelModifier(projectileOwner.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.iceDamagePercentageModifier.finalPercentageValue);

		projectileDamage *= (int)projectileOwner.damageDealtModifier.finalPercentageValue;
		damageType = (DamageType)abilityRef.damageType;
		UpdateHitByeVariable(projectileOwner.playerRef);
		isPercentageDamage = abilityRef.isDamagePercentageBased;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set weapon projectile data
	public void Initilize(EntityStats projectileOwner, SOWeapons weaponRef, int projectileDamage)
	{
		if (!MultiplayerManager.IsMultiplayer()) //fix ability owner ref as Network spawn manager wont exist in sp
		{
			InitilizeSinglePlayer(projectileOwner, weaponRef, projectileDamage);
			return;
		}

		ulong ownerId = projectileOwner.GetComponent<NetworkObject>().NetworkObjectId;
		int weaponIndex = 0;

		foreach (SOWeapons weapon in AssetDatabase.Database.weapons)
		{
			if (weaponRef != weapon)
			{
				weaponIndex++;
				continue;
			}

			SyncProjectileRpc(ownerId, weaponIndex, projectileDamage);
			break;
		}
	}

	[Rpc(SendTo.Everyone)]
	private void SyncProjectileRpc(ulong ownerId, int weaponIndex, int projectileDamahe)
	{
		SyncProjectile(ownerId, weaponIndex, projectileDamahe);
	}
	private void SyncProjectile(ulong ownerId, int weaponIndex, int projectileDamage)
	{
		trapRef = null;
		weaponRef = AssetDatabase.Database.weapons[weaponIndex];
		abilityRef = null;
		//grab owner of ability via list of spawned objs using its unique id
		projectileOwner = NetworkManager.SpawnManager.SpawnedObjects[ownerId].gameObject.GetComponent<EntityStats>();

		gameObject.name = weaponRef.itemName + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = weaponRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = weaponRef.projectileSpeed;
		this.projectileDamage = projectileDamage;
		damageType = (DamageType)weaponRef.baseDamageType;
		UpdateHitByeVariable(projectileOwner.playerRef);
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	public void InitilizeSinglePlayer(EntityStats projectileOwner, SOWeapons weaponRef, int projectileDamage)
	{
		trapRef = null;
		this.weaponRef = weaponRef;
		abilityRef = null;
		this.projectileOwner = projectileOwner;

		gameObject.name = weaponRef.itemName + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = weaponRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = weaponRef.projectileSpeed;
		this.projectileDamage = projectileDamage;
		damageType = (DamageType)weaponRef.baseDamageType;
		UpdateHitByeVariable(projectileOwner.playerRef);
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
	}

	//helps with applying damage only to enemies
	private void UpdateHitByeVariable(PlayerController player)
	{
		if (player != null)
			hitBye = IDamagable.HitBye.player;
		else
			hitBye = IDamagable.HitBye.entity;

		if (trapRef != null) //if ref not null overwrite hitbye
			hitBye = IDamagable.HitBye.enviroment;
	}

	//set projectile position, rotation and target position
	public void SetPositionAndAttackDirection(Vector3 OriginPosition, Vector3 positionOfThingToAttack)
	{
		projectileOrigin = OriginPosition;
		Vector3 rotation = positionOfThingToAttack - OriginPosition;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		transform.SetPositionAndRotation(OriginPosition, Quaternion.Euler(0, 0, rotz - 90));
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!MultiplayerManager.IsClientHost()) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			if (MultiplayerManager.IsMultiplayer())
				DisableObjectRpc();
			else
				DisableObject();
		}

		if (other.gameObject.GetComponent<Damageable>() == null) return;

		if (hitBye == IDamagable.HitBye.player && other.gameObject.layer == LayerMask.NameToLayer("Player") ||
			hitBye == IDamagable.HitBye.entity && other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
			return;

		DamageSourceInfo damageSourceInfo = new(
			projectileOwner, hitBye, projectileDamage, (IDamagable.DamageType)damageType, isPercentageDamage);

		if (trapRef != null)	//traps
		{
			damageSourceInfo.SetDeathMessage(trapRef);
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);

			if (trapRef.hasEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityRef.statusEffects, projectileOwner);
		}
		else if (abilityRef != null)	//abilities
		{
			damageSourceInfo.SetDeathMessage(abilityRef);
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);

			if (abilityRef.hasStatusEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityRef.statusEffects, projectileOwner);
		}
		else	//weapon projectiles
		{
			//half ranged weapon damage
			if (distanceTraveled < weaponRef.minAttackRange)
				projectileDamage /= 2;

			damageSourceInfo.AddKnockbackEffect(boxCollider.transform.position, weaponRef.baseKnockback);
			damageSourceInfo.SetDeathMessage(weaponRef);
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
		}

		if (MultiplayerManager.IsMultiplayer())
			DisableObjectRpc();
		else
			DisableObject();
	}
	private void FixedUpdate()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		transform.Translate(projectileSpeed * Time.deltaTime * Vector2.up);
		if (weaponRef == null) return;

		distanceTraveled = Vector2.Distance(transform.position, projectileOrigin);
		if (distanceTraveled >= weaponRef.maxAttackRange)
		{
			if (MultiplayerManager.IsMultiplayer())
				DisableObjectRpc();
			else
				DisableObject();
		}
	}

	[Rpc(SendTo.Everyone)]
	private void EnableObjectRpc()
	{
		EnableObject();
	}
	private void EnableObject()
	{
		gameObject.SetActive(true);
	}

	[Rpc(SendTo.Everyone)]
	private void DisableObjectRpc()
	{
		DisableObject();
	}
	private void DisableObject()
	{
		gameObject.SetActive(false);
		transform.position = Vector3.zero;
		ObjectPoolingManager.AddProjectileToInActivePool(this);
	}
}
