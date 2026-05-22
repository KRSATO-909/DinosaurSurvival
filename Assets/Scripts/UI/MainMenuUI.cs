using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public string gameSceneName = "Game";
    public GameObject settingsPanel; // префаб настроек

    void Start()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void StartGame()
    {
        GameManager.Instance?.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        GameManager.Instance?.QuitGame();
    }
}