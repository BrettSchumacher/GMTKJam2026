using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum MusicTracks
{
    Menu,
    Opening,
    Background,
    Dialogue_Light,
    Dialogue_Bright,
    Dialogue_Heavy,
    Pause,
    Ending
}
public enum Sounds
{
    Rolling,
    Ollie,
    Grind,
    Fall
}
[System.Serializable]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource MusicSource; 
    public GameObject playerPrefab;
    public GameObject audioPrefab;

    [SerializedDictionary("Sound", "AudioClips")]
    public SerializedDictionary<Sounds, AudioClip[]> SoundDict = new();

    [SerializedDictionary("Music track", "AudioClip")]
    public SerializedDictionary<MusicTracks, AudioClip> MusicTracks = new(); 
    void Awake()
    {
        if (Instance)
        {
            Debug.LogError("AudioManager instance already exists");
            return;
        }
        Instance = this;
        MusicSource = playerPrefab.GetComponent<AudioSource>();
    }
    private void OnDisable()
    {
        Instance = null;
    }

    public static void PlayBackgroundMusic(MusicTracks track, float volume = 1f, float delay = 0f)
    {
        if (Instance == null)
        {
            Debug.LogError("No AudioManager instance found");
            return;
        }
        if(Instance.MusicSource == null)
        {
            Debug.LogError("No music source set");
            return;
        }

        AudioClip clip = null;
        Instance.MusicTracks.TryGetValue(track, out clip);
        if (clip == null)
        {
            Debug.LogError("Music track for " + track + " missing");
            return;
        }
        if (Instance.MusicSource.isPlaying)
        {
            Instance.MusicSource.Stop();
        }
        Instance.MusicSource.clip = clip;
        Instance.MusicSource.loop = true;
        Instance.MusicSource.volume = volume;

        if (delay > 0f)
        {
            Instance.MusicSource.PlayDelayed(delay);
        }
        else
        {
            Instance.MusicSource.Play();
        }
    }

    public static void PlayAudio(Sounds track, Vector3 playAt, Transform attachTo = null, float delay = 0f, float pitch = 1f, bool stopWhenDestroyed = false)
    {
        if (Instance == null)
        {
            Debug.LogError("No AudioManager instance found");
            return;
        }

        GameObject audio = Instantiate(Instance.audioPrefab, playAt, Quaternion.identity);

        AudioClip[] clips = null;
        Instance.SoundDict.TryGetValue(track, out clips);
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
