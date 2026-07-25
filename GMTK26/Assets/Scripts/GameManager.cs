using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }

    public int countdownInMin = 60;
    private float countdownTimer = 60;
    [SerializeField] public TextMeshProUGUI countdownTimerText;

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
    }
}
