using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RingEscapeMenuBuilder
{
    private const string RootPath = "Assets/RingEscape";
    private const string MenuPath = RootPath + "/RingEscapeMenu.unity";

    [MenuItem("RingEscape/Build Landing and Levels Menu")]
    public static void BuildMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 10f;
        camera.backgroundColor = new Color(0.035f, 0.045f, 0.08f);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Canvas canvas = new GameObject("Menu UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);

        GameObject menuObject = new GameObject("RingEscape Menu");
        RingEscapeMenu menu = menuObject.AddComponent<RingEscapeMenu>();
        GameObject landingPanel = CreatePanel(canvas.transform, "Landing Page");
        GameObject levelsPanel = CreatePanel(canvas.transform, "Levels Page");
        menu.landingPanel = landingPanel;
        menu.levelsPanel = levelsPanel;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreateText(landingPanel.transform, "RING ESCAPE", 76, TextAnchor.MiddleCenter, Color.white, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.82f), font);
        CreateText(landingPanel.transform, "Swipe to dash through the gaps.\nHit a solid ring and you fail.\nEscape beyond the outer ring to win.", 34, TextAnchor.MiddleCenter, new Color(0.7f, 0.85f, 1f), new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.62f), font);
        Button proceedButton = CreateButton(landingPanel.transform, "Proceed", "CHOOSE LEVEL", new Vector2(0.25f, 0.16f), new Vector2(0.75f, 0.27f), font, 32);
        proceedButton.onClick.AddListener(menu.ShowLevels);

        CreateText(levelsPanel.transform, "CHOOSE A LEVEL", 54, TextAnchor.MiddleCenter, Color.white, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.94f), font);
        CreateText(levelsPanel.transform, "All levels are currently unlocked", 26, TextAnchor.MiddleCenter, new Color(0.65f, 0.85f, 1f), new Vector2(0.08f, 0.75f), new Vector2(0.92f, 0.82f), font);
        Button backButton = CreateButton(levelsPanel.transform, "Back", "BACK", new Vector2(0.08f, 0.08f), new Vector2(0.3f, 0.16f), font, 24);
        backButton.onClick.AddListener(menu.ShowLanding);

        for (int index = 0; index < RingEscapeNavigation.LevelCount; index++)
        {
            int levelNumber = index + 1;
            int column = index % 3;
            int row = index / 3;
            float left = 0.12f + column * 0.27f;
            float bottom = 0.48f - row * 0.2f;
            Button levelButton = CreateButton(levelsPanel.transform, "Level " + levelNumber.ToString("00"), "LEVEL " + levelNumber.ToString("00"), new Vector2(left, bottom), new Vector2(left + 0.22f, bottom + 0.14f), font, 26);
            levelButton.onClick.AddListener(() => menu.StartLevel(levelNumber));
        }

        levelsPanel.SetActive(false);
        EditorSceneManager.SaveScene(scene, MenuPath);
        RegisterScenes();
        Selection.activeGameObject = menuObject;
        EditorUtility.DisplayDialog("RingEscape", "Landing and level-select menu created.", "OK");
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private static Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Font font)
    {
        GameObject textObject = new GameObject("Text", typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string labelText, Vector2 anchorMin, Vector2 anchorMax, Font font, int fontSize)
    {
        GameObject buttonObject = new GameObject(name, typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.38f, 0.96f);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        CreateText(buttonObject.transform, labelText, fontSize, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, font);
        return buttonObject.GetComponent<Button>();
    }

    private static void RegisterScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MenuPath, true)
        };
        for (int index = 1; index <= RingEscapeNavigation.LevelCount; index++)
        {
            string scenePath = RootPath + "/Previews/Level_" + index.ToString("00") + "_Preview.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
