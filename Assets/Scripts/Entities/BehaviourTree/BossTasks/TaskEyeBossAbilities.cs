using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEyeBossAbilities : EntityAbilities, IBossAbilities
{
	/// <summary>
	/// SPECIAL MARKED EYE ABILITY:
	/// effect marked by eye casted just before casting ability 2 or 3 on a semi random player for 10 seconds
	/// (TODO)
	/// when player marked by eye, show indicator ontop of player for player and all other players in game. once timer is up do aoe damage 
	/// in a line between boss and marked player. dealing x% damage to all players hit. 
	/// 
	/// ABILITY 1:
	/// simple directional ability
	/// 
	/// ABILITY 2:
	/// directional aoe line attack from boss directed at player marked by eye (takes 10s to cast)
	/// (TODO) 
	/// add ui indicator above marked player indicating to siad marked player and any other players they are marked.
	/// 
	/// ABILITY 3:
	/// (TODO) 
	/// either partially reuse marked by eye ability and instead of doing aoe line attack do aoe attack ontop of marked player
	/// this aoe attack spreads its damage between all players within aoe, basically players are encourage to stand by marked player
	/// instead of avoiding them this time. can also lower damage delt based on how many players are so it can be solod.
	/// 
	/// or give it a more simple ability where itll attack all players it can currently see 
	/// possibly adjust damage dealt based on amount of players damaged, atm going with less players seen = more damage it does eg:
	/// ability deals 20 damage. 4 players hit = 80 damage. 2 players hit = 60 damage. 1 player hit = 40 damage)
	/// 
	/// SHIT TO DO:
	/// possibly have obstacals that spawn in that the marked player can hide behind thatll block only the aoe line abilities damage
	/// especially when fighting the boss alone or with a small group. maybe make it so less people = more obstacles to hide behind
	/// implementing it would require a line cast + new seethrough obstacles layer for these specific objects so they dont interfere
	/// with line of sight casting for regular obstacles.
	/// </summary>

	readonly SOBossEntityBehaviour bossBehaviourRef;
	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;

public TaskEyeBossAbilities(BossEntityBehaviour behaviour)
	{
		bossBehaviourRef = (SOBossEntityBehaviour)behaviour.behaviourRef;
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;
		else
		{
			if (CanUseBossAbilityOne())
			{
				CastAbilityOne(behaviour);
			}
			else if (CanUseBossAbilityTwo())
			{
				CastAbilityTwo(behaviour);
			}
			else if (CanUseBossAbilityThree())
			{
				CastAbilityThree(behaviour);
			}
			else return NodeState.FAILURE;

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseBossAbilityOne()
	{
		if (behaviour.abilityBeingCasted || stats.inPhaseTransition ||
			!behaviour.canCastAbilityOne || !HasEnoughManaToCast(stats, behaviour.abilityOne)) return false;
		else return true;
	}
	public bool CanUseBossAbilityTwo()
	{
		if (behaviour.abilityBeingCasted || stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.secondPhase ||
			!behaviour.canCastAbilityTwo || !HasEnoughManaToCast(stats, behaviour.abilityTwo)) return false;
		else return true;
	}
	public bool CanUseBossAbilityThree()
	{
		if (behaviour.abilityBeingCasted || stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.thirdPhase ||
			!behaviour.canCastAbilityThree || !HasEnoughManaToCast(stats, behaviour.abilityThree)) return false;
		else return true;
	}
}
