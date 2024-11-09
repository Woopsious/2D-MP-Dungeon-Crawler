using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;

public class AbilityAOE : MonoBehaviour
{
	private PlayerController player;
	public SOAbilities abilityRef;
	private EntityStats casterInfo;
	private Vector2 casterPosition;

	public GameObject aoeColliderIndicator;
	public LayerMask includeLayer;
	public LayerMask excludeLayer;

	public LayerMask lineOfSightLayer;

	public float abilityDurationTimer;
	private CircleCollider2D circleCollider;
	private BoxCollider2D boxCollider;
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

	//set data
	public void Initilize(SOAbilities abilityRef, EntityStats casterInfo, Vector2 targetPosition)
	{
		this.abilityRef = abilityRef;
		this.casterInfo = casterInfo;
		casterPosition = casterInfo.transform.position;
		gameObject.name = abilityRef.Name + "Aoe";
		aoeColliderIndicator.GetComponent<SpriteRenderer>().sprite = abilityRef.abilitySprite;
		aoeColliderIndicator.transform.localPosition = Vector3.zero;
		isPlayerAoe = casterInfo.IsPlayerEntity();

		if (abilityRef.isCircleAOE)
		{
			SetCircleColliderPosition(targetPosition);
			SetupCircleCollider();
		}
		else
		{
			SetBoxColliderDirection(targetPosition);
			SetupBoxCollider();
		}

		SetDamage();

		abilityDurationTimer = abilityRef.aoeDuration;
		if (abilityRef.aoeDuration == 0)
			abilityDurationTimer = 0.1f;

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

	//set up colliders + transforms
	private void SetCircleColliderPosition(Vector2 targetPosition)
	{
		transform.position = targetPosition;
	}
	private void SetupCircleCollider()
	{
		aoeColliderIndicator.transform.localScale = new Vector2(abilityRef.circleAoeSize, abilityRef.circleAoeSize);

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
		float adjustPos = (float)(abilityRef.boxAoeSizeY / 13.333333);
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

	//optional, helps with applying damage only to enemies
	public void AddPlayerRef(PlayerController player)
	{
		this.player = player;
	}

	public void OnEntityEnter2D(EntityStats entity, Collider2D other)
	{
		if (entity == null) return;

		if (abilityRef.isDamageSplitBetweenHits)
			AddCollidedEntitiesToList(entity);
		else
			ApplyDamageToCollidedEntities(entity, other, aoeDamage);
	}

	//add collided entity to list. to later apply damage to them.
	private void AddCollidedEntitiesToList(EntityStats entity)
	{
		if (entityStatsList.Contains(entity)) return;
		entityStatsList.Add(entity);
	}

	//applying damage to collided things + status effects if it was an entity
	private void ApplyDamageToCollidedEntities(EntityStats entity, Collider2D other, int damageToDeal)
	{
		if (!abilityRef.isCircleAOE)
			if (!CollidedTargetInLineOfSight(other.gameObject)) return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, damageToDeal, (IDamagable.DamageType)damageType, 0, 
			abilityRef.isDamagePercentageBased, isPlayerAoe, false);

		if (!abilityRef.hasStatusEffects) return;
		entity.ApplyNewStatusEffects(abilityRef.statusEffects, casterInfo);
	}

	//split damage, called on duration timer end
	private void SplitDamageBetweenCollidedTargets()
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

		damageToDeal = aoeDamage / entityStatsList.Count; //split damage

		foreach (EntityStats entity in  entityStatsList)
			ApplyDamageToCollidedEntities(entity, entity.GetComponent<Collider2D>(), damageToDeal);
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

	//timer
	private void AbilityDurationTimer()
	{
		abilityDurationTimer -= Time.deltaTime;

		if (abilityDurationTimer <= 0)
		{
			if (abilityRef.isDamageSplitBetweenHits)
				SplitDamageBetweenCollidedTargets();

			DungeonHandler.AoeAbilitiesCleanUp(this);
		}
	}
}
