using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAOE : MonoBehaviour
{
	private PlayerController player;
	public SOBossAbilities abilityBossRef;
	public SOAbilities abilityRef;
	private EntityStats casterInfo;
	private Vector2 casterPosition;
	private bool debugLockDamage;

	public GameObject aoeColliderIndicator;
	public LayerMask includeLayer;
	public LayerMask excludeLayer;

	public LayerMask lineOfSightLayer;

	public float abilityDurationTimer;
	private CircleCollider2D circleCollider;
	private BoxCollider2D boxCollider;
	private bool aoeLingers;
	private bool isPlayerAoe;
	public int aoeDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	public List<EntityStats> entityStatsList = new List<EntityStats>();
	public List<PlayerController> playerList = new List<PlayerController>();

	private void Update()
	{
		AbilityDurationTimer();
	}

	//SET DATA
	public void Initilize(SOAbilities abilityRef, EntityStats casterInfo, Vector2 targetPosition)
	{
		if (abilityRef is SOBossAbilities abilityBossRef)
			this.abilityBossRef = abilityBossRef;
		this.abilityRef = abilityRef;

		this.casterInfo = casterInfo;
		casterPosition = casterInfo.transform.position;
		gameObject.name = abilityRef.Name + "Aoe";
		aoeColliderIndicator.GetComponent<SpriteRenderer>().sprite = abilityRef.abilitySprite;
		aoeColliderIndicator.transform.localPosition = Vector3.zero;
		isPlayerAoe = casterInfo.IsPlayerEntity();

		if (abilityRef.aoeType == SOAbilities.AoeType.isBoxAoe)
		{
			SetBoxColliderDirection(targetPosition);
			SetupBoxCollider();
		}
		else
		{
			SetCircleColliderPosition(targetPosition);
			SetupCircleCollider();
		}

		SetDamage();

		aoeLingers = true;
		abilityDurationTimer = abilityRef.aoeDuration;
		if (abilityRef.aoeDuration == 0)
		{
			aoeLingers = false;
			abilityDurationTimer = 0.1f;
		}

		gameObject.SetActive(true);
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	private void SetDamage()
	{
		damageType = (DamageType)abilityRef.damageType;
		int newDamage = (int)(abilityRef.damageValue * Utilities.GetLevelModifier(casterInfo.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			aoeDamage = (int)(newDamage * casterInfo.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			aoeDamage = (int)(newDamage * casterInfo.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			aoeDamage = (int)(newDamage * casterInfo.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			aoeDamage = (int)(newDamage * casterInfo.iceDamagePercentageModifier.finalPercentageValue);

		aoeDamage *= (int)casterInfo.damageDealtModifier.finalPercentageValue;
	}

	//optional, helps with applying damage only to enemies
	public void AddPlayerRef(PlayerController player)
	{
		this.player = player;
	}

	//set up colliders + transforms
	private void SetCircleColliderPosition(Vector3 targetPosition)
	{
		Vector3 rotation = targetPosition - casterInfo.transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

		if (abilityRef.aoeType == SOAbilities.AoeType.isCircleAoe)
			transform.SetPositionAndRotation(targetPosition, Quaternion.Euler(0, 0, rotz - 90));
		else if (abilityRef.aoeType == SOAbilities.AoeType.isConeAoe)
			transform.SetPositionAndRotation(casterInfo.transform.position, Quaternion.Euler(0, 0, rotz - 90));
	}
	private void SetupCircleCollider()
	{
		if (abilityRef.aoeType == SOAbilities.AoeType.isCircleAoe)
			aoeColliderIndicator.transform.localScale = new Vector2(abilityRef.circleAoeRadius * 2, abilityRef.circleAoeRadius * 2);
		else
			aoeColliderIndicator.transform.localScale = new Vector2(abilityRef.coneAoeRadius * 2, abilityRef.coneAoeRadius * 2);

		circleCollider = aoeColliderIndicator.AddComponent(typeof(CircleCollider2D)) as CircleCollider2D;
		circleCollider.isTrigger = true;
		circleCollider.includeLayers = includeLayer;
		circleCollider.excludeLayers = excludeLayer;
		circleCollider.radius = 0.1f;
		circleCollider.offset = new Vector2(0, 0);
	}
	private void SetBoxColliderDirection(Vector3 targetPosition)
	{
		Vector3 rotation = targetPosition - casterInfo.transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		transform.SetPositionAndRotation(casterInfo.transform.position, Quaternion.Euler(0, 0, rotz - 90));

		//adjustment ratio / 13.33333333
		float adjustPos = (float)(abilityRef.boxAoeSizeY / 13.33333333);
		aoeColliderIndicator.transform.localPosition = new Vector2(0, adjustPos);
	}
	private void SetupBoxCollider()
	{
		aoeColliderIndicator.transform.localScale = new Vector2(abilityRef.boxAoeSizeX, abilityRef.boxAoeSizeY);

		boxCollider = aoeColliderIndicator.AddComponent(typeof(BoxCollider2D)) as BoxCollider2D;
		boxCollider.isTrigger = true;
		boxCollider.includeLayers = includeLayer;
		boxCollider.excludeLayers = excludeLayer;
		boxCollider.size = new Vector2(0.15f, 0.15f);
		boxCollider.offset = new Vector2(0, 0);
	}

	//OnTriggerEnter2D call
	public void OnEntityEnter2D(EntityStats entity)
	{
		if (entity == null) return;

		//allows aoes that linger to still apply damage to new enemies (duration set to 0.1f for non lingering aoes)
		if (aoeLingers)
			ApplyDamageToCollidedEntity(entity, aoeDamage);
		else
			AddCollidedEntitiesToList(entity);
	}

	private void AddCollidedEntitiesToList(EntityStats entity)
	{
		if (entityStatsList.Contains(entity)) return;
		entityStatsList.Add(entity);
	}

	//timer for aoe clean up + applying damage to entities in list
	private void AbilityDurationTimer()
	{
		abilityDurationTimer -= Time.deltaTime;

		if (abilityDurationTimer <= 0)
		{
			if (debugLockDamage)
			{
				DungeonHandler.AoeAbilitiesCleanUp(this);
				return;
			}

			if (!aoeLingers)
			{
				if (abilityRef.isDamageSplitBetweenHits)
					SplitDamageBetweenCollidedEntities();
				else
					DamageAllCollidedEntities();
			}
			debugLockDamage = true;
			DungeonHandler.AoeAbilitiesCleanUp(this);
		}
	}

	//damage all entites in list
	private void DamageAllCollidedEntities()
	{
		foreach (EntityStats entity in entityStatsList)
			ApplyDamageToCollidedEntity(entity, aoeDamage);
	}

	//split damage between all entites in list
	private void SplitDamageBetweenCollidedEntities()
	{
		int damageToDeal = aoeDamage;

		if (abilityRef.isBossAbility) //noop atm
		{
			damageToDeal = (int)(damageToDeal * 0.4f); //whilst MP not implamented set to 40% damage

			///<Summery>
			///adjust damage dealt to players further based on players in lobby, ensuring boss can still be beat with less then a full lobby
			///4 players = aoeDamage * 1 | 3 players = aoeDamage * 0.75
			///2 players = aoeDamage * 0.55 | players = aoeDamage * 0.4 (subject to change)
			///</Summery>
		}

		if (entityStatsList.Count > 1)
			damageToDeal = aoeDamage / entityStatsList.Count; //split damage

		foreach (EntityStats entity in entityStatsList)
			ApplyDamageToCollidedEntity(entity, damageToDeal);
	}

	//apply damage + status effects to entity
	private void ApplyDamageToCollidedEntity(EntityStats entity, float damageToDeal)
	{
		if (abilityRef.aoeType != SOAbilities.AoeType.isCircleAoe)
			if (!CollidedTargetInLineOfSight(entity.gameObject)) return;
		else if (abilityRef.aoeType == SOAbilities.AoeType.isConeAoe)
			if (!CollidedTargetInConeAngle(entity.gameObject)) return;

		entity.GetComponent<Damageable>().OnHitFromDamageSource(player, entity.GetComponent<Collider2D>(), damageToDeal,
			(IDamagable.DamageType)damageType, 0, abilityRef.isDamagePercentageBased, isPlayerAoe, false);

		if (!abilityRef.hasStatusEffects) return;
		entity.ApplyNewStatusEffects(abilityRef.statusEffects, casterInfo);
	}

	//checks
	public bool IsCollidedObjEnemy(Collider2D other)
	{
		//status effects only apply to friendlies (add checks later to apply off effects only to enemies etc...)
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerAoe ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemies") && !isPlayerAoe)
			return false;
		else return true;
	}
	private bool CollidedTargetInLineOfSight(GameObject collidedObject)
	{
		RaycastHit2D hit = Physics2D.Linecast(casterPosition, collidedObject.transform.position, lineOfSightLayer);
		if (hit.collider == null)
			return true;
		else
			return false;
	}
	private bool CollidedTargetInConeAngle(GameObject collidedObject)
	{
		Vector3 targetDir = collidedObject.transform.position - transform.position;
		float angle = Vector3.Angle(targetDir, aoeColliderIndicator.transform.up);

		if (angle <= abilityRef.angle / 2)
		{
			Debug.LogError("inside angle: " + angle);
			return true;
		}
		else
		{
			Debug.LogError("outside angle: " + angle);
			return false;
		}
	}
}
