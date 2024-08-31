using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class Projectiles : MonoBehaviour
{
	private PlayerController player;
	public SOClassAbilities abilityBaseRef;
	public SOWeapons weaponBaseRef;

	private BoxCollider2D boxCollider;
	private SpriteRenderer projectileSprite;
	private bool isPlayerProjectile;
	private float projectileSpeed;
	private int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	Vector2 projectileOrigin;
	float distanceTraveled;

	public void Initilize(PlayerController player, SOClassAbilities abilityBaseRef, EntityStats casterInfo)
	{
		this.abilityBaseRef = abilityBaseRef;
		this.weaponBaseRef = null;
		gameObject.name = abilityBaseRef.Name + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		this.player = player;
		isPlayerProjectile = casterInfo.IsPlayerEntity();
		projectileSpeed = abilityBaseRef.projectileSpeed;
		damageType = (DamageType)abilityBaseRef.damageType;
		int newDamage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(casterInfo.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			projectileDamage = (int)(newDamage * casterInfo.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			projectileDamage = (int)(newDamage * casterInfo.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			projectileDamage = (int)(newDamage * casterInfo.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			projectileDamage = (int)(newDamage * casterInfo.iceDamagePercentageModifier.finalPercentageValue);

		projectileDamage *= (int)casterInfo.damageDealtModifier.finalPercentageValue;
		gameObject.SetActive(true);
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	public void Initilize(PlayerController player, Weapons weaponRef)
	{
		this.player = player;
		this.abilityBaseRef = null;
		this.weaponBaseRef = weaponRef.weaponBaseRef;
		gameObject.name = weaponRef.itemName + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = weaponRef.weaponBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		isPlayerProjectile = weaponRef.isEquippedByPlayer;
		projectileSpeed = weaponRef.weaponBaseRef.projectileSpeed;
		damageType = (DamageType)weaponRef.weaponBaseRef.baseDamageType;
		projectileDamage = weaponRef.damage;
		projectileOrigin = transform.position;
		gameObject.SetActive(true);
	}
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
			DungeonHandler.ProjectileCleanUp(this);

		if (other.gameObject.GetComponent<Damageable>() == null) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerProjectile ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemies") && !isPlayerProjectile)
			return;

		if (abilityBaseRef != null)
			other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, projectileDamage, (IDamagable.DamageType)damageType, 0,
				abilityBaseRef.isDamagePercentageBased, isPlayerProjectile, false);
		else
		{
			//half ranged weapon damage
			if (distanceTraveled < weaponBaseRef.minAttackRange)
				projectileDamage /= 2;

			other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, projectileDamage, (IDamagable.DamageType)damageType, 0,
				false, isPlayerProjectile, false);
		}
		DungeonHandler.ProjectileCleanUp(this);
	}

	private void FixedUpdate()
	{
		distanceTraveled = Vector2.Distance(transform.position, projectileOrigin);
		transform.Translate(projectileSpeed * Time.deltaTime * Vector2.up);

		if (weaponBaseRef == null) return;
		if (distanceTraveled >= weaponBaseRef.maxAttackRange)
			DungeonHandler.ProjectileCleanUp(this);
	}
}
