using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityIndicators : MonoBehaviour
{
	private PlayerController player;
	private bool showIndicators;

	private AoeIndicatorType indicatorType;
	private enum AoeIndicatorType
	{
		isDirectional, isCircleAoe, isConeAoe, isBoxAoe
	}

	[Header("Directional")]
	public GameObject directionalIndicator;

	[Header("Circle Aoe")]
	public GameObject circleAoeIndicator;
	private SpriteRenderer circleAoeIndicatorSprite;

	[Header("Cone Aoe")]
	public GameObject coneAoeIndicator;
	private ConeMesh coneAoeIndicatorMesh;

	[Header("Box Aoe")]
	public GameObject boxAoeIndicator;
	private SpriteRenderer boxAoeIndicatorSprite;

	private void Awake()
	{
		player = GetComponentInParent<PlayerController>();
		circleAoeIndicatorSprite =  circleAoeIndicator.GetComponent<SpriteRenderer>();
		coneAoeIndicatorMesh = coneAoeIndicator.GetComponent<ConeMesh>();
		boxAoeIndicatorSprite = boxAoeIndicator.GetComponent<SpriteRenderer>();
	}

	private void OnEnable()
	{
		PlayerController.OnPlayerUseAbility += PlayerQueueAbility;
		PlayerController.OnPlayerCastAbility += PlayerCastAbility;
		PlayerController.OnPlayerCancelAbility += PlayerCancelAbility;
	}
	private void OnDisable()
	{
		PlayerController.OnPlayerUseAbility -= PlayerQueueAbility;
		PlayerController.OnPlayerCastAbility -= PlayerCastAbility;
		PlayerController.OnPlayerCancelAbility -= PlayerCancelAbility;
	}

	private void Update()
	{
		if (!showIndicators) return;

		if (indicatorType == AoeIndicatorType.isDirectional) return; //noop
		else if (indicatorType == AoeIndicatorType.isCircleAoe)
			UpdateCircleIndicator();
		else if (indicatorType == AoeIndicatorType.isConeAoe)
			UpdateConeIndicator();
		else if (indicatorType == AoeIndicatorType.isBoxAoe)
			UpdateBoxIndicator();
	}

	private void PlayerQueueAbility(Abilities ability)
	{
		if (ability.abilityBaseRef.isProjectile) return;
		else
		{
			if (ability.abilityBaseRef.aoeType == SOAbilities.AoeType.isCircleAoe)
				SetUpCircleIndicator(ability.abilityBaseRef);
			else if (ability.abilityBaseRef.aoeType == SOAbilities.AoeType.isConeAoe)
				SetUpConeIndicator(ability.abilityBaseRef);
			else if (ability.abilityBaseRef.aoeType == SOAbilities.AoeType.isBoxAoe)
				SetUpBoxIndicator(ability.abilityBaseRef);
		}
		showIndicators = true;
	}
	private void SetUpCircleIndicator(SOAbilities abilityRef)
	{
		indicatorType = AoeIndicatorType.isCircleAoe;
		transform.rotation = Quaternion.Euler(0, 0, 0);
		circleAoeIndicatorSprite.sprite = abilityRef.abilitySprite;

		Vector3 scale = new(abilityRef.circleAoeRadius * 2, abilityRef.circleAoeRadius * 2, 0);
		circleAoeIndicator.transform.localScale = scale;
		circleAoeIndicator.SetActive(true);
	}
	private void SetUpConeIndicator(SOAbilities abilityRef)
	{
		indicatorType = AoeIndicatorType.isConeAoe;
		transform.rotation = Quaternion.Euler(0, 0, 0);
		coneAoeIndicatorMesh.CreateConeMesh(abilityRef.angle, abilityRef.coneAoeRadius);

		//Vector3 scale = new(abilityRef.circleAoeRadius * 2, abilityRef.circleAoeRadius * 2, 0);
		//coneAoeIndicator.transform.localScale = scale;
		coneAoeIndicator.SetActive(true);
	}
	private void SetUpBoxIndicator(SOAbilities abilityRef)
	{
		indicatorType = AoeIndicatorType.isBoxAoe;
		transform.rotation = Quaternion.Euler(0, 0, 0);
		boxAoeIndicatorSprite.sprite = abilityRef.abilitySprite;

		Vector3 scale = new(abilityRef.boxAoeSizeX, abilityRef.boxAoeSizeY, 0);
		boxAoeIndicator.transform.localScale = scale;
		boxAoeIndicator.SetActive(true);
	}

	private void PlayerCastAbility()
	{
		showIndicators = false;
		//directionalIndicator.SetActive(false);
		circleAoeIndicator.SetActive(false);
		coneAoeIndicator.SetActive(false);
		boxAoeIndicator.SetActive(false);
	}
	private void PlayerCancelAbility()
	{
		showIndicators = false;
		//directionalIndicator.SetActive(false);
		circleAoeIndicator.SetActive(false);
		coneAoeIndicator.SetActive(false);
		boxAoeIndicator.SetActive(false);
	}

	private void UpdateCircleIndicator()
	{
		Vector2 movePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		circleAoeIndicator.transform.position = movePos;
	}
	private void UpdateConeIndicator()
	{
		Vector3 rotation = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg - (coneAoeIndicatorMesh.angle / 2);
		transform.rotation = Quaternion.Euler(0, 0, rotz);

		//adjustment ratio / 13.33333333 (not pixel perfect)
		float adjustPos = (float)(coneAoeIndicator.transform.localScale.y / 13.33333333);
		coneAoeIndicator.transform.localPosition = new Vector2(0, adjustPos);
	}
	private void UpdateBoxIndicator()
	{
		Vector3 rotation = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, rotz - 90);

		//adjustment ratio / 13.33333333 (not pixel perfect)
		float adjustPos = (float)(boxAoeIndicator.transform.localScale.y / 13.33333333);
		boxAoeIndicator.transform.localPosition = new Vector2(0, adjustPos);
	}
}
