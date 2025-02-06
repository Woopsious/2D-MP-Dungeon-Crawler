using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetDatabase : MonoBehaviour
{
	public static AssetDatabase Database;

	public List<SOEntityStats> entities = new List<SOEntityStats>();

	private void Awake()
	{
		Database = GetComponent<AssetDatabase>();
	}
}
