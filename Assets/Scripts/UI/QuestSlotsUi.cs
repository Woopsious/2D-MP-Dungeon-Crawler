using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlotsUi : MonoBehaviour
{
	[Header("Quest Ui")]
	public TMP_Text questNameUi;
	public TMP_Text questDescriptionUi;
	public TMP_Text questTrackerUi;
	public TMP_Text questRewardUi;
	public Image questImage;

	[Header("Quest Info")]
	public string questName;
	[TextArea(3, 10)]
	public string questDescription;

	public QuestType questType;
	public enum QuestType
	{
		isBossKillQuest, isKillQuest, isItemHandInQuest
	}
	public bool isCurrentlyActiveQuest;
	public float amount;
	public float currentAmount;

	[Header("Kill Quest Info")]
	public SOEntityStats entityToKill;

	[Header("Item Quest Info")]
	public SOWeapons weaponToHandIn;
	public SOArmors armorToHandIn;
	public SOAccessories accessoryToHandIn;
	public SOConsumables consumableToHandIn;

	public ItemType itemTypeToHandIn;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}

	[Header("Quest Reward")]
	public RewardType questRewardType;
	public enum RewardType
	{
		isExpReward, isGoldReward
	}
	public int rewardToAdd;

	[Header("List of Items/Enemies")]
	public List<SOEntityStats> possibleBossTargets = new List<SOEntityStats>();
	public List<SOEntityStats> possibleEntityTargets = new List<SOEntityStats>();
	public List<SOWeapons> possibleWeapons = new List<SOWeapons>();
	public List<SOArmors> possibleArmors = new List<SOArmors>();
	public List<SOAccessories> possibleAccessories = new List<SOAccessories>();
	public List<SOConsumables> possibleConsumables = new List<SOConsumables>();

	public event Action OnQuestAbandon;

	public void InitilizeBossKillQuest()
	{
		questType = QuestType.isBossKillQuest;
		//entityToKill = (randomly select one from list of bosses)
		amount = 1;

		SetUpQuestName();
		SetUpQuestDescription();
		SetUpQuestReward();
	}
	public void InitilizeKillQuest()
	{
		questType = QuestType.isKillQuest;
		//entityToKill = (randomly select one from list of enemies)
		amount = Utilities.GetRandomNumberBetween(4, 8);

		SetUpQuestName();
		SetUpQuestDescription();
		SetUpQuestReward();
	}
	public void InitilizeItemHandInQuest()
	{
		questType = QuestType.isItemHandInQuest;
		itemTypeToHandIn = (ItemType)Utilities.GetRandomNumber(4);

		if (itemTypeToHandIn == ItemType.isWeapon)
		{
			//weaponToHandIn = (get random item to hand in from list)
			amount = 1;
		}
		else if (itemTypeToHandIn == ItemType.isArmor)
		{
			//armorToHandIn = (get random item to hand in from list)
			amount = 1;
		}
		else if (itemTypeToHandIn == ItemType.isAccessory)
		{
			//accessoryToHandIn = (get random item to hand in from list)
			amount = 1;
		}
		else if (itemTypeToHandIn == ItemType.isConsumable)
		{
			//consumableToHandIn = (get random item to hand in from list)
			amount = 5;
		}

		SetUpQuestName();
		SetUpQuestDescription();
		SetUpQuestReward();
	}

	private void SetUpQuestName()
	{
		string questName = "";

		if (questType == QuestType.isBossKillQuest)
		{
			questName = $"Kill {amount} {entityToKill.entityName}";
		}
		else if (questType == QuestType.isKillQuest)
		{
			questName = $"Kill {amount} {entityToKill.entityName}";
		}
		else if(questType == QuestType.isItemHandInQuest)
		{
			if (itemTypeToHandIn == ItemType.isWeapon)
				questName = $"Hand In {amount} {weaponToHandIn.itemName}";

			else if (itemTypeToHandIn == ItemType.isArmor)
				questName = $"Hand In {amount} {armorToHandIn.itemName}";

			else if (itemTypeToHandIn == ItemType.isAccessory)
				questName = $"Hand In {amount} {accessoryToHandIn.itemName}";

			else if (itemTypeToHandIn == ItemType.isConsumable)
				questName = $"Hand In {amount} {consumableToHandIn.itemName}";
		}

		questNameUi.text = questName;
		this.questName = questName;
	}
	private void SetUpQuestDescription()
	{
		string questDescription = "";

		if (questType == QuestType.isBossKillQuest)
		{
			questDescription = $"Venture into a special dungeons and kill {amount} {entityToKill.entityName} boss.";
			questTrackerUi.text = $"{currentAmount} / {amount} Killed";
		}
		else if (questType == QuestType.isKillQuest)
		{
			questDescription = $"Venture into a dungeon and kill {amount} {entityToKill.entityName}.";
			questTrackerUi.text = $"{currentAmount} / {amount} Killed";
		}
		else if(questType == QuestType.isItemHandInQuest)
		{
			if (itemTypeToHandIn == ItemType.isWeapon)
				questDescription = $"Collect {amount} " +
					$"{weaponToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			else if (itemTypeToHandIn == ItemType.isArmor)
				questDescription = $"Collect {amount} " +
					$"{armorToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			else if (itemTypeToHandIn == ItemType.isAccessory)
				questDescription = $"Collect {amount} " +
					$"{accessoryToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			else if (itemTypeToHandIn == ItemType.isConsumable)
				questDescription = $"Collect {amount} " +
					$"{consumableToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			questTrackerUi.text = $"{currentAmount} / {amount} Handed In";
		}

		questDescriptionUi.text = questDescription;
		this.questDescription = questDescription;
	}
	private void SetUpQuestReward()
	{
		questRewardType = (RewardType)Utilities.GetRandomNumber(1);

		if (questRewardType == RewardType.isExpReward)
		{
			rewardToAdd = Utilities.GetRandomNumberBetween(149, 300);
			questRewardUi.text = $"Reward: {rewardToAdd} Exp";
		}
		if (questRewardType == RewardType.isGoldReward)
		{
			rewardToAdd = Utilities.GetRandomNumberBetween(299, 500);
			questRewardUi.text = $"Reward: {rewardToAdd} Gold";
		}
	}

	private void OnEntityDeathCheckKillAmount(SOEntityStats entityStats)
	{
		if (questType == QuestType.isBossKillQuest || questType == QuestType.isKillQuest)
		{
			if (entityToKill = entityStats)
			{
				currentAmount++;
				questTrackerUi.text = $"{currentAmount} / {amount} Killed";
			}
		}

		if (currentAmount >= amount)
			CompleteThisQuest();
	}
	private void OnItemHandInCheckHandInAmount(InventoryItem item)
	{
		if (questType == QuestType.isBossKillQuest || questType == QuestType.isKillQuest) return;

		if (itemTypeToHandIn == ItemType.isWeapon)
			currentAmount++;
		else if (itemTypeToHandIn == ItemType.isArmor)
			currentAmount++;
		else if (itemTypeToHandIn == ItemType.isAccessory)
			currentAmount++;
		else if (itemTypeToHandIn == ItemType.isConsumable)
			currentAmount++;

		if (currentAmount >= amount)
			CompleteThisQuest();
	}

	private void CompleteThisQuest()
	{
		//do stuff to complete quest
	}
	public void AbandonQuest()
	{
		OnQuestAbandon?.Invoke();
	}
}
