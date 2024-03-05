using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHotbarUi : MonoBehaviour
{
	[SerializeField]
	public static PlayerHotbarUi Instance;

	[Header("Hotbar Consumables")]
	public GameObject HotbarPanelUi;
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
	public List<SOClassAbilities> equippedAbilities = new List<SOClassAbilities>();
	public Abilities equippedAbilityOne;
	public Abilities equippedAbilityTwo;
	public Abilities equippedAbilityThree;
	public Abilities equippedAbilityFour;
	public Abilities equippedAbilityFive;

	[Header("Selected Target Ui")]
	public EntityStats selectedTarget;
	public GameObject selectedTargetPanelUi;
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
	}
	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		ClassesUi.OnClassReset += ResetEquippedAbilities;
		InventorySlotUi.OnHotbarItemEquip += EquipHotbarItem;
	}
	private void OnDisable()
	{
		ClassesUi.OnClassReset -= ResetEquippedAbilities;
		InventorySlotUi.OnHotbarItemEquip -= EquipHotbarItem;
	}
	private void Initilize()
	{
		foreach (GameObject slot in ConsumableSlots)
			slot.GetComponent<InventorySlotUi>().SetSlotIndex();

		foreach (GameObject slot in AbilitySlots)
			slot.GetComponent<InventorySlotUi>().SetSlotIndex();

		selectedTargetPanelUi.SetActive(false);
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
	private void EquipHotbarItem(InventoryItem item, InventorySlotUi slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);

		else if (item.itemType == InventoryItem.ItemType.isConsumable)
			EquipConsumables(item.GetComponent<Consumables>(), slot);
		else if (item.itemType == InventoryItem.ItemType.isAbility)
			EquipAbility(item.abilityBaseRef, slot);
	}
	private void EquipConsumables(Consumables consumableToEquip, InventorySlotUi slotEquippedTo)
	{
		if (slotEquippedTo.slotIndex == 0)
			equippedConsumableOne = consumableToEquip;
		else if (slotEquippedTo.slotIndex == 1)
			equippedConsumableTwo = consumableToEquip;
	}
	private void EquipAbility(SOClassAbilities newAbility, InventorySlotUi slotToEquipTo)
	{
		if (slotToEquipTo.itemInSlot != null)
		{
			if (equippedAbilities.Contains(slotToEquipTo.itemInSlot.abilityBaseRef))
				equippedAbilities.Remove(slotToEquipTo.itemInSlot.abilityBaseRef);

			Destroy(slotToEquipTo.itemInSlot.gameObject);
		}

		GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItem item = go.GetComponent<InventoryItem>();
		PlayerInventoryUi.Instance.SetAbilityData(item, newAbility);

		item.inventorySlotIndex = slotToEquipTo.slotIndex;
		item.transform.SetParent(slotToEquipTo.transform);
		item.SetTextColour();
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
	private void HandleEmptySlots(InventorySlotUi slot)
	{
		if (slot.slotType == InventorySlotUi.SlotType.consumables)
		{
			if (slot.slotIndex == 0)
				equippedConsumableOne = null;
			if (slot.slotIndex == 1)
				equippedConsumableTwo = null;
		}
		if (slot.slotType == InventorySlotUi.SlotType.ability)
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
		PlayerInventoryUi.Instance.ShowInventory();
	}
	public void OpenLearntAbilitiesButton()
	{
		PlayerInventoryUi.Instance.ShowLearntAbilities();
	}
	public void OpenClassSelectionButton()
	{
		ClassesUi.Instance.ShowPlayerClassSelection();
	}
	public void OpenClassSkillTreeButton()
	{
		ClassesUi.Instance.ShowClassSkillTree(ClassesUi.Instance.currentPlayerClass);
	}

	//UI TargetSelection
	public void OnNewTargetSelected(EntityStats entityStats)
	{
		if (!selectedTargetPanelUi.activeInHierarchy)
			selectedTargetPanelUi.SetActive(true);

		if (selectedTarget != null)
		{
			selectedTarget.OnDeathEvent -= OnTargetDeathUnSelect;
			selectedTarget.OnHealthChangeEvent -= OnTargetHealthChange;
			selectedTarget.OnManaChangeEvent -= OnManaChange;
		}

		selectedTarget = entityStats;
		selectedTarget.OnDeathEvent += OnTargetDeathUnSelect;
		selectedTarget.OnHealthChangeEvent += OnTargetHealthChange;
		selectedTarget.OnManaChangeEvent += OnManaChange;

		OnTargetHealthChange(selectedTarget.maxHealth.finalValue, selectedTarget.currentHealth);
		OnTargetManaChange(selectedTarget.maxMana.finalValue, selectedTarget.currentMana);
	}
	public void OnTargetHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedTargetHealthBarFiller.fillAmount = percentage;
		selectedTargetHealth.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	public void OnTargetManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		selectedTargetHealthBarFiller.fillAmount = percentage;
		selectedTargetMana.text = currentValue.ToString() + "/" + MaxValue.ToString();
	}
	public void OnTargetDeathUnSelect(GameObject obj)
	{
		if (selectedTargetPanelUi.activeInHierarchy)
			selectedTargetPanelUi.SetActive(false);

		obj.GetComponent<EntityStats>().OnDeathEvent -= OnTargetDeathUnSelect;
		obj.GetComponent<EntityStats>().OnHealthChangeEvent -= OnTargetHealthChange;
		obj.GetComponent<EntityStats>().OnManaChangeEvent -= OnManaChange;
	}

	//UI Player Updates
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
