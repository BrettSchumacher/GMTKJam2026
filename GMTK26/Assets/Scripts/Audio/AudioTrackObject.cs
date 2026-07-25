using UnityEngine;

public class AudioTrackObject : MonoBehaviour
{
    Transform transformToFollow;
    Vector3 localPos;
    bool bInitialized = false;
    bool bStopWhenDestroyed = false;

    public void Initialize(Vector3 worldPos, Transform toFollow, bool stopWhenDestroyed)
    {
        if (toFollow == null)
        {
            return;
        }

        transform.position = worldPos;
        transformToFollow = toFollow;
        bStopWhenDestroyed = stopWhenDestroyed;
        localPos = transformToFollow.InverseTransformPoint(worldPos);
        bInitialized = true;
    }

    private void Update()
    {
        if (!bInitialized)
        {
            return;
        }

        if (transformToFollow != null)
        {
            transform.position = transformToFollow.TransformPoint(localPos);
        }
        else if (bStopWhenDestroyed)
        {
            GetComponent<AudioSource>()?.Stop();
            Destroy(gameObject);
        }
    }
}
