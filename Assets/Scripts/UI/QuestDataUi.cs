using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;

public class QuestDataUi : MonoBehaviour
{
	[Header("Quest Ui")]
	public TMP_Text questNameUi;
	public TMP_Text questDescriptionUi;
	public TMP_Text questTrackerUi;
	public TMP_Text questRewardUi;
	public Image questImage;
	public GameObject acceptQuestButtonObj;

	public GameObject DebugCompleteQuestButton;

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
	public SOEntityStats.HumanoidTypes humanoidTypeToKill;

	public SOEntityStats entityToKill;

	[Header("Item Quest Info")]
	public SOWeapons weaponToHandIn;
	public SOArmors armorToHandIn;
	public SOAccessories accessoryToHandIn;
	public SOConsumables consumableToHandIn;

	public SOItems.ItemType itemTypeToHandIn;

	[Header("Quest Reward")]
	public RewardType questRewardType;
	public enum RewardType
	{
		isExpReward, isGoldReward
	}
	public int rewardToAdd;

	private void OnDisable()
	{
		PlayerJournalUi.OnNewQuestAccepted -= OnQuestAccepted;
	}

	//set up quest data
	public void InitilizeBossKillQuest()
	{
		questType = QuestType.isBossKillQuest;
		entityToKill = AssetDatabase.Database.bossEntities[Utilities.GetRandomNumber(AssetDatabase.Database.bossEntities.Count - 1)];
		amount = 1;
		InitilizeReward();
		InitilizeText();
	}
	public void InitilizeKillQuest()
	{
		questType = QuestType.isKillQuest;
		entityToKill = AssetDatabase.Database.entities[Utilities.GetRandomNumber(AssetDatabase.Database.entities.Count - 1)];
		amount = Utilities.GetRandomNumberBetween(5, 8);
		InitilizeReward();
		InitilizeText();
	}
	public void InitilizeItemHandInQuest()
	{
		questType = QuestType.isItemHandInQuest;
		itemTypeToHandIn = (SOItems.ItemType)Utilities.GetRandomNumber(3);

		if (itemTypeToHandIn == SOItems.ItemType.isWeapon)
		{
			weaponToHandIn = AssetDatabase.Database.weapons[Utilities.GetRandomNumber(AssetDatabase.Database.weapons.Count - 1)];
			amount = 1;
		}
		else if (itemTypeToHandIn == SOItems.ItemType.isArmor)
		{
			armorToHandIn = AssetDatabase.Database.armours[Utilities.GetRandomNumber(AssetDatabase.Database.armours.Count - 1)];
			amount = 1;
		}
		else if (itemTypeToHandIn == SOItems.ItemType.isAccessory)
		{
			accessoryToHandIn = AssetDatabase.Database.accessories[Utilities.GetRandomNumber(AssetDatabase.Database.accessories.Count - 1)];
			amount = 1;
		}
		else if (itemTypeToHandIn == SOItems.ItemType.isConsumable)
		{
			consumableToHandIn = AssetDatabase.Database.consumables[Utilities.GetRandomNumber(AssetDatabase.Database.consumables.Count - 1)];
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
			rewardToAdd = Utilities.GetRandomNumberBetween((int)(50 * GameManager.Localplayer.playerStats.levelModifier), 
				(int)(75 * GameManager.Localplayer.playerStats.levelModifier));
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
			questImage.sprite = entityToKill.sprite;
		}
		else if (questType == QuestType.isKillQuest)
		{
			questName = $"Kill {amount} {entityToKill.entityName}'s";
			questImage.sprite = entityToKill.sprite;
		}
		else if(questType == QuestType.isItemHandInQuest)
		{
			if (itemTypeToHandIn == SOItems.ItemType.isWeapon)
			{
				questName = $"Hand In {amount} {weaponToHandIn.itemName}";
				questImage.sprite = weaponToHandIn.itemImage;
			}
			else if (itemTypeToHandIn == SOItems.ItemType.isArmor)
			{
				questName = $"Hand In {amount} {armorToHandIn.itemName}";
				questImage.sprite = armorToHandIn.itemImage;
			}
			else if (itemTypeToHandIn == SOItems.ItemType.isAccessory)
			{
				questName = $"Hand In {amount} {accessoryToHandIn.itemName}";
				questImage.sprite = accessoryToHandIn.itemImage;
			}
			else if (itemTypeToHandIn == SOItems.ItemType.isConsumable)
			{
				questName = $"Hand In {amount} {consumableToHandIn.itemName}";
				questImage.sprite = consumableToHandIn.itemImage;
			}
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
			questDescription = $"Venture into a dungeon and kill {amount} {entityToKill.entityName}'s.";
			questTrackerUi.text = $"{currentAmount} / {amount} Killed";
		}
		else if(questType == QuestType.isItemHandInQuest)
		{
			if (itemTypeToHandIn == SOItems.ItemType.isWeapon)
				questDescription = $"Collect {amount} " +
					$"{weaponToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			else if (itemTypeToHandIn == SOItems.ItemType.isArmor)
				questDescription = $"Collect {amount} " +
					$"{armorToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			else if (itemTypeToHandIn == SOItems.ItemType.isAccessory)
				questDescription = $"Collect {amount} " +
					$"{accessoryToHandIn.itemName} and hand them in via your inventory. (items will be marked)";

			else if (itemTypeToHandIn == SOItems.ItemType.isConsumable)
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

	//item match check
	public bool DoesHandInItemMatch(InventoryItemUi item)
	{
		if (itemTypeToHandIn == SOItems.ItemType.isWeapon)
		{
			if (weaponToHandIn == item.weaponBaseRef)
				return true;
			else return false;
		}

		else if (itemTypeToHandIn == SOItems.ItemType.isArmor)
		{
			if (armorToHandIn == item.armorBaseRef)
				return true;
			else return false;
		}

		else if (itemTypeToHandIn == SOItems.ItemType.isAccessory)
		{
			if (accessoryToHandIn == item.accessoryBaseRef)
				return true;
			else return false;
		}

		else if (itemTypeToHandIn == SOItems.ItemType.isConsumable)
		{
			if (consumableToHandIn == item.consumableBaseRef)
				return true;
			else return false;
		}
		else return false;
	}

	//quest events
	public void OnQuestAccepted(QuestDataUi quest)
	{
		PlayerJournalUi.OnNewQuestAccepted -= quest.OnQuestAccepted;
	}
	private void UpdateShowDebugUi(bool showDebugUi)
	{
		if (showDebugUi)
			DebugCompleteQuestButton.SetActive(true);
		else
			DebugCompleteQuestButton.SetActive(false);
	}

	//quest actions
	public void AcceptThisQuest() //button call
	{
		PlayerJournalUi.Instance.AcceptQuest(this);
		DebugUi.UpdateShowDebubUiEvent += UpdateShowDebugUi;
	}
	public void CompleteThisQuest() //autoChecked
	{
		PlayerJournalUi.Instance.CompleteQuest(this);
		DebugUi.UpdateShowDebubUiEvent -= UpdateShowDebugUi;
	}
	public void AbandonThisQuest() //button call
	{
		PlayerJournalUi.Instance.AbandonQuest(this);
		DebugUi.UpdateShowDebubUiEvent -= UpdateShowDebugUi;
	}
}
