using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class PlayerExperienceHandler : MonoBehaviour
{
	[HideInInspector] private PlayerController playerController;

	private int maxLevel = 50;
	private int maxExp = 1000;
	public int currentExp;

	private EntityStats playerStats;

	public event Action<int, int> OnAddExperienceEvent;
	public event Action<int> OnPlayerLevelUpEvent;

	private void Start()
	{
		playerController = GetComponent<PlayerController>();
		playerStats = GetComponent<EntityStats>();

		OnAddExperienceEvent += PlayerHotbarUi.Instance.OnExperienceChange; //would be in OnEnable but i get null ref
	}

	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerExp;
	}

	private void OnDisable()
	{
		OnAddExperienceEvent -= PlayerHotbarUi.Instance.OnExperienceChange;
		SaveManager.OnGameLoad -= ReloadPlayerExp;
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
		OnAddExperienceEvent?.Invoke(maxExp, currentExp);
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
		OnAddExperienceEvent?.Invoke(maxExp, currentExp);

		if (!CheckIfPLayerCanLevelUp()) return;
		OnPlayerLevelUp();
	}

	private void OnPlayerLevelUp()
	{
		int r = currentExp % maxExp;
		currentExp = r;

		OnAddExperienceEvent?.Invoke(maxExp, currentExp);
		OnPlayerLevelUpEvent?.Invoke(playerStats.entityLevel + 1);
	}
	private bool CheckIfPLayerCanLevelUp()
	{
		if (playerStats.entityLevel >= maxLevel ) return false;

		if (currentExp >= maxExp)
			return true;
		else
			return false;
	}
}
