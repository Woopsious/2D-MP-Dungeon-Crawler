using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityIndicators : MonoBehaviour
{
	private EntityBehaviour caster;
	private GameObject originObj;

	private bool lockTargetPosition;
	private Vector3 targetPosition;

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
		circleAoeIndicatorSprite =  circleAoeIndicator.GetComponent<SpriteRenderer>();
		coneAoeIndicatorMesh = coneAoeIndicator.GetComponent<ConeMesh>();
		boxAoeIndicatorSprite = boxAoeIndicator.GetComponent<SpriteRenderer>();

		circleAoeIndicator.SetActive(false);
		coneAoeIndicator.SetActive(false);
		boxAoeIndicator.SetActive(false);
	}
	private void Update()
	{
		if (!showIndicators) return;

		UpdateTargetPosition();

		if (indicatorType == AoeIndicatorType.isDirectional) return; //noop
		else if (indicatorType == AoeIndicatorType.isCircleAoe)
			UpdateCircleIndicator();
		else if (indicatorType == AoeIndicatorType.isConeAoe)
			UpdateConeIndicator();
		else if (indicatorType == AoeIndicatorType.isBoxAoe)
			UpdateBoxIndicator();
	}

	public void ShowAoeIndicators(SOAbilities abilityToShow, EntityBehaviour entity)
	{
		lockTargetPosition = false;
		caster = entity;
		originObj = entity.gameObject;

		if (abilityToShow.aoeType == SOAbilities.AoeType.isCircleAoe)
			SetUpCircleIndicator(abilityToShow);
		else if (abilityToShow.aoeType == SOAbilities.AoeType.isConeAoe)
			SetUpConeIndicator(abilityToShow);
		else if (abilityToShow.aoeType == SOAbilities.AoeType.isBoxAoe)
			SetUpBoxIndicator(abilityToShow);

		showIndicators = true;
	}
	public void ShowAoeIndicators(SOAbilities abilityToShow, EntityBehaviour entity, Vector3 targetPosition)
	{
		lockTargetPosition = true;
		caster = entity;
		this.targetPosition = targetPosition;

		if (abilityToShow.aoeType == SOAbilities.AoeType.isCircleAoe)
			SetUpCircleIndicator(abilityToShow);
		else if (abilityToShow.aoeType == SOAbilities.AoeType.isConeAoe)
			SetUpConeIndicator(abilityToShow);
		else if (abilityToShow.aoeType == SOAbilities.AoeType.isBoxAoe)
			SetUpBoxIndicator(abilityToShow);

		showIndicators = true;
	}
	public void HideAoeIndicators()
	{
		showIndicators = false;
		circleAoeIndicator.SetActive(false);
		coneAoeIndicator.SetActive(false);
		boxAoeIndicator.SetActive(false);
	}

	//set up indicators
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

		coneAoeIndicator.SetActive(true);
		coneAoeIndicatorMesh.CreateConeMesh(abilityRef.angle, abilityRef.coneAoeRadius / 5f, false);
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

	//update indicators positions + rotations
	private void UpdateCircleIndicator()
	{
		circleAoeIndicator.transform.position = targetPosition;
	}
	private void UpdateConeIndicator()
	{
		Vector3 rotation = targetPosition - originObj.transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg - (coneAoeIndicatorMesh.angle / 2);
		transform.rotation = Quaternion.Euler(0, 0, rotz);

		//adjustment ratio / 13.33333333 (not pixel perfect)
		float adjustPos = (float)(coneAoeIndicator.transform.localScale.y / 13.33333333);
		coneAoeIndicator.transform.localPosition = new Vector2(0, adjustPos);
	}
	private void UpdateBoxIndicator()
	{
		Vector3 rotation = targetPosition - originObj.transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, rotz - 90);

		//adjustment ratio / 13.33333333 (not pixel perfect)
		float adjustPos = (float)(boxAoeIndicator.transform.localScale.y / 13.33333333);
		boxAoeIndicator.transform.localPosition = new Vector2(0, adjustPos);
	}

	//work out where the indicator should be
	private void UpdateTargetPosition()
	{
		if (lockTargetPosition) return;

		if (caster.overriddenPlayerTarget != null)
			targetPosition = caster.overriddenPlayerTarget.gameObject.transform.position;
		else if (caster.playerTarget != null)
			targetPosition = caster.playerTarget.gameObject.transform.position;
	}
}
