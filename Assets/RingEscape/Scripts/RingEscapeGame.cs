using UnityEngine;
using UnityEngine.UI;

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

    private bool gameOver;

    private void Awake()
    {
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
    }
}
