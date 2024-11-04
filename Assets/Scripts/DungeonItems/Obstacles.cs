using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
	//private LootSpawnHandler lootSpawnHandler;
	private BoxCollider2D boxCollider2D;
	//private Animator animator;
	private SpriteRenderer spriteRenderer;
	//private AudioHandler audioHandler;

	public ObstacleType obstacleType;
	public enum ObstacleType
	{
		isBossRoomObstacle
	}

	private void Awake()
	{
		//lootSpawnHandler = GetComponent<LootSpawnHandler>();
		boxCollider2D = GetComponent<BoxCollider2D>();
		//animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		//audioHandler = GetComponent<AudioHandler>();
	}

	private void Start()
	{
		//based on enum generate objects of similar type with different settings like interactable obstacles, destructable/undestructable
		//obstacles etc... could also use scriptable objects if/when more objects are added to store obstacle data
		Initilize(obstacleType);
	}

	public void Initilize(ObstacleType obstacleType)
	{
		//noop
	}

	public void InitilizeBossRoomObstacle(float timeToDissapear)
	{
		gameObject.layer = 13;
		StartCoroutine(ClearObstacleTimer(timeToDissapear + 3f));
	}

	private IEnumerator ClearObstacleTimer(float timeToDissapear)
	{
		yield return new WaitForSeconds(timeToDissapear);
		ClearObstacle();
	}

	private void ClearObstacle()
	{
		Destroy(gameObject);
	}
}
