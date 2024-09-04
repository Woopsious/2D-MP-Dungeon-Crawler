using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public static event Action<float, float, float, float> OnAudioVolumeChangeEvent;

	public Slider musicVolumeSlider;
	public Slider menusSfxVolumeSlider;
	public Slider ambienceVolumeSlider;
	public Slider sfxVolumeSlider;

	public float musicVolume;
	public float menuSfxVolume;
	public float ambienceVolume;
	public float sfxVolume;

	private void Awake()
	{
		Instance = this;
	}
	
	//restore audio data
	public void RestoreAudioVolume(float musicVolume, float menuSfxVolume, float ambienceVolume, float sfxVolume)
	{
		this.musicVolume = musicVolume;
		this.menuSfxVolume = menuSfxVolume;
		this.ambienceVolume = ambienceVolume;
		this.sfxVolume = sfxVolume;

		UpdateAudioVolume();
	}

	//update audio event
	public void UpdateAudioVolume()
	{
		OnAudioVolumeChangeEvent?.Invoke(musicVolume, menuSfxVolume, ambienceVolume, sfxVolume);
	}

	//update slider ui values to match
	public void SetVolumeSliderValues()
	{
		musicVolumeSlider.value = musicVolume;
		menusSfxVolumeSlider.value = menuSfxVolume;
		ambienceVolumeSlider.value = ambienceVolume;
		sfxVolumeSlider.value = sfxVolume;
	}

	//ui slider events
	public void UpdateMusicVolumeSlider()
	{
		musicVolume = musicVolumeSlider.value;
	}
	public void UpdateMenuSfxVolumeSlider()
	{
		menuSfxVolume = menusSfxVolumeSlider.value;
	}
	public void UpdateAmbienceVolumeSlider()
	{
		ambienceVolume = ambienceVolumeSlider.value;
	}
	public void UpdateSfxVolumeSlider()
	{
		sfxVolume = sfxVolumeSlider.value;
	}
}
