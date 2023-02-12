using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateBand : Group
{
    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void onDefeat() {
        GameManager.instance.GameOver(true);
    }

    public override void onKill(bool melee) {
        if (melee) audioSource.PlayOneShot(audioSource.clip, audioSource.volume);
    }
}
