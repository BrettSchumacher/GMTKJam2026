using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }

    public TradingSystem tradingSystem;

    PlayerInput playerInput;
    InputAction interactAction;

    public int countdownInMin = 60;
    public bool playIntro = true;
    public GameObject HUD;
    private float countdownTimer = 60;
    [SerializeField] public TextMeshProUGUI countdownTimerText;

    [SerializeField] public TextMeshProUGUI itemsListText;  // Text for wedding items that still must be obtained
    [SerializeField] public TextMeshProUGUI currentTradeItemText;
    public GameObject TrickTutorialText;

    private int numPeopleToInvite = 0;
    [SerializeField] public TextMeshProUGUI numPeopleToInviteText;

    private Interactable currentInteractable;
    [SerializeField] private GameObject interactHudObj;
    [SerializeField] private TextMeshProUGUI interactText;
    private bool IsWaitingOnTrick;
    private bool isPaused = false;

    [SerializeField] private TextMeshProUGUI trickText;
    [SerializeField] private TextMeshProUGUI trickScoreText;
    public float trickTextDisplayTime = 1;
    private float trickTimer;
    private int pointIncrease;

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

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
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

        if (playIntro && IntroManager.Instance)
        {
            IntroManager.Instance.StartIntro();
        }

        trickTimer = trickTextDisplayTime;
        trickScoreText.text = "";
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

    public void PauseGameplay(bool hideHud = false)
    {
        if (hideHud)
        {
            HUD?.SetActive(false);
        }
        isPaused = true;
        AudioManager.PlayBackgroundMusic(MusicTracks.None);
    }

    public void UnpauseGameplay()
    {
        HUD?.SetActive(true);
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

        trickTimer -= Time.deltaTime;
        if (trickTimer <= 0)
        {
            ClearTrickText();
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

    // Text for wedding items that still must be obtained
    public void SetItemsListText(Dictionary<string, WeddingItem> checklist)
    {
        string text = "Items for wedding:\n";
        foreach(KeyValuePair<string, WeddingItem> item in checklist)
        {
            if (item.Value.obtained)
            {
                text += "<s>";
            }
            text += item.Value.item.ItemName;

            if (item.Value.obtained)
            {
                text += "</s>";
            }
            text += "\n";
        }

        itemsListText.text = text;
    }

    public void SetCurrentTradeItemText(string text)
    {
        currentTradeItemText.text = "To trade: " + text;
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

    public void SetTrickText(string trickName, float pointsGained, bool combo)
    {
        trickText.text = trickName;

        if (combo)
        {
            pointIncrease += (int) pointsGained;
            trickScoreText.text = "+ " + pointIncrease;
        }
        else
        {
            trickScoreText.text = "+ " + (int) pointsGained;
        }

        trickTimer = trickTextDisplayTime;
    }

    private void ClearTrickText()
    {
        trickText.text = string.Empty;
        trickScoreText.text = string.Empty;
        pointIncrease = 0;
    }

    public void SetWaitingForTrick(bool waiting)
    {
        IsWaitingOnTrick = waiting;
        TrickTutorialText?.SetActive(waiting);

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
    
    public void SetNumPeopleToInviteText()
    {
        numPeopleToInviteText.text = "Invites left: " + numPeopleToInvite;
    }

    public void IncrementPeopleToInviteCount()
    {
        numPeopleToInvite++;
        SetNumPeopleToInviteText();
    }

    public void DecrementPeopleToInviteCount()
    {
        numPeopleToInvite--;
        SetNumPeopleToInviteText();
    }
}
