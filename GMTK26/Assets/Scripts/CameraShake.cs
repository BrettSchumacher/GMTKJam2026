using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeAmplitude = 0.02f;
    public float shakeFrequency = 25f;

    Vector3 basePosition;
    float noiseSeed;

    public bool isShaking {  get; set; }

    void Start()
    {
        basePosition = transform.localPosition;
        noiseSeed = Random.value * 100;
    }

    void Update()
    {
        if (isShaking)
        {
            float offsetX = (Mathf.PerlinNoise(noiseSeed, Time.time * shakeFrequency) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(noiseSeed + 1f, Time.time * shakeFrequency) - 0.5f) * 2f;
            transform.localPosition = basePosition + new Vector3(offsetX, offsetY, 0f) * shakeAmplitude;
        }
        else
        {
            transform.localPosition = basePosition;
        }
    }
}
