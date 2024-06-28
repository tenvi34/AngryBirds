using UnityEngine;

public class Trajectory : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0; // 궤적을 초기화
    }

    public void RenderLine(Vector3 startPoint, Vector3[] points)
    {
        lineRenderer.positionCount = points.Length + 1;
        lineRenderer.SetPosition(0, startPoint);

        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i + 1, points[i]);
        }
    }

    public void ClearLine()
    {
        lineRenderer.positionCount = 0;
    }
}
