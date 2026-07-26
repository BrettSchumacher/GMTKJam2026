using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance;
    
    public GameObject IntroCanvas;
    public GameObject[] ObjectsToHideOnEnd;
    public AudioClip Sirens;
    public float introSirensDuration;
    public AnimationCurve introSirensVolume;
    public float outroSirensDuration;
    public AnimationCurve outroSirensVolume;
    public AudioClip EmergencyBroadcast;
    public ConversationSO StartingConversation;

    private AudioSource sirensSource;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("DUPLICATE");
            return;
        }

        Instance = this;
    }

    public void StartIntro()
    {
        if (!PlayerManager.Instance)
        {
            Debug.LogError("NO PLAYER");
            return;
        }
        
        GameManager.gameManager?.PauseGameplay();

        if (InputManager.Instance)
        {
            InputManager.Instance.PushInputState(InputState.Cutscene);
        }
        
        IntroCanvas.SetActive(true);
        
        Transform player = PlayerManager.Instance.transform;
        sirensSource = AudioManager.PlayAudio(Sirens, player.position, player);
        if (sirensSource)
        {
            StartCoroutine(PlaySirens(true, PlayEmergencyBroadcast));
        }
        else
        {
            PlayEmergencyBroadcast();
        }
    }

    void PlayEmergencyBroadcast()
    {
        Transform player = PlayerManager.Instance.transform;
        AudioManager.PlayAudio(EmergencyBroadcast, player.position, player);

        StartCoroutine(ExecuteAfterDelay(EmergencyBroadcast.length, StartEndSirens));
    }

    void StartEndSirens()
    {
        Transform player = PlayerManager.Instance.transform;
        sirensSource = AudioManager.PlayAudio(Sirens, player.position, player);
        if (sirensSource)
        {
            StartCoroutine(PlaySirens(false, StartIntroConversation));
        }
        else
        {
            StartIntroConversation();
        }
    }

    void StartIntroConversation()
    {
        IntroCanvas.SetActive(false);

        if (InputManager.Instance)
        {
            InputManager.Instance.PopInputState();
        }

        if (!StartingConversation)
        {
            Debug.LogError("NO STARTING CONVO");
        }
        
        if (DialogueManager.Instance)
        {
            AudioManager.PlayBackgroundMusic(StartingConversation.Music, StartingConversation.MusicVolume);
            DialogueManager.Instance.StartDialogue(StartingConversation.GetConversationData(), OnDialogueFinished);
        }
        else
        {
            OnDialogueFinished();
        }
    }

    void OnDialogueFinished()
    {
        GameManager.gameManager?.UnpauseGameplay();

        foreach (var obj in ObjectsToHideOnEnd)
        {
            obj.SetActive(false);
        }
        
        Debug.Log("FINISHED");
    }

    IEnumerator ExecuteAfterDelay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        
        callback?.Invoke();
    }

    IEnumerator PlaySirens(bool isIntro, Action callback)
    {
        float duration = isIntro ? introSirensDuration : outroSirensDuration;
        while (sirensSource.time < duration)
        {
            float t = sirensSource.time / duration;
            float volume = isIntro ? introSirensVolume.Evaluate(t) : outroSirensVolume.Evaluate(t);
            sirensSource.volume = volume;
            
            yield return null;
        }

        sirensSource.Stop();
        sirensSource = null;
        callback?.Invoke();
    }
}
