using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
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

    [SerializeField] public TextMeshProUGUI itemsListText;  // Text for wedding items that still must be obtained
    [SerializeField] public TextMeshProUGUI currentTradeItemText;

    private int numPeopleToInvite = 0;
    [SerializeField] public TextMeshProUGUI numPeopleToInviteText;

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
