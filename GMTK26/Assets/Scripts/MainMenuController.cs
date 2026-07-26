using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public CinemachineVirtualCamera MainCam;
    public CinemachineVirtualCamera CreditsCam;

    private void Start()
    {
        ShowMainView();
    }

    public void ShowMainView()
    {
        MainCam.enabled = true;
        CreditsCam.enabled = false;
    }

    public void ShowCredits()
    {
        MainCam.enabled = false;
        CreditsCam.enabled = true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
