using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class PlayerExperienceHandler : MonoBehaviour
{
	private PlayerController playerController;
	private EntityStats playerStats;

	public bool debugDisablePlayerLevelUp;
	private int maxLevel = 20;
	private int maxExp = 1000;
	public int currentExp;

	private void Awake()
	{
		playerController = GetComponent<PlayerController>();
		playerStats = GetComponent<EntityStats>();
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerExp;
		DungeonHandler.OnEntityDeathEvent += AddExperience;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
	}

	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerExp;
		DungeonHandler.OnEntityDeathEvent -= AddExperience;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
	}

	/// <summary>
	/// on enemy death event, add experience to this player if they are in range of enemy aggro range
	/// for mp loop through every player in range (PlayerController playerInRange will be a list when implamenting MP)
	/// if this PlayerController playerController isnt the same instance in playerInRange then return
	/// addExpEvent updates ui and adds exp to currentExp, then check if currentExp > maxExp
	/// if it is call onPlayerLevelUpEvent and update player level and stats as well as other things listed below
	/// new skill/spells if applicable...
	/// </summary>

	public void ReloadPlayerExp()
	{
		currentExp = SaveManager.Instance.GameData.playerCurrentExp;
		PlayerEventManager.PlayerExpChange(maxExp, currentExp);
	}

	private void OnQuestComplete(QuestDataSlotUi quest)
	{
		if (quest.questRewardType == QuestDataSlotUi.RewardType.isExpReward)
			AddExperience(quest.gameObject);
	}
	private void AddExperience(GameObject Obj)
	{
		if (Obj.GetComponent<QuestDataSlotUi>() != null)
			currentExp += Obj.GetComponent<QuestDataSlotUi>().rewardToAdd;
		else if (Obj.GetComponent<PlayerController>() == null && Obj.GetComponent<EntityStats>() != null)
		{
			//if (playerController != Obj.GetComponent<EntityBehaviour>().player) return;
			EntityStats otherEntityStats = Obj.GetComponent<EntityStats>();
			int expToAdd = otherEntityStats.entityBaseStats.expOnDeath;

			//lower exp given based on level difference (will rarely happen unless player levels up 3+ time in same dungeon)
			int levelDifference = playerStats.entityLevel - otherEntityStats.entityLevel;
			if (levelDifference == 3)
				expToAdd = (int)(currentExp * 1.75f);
			if (levelDifference == 4)
				expToAdd = (int)(currentExp * 1.5f);
			if (levelDifference >= 5)
				expToAdd = (int)(currentExp * 1.25f);

			currentExp += expToAdd;
		}
		else
			Debug.LogError("Error no components match for adding exp");

		PlayerEventManager.PlayerExpChange(maxExp, currentExp);

		if (CheckIfPLayerCanLevelUp()) return;
		OnPlayerLevelUp();
	}

	private void OnPlayerLevelUp()
	{
		int r = currentExp % maxExp;
		currentExp = r;

		PlayerEventManager.PlayerExpChange(maxExp, currentExp);
		PlayerEventManager.PlayerLevelUp(playerStats);
	}
	private bool CheckIfPLayerCanLevelUp()
	{
		if (debugDisablePlayerLevelUp) return false;
		if (playerStats.entityLevel >= maxLevel ) return false;

		if (currentExp >= maxExp)
			return true;
		else
			return false;
	}
}
