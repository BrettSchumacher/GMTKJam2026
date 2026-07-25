using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int countdownInMin = 60;
    private float countdownTimer = 60;
    [SerializeField] public TextMeshProUGUI countdownTimerText;

    // Start is called before the first frame update
    void Start()
    {
        countdownTimer = (float) countdownInMin * 60;
    }

    // Update is called once per frame
    void Update()
    {
        countdownTimer -= Time.deltaTime;
        countdownTimerText.text = Mathf.CeilToInt(countdownTimer/60) + " min";

        if (countdownTimer <= 0f)
        {
            // TODO: Go to end screen
        }
    }
}
