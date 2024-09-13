using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class TileMapHazardsManager : MonoBehaviour
{
	public static TileMapHazardsManager Instance;

	public Tilemap map;
	public List<TileData> tileDatas;

	private Dictionary<TileBase, TileData> dataFromTiles;

	private void Awake()
	{
		Instance = this;
		dataFromTiles = new Dictionary<TileBase, TileData>();

		foreach (var tileData in tileDatas)
		{
			foreach (var tile in tileData.tiles)
				dataFromTiles.Add(tile, tileData);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.GetComponent<EntityStats>() == null) return;

		TryApplyEnviromentalEffect(collision);
	}

	//apply enviromental effects
	private void TryApplyEnviromentalEffect(Collider2D collision)
	{
		Vector3Int gridPos = map.WorldToCell(collision.transform.position);
		TileBase tile = CheckSurroundingTiles(gridPos);

		if (tile == null)
			Debug.LogError("no tile or surrounding tile found, this shouldnt happen");



		if (collision.GetComponent<Damageable>() != null && dataFromTiles[tile].doesInstantDamage)
		{
			int damageToDo = (int)(dataFromTiles[tile].baseDamageDealt * collision.GetComponent<EntityStats>().levelModifier);
			collision.GetComponent<Damageable>().OnHitFromDamageSource(null, collision, damageToDo,
				(IDamagable.DamageType)dataFromTiles[tile].baseDamageType, 0, false, false, true);
		}

		if (collision.GetComponent<EntityStats>() != null && dataFromTiles[tile].appliesEffect)
			collision.GetComponent<EntityStats>().ApplyNewStatusEffects(
				dataFromTiles[tile].effectsToApply, collision.GetComponent<EntityStats>());
	}
	public void TryReApplyEffect(EntityStats entity)
	{
		Vector3Int gridPos = map.WorldToCell(entity.transform.position);
		TileBase tile = CheckCurrentTile(gridPos);

		if (tile != null)
			entity.ApplyNewStatusEffects(dataFromTiles[tile].effectsToApply, entity);
	}

	//tile checks
	private TileBase CheckCurrentTile(Vector3Int objPos)
	{
		if (map.GetTile(objPos) != null)
			return map.GetTile(objPos);
		else return null;
	}
	private TileBase CheckSurroundingTiles(Vector3Int objPos)
	{
		TileBase currentTile = CheckCurrentTile(objPos);
		if (currentTile != null)
			return currentTile;

		foreach (var neighbourPosition in neighbourPositions)
		{
			Vector3Int surroundingTilePos = objPos + neighbourPosition;

			if (map.GetTile(surroundingTilePos) == null) continue;
			else
				return map.GetTile(surroundingTilePos);
		}
		return null;
	}
	private readonly Vector3Int[] neighbourPositions =
	{
		Vector3Int.up,
		Vector3Int.right,
		Vector3Int.down,
		Vector3Int.left,
		Vector3Int.up + Vector3Int.right,
		Vector3Int.up + Vector3Int.left,
		Vector3Int.down + Vector3Int.right,
		Vector3Int.down + Vector3Int.left
	};
}