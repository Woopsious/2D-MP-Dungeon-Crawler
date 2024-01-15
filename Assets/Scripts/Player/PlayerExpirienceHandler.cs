using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExperienceHandler : MonoBehaviour
{
	private int maxLevel = 50;
	private int maxExp = 1000;
	public int currentExp;

	EntityStats playerStats;

	public event Action<int> addExpEvent;
	public event Action<int> onPlayerLevelUpEvent;

	/// <summary>
	/// on an enemies death add experience to this player if they are in range of enemy aggro range (makes it easy to add exp to multiple players in mp)
	/// addExpEvent updates ui and adds exp to currentExp, then check if currentExp > maxExp
	/// if it is call onPlayerLevelUpEvent and update player level and stats as well as other things listed below
	/// new skill/spells if applicable...
	/// 
	/// </summary>
}
