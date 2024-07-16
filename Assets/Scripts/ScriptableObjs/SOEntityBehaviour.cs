using UnityEngine;

[CreateAssetMenu(fileName = "ItemsScriptableObject", menuName = "Entities/Behaviours")]
public class SOEntityBehaviour : ScriptableObject
{
	[Header("Idle Behaviour")]
	public int idleWaitTime;
	[Tooltip("standard value is 15")]
	public int idleWanderRadius;

	[Header("Attack Behaviour")]
	[Tooltip("max distance from player, standard value is 8")]
	public float aggroRange;
	[Tooltip("max distance till player looses aggro while in view, cant be lower then aggroRange x1.1")]
	public float maxChaseRange;

	[Header("Movement Behaviour")]
	[Tooltip("standard value is 6, to match player Physics2D move speed")]
	public float navMeshMoveSpeed;
	[Tooltip("standard value is 60")]
	public float navMeshTurnSpeed;
	[Tooltip("standard value is 5")]
	public float navMeshAcceleration;
	[Tooltip("standard value is 1.25 for sprites that are 1 tile in size")]
	public float navMeshStoppingDistance;
}
