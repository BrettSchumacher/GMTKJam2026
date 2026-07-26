using System.Collections.Generic;
using UnityEngine;

public class GrindRail : MonoBehaviour
{
    public static readonly List<GrindRail> All = new List<GrindRail>();

    public Transform[] points;
    public bool isClosedLoop;

    public float length { get; private set; }

    float[] segmentLengths;
    float[] cumulativeLengths;
    int segmentCount;

    void OnEnable()
    {
        All.Add(this);
        RecalculateLengths();
    }

    void OnDisable() => All.Remove(this);

    void RecalculateLengths()
    {
        segmentCount = isClosedLoop ? points.Length : points.Length - 1;
        segmentLengths = new float[segmentCount];
        cumulativeLengths = new float[segmentCount + 1];
        float total = 0f;
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 a = points[i].position;
            Vector3 b = points[(i + 1) % points.Length].position;
            segmentLengths[i] = Vector3.Distance(a, b);
            total += segmentLengths[i];
            cumulativeLengths[i + 1] = total;
        }
        length = total;
    }

    public Vector3 getPointAtDistance(float distance, out Vector3 direction)
    {
        distance = isClosedLoop
            ? ((distance % length) + length) % length
            : Mathf.Clamp(distance, 0f, length);

        int segment = 0;
        while (segment < segmentCount - 1 && cumulativeLengths[segment + 1] < distance)
        {
            segment++;
        }

        float segmentStart = cumulativeLengths[segment];
        float segmentT = segmentLengths[segment] > 0f
            ? (distance - segmentStart) / segmentLengths[segment] 
            : 0f;

        Vector3 a = points[segment].position;
        Vector3 b = points[(segment + 1) % points.Length].position;
        direction = (b - a).normalized;
        return Vector3.Lerp(a, b, segmentT);
    }

    public Vector3 getClosestPoint(Vector3 worldPosition, out float distanceAlongRail)
    {
        float bestDist = float.MaxValue;
        Vector3 bestPoint = points[0].position;
        float bestDistAlong = 0f;
        float runningLength = 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 a = points[i].position;
            Vector3 b = points[(i + 1) % points.Length].position;
            Vector3 segment = b - a;
            float segLen = segmentLengths[i];
            float t = segLen > 0f ? Mathf.Clamp01(Vector3.Dot(worldPosition - a, segment) / (segLen * segLen)) : 0f;
            Vector3 point = a + segment * t;
            float dist = Vector3.Distance(worldPosition, point);

            if (dist < bestDist)
            {
                bestDist = dist;
                bestPoint = point;
                bestDistAlong = runningLength + t * segLen;
            }
            runningLength += segLen;
        }

        distanceAlongRail = bestDistAlong;
        return bestPoint;
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.cyan;

        int count = isClosedLoop ? points.Length : points.Length - 1;
        for (int i = 0; i < count; i++)
        {
            Transform a = points[i];
            Transform b = points[(i + 1) % points.Length];
            if (a == null || b == null) continue;
            Gizmos.DrawLine(a.position, b.position);
        }

        foreach (Transform point in points)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.1f);
            }
        }
    }
}
