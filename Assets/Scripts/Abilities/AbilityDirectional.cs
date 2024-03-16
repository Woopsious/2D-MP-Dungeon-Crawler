using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityDirectional : MonoBehaviour
{
	public SOClassAbilities abilityBaseRef;

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

	public void Initilize(SOClassAbilities abilityBaseRef, EntityStats casterInfo)
	{
		this.abilityBaseRef = abilityBaseRef;
		gameObject.name = abilityBaseRef.Name + "Projectile";
		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

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

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
			Destroy(gameObject);

		if (other.gameObject.GetComponent<Damageable>() == null) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerProjectile ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !isPlayerProjectile)
			return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(other ,projectileDamage, (IDamagable.DamageType)damageType, 0,
			abilityBaseRef.isDamagePercentageBased ,isPlayerProjectile);

		Destroy(gameObject);
	}

	private void FixedUpdate()
	{
		transform.Translate(projectileSpeed * Time.deltaTime * Vector2.up);
	}
}
