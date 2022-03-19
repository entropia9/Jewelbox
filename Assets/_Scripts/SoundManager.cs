using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip gameOverClip;
    [Range(0, 1)]
    public float musicVolume = 0.5f;

    [Range(0, 1)]
    public float fxVolume = 1.0f;

    public float lowPitch = 0.95f;
    public float highPitch = 1.05f;
    public AudioClip[] LeftRightButtonSounds;
    public AudioClip[] VerticalHorizontalButtonSounds;
    // Start is called before the first frame update


    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX" + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;

            source.volume = volume;

            source.Play();
            Destroy(go, clip.length);
            return source;
        }

        return null;
    }

    public void PlayLoseSound()
    {
        PlayClipAtPoint(gameOverClip, Vector3.zero, fxVolume * 0.5f);
    }

    public void PlaySwitchModeSound(int index, AudioClip[] clipsSet)
    {
        PlayClipAtPoint(clipsSet[index], Vector3.zero);
    }
}
