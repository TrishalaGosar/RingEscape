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
        EnsureFolder(RootPath);
        EnsureFolder(RootPath + "/Scenes");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<RingEscapeCamera>();

        GameObject gameObject = new GameObject("Ring Escape Game");
        RingEscapeGame game = gameObject.AddComponent<RingEscapeGame>();

        GameObject dotObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        dotObject.name = "Player Dot";
        Object.DestroyImmediate(dotObject.GetComponent<MeshCollider>());
        dotObject.transform.localScale = Vector3.one * 0.32f;
        dotObject.AddComponent<CircleCollider2D>().radius = 0.5f;
        Rigidbody2D body = dotObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        SpriteRenderer sprite = dotObject.AddComponent<SpriteRenderer>();
        sprite.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sprite.color = new Color(0.3f, 0.9f, 1f);
        sprite.sortingOrder = 5;
        Object.DestroyImmediate(dotObject.GetComponent<MeshRenderer>());
        DotController dot = dotObject.AddComponent<DotController>();
        dot.game = game;
        game.dot = dot;

        RotatingRing[] rings = new RotatingRing[3];
        float[] radii = { 2f, 4.1f, 6.2f };
        float[] gaps = { 35f, 170f, 290f };
        float[] speeds = { 7f, -5f, 4f };
        for (int index = 0; index < rings.Length; index++)
        {
            GameObject ringObject = new GameObject("Ring " + (index + 1));
            RotatingRing ring = ringObject.AddComponent<RotatingRing>();
            ring.radius = radii[index];
            ring.gapCenterDegrees = gaps[index];
            ring.gapWidthDegrees = 82f;
            ring.rotationSpeed = Mathf.Abs(speeds[index]);
            ring.clockwise = speeds[index] < 0f;
            ring.thickness = 0.2f;
            ring.ringColor = Color.Lerp(new Color(1f, 0.35f, 0.75f), new Color(0.45f, 0.55f, 1f), index / 2f);
            rings[index] = ring;
        }
        game.rings = rings;
        game.winRadius = 8.2f;

        CreateUi(game);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = gameObject;
        EditorUtility.DisplayDialog("RingEscape", "Prototype scene created at " + ScenePath, "OK");
    }

    private static void CreateUi(RingEscapeGame game)
    {
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Canvas canvas = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);

        GameObject textObject = new GameObject("Status Text", typeof(Text));
        textObject.transform.SetParent(canvas.transform, false);
        Text text = textObject.GetComponent<Text>();
        text.text = "Swipe to dash through the gaps";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 42;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.05f, 0.88f);
        textRect.anchorMax = new Vector2(0.95f, 0.98f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        game.statusText = text;

        GameObject buttonObject = new GameObject("Restart Button", typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.2f, 0.38f, 1f);
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
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        buttonObject.SetActive(false);
        game.restartButton = buttonObject.GetComponent<Button>();
        Object.DontDestroyOnLoad(eventSystem);
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int index = 1; index < parts.Length; index++)
        {
            string next = current + "/" + parts[index];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[index]);
            current = next;
        }
    }
}
