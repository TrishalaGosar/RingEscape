using System.Collections.Generic;
using UnityEngine;

public class RotatingRing : MonoBehaviour
{
    public float radius = 2f;
    public float thickness = 0.18f;
    public float gapCenterDegrees = 0f;
    public float gapWidthDegrees = 80f;
    public List<RingGap> gaps = new List<RingGap>();
    public float rotationSpeed = 8f;
    public bool clockwise;
    public bool reverseDirectionPeriodically;
    public float directionReverseSeconds = 5f;
    public Color ringColor = Color.white;
    public int segmentCount = 96;

    private float directionTimer;
    private bool reversed;
    private readonly List<GameObject> segments = new List<GameObject>();

    private void Awake()
    {
        BuildRing();
    }

    private void Update()
    {
        float direction = clockwise ? -1f : 1f;
        if (reversed)
            direction *= -1f;
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);

        if (reverseDirectionPeriodically && directionReverseSeconds > 0f)
        {
            directionTimer += Time.deltaTime;
            if (directionTimer >= directionReverseSeconds)
            {
                directionTimer = 0f;
                reversed = !reversed;
            }
        }
    }

    private void BuildRing()
    {
        ClearRing();
        int safeSegments = Mathf.Max(32, segmentCount);
        float step = 360f / safeSegments;
        float segmentLength = 2f * Mathf.PI * radius / safeSegments * 1.08f;

        for (int index = 0; index < safeSegments; index++)
        {
            float angle = index * step;
            if (IsGap(angle))
                continue;

            float radians = angle * Mathf.Deg2Rad;
            GameObject segment = new GameObject("Solid Segment");
            segment.transform.SetParent(transform, false);
            segment.transform.localPosition = new Vector3(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius, 0f);
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);

            BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(segmentLength, thickness);

            LineRenderer line = segment.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(-segmentLength * 0.5f, 0f, 0f));
            line.SetPosition(1, new Vector3(segmentLength * 0.5f, 0f, 0f));
            line.startWidth = thickness;
            line.endWidth = thickness;
            line.material = CreateMaterial();
            line.startColor = ringColor;
            line.endColor = ringColor;
            segments.Add(segment);
        }
    }

    private bool IsGap(float angle)
    {
        if (gaps.Count == 0)
            return Mathf.Abs(Mathf.DeltaAngle(gapCenterDegrees, angle)) <= Mathf.Clamp(gapWidthDegrees, 0f, 359f) * 0.5f;

        for (int index = 0; index < gaps.Count; index++)
        {
            RingGap gap = gaps[index];
            float halfWidth = Mathf.Clamp(gap.widthDegrees, 0f, 359f) * 0.5f;
            if (Mathf.Abs(Mathf.DeltaAngle(gap.centerDegrees, angle)) <= halfWidth)
                return true;
        }
        return false;
    }

    private Material CreateMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        Material material = new Material(shader);
        material.color = ringColor;
        return material;
    }

    private void ClearRing()
    {
        for (int index = 0; index < segments.Count; index++)
        {
            if (segments[index] != null)
                Destroy(segments[index]);
        }
        segments.Clear();
    }
}

[System.Serializable]
public class RingGap
{
    public float centerDegrees;
    public float widthDegrees = 80f;
}
