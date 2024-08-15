using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;

public class QuestDataSlotUi : MonoBehaviour
{
	[Header("Quest Ui")]
	public TMP_Text questNameUi;
	public TMP_Text questDescriptionUi;
	public TMP_Text questTrackerUi;
	public TMP_Text questRewardUi;
	public Image questImage;
	public GameObject acceptQuestButtonObj;

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
	public int amount;
	public int currentAmount;

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

	/// <summary>
	/// entities will sub to above events when they spawn, i wont have to worry about new quests being added as for now quests can only
	/// be accepted in the hub area and no enemies will exist there (will change if i decide special sorta NPC's can spawn in dungeons)
	/// </summary>
	private void OnDisable()
	{
		PlayerJournalUi.OnNewQuestAccepted -= OnQuestAccepted;
	}
	public void InitilizeBossKillQuest()
	{
		questType = QuestType.isBossKillQuest;
		entityToKill = possibleBossTargets[Utilities.GetRandomNumber(possibleBossTargets.Count - 1)];
		amount = 1;
		InitilizeReward();
		InitilizeText();
	}
	public void InitilizeKillQuest()
	{
		questType = QuestType.isKillQuest;
		entityToKill = possibleEntityTargets[Utilities.GetRandomNumber(possibleEntityTargets.Count - 1)];
		amount = Utilities.GetRandomNumberBetween(5, 8);
		InitilizeReward();
		InitilizeText();
	}
	public void InitilizeItemHandInQuest()
	{
		questType = QuestType.isItemHandInQuest;
		itemTypeToHandIn = (ItemType)Utilities.GetRandomNumber(3);

		if (itemTypeToHandIn == ItemType.isWeapon)
		{
			weaponToHandIn = possibleWeapons[Utilities.GetRandomNumber(possibleWeapons.Count - 1)];
			amount = 1;
		}
		else if (itemTypeToHandIn == ItemType.isArmor)
		{
			armorToHandIn = possibleArmors[Utilities.GetRandomNumber(possibleArmors.Count - 1)];
			amount = 1;
		}
		else if (itemTypeToHandIn == ItemType.isAccessory)
		{
			accessoryToHandIn = possibleAccessories[Utilities.GetRandomNumber(possibleAccessories.Count - 1)];
			amount = 1;
		}
		else if (itemTypeToHandIn == ItemType.isConsumable)
		{
			consumableToHandIn = possibleConsumables[Utilities.GetRandomNumber(possibleConsumables.Count - 1)];
			amount = 5;
		}
		InitilizeReward();
		InitilizeText();
	}
	public void InitilizeReward()
	{
		questRewardType = (RewardType)Utilities.GetRandomNumber(1);

		if (questRewardType == RewardType.isExpReward)
		{
			rewardToAdd = Utilities.GetRandomNumberBetween(100, 250);
			questRewardUi.text = $"Reward: {rewardToAdd} Exp";
		}
		if (questRewardType == RewardType.isGoldReward)
		{
			rewardToAdd = Utilities.GetRandomNumberBetween((int)(50 * PlayerInfoUi.playerInstance.playerStats.levelModifier), 
				(int)(75 * PlayerInfoUi.playerInstance.playerStats.levelModifier));
			questRewardUi.text = $"Reward: {rewardToAdd} Gold";
		}
	}

	//set up ui text
	public void InitilizeText()
	{
		SetUpQuestNameUi();
		SetUpQuestDescriptionUi();
		SetUpQuestRewardUi();
	}
	private void SetUpQuestNameUi()
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
			else
				Debug.LogError("no match for hand in item quest");
		}
		else
			Debug.LogError("no match for quest type");

		questNameUi.text = questName;
		this.questName = questName;
	}
	private void SetUpQuestDescriptionUi()
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
	private void SetUpQuestRewardUi()
	{
		if (questRewardType == RewardType.isExpReward)
			questRewardUi.text = $"Reward: {rewardToAdd} Exp";
		if (questRewardType == RewardType.isGoldReward)
			questRewardUi.text = $"Reward: {rewardToAdd} Gold";

		if (isCurrentlyActiveQuest) //hide accept quest button (for when reloading active quests OnGameLoad)
			acceptQuestButtonObj.SetActive(false);
	}
	public bool DoesHandInItemMatch(InventoryItemUi item)
	{
		if (itemTypeToHandIn == ItemType.isWeapon)
		{
			if (weaponToHandIn == item.weaponBaseRef)
				return true;
			else return false;
		}

		else if (itemTypeToHandIn == ItemType.isArmor)
		{
			if (armorToHandIn == item.armorBaseRef)
				return true;
			else return false;
		}

		else if (itemTypeToHandIn == ItemType.isAccessory)
		{
			if (accessoryToHandIn == item.accessoryBaseRef)
				return true;
			else return false;
		}

		else if (itemTypeToHandIn == ItemType.isConsumable)
		{
			if (consumableToHandIn == item.consumableBaseRef)
				return true;
			else return false;
		}
		else return false;
	}

	public void OnQuestAccepted(QuestDataSlotUi quest)
	{
		PlayerJournalUi.OnNewQuestAccepted -= quest.OnQuestAccepted;
	}

	//calls to global quest events to invoke said events passing in this quest info
	public void AcceptThisQuest() //button call
	{
		PlayerJournalUi.Instance.AcceptQuest(this);
	}
	public void CompleteThisQuest() //autoChecked
	{
		PlayerJournalUi.Instance.CompleteQuest(this);
	}
	public void AbandonThisQuest() //button call
	{
		PlayerJournalUi.Instance.AbandonQuest(this);
	}
}
