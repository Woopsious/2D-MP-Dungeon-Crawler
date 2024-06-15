using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHotbarUi : MonoBehaviour
{
	[SerializeField]
	public static PlayerHotbarUi Instance;

	public static event Action<Abilities, EntityStats> OnNewQueuedAbilities;

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
	public List<SOClassAbilities> equippedAbilities = new List<SOClassAbilities>();
	public Abilities equippedAbilityOne;
	public Abilities equippedAbilityTwo;
	public Abilities equippedAbilityThree;
	public Abilities equippedAbilityFour;
	public Abilities equippedAbilityFive;

	[Header("Selected Target Ui")]
	public EntityStats selectedTarget;
	public GameObject unSelectedTargetUi;
	public GameObject selectedTargetUi;
	public TMP_Text selectedTargetUiName;
	public Image selectedTargetUiImage;
	public Image selectedTargetHealthBarFiller;
	public TMP_Text selectedTargetHealth;
	public Image selectedTargetManaBarFiller;
	public TMP_Text selectedTargetMana;

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
		DisplayQueuedAbilityUi();
	}
	private void OnEnable()
	{
		PlayerClassesUi.OnClassReset += ResetEquippedAbilities;
		InventorySlotDataUi.OnHotbarItemEquip += EquipHotbarItem;
		EventManager.OnGoldAmountChange += OnGoldAmountChange;
		EventManager.OnPlayerExpChangeEvent += OnExperienceChange;
		EventManager.OnPlayerHealthChangeEvent += OnHealthChange;
		EventManager.OnPlayerManaChangeEvent += OnManaChange;

		EventManager.OnDeathEvent += OnTargetDeathUnSelect;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassReset -= ResetEquippedAbilities;
		InventorySlotDataUi.OnHotbarItemEquip -= EquipHotbarItem;
		EventManager.OnGoldAmountChange -= OnGoldAmountChange;
		EventManager.OnPlayerExpChangeEvent -= OnExperienceChange;
		EventManager.OnPlayerHealthChangeEvent -= OnHealthChange;
		EventManager.OnPlayerManaChangeEvent -= OnManaChange;

		EventManager.OnDeathEvent -= OnTargetDeathUnSelect;
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
	}

	//reset/clear any equipped abilities from ui
	private void ResetEquippedAbilities(SOClasses currentClass)
	{
		foreach (GameObject equippedAbility in AbilitySlots)
		{
			if (equippedAbility.transform.childCount == 0)
				continue;

			Destroy(equippedAbility.transform.GetChild(0).gameObject);
		}
		equippedAbilities.Clear();
	}

	//Equip Consumables/Abilities
	//not physically spawned in
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
	}
	private void EquipAbility(SOClassAbilities newAbility, InventorySlotDataUi slotToEquipTo)
	{
		if (slotToEquipTo.itemInSlot != null)
		{
			if (equippedAbilities.Contains(slotToEquipTo.itemInSlot.abilityBaseRef))
				equippedAbilities.Remove(slotToEquipTo.itemInSlot.abilityBaseRef);

			Destroy(slotToEquipTo.itemInSlot.gameObject);
		}

		GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItemUi item = go.GetComponent<InventoryItemUi>();
		PlayerInventoryUi.Instance.SetAbilityData(item, newAbility);

		item.inventorySlotIndex = slotToEquipTo.slotIndex;
		item.transform.SetParent(slotToEquipTo.transform);
		slotToEquipTo.itemInSlot = item;
		slotToEquipTo.UpdateSlotSize();
		item.Initilize();

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

		equippedAbilities.Add(newAbility);
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
		if (slot.slotType == InventorySlotDataUi.SlotType.ability)
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

	//UI CHANGES
	public void OpenInventoryButton()
	{
		EventManager.ShowPlayerInventory();
	}
	public void OpenLearntAbilitiesButton()
	{
		EventManager.ShowPlayerLearntAbilities();
	}
	public void OpenClassSelectionButton()
	{
		EventManager.ShowPlayerClassSelection();
	}
	public void OpenClassSkillTreeButton()
	{
		EventManager.ShowPlayerSkillTree();
	}
	public void OpenPlayerJournalButton()
	{
		EventManager.ShowPlayerJournal();
	}

	//Select Target event
	public void OnNewTargetSelected(EntityStats entityStats)
	{
		if (!selectedTargetUi.activeInHierarchy)
			selectedTargetUi.SetActive(true);
		if (unSelectedTargetUi.activeInHierarchy)
			unSelectedTargetUi.SetActive(false);

		if (selectedTarget != null)
		{
			selectedTarget.OnHealthChangeEvent -= OnTargetHealthChange;
			selectedTarget.OnManaChangeEvent -= OnTargetManaChange;
		}

		selectedTarget = entityStats;
		selectedTargetUiImage.sprite = entityStats.entityBaseStats.sprite;
		selectedTargetUiName.text = entityStats.entityBaseStats.entityName;
		selectedTarget.OnHealthChangeEvent += OnTargetHealthChange;
		selectedTarget.OnManaChangeEvent += OnTargetManaChange;

		OnTargetHealthChange(selectedTarget.maxHealth.finalValue, selectedTarget.currentHealth);
		OnTargetManaChange(selectedTarget.maxMana.finalValue, selectedTarget.currentMana);
	}

	//UI Selected Target events
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
	private void OnTargetDeathUnSelect(GameObject obj)
	{
		if (selectedTarget == null || selectedTarget.gameObject != obj) return;

		if (selectedTargetUi.activeInHierarchy)
			selectedTargetUi.SetActive(false);
		if (!unSelectedTargetUi.activeInHierarchy)
			unSelectedTargetUi.SetActive(true);

		obj.GetComponent<EntityStats>().OnHealthChangeEvent -= OnTargetHealthChange;
		obj.GetComponent<EntityStats>().OnManaChangeEvent -= OnTargetManaChange;
	}

	//UI Ability Uses
	public void AddNewQueuedAbility(Abilities ability, PlayerController player, bool canInstantCast)
	{
		OnNewQueuedAbilities?.Invoke(ability, player.GetComponent<EntityStats>());
		queuedAbility = ability;

		queuedAbilityTextInfo.SetActive(true);
		if (ability.abilityBaseRef.isAOE)
		{
			queuedAbilityAoe.SetActive(true);
			SetSizeOfQueuedAbilityAoeUi(ability.abilityBaseRef);
		}

		if (canInstantCast)
			player.CastQueuedAbility(ability);
	}
	public void OnUseQueuedAbility(Abilities ability, PlayerController player)
	{
		ability.CastAbility(player.GetComponent<EntityStats>());
		queuedAbilityTextInfo.SetActive(false);
		queuedAbilityAoe.SetActive(false);
		queuedAbility = null;
	}
	public void OnCancelQueuedAbility(Abilities ability)
	{
		queuedAbilityTextInfo.SetActive(false);
		queuedAbilityAoe.SetActive(false);
		queuedAbility = null;
	}
	private void DisplayQueuedAbilityUi()
	{
		if (queuedAbilityTextInfo.activeInHierarchy)
			queuedAbilityTextInfo.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y - 50);

		if (queuedAbilityAoe.activeInHierarchy)
			queuedAbilityAoe.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}
	private void SetSizeOfQueuedAbilityAoeUi(SOClassAbilities abilityRef)
	{
		Vector3 scale = new Vector3(abilityRef.aoeSize, abilityRef.aoeSize, abilityRef.aoeSize);
		queuedAbilityAoe.transform.localScale = scale;
	}

	//UI Player Updates
	public void OnGoldAmountChange(int amount)
	{
		goldAmountText.text = $"Gold: {amount}";
	}
	public void OnExperienceChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		expBarFiller.fillAmount = percentage;
		expBarText.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	public void OnHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		healthBarFiller.fillAmount = percentage;
		HealthBarText.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	public void OnManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		manaBarFiller.fillAmount = percentage;
		manaBarText.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
}
