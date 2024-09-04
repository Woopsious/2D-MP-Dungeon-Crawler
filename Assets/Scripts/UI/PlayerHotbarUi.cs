using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHotbarUi : MonoBehaviour
{
	[SerializeField]
	public static PlayerHotbarUi Instance;

	public GameObject statusEffectUiPrefab;

	public TMP_Text playerLevelInfoText;
	public TMP_Text playerClassInfoText;
	public TMP_Text goldAmountText;

	[Header("Hotbar Consumables")]
	public GameObject HotbarPanelUi;
	public List<GameObject> ConsumableSlots = new List<GameObject>();
	public GameObject consumableSlotOne;
	public GameObject consumableSlotTwo;

	[Header("Equipped Consumables")]
	public Consumables equippedConsumableOne;
	public Consumables equippedConsumableTwo;

	[Header("Hotbar Abilities")]
	public GameObject queuedAbilityTextInfo;
	public GameObject queuedAbilityAoe;
	public Abilities queuedAbility;
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
		UpdateQueuedAbilityUiPosition();
		UpdateSelectedTargetTrackerUi();
	}
	private void OnEnable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent += UpdatePlayerLevelInfo;
		PlayerEventManager.OnGoldAmountChange += UpdatePlayerGoldAmount;
		PlayerEventManager.OnPlayerExpChangeEvent += UpdatePlayerExpBar;
		PlayerEventManager.OnPlayerHealthChangeEvent += UpdatePlayerHealthBar;
		PlayerEventManager.OnPlayerManaChangeEvent += UpdatePlayerManaBar;

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

		selectedTargetUi.SetActive(false);
		queuedAbilityTextInfo.SetActive(false);
		queuedAbilityAoe.SetActive(false);

		for (int i = 0; i < playerStatusEffectsParentObj.transform.childCount; i++)
		{
			Abilities ability = playerStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.gameObject.SetActive(false);
		}
	}

	//Equipping/Unequipping Consumables/Abilities to hotbar Ui slots
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
	private void EquipAbility(SOClassAbilities newAbility, InventorySlotDataUi slotToEquipTo)
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
	public bool IsAbilityAlreadyEquipped(SOClassAbilities newAbility)
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
	private void OnAbilityRefund(SOClassAbilities ability)
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

	//Player ui updates
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

	//Player ui abilities updates
	public void AddNewQueuedAbility(Abilities ability)
	{
		queuedAbility = ability;

		queuedAbilityTextInfo.SetActive(true);
		if (ability.abilityBaseRef.isAOE)
		{
			queuedAbilityAoe.SetActive(true);
			SetSizeOfQueuedAbilityAoeUi(ability.abilityBaseRef);
		}
	}
	public void OnUseQueuedAbility()
	{
		queuedAbilityTextInfo.SetActive(false);
		queuedAbilityAoe.SetActive(false);
		queuedAbility = null;
	}
	public void OnCancelQueuedAbility()
	{
		queuedAbilityTextInfo.SetActive(false);
		queuedAbilityAoe.SetActive(false);
		queuedAbility = null;
	}
	private void UpdateQueuedAbilityUiPosition()
	{
		if (queuedAbilityTextInfo.activeInHierarchy)
			queuedAbilityTextInfo.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y - 50);

		if (queuedAbilityAoe.activeInHierarchy)
			queuedAbilityAoe.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}
	private void SetSizeOfQueuedAbilityAoeUi(SOClassAbilities abilityRef)
	{
		Vector3 scale = new(abilityRef.aoeSize / 1.5f, abilityRef.aoeSize / 1.5f, abilityRef.aoeSize / 1.5f);
		queuedAbilityAoe.transform.localScale = scale;
	}

	//Selected target ui updates
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
		}

		for (int i = 0; i < selectedTargetStatusEffectsParentObj.transform.childCount; i++)
		{
			Abilities ability = selectedTargetStatusEffectsParentObj.transform.GetChild(i).GetComponent<Abilities>();
			ability.gameObject.SetActive(false);
		}

		selectedTarget = entityStats;
		selectedTargetUiImage.sprite = entityStats.entityBaseStats.sprite;

		string targetName;
		if (selectedTarget.entityBaseStats.canUseEquipment)
			targetName = entityStats.classHandler.currentEntityClass.className + " " + entityStats.entityBaseStats.entityName;
		else
			targetName = entityStats.entityBaseStats.entityName;
		selectedTargetUiName.text = targetName;

		//new target event subs
		selectedTarget.OnHealthChangeEvent += OnTargetHealthChange;
		selectedTarget.OnManaChangeEvent += OnTargetManaChange;
		selectedTarget.OnNewStatusEffect += OnNewStatusEffectsForSelectedTarget;

		//initial setting data for ui
		OnTargetHealthChange(selectedTarget.maxHealth.finalValue, selectedTarget.currentHealth);
		OnTargetManaChange(selectedTarget.maxMana.finalValue, selectedTarget.currentMana);

		foreach (AbilityStatusEffect statusEffect in selectedTarget.currentStatusEffects)
			OnNewStatusEffectsForSelectedTarget(statusEffect);
	}
	private void OnTargetDeathUnSelect(GameObject obj)
	{
		if (selectedTarget == null || selectedTarget.gameObject != obj) return;

		if (selectedTargetUi.activeInHierarchy)
			selectedTargetUi.SetActive(false);
		if (!unSelectedTargetUi.activeInHierarchy)
			unSelectedTargetUi.SetActive(true);

		EntityStats entity = obj.GetComponent<EntityStats>();
		entity.OnHealthChangeEvent -= OnTargetHealthChange;
		entity.OnManaChangeEvent -= OnTargetManaChange;
		entity.OnNewStatusEffect -= OnNewStatusEffectsForSelectedTarget;
	}
	private void UpdateSelectedTargetTrackerUi()
	{
		if (selectedTarget == null || !selectedTargetTrackerUi.activeInHierarchy) return;
		Vector2 position = Camera.main.WorldToScreenPoint(selectedTarget.transform.position);
		selectedTargetTrackerUi.transform.position = new Vector3(position.x, position.y + 40, 0);
	}

	//Selected target ui event updates
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

	//UI CHANGES
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
