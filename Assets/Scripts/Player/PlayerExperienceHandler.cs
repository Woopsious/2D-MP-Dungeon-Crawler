using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class PlayerExperienceHandler : MonoBehaviour
{
	PlayerController playerRef;
	private EntityStats playerStats;

	public bool debugDisablePlayerLevelUp;
	private int maxLevel = 20;
	private int maxExp = 1000;
	public int currentExp;

	private void Awake()
	{
		playerRef = GetComponent<PlayerController>();
		playerStats = GetComponent<EntityStats>();
	}
	private void OnEnable()
	{
		SaveManager.ReloadSaveGameData += ReloadPlayerExp;
		ObjectPoolingManager.OnEntityDeathEvent += OnNonPlayerEntityDeaths;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
	}
	private void OnDisable()
	{
		SaveManager.ReloadSaveGameData -= ReloadPlayerExp;
		ObjectPoolingManager.OnEntityDeathEvent -= OnNonPlayerEntityDeaths;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
	}

	/// <summary>
	/// SYNCING PLAYER LEVELS BETWEEN CLIENTS PLAN:
	/// once save game data is restored completely on joining clients side.
	/// joining client calls rpc sending networked id of themselves + players current level.
	/// host client recieves call then sends id + player level to all clients.
	/// based on if this networked player objs owner id matches this id of player calling rpc. adjust level and stats of said player object
	/// </summary>

	//set player exp
	public void Start()
	{
		currentExp = 0;
		PlayerEventManager.PlayerExpChange(maxExp, currentExp);
	}

	//restore player exp data
	public void ReloadPlayerExp()
	{
		if (!playerRef.PlayerIsLocalPlayer()) return;

		currentExp = SaveManager.Instance.GameData.playerCurrentExp;
		PlayerEventManager.PlayerExpChange(maxExp, currentExp);
	}

	//ways of adding exp to local player
	public void DebugAddExp(int expToAdd)
	{
		AddExperience(expToAdd);
	}
	private void OnQuestComplete(QuestDataUi quest)
	{
		if (!playerRef.PlayerIsLocalPlayer()) return;

		if (quest.questRewardType == QuestDataUi.RewardType.isExpReward)
			AddExperience(quest.rewardToAdd);
	}
	private void OnNonPlayerEntityDeaths(GameObject Obj)
	{
		if (!playerRef.PlayerIsLocalPlayer()) return;

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

		AddExperience(expToAdd);
	}

	//apply exp to local player
	private void AddExperience(int expToAdd)
	{
		if (!playerRef.PlayerIsLocalPlayer()) return;

		if (playerStats.entityLevel >= maxLevel && currentExp >= 1000)
		{
			currentExp = 1000;
			playerStats.entityLevel = maxLevel;
			return;
		}

		currentExp += expToAdd;
		PlayerEventManager.PlayerExpChange(maxExp, currentExp);

		TryLevelUpPlayer();
	}
	private void TryLevelUpPlayer()
	{
		if (debugDisablePlayerLevelUp) return;
		if (currentExp >= maxExp)
		{
			if (playerStats.entityLevel < maxLevel)
			{
				int r = currentExp % maxExp;
				currentExp = r;

				playerStats.entityLevel++;
				playerStats.CalculateBaseStats();

				PlayerEventManager.PlayerExpChange(maxExp, currentExp);
				PlayerEventManager.PlayerLevelChange(playerRef);
			}
			else
			{
				currentExp = maxExp;
				PlayerEventManager.PlayerExpChange(maxExp, maxExp);
			}
		}
	}
}
