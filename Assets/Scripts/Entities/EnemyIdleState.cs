using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
	public override void Enter(EntityBehaviour entity)
	{

	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		entity.UpdatePlayerPosition();

		if (entity.playersLastKnownPosition == Vector2.zero)
		{
			// 1. idle and randomly move around the map within bounds of where they spawned
			entity.IdleAtPositionTimer();
			entity.CheckDistance();
		}
		else if (entity.playersLastKnownPosition != Vector2.zero)
		{
			// 2. when play enters agro range, chase player endless till they escape max chase range
			// 2A. if they escape max chase range move to last know position
			// 2B. if player moves out of visible range move to last know position
			// 3. once there, if player not found go back to step 1.
			// 4. once there, if player is found return to step 2.

			if (entity.CheckIfPlayerVisible())
				entity.ChasePlayer();
			else
				entity.CheckDistance();
		}
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{

	}
}
