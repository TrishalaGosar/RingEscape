using UnityEngine;
using UnityEngine.UI;

public class RingEscapeMenu : MonoBehaviour
{
    public GameObject landingPanel;
    public GameObject levelsPanel;
    public Button proceedButton;
    public Button backButton;
    public Button[] levelButtons;

    private void Start()
    {
        if (proceedButton == null)
            proceedButton = FindButton(landingPanel, "Proceed");
        if (backButton == null)
            backButton = FindButton(levelsPanel, "Back");
        if (levelButtons == null || levelButtons.Length == 0)
            levelButtons = FindLevelButtons();
        if (proceedButton != null)
            proceedButton.onClick.AddListener(ShowLevels);
        if (backButton != null)
            backButton.onClick.AddListener(ShowLanding);
        for (int index = 0; levelButtons != null && index < levelButtons.Length; index++)
        {
            int levelNumber = index + 1;
            levelButtons[index].onClick.AddListener(() => StartLevel(levelNumber));
        }
        ShowLanding();
    }

    private Button FindButton(GameObject panel, string buttonName)
    {
        if (panel == null)
            return null;
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        for (int index = 0; index < buttons.Length; index++)
        {
            if (buttons[index].name == buttonName)
                return buttons[index];
        }
        return null;
    }

    private Button[] FindLevelButtons()
    {
        if (levelsPanel == null)
            return new Button[0];
        Button[] buttons = levelsPanel.GetComponentsInChildren<Button>(true);
        System.Collections.Generic.List<Button> levelButtonsFound = new System.Collections.Generic.List<Button>();
        for (int index = 0; index < buttons.Length; index++)
        {
            if (buttons[index].name.StartsWith("Level "))
                levelButtonsFound.Add(buttons[index]);
        }
        levelButtonsFound.Sort((left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));
        return levelButtonsFound.ToArray();
    }

    public void ShowLanding()
    {
        landingPanel.SetActive(true);
        levelsPanel.SetActive(false);
    }

    public void ShowLevels()
    {
        landingPanel.SetActive(false);
        levelsPanel.SetActive(true);
    }

    public void StartLevel(int levelNumber)
    {
        RingEscapeNavigation.LoadLevel(levelNumber);
    }
}
