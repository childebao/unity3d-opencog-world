using UnityEngine;
using System.Collections;

public class PlaySound : MonoBehaviour
{

	public AudioSource audioSource;
	public AudioClip sound;
	
	public void Awake () 
	{
		if (!audioSource && audio)
			audioSource = audio;
	}
	
	public void OnSignal () 
	{
		if (sound)
			audioSource.clip = sound;
		audioSource.Play ();
	}
}

