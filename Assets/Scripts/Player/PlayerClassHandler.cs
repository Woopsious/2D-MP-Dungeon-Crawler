using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerClass;
		PlayerClassesUi.OnClassChange += OnClassChanges;
		PlayerClassesUi.OnClassReset += OnClassReset;
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerClass;
		PlayerClassesUi.OnClassChange -= OnClassChanges;
		PlayerClassesUi.OnClassReset -= OnClassReset;
	}

	private void ReloadPlayerClass()
	{

	}
}
