using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject hudPanel;
    public string MainMenu = "MainMenu";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ShowPauseMenu(false);
        CloseSettings();
        if (hudPanel) hudPanel.SetActive(true);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);

        // Управление курсором
        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!show) CloseSettings();
    }

    public void OpenSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
        // Не трогаем паузу – она останется на заднем плане
    }

    public void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        GameManager.Instance?.TogglePause();
    }

    public void LoadMainMenu()
    {
        GameManager.Instance?.LoadScene(MainMenu);
    }

    public void QuitGame()
    {
        GameManager.Instance?.QuitGame();
    }
}