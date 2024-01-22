using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExperienceHandler : MonoBehaviour
{
	[HideInInspector] private PlayerController playerController;

	private int maxLevel = 50;
	private int maxExp = 1000;
	public int currentExp;

	private EntityStats playerStats;

	public event Action<int, int> onAddExperienceEvent;
	public event Action<int> onPlayerLevelUpEvent;

	public void Start()
	{
		playerController = GetComponent<PlayerController>();
		playerStats = GetComponent<EntityStats>();
	}

	/// <summary>
	/// on enemy death event, add experience to this player if they are in range of enemy aggro range
	/// for mp loop through every player in range (PlayerController playerInRange will be a list when implamenting MP)
	/// if this PlayerController playerController isnt the same instance in playerInRange then return
	/// addExpEvent updates ui and adds exp to currentExp, then check if currentExp > maxExp
	/// if it is call onPlayerLevelUpEvent and update player level and stats as well as other things listed below
	/// new skill/spells if applicable...
	/// </summary>

	public void AddExperience(GameObject Obj, PlayerController playerInRange)
	{
		return; //disabled for now
		if (playerController != playerInRange) return;

		Debug.Log("current exp amount: " + currentExp);
		currentExp += Obj.GetComponent<EntityStats>().entityBaseStats.expOnDeath;
		Debug.Log("exp added: " + Obj.GetComponent<EntityStats>().entityBaseStats.expOnDeath + "\nnew exp amount: " + currentExp);

		onAddExperienceEvent?.Invoke(currentExp, maxExp);

		if (!CheckIfPLayerCanLevelUp()) return;

		OnPlayerLevelUp();
	}

	public void OnPlayerLevelUp()
	{
		int r = currentExp % maxExp;
		currentExp = r;

		onAddExperienceEvent?.Invoke(currentExp, maxExp);
		onPlayerLevelUpEvent?.Invoke(playerStats.entityLevel + 1);
	}

	public bool CheckIfPLayerCanLevelUp()
	{
		if (playerStats.entityLevel >= maxLevel ) return false;

		if (currentExp >= maxExp)
			return true;
		else
			return false;
	}
}
