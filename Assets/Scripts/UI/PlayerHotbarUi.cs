using System.Collections;
using System.Collections.Generic;
using System.Data;
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

	[Header("equipped Consumables")]
	public Consumables equippedConsumableOne;
	public Consumables equippedConsumableTwo;

	[Header("Hotbar Abilities")]
	public List<GameObject> AbilitySlots = new List<GameObject>();
	public GameObject abilitySlotOne;
	public GameObject abilitySlotTwo;
	public GameObject abilitySlotThree;
	public GameObject abilitySlotFour;
	public GameObject abilitySlotFive;

	[Header("equipped Abilities")]
	public List<SOClassAbilities> equippedAbilities = new List<SOClassAbilities>();
	public Abilities equippedAbilityOne;
	public Abilities equippedAbilityTwo;
	public Abilities equippedAbilityThree;
	public Abilities equippedAbilityFour;
	public Abilities equippedAbilityFive;

	[Header("ExpBar")]
	public Image expBarFiller;

	[Header("HealthBar")]
	public Image healthBarFiller;

	[Header("ManaBar")]
	public Image manaBarFiller;

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

	public void OnExperienceChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		expBarFiller.fillAmount = percentage;
	}
	public void OnHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		healthBarFiller.fillAmount = percentage;
	}
	public void OnManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		manaBarFiller.fillAmount = percentage;
	}
}
