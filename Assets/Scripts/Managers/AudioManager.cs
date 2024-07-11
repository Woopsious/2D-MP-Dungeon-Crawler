using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public static event Action<float, float, float, float> OnAudioVolumeChangeEvent;

	public float musicVolume { private set; get; }
	public float menuSfxVolume { private set; get; }
	public float ambienceVolume { private set; get; }
	public float sfxVolume { private set; get; }

	private void Awake()
	{
		Instance = this;
	}
}
