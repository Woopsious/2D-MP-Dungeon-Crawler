using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour
{
	public RectTransform tipWindow;
	public TextMeshProUGUI tipText;

	public RectTransform contextWindow;
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
		contextWindow.gameObject.SetActive(false);
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
