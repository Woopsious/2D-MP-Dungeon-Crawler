using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEyeBossPhases : EntityMovement, IBossPhases
{
	/// <summary> Eye boss behaviour for phase 1-3: 
	/// PHASE 1: stay on center platfrom using ranged attacks + abilities
	/// PHASE 2: stay on center platfrom using ranged attacks + abilities and its unique Marked By Eye ability
	/// when player marked by eye, show indicator ontop of player for player and all other players in game. once timer is up do aoe damage 
	/// in a line between boss and marked player. dealing x% damage to all players hit. 
	/// MAKING MARKED BY EYE ABILITY WORK:
	/// 50% of the time marked player will be player with highest aggro to boss, marking player done via abilities + status effects,
	/// save marked player as variable, once player has marked by eye casted on them either:
	/// have a second internal timer here that then calls another ability for aoe line attack (timer set to marked by eye effect duration)
	/// or update status effects script supporting a call back event once effects timer is up. telling script that applied said effect to 
	/// entity. the timer is up. do x now like calling bosses aoe line attack.
	/// 
	/// possibly have obstacals that spawn in that the marked player can hide behind thatll block only the aoe line abilities damage
	/// especially when fighting the boss alone or with a small group. maybe make it so less people = more obstacles to hide behind
	/// implementing it would require a line cast + new seethrough obstacles layer for these specific objects so they dont interfere
	/// with line of sight casting for regular obstacles.
	/// 
	/// PHASE 3: either partially reuse marked by eye ability and instead of doing aoe line attack do aoe attack ontop of marked player
	/// this aoe attack spreads its damage between all players within aoe, basically players are encourage to stand by marked player
	/// instead of avoiding them this time. can also lower damage delt based on how many players are so it can be solod.
	/// 
	/// or give it a more simple ability where itll attack all players it can currently see 
	/// possibly adjust damage dealt based on amount of players damaged, atm going with less players seen = more damage it does eg:
	/// ability deals 20 damage. 4 players hit = 80 damage. 2 players hit = 60 damage. 1 player hit = 40 damage)
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityEquipmentHandler equipmentHandler;

	public TaskEyeBossPhases(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition == false)
			RunPhases();

		return NodeState.RUNNING;
	}

	public void RunPhases()
	{
		Debug.Log(stats.name + " boss phase task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOne();
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwo();
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThree();
	}
	public void PhaseOne()
	{
		throw new System.NotImplementedException();
	}

	public void PhaseThree()
	{
		throw new System.NotImplementedException();
	}

	public void PhaseTwo()
	{
		throw new System.NotImplementedException();
	}
}
