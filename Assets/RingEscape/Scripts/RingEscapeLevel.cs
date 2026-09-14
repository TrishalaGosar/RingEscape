using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RingEscapeLevel", menuName = "RingEscape/Level")]
public class RingEscapeLevel : ScriptableObject
{
    public string levelName = "Level 01";
    [Min(1)] public int ringCount = 3;
    [Min(0.5f)] public float firstRingRadius = 2f;
    [Min(0.5f)] public float concentricSpacing = 2.1f;
    public List<RingLevelSettings> rings = new List<RingLevelSettings>();

    public void EnsureRingData()
    {
        ringCount = Mathf.Max(1, ringCount);
        while (rings.Count < ringCount)
            rings.Add(RingLevelSettings.CreateDefault(rings.Count));
        while (rings.Count > ringCount)
            rings.RemoveAt(rings.Count - 1);
    }
}

[Serializable]
public class RingLevelSettings
{
    public float rotationSpeed = 6f;
    public bool clockwise;
    public List<GapLevelSettings> gaps = new List<GapLevelSettings>();

    public static RingLevelSettings CreateDefault(int index)
    {
        RingLevelSettings settings = new RingLevelSettings
        {
            rotationSpeed = 6f - index,
            clockwise = index % 2 == 1
        };
        settings.gaps.Add(new GapLevelSettings { centerDegrees = index * 120f, widthDegrees = 82f });
        return settings;
    }
}

[Serializable]
public class GapLevelSettings
{
    [Range(0f, 360f)] public float centerDegrees;
    [Range(1f, 359f)] public float widthDegrees = 82f;
}
