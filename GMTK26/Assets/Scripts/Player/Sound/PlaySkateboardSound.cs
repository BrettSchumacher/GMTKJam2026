using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySkateboardSound : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private SkateboardController skateboard;
    [SerializeField] private AudioSource movementAudio;
    [SerializeField] private float minimumVolume;
    [SerializeField] private float maximumVolume;
    private float minimumSpeed;
    private float maximumSpeed;
    private void Awake()
    {

        if (skateboard == null)
        {
            skateboard = GetComponentInParent<SkateboardController>();
        }
        if (movementAudio == null)
        {
            movementAudio = GetComponent<AudioSource>();
        }
        movementAudio.loop = true;
        movementAudio.playOnAwake = false;
        maximumSpeed = 10f;
        minimumSpeed = 0.1f;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float currentSpeed = skateboard.velocity.magnitude;

        UpdateMovementSound(currentSpeed);
    }
    private void UpdateMovementSound(float currentSpeed)
    {
        float normalizedSpeed = Mathf.InverseLerp(
          minimumSpeed,
          maximumSpeed,
          currentSpeed
          );
        movementAudio.volume = Mathf.Lerp(
            minimumVolume,
            maximumVolume,
            normalizedSpeed

        );
        if (!movementAudio.isPlaying)
        {
            movementAudio.Play();
        }


    }
}
