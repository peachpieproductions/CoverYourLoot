using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Sound {
    public AudioClip audioClip;
    public float vol;
    public bool pitchChange;
}

public class AudioManager : MonoBehaviour {

    public Sound[] snds;
    internal AudioSource AS;
    public static AudioManager am;

	void Start () {
        AS = gameObject.AddComponent<AudioSource>();
        am = this;
	}
	
	void Update () {
		
	}

    public void PlaySound(int index) {
        var clip = snds[index].audioClip;
        if (snds[index].pitchChange) AS.pitch = Random.Range(.8f, 1.2f);
        else AS.pitch = 1;
        AS.PlayOneShot(clip, snds[index].vol);
    }

    public void PlayClip(AudioClip ac, bool pitchShift = true, float vol = 1) {
        if (pitchShift) AS.pitch = Random.Range(.8f, 1.2f);
        else AS.pitch = 1;
        AS.PlayOneShot(ac, vol);
    }

}
