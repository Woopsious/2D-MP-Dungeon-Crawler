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
	public Button splitItemButton;
	public Button discardItemButton;
	public Button handInItemButton;

	public static Action<string, Vector2> OnMouseHover;
	public static Action OnMouseLoseFocus;

	public static Action<InventoryItemUi, Vector2> OnMouseRightClick;

	public static Action<QuestSlotsUi, InventoryItemUi> OnTryHandInItem;

	private void OnEnable()
	{
		OnMouseHover += ShowTip;
		OnMouseLoseFocus += HideTip;

		EventManagerUi.OnShowPlayerInventoryEvent += HideTip;
		EventManagerUi.OnShowPlayerClassSelectionEvent += HideTip;
		EventManagerUi.OnShowPlayerSkillTreeEvent += HideTip;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent += HideTip;
		EventManagerUi.OnShowPlayerJournalEvent += HideTip;

		OnMouseRightClick += HideShowContextMenu;

		EventManagerUi.OnShowPlayerInventoryEvent += HideContextMenu;
		EventManagerUi.OnShowPlayerClassSelectionEvent += HideContextMenu;
		EventManagerUi.OnShowPlayerSkillTreeEvent += HideContextMenu;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent += HideContextMenu;
		EventManagerUi.OnShowPlayerJournalEvent += HideContextMenu;
	}
	private void OnDisable()
	{
		OnMouseHover -= ShowTip;
		OnMouseLoseFocus -= HideTip;

		EventManagerUi.OnShowPlayerInventoryEvent -= HideTip;
		EventManagerUi.OnShowPlayerClassSelectionEvent -= HideTip;
		EventManagerUi.OnShowPlayerSkillTreeEvent -= HideTip;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent -= HideTip;
		EventManagerUi.OnShowPlayerJournalEvent -= HideTip;

		OnMouseRightClick -= HideShowContextMenu;

		EventManagerUi.OnShowPlayerInventoryEvent -= HideContextMenu;
		EventManagerUi.OnShowPlayerClassSelectionEvent -= HideContextMenu;
		EventManagerUi.OnShowPlayerSkillTreeEvent -= HideContextMenu;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent -= HideContextMenu;
		EventManagerUi.OnShowPlayerJournalEvent -= HideContextMenu;
	}

	//context menu
	private void HideShowContextMenu(InventoryItemUi item, Vector2 mousePos)
	{
		if (!contextWindow.gameObject.activeInHierarchy)
			ShowContextMenu(item.GetComponentInParent<InventorySlotUi>(), mousePos);
		else
			HideContextMenu();
	}
	private void ShowContextMenu(InventorySlotUi slot, Vector2 mousePos)
	{
		//keep hidden for learnt abilities inventory
		if (CheckIfListContainsSlot(PlayerInventoryUi.Instance.LearntAbilitySlots, slot)) return;

		contextWindow.sizeDelta = new Vector2(tipText.preferredWidth > 300 ? 300 :
			tipText.preferredWidth * 1.25f, tipText.preferredHeight * 1.25f);
		contextWindow.transform.position = new Vector2(mousePos.x + 25 + tipWindow.sizeDelta.x / 2, mousePos.y);
		contextWindow.gameObject.SetActive(true);

		equipItemButton.onClick.AddListener(delegate { EquipItem(slot); } );
		splitItemButton.onClick.AddListener(delegate { SplitItem(slot); } );

		discardItemButton.onClick.AddListener(delegate { DiscardItem(slot); } );

		QuestSlotsUi quest = CanItemBeHandedIn(slot.itemInSlot);
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
		contextWindow.gameObject.SetActive(false);
	}

	//context menu button actions
	private void EquipItem(InventorySlotUi slot)
	{
		slot.itemInSlot.parentAfterDrag = slot.transform;

		if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isWeapon)
		{
			if (slot.itemInSlot.GetComponent<Weapons>().weaponBaseRef.weaponType == SOWeapons.WeaponType.isOffhand)
				PlayerInventoryUi.Instance.offHandEquipmentSlot.GetComponent<InventorySlotUi>().EquipItemToSlot(slot.itemInSlot);
			else
				PlayerInventoryUi.Instance.weaponEquipmentSlot.GetComponent<InventorySlotUi>().EquipItemToSlot(slot.itemInSlot);
		}
		else if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isArmor)
		{
			if (slot.itemInSlot.GetComponent<Armors>().armorBaseRef.armorSlot == SOArmors.ArmorSlot.helmet)
				PlayerInventoryUi.Instance.helmetEquipmentSlot.GetComponent<InventorySlotUi>().EquipItemToSlot(slot.itemInSlot);
			else if (slot.itemInSlot.GetComponent<Armors>().armorBaseRef.armorSlot == SOArmors.ArmorSlot.chest)
				PlayerInventoryUi.Instance.chestpieceEquipmentSlot.GetComponent<InventorySlotUi>().EquipItemToSlot(slot.itemInSlot);
			else if (slot.itemInSlot.GetComponent<Armors>().armorBaseRef.armorSlot == SOArmors.ArmorSlot.legs)
				PlayerInventoryUi.Instance.legsEquipmentSlot.GetComponent<InventorySlotUi>().EquipItemToSlot(slot.itemInSlot);
		}
		else if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isAccessory)
		{
			if (slot.itemInSlot.GetComponent<Accessories>().accessoryBaseRef.accessorySlot == SOAccessories.AccessorySlot.necklace)
				PlayerInventoryUi.Instance.necklassEquipmentSlot.GetComponent<InventorySlotUi>().EquipItemToSlot(slot.itemInSlot);
			else
			{
				//handle equiping to either ring slot 1 or 2, another button pop up with ring slot 1 or 2.
				//set button text and delegate func for these buttons then show them, on click equip item to corrisponding slot for button
			}
		}
		else if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isConsumable)
		{
			//do same here as for ring slots 1 an 2
		}
		else if (slot.itemInSlot.itemType == InventoryItemUi.ItemType.isAbility)
		{
			//do same here as for ring slots 1 an 2 (maynot work as abilities are different)
		}

		HideContextMenu();
		HideTip();
	}
	private void SplitItem(InventorySlotUi slot)
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
	private void DiscardItem(InventorySlotUi slot)
	{
		InventoryItemUi item = slot.itemInSlot;
		slot.RemoveItemFromSlot();
		Destroy(item.gameObject);
		HideContextMenu();
	}
	private void HandInItem(InventorySlotUi slot)
	{
		QuestSlotsUi questItem = CanItemBeHandedIn(slot.itemInSlot);
		if (questItem != null)
		{
			int amountRequired = questItem.amount - questItem.currentAmount;
			questItem.OnItemHandInCheckHandInAmount(questItem, slot);

			if (amountRequired > slot.itemInSlot.currentStackCount)
				DiscardItem(slot);
			else
				slot.itemInSlot.SetStackCounter(slot.itemInSlot.currentStackCount - amountRequired);
		}
        else
			Debug.Log("item doesnt match");
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

	private bool CheckIfListContainsSlot(List<GameObject> listOfSlots, InventorySlotUi slot)
	{
		if (listOfSlots.Contains(slot.gameObject))
			return true;
		else
			return false;
	}
	private QuestSlotsUi CanItemBeHandedIn(InventoryItemUi item)
	{
		if (item.itemType == InventoryItemUi.ItemType.isAbility) return null;

		QuestSlotsUi matchingQuest = null;
		foreach(QuestSlotsUi quest in PlayerJournalUi.Instance.activeQuests)
		{
			if (quest.questType != QuestSlotsUi.QuestType.isItemHandInQuest)
			{
				Debug.Log("continue");
				continue;
			}

			if (quest.itemTypeToHandIn == QuestSlotsUi.ItemType.isWeapon)
			{
				if (quest.weaponToHandIn == item.weaponBaseRef)
				{
					Debug.Log("weapon");
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else if (quest.itemTypeToHandIn == QuestSlotsUi.ItemType.isArmor)
			{
				if (quest.armorToHandIn == item.armorBaseRef)
				{
					Debug.Log("armor");
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else if (quest.itemTypeToHandIn == QuestSlotsUi.ItemType.isAccessory)
			{
				if (quest.accessoryToHandIn == item.accessoryBaseRef)
				{
					Debug.Log("accessory");
					matchingQuest = quest;
					return matchingQuest;
				}
			}
			else if (quest.itemTypeToHandIn == QuestSlotsUi.ItemType.isConsumable)
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
