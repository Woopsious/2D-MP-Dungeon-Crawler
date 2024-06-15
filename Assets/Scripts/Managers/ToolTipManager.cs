using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour
{
	public RectTransform tipWindow;
	public TextMeshProUGUI tipText;

	public RectTransform contextWindow;
	public Button equipItemButton;
	public Button equipItemButtonOne;
	public Button equipItemButtonTwo;
	public Button equipItemButtonThree;
	public Button equipItemButtonFour;
	public Button equipItemButtonFive;
	public Button unEquipItemButton;
	public Button splitItemButton;
	public Button discardItemButton;
	public Button handInItemButton;

	public static Action<string, Vector2> OnMouseHover;
	public static Action OnMouseLoseFocus;

	public static Action<InventoryItemUi, Vector2> OnMouseRightClick;

	public static Action<QuestDataSlotUi, InventoryItemUi> OnTryHandInItem;

	private void OnEnable()
	{
		OnMouseHover += ShowTip;
		OnMouseLoseFocus += HideTip;

		EventManager.OnShowPlayerInventoryEvent += HideTip;
		EventManager.OnShowPlayerClassSelectionEvent += HideTip;
		EventManager.OnShowPlayerSkillTreeEvent += HideTip;
		EventManager.OnShowPlayerLearntAbilitiesEvent += HideTip;
		EventManager.OnShowPlayerJournalEvent += HideTip;

		OnMouseRightClick += HideShowContextMenu;

		EventManager.OnShowPlayerInventoryEvent += HideContextMenu;
		EventManager.OnShowPlayerClassSelectionEvent += HideContextMenu;
		EventManager.OnShowPlayerSkillTreeEvent += HideContextMenu;
		EventManager.OnShowPlayerLearntAbilitiesEvent += HideContextMenu;
		EventManager.OnShowPlayerJournalEvent += HideContextMenu;
	}
	private void OnDisable()
	{
		OnMouseHover -= ShowTip;
		OnMouseLoseFocus -= HideTip;

		EventManager.OnShowPlayerInventoryEvent -= HideTip;
		EventManager.OnShowPlayerClassSelectionEvent -= HideTip;
		EventManager.OnShowPlayerSkillTreeEvent -= HideTip;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= HideTip;
		EventManager.OnShowPlayerJournalEvent -= HideTip;

		OnMouseRightClick -= HideShowContextMenu;

		EventManager.OnShowPlayerInventoryEvent -= HideContextMenu;
		EventManager.OnShowPlayerClassSelectionEvent -= HideContextMenu;
		EventManager.OnShowPlayerSkillTreeEvent -= HideContextMenu;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= HideContextMenu;
		EventManager.OnShowPlayerJournalEvent -= HideContextMenu;
	}

	//context menu
	private void HideShowContextMenu(InventoryItemUi item, Vector2 mousePos)
	{
		if (!contextWindow.gameObject.activeInHierarchy)
			ShowContextMenu(item.GetComponentInParent<InventorySlotDataUi>(), mousePos);
		else
			HideContextMenu();
	}
	private void ShowContextMenu(InventorySlotDataUi slot, Vector2 mousePos)
	{
		contextWindow.sizeDelta = new Vector2(tipText.preferredWidth > 300 ? 300 :
			tipText.preferredWidth * 1.25f, tipText.preferredHeight * 1.25f);
		contextWindow.transform.position = new Vector2(mousePos.x + 25 + tipWindow.sizeDelta.x / 2, mousePos.y);
		contextWindow.gameObject.SetActive(true);

		if (!slot.IsPlayerEquipmentSlot()) //show either equip/uneuip item button based on if equipment slot
		{
			EquipItem(slot);    //button delegating and setting active in here
			equipItemButton.gameObject.SetActive(true);
			unEquipItemButton.gameObject.SetActive(false);
		}
		else if (slot.IsPlayerEquipmentSlot())
		{
			//overwite equipItem button with UnEquipItem button
			unEquipItemButton.onClick.AddListener(delegate { UnEquipItem(slot); });
			unEquipItemButton.gameObject.SetActive(true);
		}

		//keep rest hidden for learnt abilities inventory
		if (CheckIfListContainsSlot(PlayerInventoryUi.Instance.LearntAbilitySlots, slot)) return;

		splitItemButton.onClick.AddListener(delegate { SplitItem(slot); });
		splitItemButton.gameObject.SetActive(true);

		if (!slot.IsPlayerEquipmentSlot())
		{
			discardItemButton.onClick.AddListener(delegate { DiscardItem(slot); });
			discardItemButton.gameObject.SetActive(true);
		}

		QuestDataSlotUi quest = CanItemBeHandedIn(slot.itemInSlot);
		if (quest != null)
		{
			handInItemButton.gameObject.SetActive(true);
			handInItemButton.onClick.AddListener(delegate { HandInItem(slot); });
		}
		else
			handInItemButton.gameObject.SetActive(false);
	}
	private void HideContextMenu()
	{
		discardItemButton.onClick.RemoveAllListeners();
		handInItemButton.onClick.RemoveAllListeners();
		splitItemButton.onClick.RemoveAllListeners();
		equipItemButton.onClick.RemoveAllListeners();
		equipItemButtonOne.onClick.RemoveAllListeners();
		equipItemButtonTwo.onClick.RemoveAllListeners();
		equipItemButtonThree.onClick.RemoveAllListeners();
		equipItemButtonFour.onClick.RemoveAllListeners();
		equipItemButtonFive.onClick.RemoveAllListeners();

		contextWindow.gameObject.SetActive(false);
		equipItemButton.gameObject.SetActive(false);
		equipItemButtonOne.gameObject.SetActive(false);
		equipItemButtonTwo.gameObject.SetActive(false);
		equipItemButtonThree.gameObject.SetActive(false);
		equipItemButtonFour.gameObject.SetActive(false);
		equipItemButtonFive.gameObject.SetActive(false);
		unEquipItemButton.gameObject.SetActive(false);
		splitItemButton.gameObject.SetActive(false);
		discardItemButton.gameObject.SetActive(false);
		handInItemButton.gameObject.SetActive(false);
	}

	//context menu button actions
	private void EquipItem(InventorySlotDataUi slot)
	{
		slot.itemInSlot.parentAfterDrag = slot.transform;
		if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isWeapon)
		{
			equipItemButton.gameObject.SetActive(true);
			if (slot.itemInSlot.GetComponent<Weapons>().weaponBaseRef.weaponType == SOWeapons.WeaponType.isOffhand)
				equipItemButton.onClick.AddListener(
					delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.offHandEquipmentSlot); });
			else
				equipItemButton.onClick.AddListener(
					delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.weaponEquipmentSlot); });
		}
		else if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isArmor)
		{
			if (slot.itemInSlot.GetComponent<Armors>().armorBaseRef.armorSlot == SOArmors.ArmorSlot.helmet)
				equipItemButton.onClick.AddListener(
					delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.helmetEquipmentSlot); });
			if (slot.itemInSlot.GetComponent<Armors>().armorBaseRef.armorSlot == SOArmors.ArmorSlot.chest)
				equipItemButton.onClick.AddListener(
					delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.chestpieceEquipmentSlot); });
			if (slot.itemInSlot.GetComponent<Armors>().armorBaseRef.armorSlot == SOArmors.ArmorSlot.legs)
				equipItemButton.onClick.AddListener(
					delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.legsEquipmentSlot); });
		}
		else if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isAccessory)
		{
			if (slot.itemInSlot.GetComponent<Accessories>().accessoryBaseRef.accessorySlot == SOAccessories.AccessorySlot.necklace)
				equipItemButton.onClick.AddListener(
					delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.necklassEquipmentSlot); });
			else
				equipItemButton.onClick.AddListener(
					delegate { EquipItemWithMultipleSlotDestination(slot); });
		}
		else
			equipItemButton.onClick.AddListener(
				delegate { EquipItemWithMultipleSlotDestination(slot); });
	}
	private void EquipItemWithMultipleSlotDestination(InventorySlotDataUi slot)
	{
		equipItemButtonOne.onClick.RemoveAllListeners();
		equipItemButtonTwo.onClick.RemoveAllListeners();
		equipItemButtonThree.onClick.RemoveAllListeners();
		equipItemButtonFour.onClick.RemoveAllListeners();
		equipItemButtonFive.onClick.RemoveAllListeners();

		if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isAccessory)
		{
			equipItemButtonOne.gameObject.SetActive(true);
			equipItemButtonTwo.gameObject.SetActive(true);
			equipItemButtonOne.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.ringEquipmentSlotOne); });
			equipItemButtonTwo.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerInventoryUi.Instance.ringEquipmentSlotTwo); });
		}
		if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isConsumable)
		{
			equipItemButtonOne.gameObject.SetActive(true);
			equipItemButtonTwo.gameObject.SetActive(true);
			equipItemButtonOne.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.consumableSlotOne); });
			equipItemButtonTwo.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.consumableSlotTwo); });
		}
		if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isAbility)
		{
			equipItemButtonOne.gameObject.SetActive(true);
			equipItemButtonTwo.gameObject.SetActive(true);
			equipItemButtonThree.gameObject.SetActive(true);
			equipItemButtonFour.gameObject.SetActive(true);
			equipItemButtonFive.gameObject.SetActive(true);
			equipItemButtonOne.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.abilitySlotOne); });
			equipItemButtonTwo.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.abilitySlotTwo); });
			equipItemButtonThree.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.abilitySlotThree); });
			equipItemButtonFour.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.abilitySlotFour); });
			equipItemButtonFive.onClick.AddListener(
				delegate { EquipItemToThisSlot(slot, PlayerHotbarUi.Instance.abilitySlotFive); });
		}
	}
	private void UnEquipItem(InventorySlotDataUi oldSlot)
	{
		foreach (GameObject obj in PlayerInventoryUi.Instance.InventorySlots)
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			if (!slot.IsSlotEmpty()) continue;

			slot.AddItemToSlot(oldSlot.itemInSlot);
			HideContextMenu();
			HideTip();
			return;
		}
		Debug.LogError("inventory full");
	}
	private void EquipItemToThisSlot(InventorySlotDataUi oldSlot, GameObject newSlot)
	{
		oldSlot.itemInSlot.parentAfterDrag = oldSlot.transform;
		newSlot.GetComponent<InventorySlotDataUi>().EquipItemToSlot(oldSlot.itemInSlot);
		HideContextMenu();
		HideTip();
	}

	private void SplitItem(InventorySlotDataUi slot)
	{
		if (slot.itemInSlot.currentStackCount == 1) return;

		if (slot.itemInSlot.currentStackCount % 2 == 0)
		{
			slot.itemInSlot.SetStackCounter(slot.itemInSlot.currentStackCount / 2);
			PlayerInventoryUi.Instance.AddItemToInventory(slot.itemInSlot.GetComponent<Items>(), false);
		}
		else
		{
			float newStackCount = slot.itemInSlot.currentStackCount / 2; //split stack count (round down copy)
			slot.itemInSlot.SetStackCounter((int)newStackCount);
			PlayerInventoryUi.Instance.AddItemToInventory(slot.itemInSlot.GetComponent<Items>(), false);
			slot.itemInSlot.SetStackCounter(slot.itemInSlot.currentStackCount + 1); //(round up original)
		}
	}
	private void DiscardItem(InventorySlotDataUi slot)
	{
		InventoryItemUi item = slot.itemInSlot;
		slot.RemoveItemFromSlot();
		Destroy(item.gameObject);
		HideContextMenu();
	}
	private void HandInItem(InventorySlotDataUi slot)
	{
		PlayerJournalUi.Instance.HandInItemQuests(slot);
	}

	//tips
	private void ShowTip(string tip, Vector2 mousePos)
	{
		tipText.text = tip;
		tipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 300 ? 300 : 
			tipText.preferredWidth * 1.25f, tipText.preferredHeight * 1.25f);

		tipWindow.transform.position = new Vector2(mousePos.x + 25 + tipWindow.sizeDelta.x / 2, mousePos.y);
		tipWindow.gameObject.SetActive(true);
	}
	private void HideTip()
	{
		tipText.text = null;
		tipWindow.gameObject.SetActive(false);
	}

	private bool CheckIfListContainsSlot(List<GameObject> listOfSlots, InventorySlotDataUi slot)
	{
		if (listOfSlots.Contains(slot.gameObject))
			return true;
		else
			return false;
	}
	private QuestDataSlotUi CanItemBeHandedIn(InventoryItemUi item)
	{
		if (item.itemType == InventoryItemUi.ItemType.isAbility) return null;

		QuestDataSlotUi matchingQuest = null;
		foreach(QuestDataSlotUi quest in PlayerJournalUi.Instance.activeQuests)
		{
			if (quest.questType != QuestDataSlotUi.QuestType.isItemHandInQuest)
				continue;

			if (quest.itemTypeToHandIn == QuestDataSlotUi.ItemType.isWeapon)
			{
				if (quest.weaponToHandIn == item.weaponBaseRef)
				{
					Debug.Log("weapon");
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else if (quest.itemTypeToHandIn == QuestDataSlotUi.ItemType.isArmor)
			{
				if (quest.armorToHandIn == item.armorBaseRef)
				{
					Debug.Log("armor");
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else if (quest.itemTypeToHandIn == QuestDataSlotUi.ItemType.isAccessory)
			{
				if (quest.accessoryToHandIn == item.accessoryBaseRef)
				{
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else if (quest.itemTypeToHandIn == QuestDataSlotUi.ItemType.isConsumable)
			{
				if (quest.consumableToHandIn == item.consumableBaseRef)
				{
					Debug.Log("consumable");
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else
				return null;
		}
		return matchingQuest;
	}
}
