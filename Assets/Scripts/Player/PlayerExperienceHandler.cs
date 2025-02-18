using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class PlayerExperienceHandler : MonoBehaviour
{
	PlayerController player;
	private EntityStats playerStats;

	public bool debugDisablePlayerLevelUp;
	private int maxLevel = 20;
	private int maxExp = 1000;
	public int currentExp;

	private void Awake()
	{
		player = GetComponent<PlayerController>();
		playerStats = GetComponent<EntityStats>();
	}
	private void OnEnable()
	{
		SaveManager.ReloadSaveGameData += ReloadPlayerExp;
		ObjectPoolingManager.OnEntityDeathEvent += AddExperience;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
	}
	private void OnDisable()
	{
		SaveManager.ReloadSaveGameData -= ReloadPlayerExp;
		ObjectPoolingManager.OnEntityDeathEvent -= AddExperience;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
	}
	private void Start()
	{
		PlayerEventManager.PlayerExpChange(maxExp, currentExp);
	}

	/// <summary>
	/// SYNCING PLAYER LEVELS BETWEEN CLIENTS PLAN:
	/// once save game data is restored completely on joining clients side.
	/// joining client calls rpc sending networked id of themselves + players current level.
	/// host client recieves call then sends id + player level to all clients.
	/// based on if this networked player objs owner id matches this id of player calling rpc. adjust level and stats of said player object
	/// </summary>

	//restore player exp data
	public void ReloadPlayerExp()
	{
		currentExp = SaveManager.Instance.GameData.playerCurrentExp;
		PlayerEventManager.PlayerExpChange(maxExp, currentExp);
	}

	//add exp to player
	private void OnQuestComplete(QuestDataUi quest)
	{
		if (quest.questRewardType == QuestDataUi.RewardType.isExpReward)
			AddExperience(quest.gameObject);
	}
	private void AddExperience(GameObject Obj)
	{
		if (Obj.GetComponent<QuestDataUi>() != null)
			currentExp += Obj.GetComponent<QuestDataUi>().rewardToAdd;
		else if (Obj.GetComponent<PlayerController>() == null && Obj.GetComponent<EntityStats>() != null)
		{
			EntityStats otherEntityStats = Obj.GetComponent<EntityStats>();
			int expToAdd = otherEntityStats.statsRef.expOnDeath;

			//reduce exp given based on level difference (should rarely happen as entities scale to player)
			int levelDifference = playerStats.entityLevel - otherEntityStats.entityLevel;
			if (levelDifference == 3)
				expToAdd = (int)(expToAdd * 0.75f);
			if (levelDifference == 4)
				expToAdd = (int)(expToAdd * 0.5f);
			if (levelDifference >= 5)
				expToAdd = (int)(expToAdd * 0.25f);

			currentExp += expToAdd;
		}
		else
			Debug.LogError("Error no components match for adding exp");

		PlayerEventManager.PlayerExpChange(maxExp, currentExp);

		if (CheckIfPLayerCanLevelUp()) return;
		OnPlayerLevelUp();
	}

	//level up player
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
