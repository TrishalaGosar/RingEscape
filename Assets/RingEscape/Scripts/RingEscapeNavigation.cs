using UnityEngine.SceneManagement;

public static class RingEscapeNavigation
{
    public const string MenuSceneName = "RingEscapeMenu";
    public const int LevelCount = 6;

    public static void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > LevelCount)
            return;
        SceneManager.LoadScene("Level_" + levelNumber.ToString("00") + "_Preview");
    }

    public static void LoadNextLevel(int currentLevel)
    {
        LoadLevel(currentLevel >= LevelCount ? 1 : currentLevel + 1);
    }

    public static void LoadPreviousLevel(int currentLevel)
    {
        LoadLevel(currentLevel <= 1 ? LevelCount : currentLevel - 1);
    }

    public static void LoadMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }
}
