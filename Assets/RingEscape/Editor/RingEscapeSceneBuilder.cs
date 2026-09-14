using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RingEscapeSceneBuilder
{
    private const string RootPath = "Assets/RingEscape";
    private const string ScenePath = RootPath + "/RingEscapePrototype.unity";

    [MenuItem("RingEscape/Build Prototype Scene")]
    public static void BuildPrototypeScene()
    {
        BuildScene(null, ScenePath);
    }

    [MenuItem("RingEscape/Build Level Preview From Selected")]
    public static void BuildLevelPreviewFromSelected()
    {
        RingEscapeLevel level = Selection.activeObject as RingEscapeLevel;
        if (level == null)
            level = AssetDatabase.LoadAssetAtPath<RingEscapeLevel>(RootPath + "/Levels/Level_01.asset");
        if (level == null)
        {
            EditorUtility.DisplayDialog("RingEscape", "Select a RingEscapeLevel asset first.", "OK");
            return;
        }

        EnsureFolder(RootPath + "/Previews");
        string previewPath = RootPath + "/Previews/" + level.name + "_Preview.unity";
        BuildScene(level, previewPath);
    }

    [MenuItem("RingEscape/Build All Level Previews")]
    public static void BuildAllLevelPreviews()
    {
        string[] levelGuids = AssetDatabase.FindAssets("t:RingEscapeLevel", new[] { RootPath + "/Levels" });
        if (levelGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("RingEscape", "No RingEscapeLevel assets were found.", "OK");
            return;
        }

        Array.Sort(levelGuids, (left, right) => string.Compare(AssetDatabase.GUIDToAssetPath(left), AssetDatabase.GUIDToAssetPath(right), StringComparison.Ordinal));
        EnsureFolder(RootPath + "/Previews");
        for (int index = 0; index < levelGuids.Length; index++)
        {
            RingEscapeLevel level = AssetDatabase.LoadAssetAtPath<RingEscapeLevel>(AssetDatabase.GUIDToAssetPath(levelGuids[index]));
            string previewPath = RootPath + "/Previews/" + level.name + "_Preview.unity";
            BuildScene(level, previewPath, false);
        }
        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("RingEscape", "Built " + levelGuids.Length + " level preview scenes.", "OK");
    }

    private static void BuildScene(RingEscapeLevel level, string outputPath, bool showDialog = true)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<RingEscapeCamera>();

        GameObject gameObject = new GameObject("Ring Escape Game");
        RingEscapeGame game = gameObject.AddComponent<RingEscapeGame>();
        game.levelNumber = level != null ? GetLevelNumber(level.name) : 0;

        GameObject dotObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        dotObject.name = "Player Dot";
        UnityEngine.Object.DestroyImmediate(dotObject.GetComponent<MeshCollider>());
        dotObject.transform.localScale = Vector3.one * 0.32f;
        CircleCollider2D dotCollider = dotObject.AddComponent<CircleCollider2D>();
        dotCollider.radius = 0.5f;
        Rigidbody2D body = dotObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        MeshRenderer renderer = dotObject.GetComponent<MeshRenderer>();
        Material dotMaterial = new Material(Shader.Find("Unlit/Color"));
        dotMaterial.color = new Color(0.3f, 0.9f, 1f);
        renderer.sharedMaterial = dotMaterial;
        renderer.sortingOrder = 5;
        DotController dot = dotObject.AddComponent<DotController>();
        dot.game = game;
        game.dot = dot;

        if (level != null)
            level.EnsureRingData();
        int ringCount = level != null ? level.ringCount : 3;
        RotatingRing[] rings = new RotatingRing[ringCount];
        float[] originalRadii = { 2f, 4.1f, 6.2f };
        float[] originalGaps = { 35f, 170f, 290f };
        float[] originalSpeeds = { 7f, -5f, 4f };
        for (int index = 0; index < rings.Length; index++)
        {
            GameObject ringObject = new GameObject("Ring " + (index + 1));
            RotatingRing ring = ringObject.AddComponent<RotatingRing>();
            if (level != null)
            {
                RingLevelSettings settings = level.rings[index];
                GapLevelSettings gap = settings.gaps.Count > 0 ? settings.gaps[0] : new GapLevelSettings { centerDegrees = 0f, widthDegrees = 82f };
                ring.radius = level.firstRingRadius + index * level.concentricSpacing;
                ring.gapCenterDegrees = gap.centerDegrees;
                ring.gapWidthDegrees = gap.widthDegrees;
                ring.rotationSpeed = settings.rotationSpeed;
                ring.clockwise = settings.clockwise;
                for (int gapIndex = 0; gapIndex < settings.gaps.Count; gapIndex++)
                {
                    GapLevelSettings configuredGap = settings.gaps[gapIndex];
                    ring.gaps.Add(new RingGap { centerDegrees = configuredGap.centerDegrees, widthDegrees = configuredGap.widthDegrees });
                }
            }
            else
            {
                ring.radius = originalRadii[index];
                ring.gapCenterDegrees = originalGaps[index];
                ring.gapWidthDegrees = 82f;
                ring.rotationSpeed = Mathf.Abs(originalSpeeds[index]);
                ring.clockwise = originalSpeeds[index] < 0f;
            }
            ring.thickness = 0.2f;
            ring.ringColor = Color.Lerp(new Color(1f, 0.35f, 0.75f), new Color(0.45f, 0.55f, 1f), rings.Length > 1 ? index / (rings.Length - 1f) : 0f);
            rings[index] = ring;
        }
        game.rings = rings;
        game.winRadius = (level != null ? level.firstRingRadius + ringCount * level.concentricSpacing : 8.2f) + 1f;
        GameObject freedomObject = new GameObject("This is freedom");
        FreedomMarker freedomMarker = freedomObject.AddComponent<FreedomMarker>();
        freedomMarker.radius = game.winRadius;

        CreateUi(game, level != null ? level.name.ToUpperInvariant() : "PROTOTYPE", game.levelNumber);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, outputPath);
        Selection.activeGameObject = gameObject;
        if (showDialog)
            EditorUtility.DisplayDialog("RingEscape", "Scene created at " + outputPath, "OK");
    }

    private static void CreateUi(RingEscapeGame game, string levelTitle, int levelNumber)
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Canvas canvas = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);

        GameObject textObject = new GameObject("Status Text", typeof(Text));
        textObject.transform.SetParent(canvas.transform, false);
        Text text = textObject.GetComponent<Text>();
        text.text = "Swipe to dash through the gaps";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 42;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.05f, 0.88f);
        textRect.anchorMax = new Vector2(0.95f, 0.98f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        game.statusText = text;

        GameObject levelObject = new GameObject("Level Label", typeof(Text));
        levelObject.transform.SetParent(canvas.transform, false);
        Text levelText = levelObject.GetComponent<Text>();
        levelText.text = levelTitle;
        levelText.font = text.font;
        levelText.fontSize = 34;
        levelText.alignment = TextAnchor.MiddleLeft;
        levelText.color = new Color(0.65f, 0.85f, 1f, 0.9f);
        RectTransform levelRect = levelText.rectTransform;
        levelRect.anchorMin = new Vector2(0.05f, 0.81f);
        levelRect.anchorMax = new Vector2(0.5f, 0.87f);
        levelRect.offsetMin = Vector2.zero;
        levelRect.offsetMax = Vector2.zero;

        GameObject buttonObject = new GameObject("Restart Button", typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.38f, 1f);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.32f, 0.08f);
        buttonRect.anchorMax = new Vector2(0.68f, 0.16f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        GameObject labelObject = new GameObject("Label", typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        Text label = labelObject.GetComponent<Text>();
        label.text = "RESTART";
        label.font = text.font;
        label.fontSize = 34;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        buttonObject.SetActive(false);
        game.restartButton = buttonObject.GetComponent<Button>();

        if (levelNumber > 0)
        {
            game.previousLevelButton = CreateButton(canvas.transform, "Previous Level", "←", new Vector2(0.06f, 0.08f), new Vector2(0.22f, 0.16f), text.font);
            game.levelsButton = CreateButton(canvas.transform, "Levels", "LEVELS", new Vector2(0.39f, 0.08f), new Vector2(0.61f, 0.16f), text.font);
            game.nextLevelButton = CreateButton(canvas.transform, "Next Level", "→", new Vector2(0.78f, 0.08f), new Vector2(0.94f, 0.16f), text.font);
        }
    }

    private static Button CreateButton(Transform parent, string objectName, string labelText, Vector2 anchorMin, Vector2 anchorMax, Font font)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.38f, 0.95f);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        GameObject labelObject = new GameObject("Label", typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        Text label = labelObject.GetComponent<Text>();
        label.text = labelText;
        label.font = font;
        label.fontSize = labelText.Length == 1 ? 54 : 24;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        return buttonObject.GetComponent<Button>();
    }

    private static int GetLevelNumber(string assetName)
    {
        string digits = string.Empty;
        for (int index = 0; index < assetName.Length; index++)
        {
            if (char.IsDigit(assetName[index]))
                digits += assetName[index];
        }
        int number;
        return int.TryParse(digits, out number) ? number : 0;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = path.Substring(0, path.LastIndexOf('/'));
            string folder = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
