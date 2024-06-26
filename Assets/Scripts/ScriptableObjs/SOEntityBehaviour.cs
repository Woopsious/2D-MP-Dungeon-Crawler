using UnityEngine;

[CreateAssetMenu(fileName = "ItemsScriptableObject", menuName = "Entities/Behaviours")]
public class SOEntityBehaviour : ScriptableObject
{
	[Header("Idle Behaviour")]
	public int idleWaitTime;
	[Tooltip("15 is min value")]
	public int idleWanderRadius;

	[Header("Attack Behaviour")]
	[Tooltip("max distance from player, 8 is min value")]
	public float aggroRange;
	[Tooltip("max distance till player looses aggro while in view, cant be lower then aggroRange x1.1")]
	public float maxChaseRange;

	[Header("Movement Behaviour")]
	[Tooltip("base value is 6, to match player Physics2D move speed")]
	public float navMeshMoveSpeed;
	[Tooltip("base value is 60")]
	public float navMeshTurnSpeed;
	[Tooltip("base value is 5")]
	public float navMeshAcceleration;
	[Tooltip("base value is 1.5 for sprites that are 1 tile in size")]
	public float navMeshStoppingDistance;
}
