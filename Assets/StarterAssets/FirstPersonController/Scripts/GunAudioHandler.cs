using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAudioHandler : MonoBehaviour {
    private readonly float[] pitchArray = { 0.85f, 0.95f, 1.01f, 1.15f };

    public static GunAudioHandler Instance { get; private set; }

    void Start() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else {
            Instance = this;
        }
    }

    public void PlayGunAudio(AudioSource gunShotAudio) {
        DontDestroyOnLoad(gunShotAudio);
        int randomPitch = Random.Range(0, pitchArray.Length);
        gunShotAudio.pitch = pitchArray[randomPitch];
        gunShotAudio.PlayOneShot(gunShotAudio.clip);
    }
}
