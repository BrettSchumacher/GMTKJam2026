using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioObject : MonoBehaviour
{
    private AudioSource audioSource;
    private bool played = false;

    void Awake()
    {
        this.audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Only destroy object after it is done playing.
        if (played && !audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    // Plays sound
    public void Play(AudioClip clip, float volume)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        played = true;
    }
}
