using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class PlayerExperienceHandler : MonoBehaviour
{
	[HideInInspector] private PlayerController playerController;

	public bool debugDisablePlayerLevelUp;
	private int maxLevel = 25;
	private int maxExp = 1000;
	public int currentExp;

	private EntityStats playerStats;

	private void Start()
	{
		playerController = GetComponent<PlayerController>();
		playerStats = GetComponent<EntityStats>();
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerExp;
		EventManager.OnDeathEvent += AddExperience;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
	}

	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerExp;
		EventManager.OnDeathEvent -= AddExperience;
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
		EventManager.PlayerExpChange(maxExp, currentExp);
	}

	private void OnQuestComplete(QuestSlotsUi quest)
	{
		if (quest.questRewardType == QuestSlotsUi.RewardType.isExpReward)
			AddExperience(quest.gameObject);
	}
	public void AddExperience(GameObject Obj)
	{
		if (Obj.GetComponent<QuestSlotsUi>() != null)
			currentExp += Obj.GetComponent<QuestSlotsUi>().rewardToAdd;
		else
		{
			if (playerController != Obj.GetComponent<EntityBehaviour>().player) return;
			currentExp += Obj.GetComponent<EntityStats>().entityBaseStats.expOnDeath;
		}
		EventManager.PlayerExpChange(maxExp, currentExp);

		if (CheckIfPLayerCanLevelUp()) return;
		OnPlayerLevelUp();
	}

	private void OnPlayerLevelUp()
	{
		int r = currentExp % maxExp;
		currentExp = r;

		EventManager.PlayerExpChange(maxExp, currentExp);
		EventManager.PlayerLevelUp(playerStats);
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
