using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHotbarUi : MonoBehaviour
{
	public static PlayerHotbarUi Instance;

	public GameObject statusEffectUiPrefab;

	public TMP_Text playerLevelInfoText;
	public TMP_Text playerClassInfoText;
	public TMP_Text goldAmountText;

	public GameObject HotbarPanelUi;

	[Header("Hotbar Consumables")]
	public List<GameObject> ConsumableSlots = new List<GameObject>();
	public GameObject consumableSlotOne;
	public GameObject consumableSlotTwo;

	[Header("Equipped Consumables")]
	public Consumables equippedConsumableOne;
	public Consumables equippedConsumableTwo;

	[Header("Hotbar Abilities")]
	public List<GameObject> AbilitySlots = new List<GameObject>();
	public GameObject abilitySlotOne;
	public GameObject abilitySlotTwo;
	public GameObject abilitySlotThree;
	public GameObject abilitySlotFour;
	public GameObject abilitySlotFive;

	[Header("Equipped Abilities")]
	public Abilities equippedAbilityOne;
	public Abilities equippedAbilityTwo;
	public Abilities equippedAbilityThree;
	public Abilities equippedAbilityFour;
	public Abilities equippedAbilityFive;

	[Header("Ability Indicators")]
	private Vector3 playerScreenPosition;
	public GameObject queuedAbilityUi;
	public GameObject queuedAbilityIndicatorUi;
	public TMP_Text queuedAbilityTextInfo;
	public Abilities queuedAbility;

	private AoeIndicatorType indicatorType;
	private enum AoeIndicatorType
	{
		isDirectional, isCircleAoe, isConeAoe, isBoxAoe
	}
	[Header("Circle Indicator")]
	public GameObject circleIndicatorUi;
	private Image circleIndicatorImage;

	[Header("Cone Indicator")]
	public GameObject coneIndicatorUi;
	private ConeMesh coneMeshIndicator;

	[Header("Box Indicator")]
	public GameObject boxIndicatorUi;
	private Image boxIndicatorImage;

	[Header("Player Status Effects Ui")]
	public GameObject playerStatusEffectsParentObj;

	[Header("Selected Target Ui")]
	public GameObject unSelectedTargetUi;
	public GameObject selectedTargetUi;
	public GameObject selectedTargetTrackerUi;
	public TMP_Text selectedTargetUiName;
	public Image selectedTargetUiImage;
	public Image selectedTargetHealthBarFiller;
	public TMP_Text selectedTargetHealth;
	public Image selectedTargetManaBarFiller;
	public TMP_Text selectedTargetMana;
	public EntityStats selectedTarget;

	[Header("Selected Target Status Effects Ui")]
	public GameObject selectedTargetStatusEffectsParentObj;

	[Header("ExpBar")]
	public Image expBarFiller;
	public TMP_Text expBarText;

	[Header("HealthBar")]
	public Image healthBarFiller;
	public TMP_Text HealthBarText;

	[Header("ManaBar")]
	public Image manaBarFiller;
	public TMP_Text manaBarText;

	public void Awake()
	{
		Instance = this;
		Initilize();
	}
	private void Update()
	{
		UpdateAbilityIndicators();
		UpdateSelectedTargetTrackerUi();
	}
	private void OnEnable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent += UpdatePlayerLevelInfo;
		PlayerEventManager.OnGoldAmountChange += UpdatePlayerGoldAmount;
		PlayerEventManager.OnPlayerExpChangeEvent += UpdatePlayerExpBar;
		PlayerEventManager.OnPlayerHealthChangeEvent += UpdatePlayerHealthBar;
		PlayerEventManager.OnPlayerManaChangeEvent += UpdatePlayerManaBar;

		PlayerController.OnPlayerUseAbility += PlayerQueueAbility;
		PlayerController.OnPlayerCastAbility += PlayerCastAbility;
		PlayerController.OnPlayerCancelAbility += PlayerCancelAbility;

		PlayerClassesUi.OnClassChanges += UpdatePlayerClassInfo;
		PlayerClassesUi.OnRefundAbilityUnlock += OnAbilityRefund;
		InventorySlotDataUi.OnHotbarItemEquip += EquipHotbarItem;
		DungeonHandler.OnEntityDeathEvent += OnTargetDeathUnSelect;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdatePlayerLevelInfo;
		PlayerEventManager.OnGoldAmountChange -= UpdatePlayerGoldAmount;
		PlayerEventManager.OnPlayerExpChangeEvent -= UpdatePlayerExpBar;
		PlayerEventManager.OnPlayerHealthChangeEvent -= UpdatePlayerHealthBar;
		PlayerEventManager.OnPlayerManaChangeEvent -= UpdatePlayerManaBar;

		PlayerController.OnPlayerUseAbility -= PlayerQueueAbility;
		PlayerController.OnPlayerCastAbility -= PlayerCastAbility;
		PlayerController.OnPlayerCancelAbility -= PlayerCancelAbility;

		PlayerClassesUi.OnClassChanges -= UpdatePlayerClassInfo;
		PlayerClassesUi.OnRefundAbilityUnlock -= OnAbilityRefund;
		InventorySlotDataUi.OnHotbarItemEquip -= EquipHotbarItem;
		DungeonHandler.OnEntityDeathEvent -= OnTargetDeathUnSelect;
	}
	private void Initilize()
	{
		foreach (GameObject slot in ConsumableSlots)
			slot.GetComponent<InventorySlotDataUi>().SetSlotIndex();

		foreach (GameObject slot in AbilitySlots)
			slot.GetComponent<InventorySlotDataUi>().SetSlotIndex();

		circleIndicatorImage = circleIndicatorUi.GetComponent<Image>();
		coneMeshIndicator = coneIndicatorUi.GetComponent<ConeMesh>();
		boxIndicatorImage = boxIndicatorUi.GetComponent<Image>();

		selectedTargetUi.SetActive(false);
		queuedAbilityUi.SetActive(false);
		queuedAbilityTextInfo.gameObject.SetActive(false);
		queuedAbilityIndicatorUi.SetActive(false);
		circleIndicatorUi.SetActive(false);
		coneIndicatorUi.SetActive(false);
		boxIndicatorUi.SetActive(false);
		playerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

		for (int i = 0; i < playerStatusEffectsParentObj.transform.childCount; i++)
		{
			Abilities ability = playerStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.gameObject.SetActive(false);
		}
	}

	//Equipping/Unequipping Consumables/Abilities to hotbar Ui slots events
	private void EquipHotbarItem(InventoryItemUi item, InventorySlotDataUi slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);

		else if (item.itemType == InventoryItemUi.ItemType.isConsumable)
			EquipConsumables(item.GetComponent<Consumables>(), slot);
		else if (item.itemType == InventoryItemUi.ItemType.isAbility)
			EquipAbility(item.abilityBaseRef, slot);
	}
	private void EquipConsumables(Consumables consumableToEquip, InventorySlotDataUi slotEquippedTo)
	{
		if (slotEquippedTo.slotIndex == 0)
			equippedConsumableOne = consumableToEquip;
		else if (slotEquippedTo.slotIndex == 1)
			equippedConsumableTwo = consumableToEquip;

		slotEquippedTo.itemInSlot.CheckIfCanUseConsumables(0, 0);
	}
	private void EquipAbility(SOAbilities newAbility, InventorySlotDataUi slotToEquipTo)
	{
		if (slotToEquipTo.itemInSlot != null)
			Destroy(slotToEquipTo.itemInSlot.gameObject);

		GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItemUi item = go.GetComponent<InventoryItemUi>();
		PlayerInventoryUi.Instance.SetAbilityData(item, newAbility);

		item.inventorySlotIndex = slotToEquipTo.slotIndex;
		item.transform.SetParent(slotToEquipTo.transform);
		slotToEquipTo.itemInSlot = item;
		slotToEquipTo.UpdateSlotSize();
		item.Initilize();
		item.UpdateCanCastAbility(0,0);

		if (slotToEquipTo.slotIndex == 0)
			equippedAbilityOne = item.GetComponent<Abilities>();
		else if (slotToEquipTo.slotIndex == 1)
			equippedAbilityTwo = item.GetComponent<Abilities>();
		else if (slotToEquipTo.slotIndex == 2)
			equippedAbilityThree = item.GetComponent<Abilities>();
		else if (slotToEquipTo.slotIndex == 3)
			equippedAbilityFour = item.GetComponent<Abilities>();
		else if (slotToEquipTo.slotIndex == 4)
			equippedAbilityFive = item.GetComponent<Abilities>();
	}
	private void HandleEmptySlots(InventorySlotDataUi slot)
	{
		if (slot.slotType == InventorySlotDataUi.SlotType.consumables)
		{
			if (slot.slotIndex == 0)
				equippedConsumableOne = null;
			if (slot.slotIndex == 1)
				equippedConsumableTwo = null;
		}
		if (slot.slotType == InventorySlotDataUi.SlotType.equippedAbilities)
		{
			if (slot.slotIndex == 0)
				equippedAbilityOne = null;
			else if (slot.slotIndex == 1)
				equippedAbilityTwo = null;
			else if (slot.slotIndex == 2)
				equippedAbilityThree = null;
			else if (slot.slotIndex == 3)
				equippedAbilityFour = null;
			else if (slot.slotIndex == 4)
				equippedAbilityFive = null;
		}
	}
	public bool IsAbilityAlreadyEquipped(SOAbilities newAbility)
	{
		if (equippedAbilityOne != null && equippedAbilityOne.abilityBaseRef == newAbility)
			return true;

		if (equippedAbilityTwo != null && equippedAbilityTwo.abilityBaseRef == newAbility)
			return true;

		if (equippedAbilityThree != null && equippedAbilityThree.abilityBaseRef == newAbility)
			return true;

		if (equippedAbilityFour != null && equippedAbilityFour.abilityBaseRef == newAbility)
			return true;

		if (equippedAbilityFive != null && equippedAbilityFive.abilityBaseRef == newAbility)
			return true;

       return false;
    }
	private void OnAbilityRefund(SOAbilities ability)
	{
		foreach (GameObject abilitySlot in AbilitySlots)
		{
			InventorySlotDataUi slotData = abilitySlot.GetComponent<InventorySlotDataUi>();
			if (slotData.itemInSlot == null)
				continue;

			if (slotData.itemInSlot.abilityBaseRef == ability)
			{
				Destroy(slotData.itemInSlot.gameObject);
				slotData.RemoveItemFromSlot();
			}
		}
	}

	//PLAYER UI UPDATES
	//events
	private void UpdatePlayerLevelInfo(EntityStats playerStats)
	{
		playerLevelInfoText.text = $"Level {playerStats.entityLevel}";
	}
	private void UpdatePlayerClassInfo(SOClasses newClass)
	{
		playerClassInfoText.text = newClass.className;
	}
	private void UpdatePlayerGoldAmount(int amount)
	{
		goldAmountText.text = $"Gold: {amount}";
	}
	private void UpdatePlayerExpBar(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		expBarFiller.fillAmount = percentage;
		expBarText.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void UpdatePlayerHealthBar(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		healthBarFiller.fillAmount = percentage;
		HealthBarText.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	private void UpdatePlayerManaBar(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		manaBarFiller.fillAmount = percentage;
		manaBarText.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	public void OnNewStatusEffectsForPlayer(AbilityStatusEffect statusEffect)
	{
		for (int i = 0; i < playerStatusEffectsParentObj.transform.childCount; i++)
		{
			if (!playerStatusEffectsParentObj.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				Abilities ability = playerStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
				ability.InitilizeStatusEffectUiTimer(statusEffect.GrabAbilityBaseRef(), statusEffect.GetTimer());
				ability.gameObject.SetActive(true);
				return;
			}
		}
	}
	public void OnResetStatusEffectTimerForPlayer(SOStatusEffects effect)
	{
		for (int i = 0; i < playerStatusEffectsParentObj.transform.childCount; i++)
		{
			if (!playerStatusEffectsParentObj.transform.GetChild(i).gameObject.activeInHierarchy)
				continue;

			Abilities ability = playerStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
			if (ability.effectBaseRef != effect)
				continue;
			else
			{
				ability.ResetEffectTimer();
				return;
			}
		}
	}

	//PLAYER ABILITIES UI
	//events
	public void PlayerQueueAbility(Abilities ability)
	{
		queuedAbility = ability;
		queuedAbilityIndicatorUi.transform.SetPositionAndRotation(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
		queuedAbilityUi.SetActive(true);
		queuedAbilityTextInfo.gameObject.SetActive(true);

		if (ability.abilityBaseRef.isProjectile)
		{
			queuedAbilityTextInfo.text = "L Click to Fire\nR Click to Cancel";
		}
		else if (ability.abilityBaseRef.isAOE)
		{
			queuedAbilityIndicatorUi.SetActive(true);
			queuedAbilityTextInfo.text = "L Click Place\nR Click to Cancel";

			if (ability.abilityBaseRef.aoeType == SOAbilities.AoeType.isCircleAoe)
				SetUpCircleIndicator(ability.abilityBaseRef);
			else if (ability.abilityBaseRef.aoeType == SOAbilities.AoeType.isConeAoe)
				SetUpConeIndicator(ability.abilityBaseRef);
			else if (ability.abilityBaseRef.aoeType == SOAbilities.AoeType.isBoxAoe)
				SetUpBoxIndicator(ability.abilityBaseRef);
		}
		else
		{
			if (ability.abilityBaseRef.isOffensiveAbility)
				queuedAbilityTextInfo.text = "L Click on Enemy\nR Click to Cancel";
			else
				queuedAbilityTextInfo.text = "L Click on Friendly\nR Click to Cancel";
		}
	}
	public void PlayerCastAbility()
	{
		queuedAbilityUi.SetActive(false);
		queuedAbilityTextInfo.gameObject.SetActive(false);
		queuedAbilityIndicatorUi.SetActive(false);
		circleIndicatorUi.SetActive(false);
		coneIndicatorUi.SetActive(false);
		boxIndicatorUi.SetActive(false);
		queuedAbility = null;
	}
	public void PlayerCancelAbility()
	{
		queuedAbilityUi.SetActive(false);
		queuedAbilityTextInfo.gameObject.SetActive(false);
		queuedAbilityIndicatorUi.SetActive(false);
		circleIndicatorUi.SetActive(false);
		coneIndicatorUi.SetActive(false);
		boxIndicatorUi.SetActive(false);
		queuedAbility = null;
	}

	private void SetUpCircleIndicator(SOAbilities abilityRef)
	{
		indicatorType = AoeIndicatorType.isCircleAoe;
		circleIndicatorImage.sprite = abilityRef.abilitySprite;

		Vector3 scale = new(abilityRef.circleAoeRadius * 18, abilityRef.circleAoeRadius * 18, 0);
		circleIndicatorUi.transform.localScale = scale;
		circleIndicatorUi.SetActive(true);
	}
	private void SetUpConeIndicator(SOAbilities abilityRef)
	{
		indicatorType = AoeIndicatorType.isConeAoe;
		coneMeshIndicator.CreateConeMesh(abilityRef.angle, abilityRef.coneAoeRadius * 9, true);
		coneIndicatorUi.SetActive(true);
	}
	private void SetUpBoxIndicator(SOAbilities abilityRef)
	{
		indicatorType = AoeIndicatorType.isBoxAoe;
		boxIndicatorImage.sprite = abilityRef.abilitySprite;

		Vector3 scale = new(abilityRef.boxAoeSizeX * 9, abilityRef.boxAoeSizeY * 9, 0);
		boxIndicatorUi.transform.localScale = scale;
		boxIndicatorUi.SetActive(true);
	}

	//updated ui position of indicators
	private void UpdateAbilityIndicators()
	{
		if (!queuedAbilityTextInfo.gameObject.activeInHierarchy) return;
			
		queuedAbilityTextInfo.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y - 60);

		if (!queuedAbilityIndicatorUi.activeInHierarchy) return;

		if (indicatorType == AoeIndicatorType.isCircleAoe)
			UpdateCircleIndicator();
		else if (indicatorType == AoeIndicatorType.isConeAoe)
			UpdateConeIndicator();
		else if (indicatorType == AoeIndicatorType.isBoxAoe)
			UpdateBoxIndicator();
	}
	private void UpdateCircleIndicator()
	{
		Vector2 movePos = Input.mousePosition;
		queuedAbilityIndicatorUi.transform.position = movePos;
	}
	private void UpdateConeIndicator()
	{
		Vector3 rotation = Input.mousePosition - playerScreenPosition;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg - (coneMeshIndicator.angle / 2);
		queuedAbilityIndicatorUi.transform.SetPositionAndRotation(playerScreenPosition, Quaternion.Euler(0, 0, rotz));

		float adjustPos = (float)(coneIndicatorUi.transform.localScale.y / 2);
		coneIndicatorUi.transform.localPosition = new Vector2(0, adjustPos);
	}
	private void UpdateBoxIndicator()
	{
		Vector3 rotation = Input.mousePosition - playerScreenPosition;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		queuedAbilityIndicatorUi.transform.SetPositionAndRotation(playerScreenPosition, Quaternion.Euler(0, 0, rotz - 90));

		float adjustPos = (float)(boxIndicatorUi.transform.localScale.y / 2);
		boxIndicatorUi.transform.localPosition = new Vector2(0, adjustPos);
	}

	//SELECTED TARGET UI
	//events
	public void OnNewTargetSelected(EntityStats entityStats)
	{
		if (!selectedTargetUi.activeInHierarchy)
			selectedTargetUi.SetActive(true);
		if (unSelectedTargetUi.activeInHierarchy)
			unSelectedTargetUi.SetActive(false);

		if (selectedTarget != null) //unsub from old target
		{
			selectedTarget.OnHealthChangeEvent -= OnTargetHealthChange;
			selectedTarget.OnManaChangeEvent -= OnTargetManaChange;
			selectedTarget.OnNewStatusEffect -= OnNewStatusEffectsForSelectedTarget;
			selectedTarget.OnResetStatusEffectTimer -= OnResetStatusEffectTimerForSelectedTarget;
		}

		for (int i = 0; i < selectedTargetStatusEffectsParentObj.transform.childCount; i++)
		{
			Abilities ability = selectedTargetStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
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
	public void ClearSelectedTarget() //called via event + button in ui
	{
		if (selectedTarget == null) return;

		if (selectedTargetUi.activeInHierarchy)
			selectedTargetUi.SetActive(false);
		if (!unSelectedTargetUi.activeInHierarchy)
			unSelectedTargetUi.SetActive(true);

		selectedTarget.OnHealthChangeEvent -= OnTargetHealthChange;
		selectedTarget.OnManaChangeEvent -= OnTargetManaChange;
		selectedTarget.OnNewStatusEffect -= OnNewStatusEffectsForSelectedTarget;
		selectedTarget.OnResetStatusEffectTimer -= OnResetStatusEffectTimerForSelectedTarget;

		PlayerInfoUi.playerInstance.ClearSelectedTarget();
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
		for (int i = 0; i < selectedTargetStatusEffectsParentObj.transform.childCount; i++)
		{
			if (!selectedTargetStatusEffectsParentObj.transform.GetChild(i).gameObject.activeInHierarchy)
			{
				Abilities ability = selectedTargetStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
				ability.InitilizeStatusEffectUiTimer(statusEffect.GrabAbilityBaseRef(), statusEffect.GetTimer());
				ability.gameObject.SetActive(true);
				return;
			}
		}
	}
	private void OnResetStatusEffectTimerForSelectedTarget(SOStatusEffects effect)
	{
		for (int i = 0; i < selectedTargetStatusEffectsParentObj.transform.childCount; i++)
		{
			if (!selectedTargetStatusEffectsParentObj.transform.GetChild(i).gameObject.activeInHierarchy)
				continue;

			Abilities ability = selectedTargetStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
			if (ability.effectBaseRef != effect)
				continue;
			else
			{
				ability.ResetEffectTimer();
				return;
			}
		}
	}

	//UI PANEL CHANGE EVENTS
	public void OpenPlayerDeathScreen(PlayerController player)
	{

	}
	public void OpenInventoryButton()
	{
		PlayerEventManager.ShowPlayerInventory();
	}
	public void OpenLearntAbilitiesButton()
	{
		PlayerEventManager.ShowPlayerLearntAbilities();
	}
	public void OpenClassSelectionButton()
	{
		PlayerEventManager.ShowPlayerClassSelection();
	}
	public void OpenClassSkillTreeButton()
	{
		PlayerEventManager.ShowPlayerSkillTree();
	}
	public void OpenPlayerJournalButton()
	{
		PlayerEventManager.ShowPlayerJournal();
	}
}
