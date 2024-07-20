using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
	public AudioType audioType;
	public enum AudioType
	{
		music, menuSfx, ambience, sfx
	}	

	public AudioSource audioSource;

	private void Start()
	{
		AudioManager aM = AudioManager.Instance;
		UpdateAudioSettings(aM.musicVolume, aM.menuSfxVolume, aM.ambienceVolume, aM.sfxVolume);
	}

	private void OnEnable()
	{
		AudioManager.OnAudioVolumeChangeEvent += UpdateAudioSettings;
	}
	private void OnDisable()
	{
		AudioManager.OnAudioVolumeChangeEvent -= UpdateAudioSettings;
	}

	private void UpdateAudioSettings(float musicVolume, float menuSfxVolume, float ambienceVolume, float sfxVolume)
	{
		if (audioType == AudioType.music)
			audioSource.volume = musicVolume;

		if (audioType == AudioType.menuSfx)
			audioSource.volume = menuSfxVolume;

		if (audioType == AudioType.ambience)
			audioSource.volume = ambienceVolume;

		if (audioType == AudioType.sfx)
			audioSource.volume = sfxVolume;
	}

	public void PlayAudio(AudioClip clip)
	{
		if (IsAudioPlaying())
			audioSource.Stop();

		audioSource.clip = clip;
		audioSource.Play();
	}
	public bool IsAudioPlaying()
	{
		if (audioSource.isPlaying)
			return true;
		else return false;
	}
}
