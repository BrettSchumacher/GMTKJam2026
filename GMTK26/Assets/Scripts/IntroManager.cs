using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public float BroadcastDelay = 2f;
    public ConversationSO StartingConversation;
    public float broadcastFadeInDur = 3f;
    public AnimationCurve fadeInCurve;
    public float broadcastFadeOutDur = 3f;
    public AnimationCurve fadeOutCurve;
    public Image fadeinoutImage;
    public Image EmergencyBroadcastImage;
    public float broadcastFadeBackInDur = 3f;
    public AnimationCurve fadeBackInCurve;

    private AudioSource sirensSource;
    private AudioSource broadcastSource;
    private InputAction skipInputAction;

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

        StartCoroutine(FadeIn());
        GameManager.gameManager?.PauseGameplay(true);

        if (InputManager.Instance)
        {
            InputManager.Instance.PushInputState(InputState.Cutscene);
        }
        
        IntroCanvas.SetActive(true);
        foreach (var obj in ObjectsToHideOnEnd)
        {
            obj.SetActive(true);
        }
        
        Transform player = PlayerManager.Instance.transform;
        sirensSource = AudioManager.PlayAudio(Sirens, player.position, player);
        if (sirensSource)
        {
            StartCoroutine(PlaySirens(true, null));
        }
       
        StartCoroutine(ExecuteAfterDelay(BroadcastDelay, PlayEmergencyBroadcast));
    }

    void SetupInput()
    {
        if (!PlayerManager.Instance)
        {
            return;
        }

        var inputComponent = PlayerManager.Instance.InputComponent;

        if (!inputComponent)
        {
            return;
        }

        skipInputAction = inputComponent.actions.FindAction("AdvanceDialogue");
        if (skipInputAction != null)
        {
            skipInputAction.performed += SkipEmergencyBroadcast;
        }
    }

    void SkipEmergencyBroadcast(InputAction.CallbackContext context)
    {
        StartCoroutine(FadeOut());
        sirensSource?.Stop();
        broadcastSource?.Stop();
        StopAllCoroutines();
        Transition();
    }

    void RemoveInput()
    {
        if (!PlayerManager.Instance)
        {
            return;
        }

        var inputComponent = PlayerManager.Instance.InputComponent;

        if (!inputComponent)
        {
            return;
        }

        skipInputAction = inputComponent.actions.FindAction("AdvanceDialogue");
        if (skipInputAction != null)
        {
            skipInputAction.performed -= SkipEmergencyBroadcast;
        }
    }

    void PlayEmergencyBroadcast()
    {
        Transform player = PlayerManager.Instance.transform;
        broadcastSource = AudioManager.PlayAudio(EmergencyBroadcast, player.position, player);

        StartCoroutine(ExecuteAfterDelay(EmergencyBroadcast.length, StartEndSirens));
    }

    void StartEndSirens()
    {
        Transform player = PlayerManager.Instance.transform;
        sirensSource = AudioManager.PlayAudio(Sirens, player.position, player);
        if (sirensSource)
        {
            StartCoroutine(PlaySirens(false, Transition));
        }
        else
        {
            Transition();
        }
    }

    void Transition()
    {
        StartCoroutine(FadeOut());
    }

    void StartIntroConversation()
    {
        RemoveInput();
        
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

    IEnumerator FadeIn()
    {
        fadeinoutImage.color = new Color(0f, 0f, 0f, 1f);
        float time = 0f;
        while (time < broadcastFadeInDur)
        {
            time += Time.deltaTime;
            float t = time / broadcastFadeInDur;
            float alpha = fadeInCurve.Evaluate(t);
            fadeinoutImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        
        fadeinoutImage.color = new Color(0f, 0f, 0f, 0f);
        SetupInput();
    }

    IEnumerator FadeOut()
    {
        fadeinoutImage.color = new Color(0f, 0f, 0f, 0f);
        float time = 0f;
        while (time < broadcastFadeOutDur)
        {
            time += Time.deltaTime;
            float t = time / broadcastFadeOutDur;
            float alpha = fadeOutCurve.Evaluate(t);
            fadeinoutImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        
        fadeinoutImage.color = new Color(0f, 0f, 0f, 1f);
        StartCoroutine(FadeBackIn());
    }

    IEnumerator FadeBackIn()
    {
        fadeinoutImage.color = new Color(0f, 0f, 0f, 1f);
        IntroCanvas.SetActive(false);
        float time = 0f;
        while (time < broadcastFadeBackInDur)
        {
            time += Time.deltaTime;
            float t = time / broadcastFadeBackInDur;
            float alpha = fadeBackInCurve.Evaluate(t);
            fadeinoutImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        
        fadeinoutImage.color = new Color(0f, 0f, 0f, 0f);
        StartIntroConversation();
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
