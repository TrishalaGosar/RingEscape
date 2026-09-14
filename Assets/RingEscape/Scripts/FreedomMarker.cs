using System.Collections.Generic;
using UnityEngine;

public class FreedomMarker : MonoBehaviour
{
    public float radius = 8f;
    public int dashCount = 48;
    public float dashDegrees = 4f;
    public float lineWidth = 0.06f;
    public Color markerColor = new Color(0.45f, 0.85f, 1f, 0.35f);

    private readonly List<GameObject> dashes = new List<GameObject>();

    private void Awake()
    {
        BuildMarker();
    }

    private void BuildMarker()
    {
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = markerColor;
        float angleStep = 360f / Mathf.Max(1, dashCount);
        float halfDash = Mathf.Clamp(dashDegrees, 0.1f, angleStep) * 0.5f;

        for (int index = 0; index < dashCount; index++)
        {
            float centerAngle = index * angleStep;
            float startAngle = (centerAngle - halfDash) * Mathf.Deg2Rad;
            float endAngle = (centerAngle + halfDash) * Mathf.Deg2Rad;
            GameObject dash = new GameObject("Freedom Dash");
            dash.transform.SetParent(transform, false);
            LineRenderer line = dash.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = markerColor;
            line.endColor = markerColor;
            line.material = material;
            line.SetPosition(0, new Vector3(Mathf.Cos(startAngle) * radius, Mathf.Sin(startAngle) * radius, 0f));
            line.SetPosition(1, new Vector3(Mathf.Cos(endAngle) * radius, Mathf.Sin(endAngle) * radius, 0f));
            dashes.Add(dash);
        }
    }
}
