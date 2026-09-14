using UnityEditor;
using UnityEngine;

public class RingEscapeLevelEditorWindow : EditorWindow
{
    private RingEscapeLevel level;
    private Vector2 scrollPosition;

    [MenuItem("RingEscape/Create Level Asset")]
    private static void CreateLevelAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create RingEscape Level", "Level_01", "asset", "Choose where to save the level.", "Assets/RingEscape");
        if (string.IsNullOrEmpty(path))
            return;

        RingEscapeLevel newLevel = CreateInstance<RingEscapeLevel>();
        newLevel.EnsureRingData();
        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = newLevel;
        Open(newLevel);
    }

    [MenuItem("RingEscape/Edit Selected Level")]
    private static void EditSelectedLevel()
    {
        RingEscapeLevel selectedLevel = Selection.activeObject as RingEscapeLevel;
        if (selectedLevel == null)
        {
            EditorUtility.DisplayDialog("RingEscape", "Select a RingEscapeLevel asset first.", "OK");
            return;
        }
        Open(selectedLevel);
    }

    private static void Open(RingEscapeLevel selectedLevel)
    {
        RingEscapeLevelEditorWindow window = GetWindow<RingEscapeLevelEditorWindow>("RingEscape Level");
        window.level = selectedLevel;
        window.Show();
    }

    private void OnGUI()
    {
        if (level == null)
        {
            EditorGUILayout.HelpBox("Create or select a RingEscapeLevel asset.", MessageType.Info);
            return;
        }

        EditorGUI.BeginChangeCheck();
        level.levelName = EditorGUILayout.TextField("Level Name", level.levelName);
        level.ringCount = Mathf.Max(1, EditorGUILayout.IntField("Number of Rings", level.ringCount));
        level.firstRingRadius = Mathf.Max(0.5f, EditorGUILayout.FloatField("First Ring Radius", level.firstRingRadius));
        level.concentricSpacing = Mathf.Max(0.5f, EditorGUILayout.FloatField("Distance Between Rings", level.concentricSpacing));
        level.EnsureRingData();

        EditorGUILayout.Space(8f);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int ringIndex = 0; ringIndex < level.rings.Count; ringIndex++)
            DrawRing(ringIndex, level.rings[ringIndex]);
        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawRing(int ringIndex, RingLevelSettings ring)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Ring " + (ringIndex + 1), EditorStyles.boldLabel);
        ring.rotationSpeed = Mathf.Max(0f, EditorGUILayout.FloatField("Rotation Speed", ring.rotationSpeed));
        ring.clockwise = EditorGUILayout.Toggle("Clockwise", ring.clockwise);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Gaps", GUILayout.Width(45f));
        if (GUILayout.Button("Add Gap", GUILayout.Width(75f)))
            ring.gaps.Add(new GapLevelSettings());
        EditorGUILayout.EndHorizontal();

        for (int gapIndex = 0; gapIndex < ring.gaps.Count; gapIndex++)
        {
            GapLevelSettings gap = ring.gaps[gapIndex];
            EditorGUILayout.BeginHorizontal();
            gap.centerDegrees = EditorGUILayout.FloatField("Center°", gap.centerDegrees);
            gap.widthDegrees = Mathf.Clamp(EditorGUILayout.FloatField("Width°", gap.widthDegrees), 1f, 359f);
            if (GUILayout.Button("Remove", GUILayout.Width(65f)))
            {
                ring.gaps.RemoveAt(gapIndex);
                gapIndex--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5f);
    }
}
