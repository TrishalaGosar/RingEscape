using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RingEscapeGame : MonoBehaviour
{
    public DotController dot;
    public RotatingRing[] rings;
    public float winRadius = 8.2f;
    public Text statusText;
    public Button restartButton;
    public int levelNumber;
    public Button previousLevelButton;
    public Button nextLevelButton;
    public Button levelsButton;
    public RingEscapeLevel levelData;

    private bool gameOver;

    private void Awake()
    {
        ResolveLevelNumber();
        EnsureNavigationButtons();
        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
        if (previousLevelButton != null)
            previousLevelButton.onClick.AddListener(LoadPreviousLevel);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        if (levelsButton != null)
            levelsButton.onClick.AddListener(OpenLevels);
    }

    private void Update()
    {
        if (!gameOver && dot != null && dot.transform.position.magnitude >= winRadius)
            Win();
    }

    public void Fail()
    {
        if (gameOver)
            return;

        gameOver = true;
        dot.StopDash();
        SetOutcome("You hit a ring!", new Color(1f, 0.35f, 0.35f));
    }

    public void Win()
    {
        if (gameOver)
            return;

        gameOver = true;
        dot.StopDash();
        SetOutcome("You escaped!", new Color(0.35f, 1f, 0.6f));
    }

    public void Restart()
    {
        gameOver = false;
        if (dot != null)
            dot.ResetDot();
        if (statusText != null)
            statusText.text = "Swipe to dash through the gaps";
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public void LoadPreviousLevel()
    {
        RingEscapeNavigation.LoadPreviousLevel(levelNumber);
    }

    public void LoadNextLevel()
    {
        RingEscapeNavigation.LoadNextLevel(levelNumber);
    }

    public void OpenLevels()
    {
        RingEscapeNavigation.LoadMenu();
    }

    private void SetOutcome(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(true);
    }

    private void ResolveLevelNumber()
    {
        if (levelNumber > 0)
            return;
        string sceneName = SceneManager.GetActiveScene().name;
        int separator = sceneName.IndexOf("Level_");
        if (separator >= 0 && int.TryParse(sceneName.Substring(separator + 6, 2), out int parsed))
            levelNumber = parsed;
    }

    private void ApplyLevelData()
    {
        if (levelData == null)
            return;

        levelData.EnsureRingData();
        int count = levelData.ringCount;
        List<RotatingRing> configuredRings = new List<RotatingRing>();
        for (int index = 0; index < count; index++)
        {
            RotatingRing ring = index < rings.Length ? rings[index] : CreateRing(index);
            RingLevelSettings settings = levelData.rings[index];
            List<RingGap> gaps = new List<RingGap>();
            for (int gapIndex = 0; gapIndex < settings.gaps.Count; gapIndex++)
                gaps.Add(new RingGap { centerDegrees = settings.gaps[gapIndex].centerDegrees, widthDegrees = settings.gaps[gapIndex].widthDegrees });
            Color color = Color.Lerp(new Color(1f, 0.35f, 0.75f), new Color(0.45f, 0.55f, 1f), count > 1 ? index / (count - 1f) : 0f);
            ring.gameObject.SetActive(true);
            ring.Configure(levelData.firstRingRadius + index * levelData.concentricSpacing, settings.rotationSpeed, settings.clockwise, gaps, color);
            configuredRings.Add(ring);
        }
        for (int index = count; index < rings.Length; index++)
            if (rings[index] != null) rings[index].gameObject.SetActive(false);
        rings = configuredRings.ToArray();
        winRadius = levelData.firstRingRadius + count * levelData.concentricSpacing + 1f;
        FreedomMarker marker = FindObjectOfType<FreedomMarker>();
        if (marker != null)
            marker.transform.localScale = Vector3.one * (winRadius / Mathf.Max(0.01f, marker.radius));
    }

    private RotatingRing CreateRing(int index)
    {
        GameObject ringObject = new GameObject("Ring " + (index + 1));
        ringObject.transform.SetParent(transform.parent, false);
        return ringObject.AddComponent<RotatingRing>();
    }

    private void EnsureNavigationButtons()
    {
        if (levelNumber <= 0)
            return;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;
        if (levelsButton == null)
            levelsButton = CreateNavigationButton(canvas.transform, "Levels", "LEVELS", new Vector2(0.39f, 0.08f), new Vector2(0.61f, 0.16f));
        Text levelsLabel = levelsButton.GetComponentInChildren<Text>();
        if (levelsLabel != null)
            levelsLabel.text = "BACK";
        if (nextLevelButton == null)
            nextLevelButton = CreateNavigationButton(canvas.transform, "Next Level", "NEXT LEVEL", new Vector2(0.72f, 0.08f), new Vector2(0.96f, 0.16f));
        nextLevelButton.gameObject.SetActive(false);
    }

    private Button CreateNavigationButton(Transform parent, string objectName, string labelText, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.38f, 0.95f);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        GameObject labelObject = new GameObject("Label", typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        Text label = labelObject.GetComponent<Text>();
        label.text = labelText;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
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
}
