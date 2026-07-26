using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DampenZone : MonoBehaviour
{
    public float maxDampenStrength = 1f;

    SphereCollider dampeningZone;

    void Awake()
    {
        dampeningZone = GetComponent<SphereCollider>();
        dampeningZone.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        SkateboardController player = other.GetComponent<SkateboardController>();
        if (player == null) { return; }

        float distance = Vector3.Distance(transform.position, other.transform.position);
        // scales from 0 to 1 depending on how close to center
        float t = Mathf.Clamp01(1f - distance / dampeningZone.radius);
        player.RequestDampen(t * maxDampenStrength, transform.position);
    }
}
