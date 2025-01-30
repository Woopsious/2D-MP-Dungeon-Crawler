using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPartyUi : MonoBehaviour
{
	public static PlayerPartyUi Instance;

	public GameObject playerPartyPanelUi;

	public GameObject PlayerOnePanelUi;
	public GameObject PlayerTwoPanelUi;
	public GameObject PlayerThreePanelUi;
	public GameObject PlayerFourPanelUi;

	private void Awake()
	{
		Instance = this;
	}
}
