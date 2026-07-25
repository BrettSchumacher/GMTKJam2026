using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GrindRail : MonoBehaviour
{
    public static readonly List<GrindRail> All = new List<GrindRail>();

    public Transform start;
    public Transform end;

    public Vector3 startPosition => start.position;
    public Vector3 endPosition => end.position;
    public Vector3 direction => (endPosition  - startPosition).normalized;
    public float length => Vector3.Distance(startPosition, endPosition);

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public Vector3 getClosestPoint(Vector3 worldPosition, out float distanceAlongRail)
    {
        Vector3 toPoint = worldPosition - startPosition;
        float projection = Mathf.Clamp(Vector3.Dot(toPoint, direction), 0f, length);
        distanceAlongRail = projection;
        return startPosition + direction * projection;
    }

    private void OnDrawGizmos()
    {
        if (start == null || end == null) { return; }
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPosition, endPosition);
        Gizmos.DrawWireSphere(startPosition, 0.1f);
        Gizmos.DrawWireSphere(endPosition, 0.1f);
    }
}
