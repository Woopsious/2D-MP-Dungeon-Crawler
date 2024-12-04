using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System;

public class MultiplayerManager : MonoBehaviour
{
	public bool isMultiplayer;
}

public static class AuthenticationHandler
{
	public static async void AuthenticateUser()
	{
		try
		{
			await UnityServices.InitializeAsync();
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
}
