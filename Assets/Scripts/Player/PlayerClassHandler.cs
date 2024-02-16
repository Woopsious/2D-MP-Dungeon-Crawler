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
		PlayerClassesUi.OnClassChange += OnClassChanges;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassChange -= OnClassChanges;
	}
}
