using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SkateboardController))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [HideInInspector] public SkateboardController SkateboardController;
    [HideInInspector] public PlayerInput InputComponent;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("Duplicate Player Manager found");
            return;
        }

        Instance = this;

        SkateboardController = GetComponent<SkateboardController>();
        InputComponent = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
