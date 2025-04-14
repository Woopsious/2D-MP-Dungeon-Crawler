using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
	private IDamagable.DamageType damageType;
	private IDamagable.HitBye hitBye;
	private bool isPercentageDamage;

	Vector2 projectileOrigin;
	float distanceTraveled;

	//set trap projectile data
	public void Initilize(SOTraps trap, int trapDamage, Vector2 trapPosition, Vector2 attackPos)
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			for (int i = 0; i < AssetDatabase.Database.traps.Count; i++)
			{
				if (trapRef == AssetDatabase.Database.traps[i])
				{
					SetUpTrapProjectileRpc(i, trapDamage, trapPosition, attackPos);
					return;
				}
			}
			Debug.LogError("projectile set up failed");
		}
		else
			SetUpTrapProjectile(trap, trapDamage, trapPosition, attackPos);
	}

	[Rpc(SendTo.Everyone)]
	private void SetUpTrapProjectileRpc(int trapIndex, int trapDamage, Vector2 trapPosition, Vector2 attackPos)
	{
		SOTraps trapRef = AssetDatabase.Database.traps[trapIndex];
		SetUpTrapProjectile(trapRef, trapDamage, trapPosition, attackPos);
	}
	private void SetUpTrapProjectile(SOTraps trapRef, int trapDamage, Vector2 trapPosition, Vector2 attackPos)
	{
		transform.SetParent(null);
		SetPositionAndAttackDirection(trapPosition, attackPos);
		this.trapRef = trapRef;
		weaponRef = null;
		abilityRef = null;
		UpdateHitByeVariable();

		gameObject.name = trapRef.name + " Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = trapRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = trapRef.projectileSpeed;
		projectileDamage = trapDamage;
		damageType = (IDamagable.DamageType)trapRef.baseDamageType;
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set ability projectile data
	public void Initilize(EntityStats ownerStats, SOAbilities abilityRef, Vector2 attackPos)
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			ulong ownerId = ownerStats.GetComponent<NetworkObject>().NetworkObjectId;

			for (int i = 0; i < AssetDatabase.Database.abilities.Count; i++)
			{
				if (abilityRef == AssetDatabase.Database.abilities[i])
				{
					SetUpAbilityProjectileRpc(ownerId, i, attackPos);
					return;
				}
			}
			Debug.LogError("projectile set up failed");
		}
		else
			SetUpAbilityProjectile(ownerStats, abilityRef, attackPos);
	}

	[Rpc(SendTo.Everyone)]
	private void SetUpAbilityProjectileRpc(ulong ownerId, int abilityIndex, Vector2 attackPos)
	{
		EntityStats ownerStats = NetworkManager.SpawnManager.SpawnedObjects[ownerId].GetComponent<EntityStats>();
		SOAbilities abilityRef = AssetDatabase.Database.abilities[abilityIndex];
		SetUpAbilityProjectile(ownerStats, abilityRef, attackPos);
	}
	private void SetUpAbilityProjectile(EntityStats ownerStats, SOAbilities abilityRef, Vector2 attackPos)
	{
		Debug.LogError(abilityRef.Name + " damage: " + abilityRef.damageValue);

		transform.SetParent(null);
		SetPositionAndAttackDirection(ownerStats.transform.position, attackPos);
		trapRef = null;
		weaponRef = null;
		this.abilityRef = abilityRef;
		projectileOwner = ownerStats;
		UpdateHitByeVariable();

		gameObject.name = abilityRef.Name + " Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = abilityRef.projectileSpeed;
		projectileDamage = SetAbilityDamage();
		damageType = abilityRef.damageType;
		isPercentageDamage = abilityRef.isDamagePercentageBased;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	private int SetAbilityDamage()
	{
		int newDamage = (int)(abilityRef.damageValue * Utilities.GetLevelModifier(projectileOwner.entityLevel));

		if (damageType == IDamagable.DamageType.isPhysicalDamage)
			newDamage = (int)(newDamage * projectileOwner.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == IDamagable.DamageType.isPoisonDamage)
			newDamage = (int)(newDamage * projectileOwner.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == IDamagable.DamageType.isFireDamage)
			newDamage = (int)(newDamage * projectileOwner.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == IDamagable.DamageType.isIceDamage)
			newDamage = (int)(newDamage * projectileOwner.iceDamagePercentageModifier.finalPercentageValue);

		newDamage = (int)(newDamage * projectileOwner.damageDealtModifier.finalPercentageValue);
		return newDamage;
	}

	//set weapon projectile data
	public void Initilize(EntityStats ownerStats, SOWeapons weaponRef, int projectileDamage, Vector2 attackPos)
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			ulong ownerId = ownerStats.GetComponent<NetworkObject>().NetworkObjectId;

			for (int i = 0; i < AssetDatabase.Database.weapons.Count; i++)
			{
				if (weaponRef == AssetDatabase.Database.weapons[i])
				{
					SetUpWeaponProjectileRpc(ownerId, i, projectileDamage, attackPos);
					return;
				}
			}
			Debug.LogError("projectile set up failed");
		}
		else
			SetUpWeaponProjectile(ownerStats, weaponRef, projectileDamage, attackPos);
	}

	[Rpc(SendTo.Everyone)]
	private void SetUpWeaponProjectileRpc(ulong ownerId, int weaponIndex, int projectileDamage, Vector2 attackPos)
	{
		EntityStats ownerStats = NetworkManager.SpawnManager.SpawnedObjects[ownerId].GetComponent<EntityStats>();
		SOWeapons weaponRef = AssetDatabase.Database.weapons[weaponIndex];
		SetUpWeaponProjectile(ownerStats, weaponRef, projectileDamage, attackPos);
	}
	private void SetUpWeaponProjectile(EntityStats ownerStats, SOWeapons weaponRef, int weaponProjectileDamage, Vector2 attackPos)
	{
		transform.SetParent(null);
		SetPositionAndAttackDirection(ownerStats.transform.position, attackPos);
		trapRef = null;
		this.weaponRef = weaponRef;
		abilityRef = null;
		projectileOwner = ownerStats;
		UpdateHitByeVariable();

		gameObject.name = weaponRef.itemName + " Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = weaponRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = weaponRef.projectileSpeed;
		projectileDamage = weaponProjectileDamage;
		damageType = (IDamagable.DamageType)weaponRef.baseDamageType;
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set projectile position, rotation and target position
	private void SetPositionAndAttackDirection(Vector3 OriginPosition, Vector3 positionOfThingToAttack)
	{
		transform.position = OriginPosition;
		projectileOrigin = OriginPosition;
		Vector3 rotation = positionOfThingToAttack - OriginPosition;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		transform.SetPositionAndRotation(OriginPosition, Quaternion.Euler(0, 0, rotz - 90));
	}

	//helper to limit who can damage who
	private void UpdateHitByeVariable()
	{
		if (projectileOwner.IsPlayerEntity())
			hitBye = IDamagable.HitBye.player;
		else
			hitBye = IDamagable.HitBye.entity;

		//overwrites
		if (trapRef != null)
			hitBye = IDamagable.HitBye.enviroment;

		if (abilityRef != null && abilityRef.abilityEnviromental)
			hitBye = IDamagable.HitBye.enviroment;
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
