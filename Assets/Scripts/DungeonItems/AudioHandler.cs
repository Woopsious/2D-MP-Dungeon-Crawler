using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
	public List<AudioSource> musicAudioList = new List<AudioSource>();
	public List<AudioSource> menuSfxAudioList = new List<AudioSource>();

	public List<AudioSource> ambienceAudioList = new List<AudioSource>();
	public List<AudioSource> sfxAudioList = new List<AudioSource>();

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
		if (musicAudioList.Count != 0)
			UpdateAudio(musicAudioList, musicVolume);

		if (menuSfxAudioList.Count != 0)
			UpdateAudio(menuSfxAudioList, musicVolume);

		if (ambienceAudioList.Count != 0)
			UpdateAudio(ambienceAudioList, musicVolume);

		if (sfxAudioList.Count != 0)
			UpdateAudio(sfxAudioList, musicVolume);
	}
	private void UpdateAudio(List<AudioSource> audioSources, float newVolume)
	{
		foreach (AudioSource audioSource in audioSources)
			audioSource.volume = newVolume;
	}
}
