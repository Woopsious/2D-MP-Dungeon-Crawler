using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityClassHandler : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;

	public SOPlayerClasses currentEntityClass;

	public GameObject itemPrefab;

	[Header("Bonuses Provided By Class")]
	public int classHealth;
	public int classMana;

	public int classPhysicalResistance;
	public int classPoisonResistance;
	public int classFireResistance;
	public int classIceResistance;

	public int physicalDamagePercentage;
	public int poisonDamagePercentage;
	public int fireDamagePercentage;
	public int iceDamagePercentage;

	public event Action<SOPlayerClasses> OnClassChange;

	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		PlayerClassesUi.OnClassChange += OnClassChange;
	}
	private void OnDisable()
	{
		
	}

	private void Initilize()
	{
		entityStats = GetComponent<EntityStats>();
	}

	public void OnNewClassTreeUnlock()
	{

	}

	public void OnClassReset()
	{

	}

	public void OnClassChanges(SOPlayerClasses newPlayerClass)
	{
		OnClassChange?.Invoke(newPlayerClass);
	}
}
