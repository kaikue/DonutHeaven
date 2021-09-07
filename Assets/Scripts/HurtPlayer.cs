using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtPlayer : MonoBehaviour
{
    private const float length = 0.75f;
    private float time = 0;
    private Player player;
    public AudioClip[] voiceSounds;
    public AudioClip[] hitSounds;

    private void Start()
    {
        PlayRandomSound(voiceSounds);
        PlayRandomSound(hitSounds);
    }

    private void PlayRandomSound(AudioClip[] sounds)
	{
        int i = Random.Range(0, sounds.Length);
        AudioClip sound = sounds[i];
        gameObject.GetComponent<AudioSource>().PlayOneShot(sound);
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
        GetComponent<SpriteRenderer>().flipX = player.facingLeft;
    }

    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time >= length)
        {
            player.gameObject.SetActive(true);
            player.Respawn();
            Destroy(gameObject);
        }
    }
}
