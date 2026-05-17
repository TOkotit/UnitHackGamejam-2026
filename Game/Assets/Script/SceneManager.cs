using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneManager
{
    public static void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Неверный индекс сцены: {sceneIndex}");
            return;
        }
        SceneManager.LoadScene(sceneIndex);
    }

    public static void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneManager.LoadScene(sceneName);
    }
    
    public static void LoadNextScene(MonoBehaviour caller)
    {
        int currentIndex = caller.gameObject.scene.buildIndex;

        int next = currentIndex + 1;

        if (next >= SceneManager.sceneCountInBuildSettings)
            next = 0;
        LoadScene(next);
    }

    public static void LoadPreviousScene()
    {
        int prev = (SceneManager.GetActiveScene().buildIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        LoadScene(prev);
    }
}