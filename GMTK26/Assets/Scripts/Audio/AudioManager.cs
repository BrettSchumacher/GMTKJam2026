using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Sounds
{
    Menu,
    Opening,
    Background,
    Pause,
    Ending,
    Rolling,
    Ollie,
    Grind,
    Fall
}
[System.Serializable]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public GameObject audioPrefab;

    [SerializeField] public static Dictionary<Sounds, AudioClip[]> SoundDict;
    void Awake()
    {
        if (Instance)
        {
            Debug.LogError("AudioManager instance already exists");
            return;
        }
        Instance = this;
    }
    private void OnDisable()
    {
        Instance = null;
    }

    public static void PlayAudio(Sounds track, Vector3 playAt, Transform attachTo, float delay = 0f, float pitch = 1f, bool stopWhenDestroyed = false)
    {
        if (Instance == null)
        {
            Debug.LogError("No AudioManager instance found");
            return;
        }

        GameObject audio = Instantiate(Instance.audioPrefab, playAt, Quaternion.identity);

        AudioClip[] clips = null;
        SoundDict.TryGetValue(track, out clips);
        if (clips == null)
        {
            Debug.LogError("AudioManager::PlayMusic - AudioClip missing for " + track);
            return;
        }

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

        audio.name = clip.name;

        if (attachTo != null)
        {
            audio.GetComponent<AudioTrackObject>()?.Initialize(playAt, attachTo, stopWhenDestroyed);
        }

        AudioSource audioSource = audio.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("Audio prefab has no audiosource");
            return;
        }

        audioSource.pitch = pitch;
        audioSource.clip = clip;
        if (delay > 0f)
        {
            audioSource.PlayDelayed(delay);
        }
        else
        {
            audioSource.Play();
        }

        Instance.StartCoroutine(DestroyAudioObj(audio, delay + clip.length + 0.2f));
    }
    static IEnumerator DestroyAudioObj(GameObject audio, float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(audio);
    }
}
