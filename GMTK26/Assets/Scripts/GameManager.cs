using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }

    public TradingSystem tradingSystem;

    PlayerInput playerInput;
    InputAction interactAction;

    public int countdownInMin = 60;
    private float countdownTimer = 60;
    [SerializeField] public TextMeshProUGUI countdownTimerText;

    private Interactable currentInteractable;
    [SerializeField] private GameObject interactHudObj;
    [SerializeField] private TextMeshProUGUI interactText;
    private bool IsWaitingOnTrick;
    private bool isPaused = false;


    private void Awake()
    {
        // Setup Game Manager singleton 
        if (gameManager != null && gameManager != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            gameManager = this;
        }

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        countdownTimer = (float) countdownInMin * 60;
        countdownTimerText.text = String.Format("{0:00}",(Mathf.CeilToInt(countdownTimer / 60) - 1)) + ":" + String.Format("{0:00}", (Mathf.CeilToInt(countdownTimer % 60) - 1));
        SetupInteractInput();
        interactHudObj.SetActive(false);

        if (AudioManager.Instance)
        {
            AudioManager.PlayBackgroundMusic(MusicTracks.Background, 0.5f);
        }

        if (IntroManager.Instance)
        {
            IntroManager.Instance.StartIntro();
        }
    }

    private void SetupInteractInput()
    {
        if (PlayerManager.Instance.InputComponent == null)
        {
            return;
        }
        
        playerInput = PlayerManager.Instance.InputComponent;
        interactAction = playerInput.actions["Interact"];
    }

    public void PauseGameplay()
    {
        isPaused = true;
        AudioManager.PlayBackgroundMusic(MusicTracks.None);
    }

    public void UnpauseGameplay()
    {
        isPaused = false;
        AudioManager.PlayBackgroundMusic(MusicTracks.Background, 0.5f);
    }

    void Update()
    {
        if (isPaused)
        {
            return;
        }
        
        countdownTimer -= Time.deltaTime;
        countdownTimerText.text = String.Format("{0:00}",(Mathf.CeilToInt(countdownTimer / 60) - 1)) + ":" + String.Format("{0:00}", (Mathf.CeilToInt(countdownTimer % 60) - 1));

        if (countdownTimer <= 0f)
        {
            // TODO: Go to end screen
        }

        if (playerInput == null)
        {
            SetupInteractInput();
        }

        if (interactAction.WasPressedThisFrame() && currentInteractable != null)
        {
            currentInteractable.TriggerAction();
        }
    }

    public void SetSelectedInteractableObj(Interactable interactableObj)
    {
        currentInteractable = interactableObj;
    }

    public Interactable GetSelectedInteractableObj()
    {
        return currentInteractable;
    }

    public void SetInteractText(string text)
    {
        interactHudObj.SetActive(true);
        interactText.text = text;

        if (text == null || text == "")
        {
            interactHudObj.SetActive(false);
        }
    }

    public void SetWaitingForTrick(bool waiting)
    {
        IsWaitingOnTrick = waiting;

        foreach (var interact in Interactable.Interactives)
        {
            if (interact && interact.DisableWhileWaitingForTrick)
            {
                interact.SetOverrideInteractable(waiting);
            }
        }
    }

    public bool GetIsWaitingForTrick()
    {
        return IsWaitingOnTrick;
    }
}
