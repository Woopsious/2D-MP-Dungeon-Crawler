using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassesUi : MonoBehaviour
{
	public static PlayerClassesUi Instance;

	private void Start()
	{
		Instance = this;
	}
}
