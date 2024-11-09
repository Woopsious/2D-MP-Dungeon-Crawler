using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/BossAbilities")]
public class SOBossAbilities : SOAbilities
{
	[Header("Boss Ability Settings")]
	public bool marksPlayer;
	public bool spawnsObstacles;

	[Header("Player Marking Settings")]
	public bool markedPlayerIsRandom;
	[Range(0f, 0.5f)]
	public float chanceMarkedPlayerIsAggroTarget;

	[Header("Spawnt Obstacles Settings")]
	[Tooltip("leave empty for random positions")]
	[Range(5f, 25f)]
	public float obstaclesRadius;
	public List<Vector2> obstaclePositions = new List<Vector2>();
}
