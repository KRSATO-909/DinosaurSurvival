using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle hintsToggle;

    void Start()
    {
        // Загружаем сохранённые настройки
        var gm = GameManager.Instance;
        if (gm != null)
        {
            musicSlider.value = gm.musicVolume;
            sfxSlider.value = gm.sfxVolume;
            hintsToggle.isOn = gm.showHints;
        }

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        hintsToggle.onValueChanged.AddListener(OnHintsChanged);
    }

    void OnMusicChanged(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.musicVolume = value;
            GameManager.Instance.SaveSettings();
            // Применить к AudioSource музыки
            var audio = GameObject.FindWithTag("Music")?.GetComponent<AudioSource>();
            if (audio) audio.volume = value;
        }
    }

    void OnSFXChanged(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.sfxVolume = value;
            GameManager.Instance.SaveSettings();
            // Для звуков можно использовать отдельный AudioMixer или общий AudioSource
        }
    }

    void OnHintsChanged(bool value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.showHints = value;
            GameManager.Instance.SaveSettings();
        }
    }
}