using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityClassHandler : MonoBehaviour
{
	public SOPlayerClasses currentEntityClass;

	public GameObject itemPrefab;

	[HideInInspector] private EntityStats entityStats;
	[HideInInspector] public bool isPlayerEquipment;

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

	}
	private void OnDisable()
	{
		
	}

	private void Initilize()
	{

	}

	public void ChangePlayerClass(SOPlayerClasses newPlayerClass)
	{
		OnClassChange?.Invoke(newPlayerClass);
	}
}
