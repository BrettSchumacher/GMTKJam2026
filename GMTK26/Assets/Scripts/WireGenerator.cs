using UnityEngine;

[ExecuteInEditMode]
public class WireGenerator : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public LineRenderer lineRenderer;

    [Range(10, 50)] public int resolution = 20;
    public float sagAmount = 1.5f;

    void Update()
    {
        if (startPoint == null || endPoint == null || lineRenderer == null) return;

        lineRenderer.positionCount = resolution;

        Vector3 midPoint = Vector3.Lerp(startPoint.position, endPoint.position, 0.5f);
        midPoint.y -= sagAmount;

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);

            Vector3 m1 = Vector3.Lerp(startPoint.position, midPoint, t);
            Vector3 m2 = Vector3.Lerp(midPoint, endPoint.position, t);
            Vector3 curvePoint = Vector3.Lerp(m1, m2, t);

            lineRenderer.SetPosition(i, curvePoint);
        }
    }
}
