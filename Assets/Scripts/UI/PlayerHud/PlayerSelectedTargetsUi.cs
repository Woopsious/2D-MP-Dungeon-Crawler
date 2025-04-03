using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectedTargetsUi : MonoBehaviour
{
	public static PlayerSelectedTargetsUi Instance;

	private EntityStats selectedEnemyTarget;
	private EntityStats selectedFriendlyTarget;

	[Header("Selected Enemy Target Uis")]
	public GameObject selectedEnemyTargetPanelUi;
	public GameObject unSelectedEnemyTargetUi;
	public GameObject selectedEnemyTargetTrackerUi;
	public GameObject selectedEnemyTargetUi;

	[Header("Selected Enemy Target Ui comps")]
	public TMP_Text selectedEnemyTargetUiName;
	public Image selectedEnemyTargetUiImage;
	public Image selectedEnemyTargetHealthBarFiller;
	public TMP_Text selectedEnemyTargetHealth;
	public Image selectedEnemyTargetManaBarFiller;
	public TMP_Text selectedEnemyTargetMana;

	[Header("Selected Enemy Target Status Effects Ui")]
	public GameObject selectedEnemyTargetEffectsContentObj;

	[Header("Selected Friendly Target Uis")]
	public GameObject selectedFriendlyTargetPanelUi;
	public GameObject unSelectedFriendlyTargetUi;
	public GameObject selectedFriendlyTargetTrackerUi;
	public GameObject selectedFriendlyTargetUi;

	[Header("Selected Friendly Target Ui comps")]
	public TMP_Text selectedFriendlyTargetUiName;
	public Image selectedFriendlyTargetUiImage;
	public Image selectedFriendlyTargetHealthBarFiller;
	public TMP_Text selectedFriendlyTargetHealth;
	public Image selectedFriendlyTargetManaBarFiller;
	public TMP_Text selectedFriendlyTargetMana;

	[Header("Selected Friendly Target Status Effects Ui")]
	public GameObject selectedFriendlyTargetEffectsContentObj;

	public void Awake()
	{
		Instance = this;
		selectedEnemyTargetTrackerUi.SetActive(false);
		selectedEnemyTargetUi.SetActive(false);
		unSelectedEnemyTargetUi.SetActive(false);

		selectedFriendlyTargetTrackerUi.SetActive(false);
		selectedFriendlyTargetUi.SetActive(false);
		unSelectedFriendlyTargetUi.SetActive(false);
	}
	private void Update()
	{
		UpdateSelectedEnemyTargetTrackerUi();
		UpdateSelectedFriendlyTargetTrackerUi();
	}
	private void OnEnable()
	{
		PlayerController.OnNewTargetSelected += OnNewTargetSelected;
		ObjectPoolingManager.OnEntityDeathEvent += OnTargetDeathUnSelect;
		PlayerEventManager.OnPlayerDeathEvent += OnLocalPlayerDeath;
	}
	private void OnDisable()
	{
		PlayerController.OnNewTargetSelected -= OnNewTargetSelected;
		ObjectPoolingManager.OnEntityDeathEvent -= OnTargetDeathUnSelect;
		PlayerEventManager.OnPlayerDeathEvent -= OnLocalPlayerDeath;
	}

	//SELECTED TARGET UI
	//events
	private void OnNewTargetSelected(EntityStats entityStats)
	{
		if (!entityStats.IsPlayerEntity())
			SelectEnemyTarget(entityStats);
		else
			SelectFriendlyTarget(entityStats);
	}
	private void OnTargetDeathUnSelect(GameObject obj)
	{
		EntityStats entityStats = obj.GetComponent<EntityStats>();

		if (!entityStats.IsPlayerEntity())
			ClearSelectedEnemyTarget();
		else
			ClearSelectedFriendlyTarget();
	}
	private void OnLocalPlayerDeath(PlayerController player, string deathMessage)
	{
		if (GameManager.Localplayer != player) return;

		ClearSelectedEnemyTarget();
		ClearSelectedFriendlyTarget();
	}

	//target select types
	private void SelectEnemyTarget(EntityStats entityStats)
	{
		selectedEnemyTargetTrackerUi.SetActive(true);
		selectedEnemyTargetUi.SetActive(true);
		unSelectedEnemyTargetUi.SetActive(false);

		if (selectedEnemyTarget != null) //unsub from old target
		{
			selectedEnemyTarget.OnHealthChangeEvent -= OnEnemyTargetHealthChange;
			selectedEnemyTarget.OnManaChangeEvent -= OnEnemyTargetManaChange;
			selectedEnemyTarget.OnStatusEffectAppliedEvent -= OnEnemyTargetStatusEffectApplied;
		}

		for (int i = 0; i < selectedEnemyTargetEffectsContentObj.transform.childCount; i++)
		{
			Abilities ability = selectedEnemyTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.gameObject.SetActive(false);
		}

		selectedEnemyTarget = entityStats;
		selectedEnemyTargetUiImage.sprite = entityStats.statsRef.sprite;

		if (SelectedTargetHasUniqueName(selectedEnemyTarget.statsRef.isBossVersion, selectedEnemyTarget.statsRef.humanoidType))
			selectedEnemyTargetUiName.text = entityStats.statsRef.entityName;
		else
			selectedEnemyTargetUiName.text = entityStats.classHandler.currentEntityClass.className + " " + entityStats.statsRef.entityName;

		//new target event subs
		selectedEnemyTarget.OnHealthChangeEvent += OnEnemyTargetHealthChange;
		selectedEnemyTarget.OnManaChangeEvent += OnEnemyTargetManaChange;
		selectedEnemyTarget.OnStatusEffectAppliedEvent += OnEnemyTargetStatusEffectApplied;

		//initial setting data for ui
		OnEnemyTargetHealthChange(selectedEnemyTarget.maxHealth.finalValue, selectedEnemyTarget.currentHealth);
		OnEnemyTargetManaChange(selectedEnemyTarget.maxMana.finalValue, selectedEnemyTarget.currentMana);

		foreach (AbilityStatusEffect statusEffect in selectedEnemyTarget.currentStatusEffects)
			OnEnemyTargetStatusEffectApplied(statusEffect);
	}
	private void SelectFriendlyTarget(EntityStats entityStats)
	{
		selectedFriendlyTargetTrackerUi.SetActive(true);
		selectedFriendlyTargetUi.SetActive(true);
		unSelectedFriendlyTargetUi.SetActive(false);

		if (selectedFriendlyTarget != null) //unsub from old target
		{
			selectedFriendlyTarget.OnHealthChangeEvent -= OnFriendlyTargetHealthChange;
			selectedFriendlyTarget.OnManaChangeEvent -= OnFriendlyTargetManaChange;
			selectedFriendlyTarget.OnStatusEffectAppliedEvent -= OnFriendlyTargetStatusEffectApplied;
		}

		for (int i = 0; i < selectedFriendlyTargetEffectsContentObj.transform.childCount; i++)
		{
			Abilities ability = selectedFriendlyTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.gameObject.SetActive(false);
		}

		selectedFriendlyTarget = entityStats;
		selectedFriendlyTargetUiImage.sprite = entityStats.statsRef.sprite;

		if (SelectedTargetHasUniqueName(selectedFriendlyTarget.statsRef.isBossVersion, selectedFriendlyTarget.statsRef.humanoidType))
			selectedFriendlyTargetUiName.text = entityStats.statsRef.entityName;
		else
			selectedFriendlyTargetUiName.text = entityStats.classHandler.currentEntityClass.className + " " + entityStats.statsRef.entityName;

		//new target event subs
		selectedFriendlyTarget.OnHealthChangeEvent += OnFriendlyTargetHealthChange;
		selectedFriendlyTarget.OnManaChangeEvent += OnFriendlyTargetManaChange;
		selectedFriendlyTarget.OnStatusEffectAppliedEvent += OnFriendlyTargetStatusEffectApplied;

		//initial setting data for ui
		OnFriendlyTargetHealthChange(selectedFriendlyTarget.maxHealth.finalValue, selectedFriendlyTarget.currentHealth);
		OnFriendlyTargetManaChange(selectedFriendlyTarget.maxMana.finalValue, selectedFriendlyTarget.currentMana);

		foreach (AbilityStatusEffect statusEffect in selectedFriendlyTarget.currentStatusEffects)
			OnFriendlyTargetStatusEffectApplied(statusEffect);
	}

	//clear targets + on button click
	public void ClearSelectedEnemyTarget()
	{
		selectedEnemyTargetTrackerUi.SetActive(false);
		selectedEnemyTargetUi.SetActive(false);
		unSelectedEnemyTargetUi.SetActive(true);

		GameManager.Localplayer.ClearSelectedTarget(false);

		if (selectedEnemyTarget == null) return;

		selectedEnemyTarget.OnHealthChangeEvent -= OnEnemyTargetHealthChange;
		selectedEnemyTarget.OnManaChangeEvent -= OnEnemyTargetManaChange;
		selectedEnemyTarget.OnStatusEffectAppliedEvent -= OnEnemyTargetStatusEffectApplied;

		selectedEnemyTarget = null;
	}
	public void ClearSelectedFriendlyTarget()
	{
		selectedFriendlyTargetTrackerUi.SetActive(false);
		selectedFriendlyTargetUi.SetActive(false);
		unSelectedFriendlyTargetUi.SetActive(true);

		GameManager.Localplayer.ClearSelectedTarget(true);

		if (selectedFriendlyTarget == null) return;

		selectedFriendlyTarget.OnHealthChangeEvent -= OnFriendlyTargetHealthChange;
		selectedFriendlyTarget.OnManaChangeEvent -= OnFriendlyTargetManaChange;
		selectedFriendlyTarget.OnStatusEffectAppliedEvent -= OnFriendlyTargetStatusEffectApplied;

		selectedFriendlyTarget = null;
	}

	//bool check
	private bool SelectedTargetHasUniqueName(bool isBoss, SOEntityStats.HumanoidTypes humanoidType)
	{
		if (isBoss)
			return true;
		else if (humanoidType == SOEntityStats.HumanoidTypes.isBat || humanoidType == SOEntityStats.HumanoidTypes.isSlime)
			return true;
		else return false;
	}

	//ui updates
	private void UpdateSelectedEnemyTargetTrackerUi()
	{
		if (selectedEnemyTarget == null || !selectedEnemyTargetTrackerUi.activeInHierarchy) return;
		Vector2 position = Camera.main.WorldToScreenPoint(selectedEnemyTarget.transform.position);
		selectedEnemyTargetTrackerUi.transform.position = new Vector3(position.x, position.y + 40, 0);
	}
	private void UpdateSelectedFriendlyTargetTrackerUi()
	{
		if (selectedFriendlyTarget == null || !selectedFriendlyTargetTrackerUi.activeInHierarchy) return;
		Vector2 position = Camera.main.WorldToScreenPoint(selectedFriendlyTarget.transform.position);
		selectedFriendlyTargetTrackerUi.transform.position = new Vector3(position.x, position.y + 40, 0);
	}

	//ui enemy event updates
	private void OnEnemyTargetHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedEnemyTargetHealthBarFiller.fillAmount = percentage;
		selectedEnemyTargetHealth.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void OnEnemyTargetManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedEnemyTargetManaBarFiller.fillAmount = percentage;
		selectedEnemyTargetMana.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void OnEnemyTargetStatusEffectApplied(AbilityStatusEffect statusEffect)
	{
		bool createNewUiTimer = true;

		//check for dup effect timers, if found reset effect timer
		for (int i = 0; i < selectedEnemyTargetEffectsContentObj.transform.childCount; i++)
		{
			Abilities ability = selectedEnemyTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();

			if (selectedEnemyTargetEffectsContentObj.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				if (ability.effectBaseRef == statusEffect.GetBaseStatusEffect())
				{
					ability.ResetEffectTimer();
					createNewUiTimer = false;
				}
			}
		}

		//if none found set up new timer for said effect
		if (!createNewUiTimer) return;
		for (int i = 0; i < selectedEnemyTargetEffectsContentObj.transform.childCount; i++)
		{
			if (selectedEnemyTargetEffectsContentObj.transform.GetChild(i).gameObject.activeInHierarchy) continue;

			Abilities ability = selectedEnemyTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.InitilizeStatusEffectUiTimer(statusEffect.GetBaseStatusEffect(), statusEffect.GetAbilityDuration());
			ability.gameObject.SetActive(true);
			return;
		}
	}

	//ui friendly event updates
	private void OnFriendlyTargetHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedFriendlyTargetHealthBarFiller.fillAmount = percentage;
		selectedFriendlyTargetHealth.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void OnFriendlyTargetManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedFriendlyTargetManaBarFiller.fillAmount = percentage;
		selectedFriendlyTargetMana.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void OnFriendlyTargetStatusEffectApplied(AbilityStatusEffect statusEffect)
	{
		bool createNewUiTimer = true;

		//check for dup effect timers, if found reset effect timer
		for (int i = 0; i < selectedFriendlyTargetEffectsContentObj.transform.childCount; i++)
		{
			Abilities ability = selectedFriendlyTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();

			if (selectedFriendlyTargetEffectsContentObj.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				if (ability.effectBaseRef == statusEffect.GetBaseStatusEffect())
				{
					ability.ResetEffectTimer();
					createNewUiTimer = false;
				}
			}
		}

		//if none found set up new timer for said effect
		if (!createNewUiTimer) return;
		for (int i = 0; i < selectedFriendlyTargetEffectsContentObj.transform.childCount; i++)
		{
			if (selectedFriendlyTargetEffectsContentObj.transform.GetChild(i).gameObject.activeInHierarchy) continue;

			Abilities ability = selectedFriendlyTargetEffectsContentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.InitilizeStatusEffectUiTimer(statusEffect.GetBaseStatusEffect(), statusEffect.GetAbilityDuration());
			ability.gameObject.SetActive(true);
			return;
		}
	}
}
