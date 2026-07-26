using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

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
        SetupInteractInput();
        interactHudObj.SetActive(false);

        if (AudioManager.Instance)
        {
            AudioManager.PlayBackgroundMusic(MusicTracks.Background, 0.5f);
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

    void Update()
    {
        countdownTimer -= Time.deltaTime;
        if (countdownTimer < 60f)
        {
            countdownTimerText.text = Mathf.CeilToInt(countdownTimer) + " s";
        }
        else
        {
            countdownTimerText.text = Mathf.CeilToInt(countdownTimer / 60) + " min";
        }

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
}
