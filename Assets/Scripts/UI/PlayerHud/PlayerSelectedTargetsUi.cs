using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectedTargetsUi : MonoBehaviour
{
	public static PlayerSelectedTargetsUi Instance;

	public GameObject selectedTargetPanelUi;
	private EntityStats selectedTarget;

	[Header("Selected Target Uis")]
	public GameObject selectedTargetTrackerUi;
	public GameObject unSelectedTargetUi;
	public GameObject selectedTargetUi;

	[Header("Selected Target Ui comps")]
	public TMP_Text selectedTargetUiName;
	public Image selectedTargetUiImage;
	public Image selectedTargetHealthBarFiller;
	public TMP_Text selectedTargetHealth;
	public Image selectedTargetManaBarFiller;
	public TMP_Text selectedTargetMana;

	[Header("Selected Target Status Effects Ui")]
	public GameObject selectedTargetEffectsContentObj;

	public void Awake()
	{
		Instance = this;
		selectedTargetTrackerUi.SetActive(false);
		unSelectedTargetUi.SetActive(false);
		selectedTargetUi.SetActive(false);
	}
	private void Update()
	{
		UpdateSelectedTargetTrackerUi();
	}
	private void OnEnable()
	{
		ObjectPoolingManager.OnEntityDeathEvent += OnTargetDeathUnSelect;
	}
	private void OnDisable()
	{
		ObjectPoolingManager.OnEntityDeathEvent -= OnTargetDeathUnSelect;
	}

	//SELECTED TARGET UI
	//events
	public void OnNewTargetSelected(EntityStats entityStats)
	{
		selectedTargetTrackerUi.SetActive(true);
		selectedTargetUi.SetActive(true);
		unSelectedTargetUi.SetActive(false);

		if (selectedTarget != null) //unsub from old target
		{
			selectedTarget.OnHealthChangeEvent -= OnTargetHealthChange;
			selectedTarget.OnManaChangeEvent -= OnTargetManaChange;
			selectedTarget.OnNewStatusEffect -= OnNewStatusEffectsForSelectedTarget;
			selectedTarget.OnResetStatusEffectTimer -= OnResetStatusEffectTimerForSelectedTarget;
		}

		for (int i = 0; i < selectedTargetEffectsContentObj.transform.childCount; i++)
		{
			Abilities ability = selectedTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.gameObject.SetActive(false);
		}

		selectedTarget = entityStats;
		selectedTargetUiImage.sprite = entityStats.statsRef.sprite;

		string targetName;
		if (selectedTarget.statsRef.canUseEquipment)
			targetName = entityStats.classHandler.currentEntityClass.className + " " + entityStats.statsRef.entityName;
		else
			targetName = entityStats.statsRef.entityName;
		selectedTargetUiName.text = targetName;

		//new target event subs
		selectedTarget.OnHealthChangeEvent += OnTargetHealthChange;
		selectedTarget.OnManaChangeEvent += OnTargetManaChange;
		selectedTarget.OnNewStatusEffect += OnNewStatusEffectsForSelectedTarget;
		selectedTarget.OnResetStatusEffectTimer += OnResetStatusEffectTimerForSelectedTarget;

		//initial setting data for ui
		OnTargetHealthChange(selectedTarget.maxHealth.finalValue, selectedTarget.currentHealth);
		OnTargetManaChange(selectedTarget.maxMana.finalValue, selectedTarget.currentMana);

		foreach (AbilityStatusEffect statusEffect in selectedTarget.currentStatusEffects)
			OnNewStatusEffectsForSelectedTarget(statusEffect);
	}
	private void OnTargetDeathUnSelect(GameObject obj)
	{
		ClearSelectedTarget();
	}
	public void ClearSelectedTarget()
	{
		selectedTargetTrackerUi.SetActive(false);
		selectedTargetUi.SetActive(false);
		unSelectedTargetUi.SetActive(true);
		GameManager.Localplayer.ClearSelectedTarget();

		if (selectedTarget == null) return;

		selectedTarget.OnHealthChangeEvent -= OnTargetHealthChange;
		selectedTarget.OnManaChangeEvent -= OnTargetManaChange;
		selectedTarget.OnNewStatusEffect -= OnNewStatusEffectsForSelectedTarget;
		selectedTarget.OnResetStatusEffectTimer -= OnResetStatusEffectTimerForSelectedTarget;

		selectedTarget = null;
	}

	//ui updates
	private void UpdateSelectedTargetTrackerUi()
	{
		if (selectedTarget == null || !selectedTargetTrackerUi.activeInHierarchy) return;
		Vector2 position = Camera.main.WorldToScreenPoint(selectedTarget.transform.position);
		selectedTargetTrackerUi.transform.position = new Vector3(position.x, position.y + 40, 0);
	}

	//ui event updates
	private void OnTargetHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedTargetHealthBarFiller.fillAmount = percentage;
		selectedTargetHealth.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void OnTargetManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedTargetManaBarFiller.fillAmount = percentage;
		selectedTargetMana.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void OnNewStatusEffectsForSelectedTarget(AbilityStatusEffect statusEffect)
	{
		for (int i = 0; i < selectedTargetEffectsContentObj.transform.childCount; i++)
		{
			if (!selectedTargetEffectsContentObj.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				Abilities ability = selectedTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
				ability.InitilizeStatusEffectUiTimer(statusEffect.GrabAbilityBaseRef(), statusEffect.GetAbilityDuration());
				ability.gameObject.SetActive(true);
				return;
			}
		}
	}
	private void OnResetStatusEffectTimerForSelectedTarget(SOStatusEffects effect)
	{
		for (int i = 0; i < selectedTargetEffectsContentObj.transform.childCount; i++)
		{
			if (!selectedTargetEffectsContentObj.transform.GetChild(i).gameObject.activeInHierarchy)
				continue;

			Abilities ability = selectedTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
			if (ability.effectBaseRef != effect)
				continue;
			else
			{
				ability.ResetEffectTimer();
				return;
			}
		}
	}
}
