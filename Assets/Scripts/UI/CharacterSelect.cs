using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button trexButton;
    public Button quetzButton;
    public Button mosasaurusButton;
    public Button startButton;

    [Header("Icons")]
    public Image trexIcon;
    public Image quetzIcon;
    public Image mosasaurusIcon;

    [Header("Selection Highlight")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    private GameManager.DinoType selectedDino = GameManager.DinoType.None;

    void Start()
    {
        trexButton.onClick.AddListener(() => SelectDino(GameManager.DinoType.TRex));
        quetzButton.onClick.AddListener(() => SelectDino(GameManager.DinoType.Quetzalcoatlus));
        mosasaurusButton.onClick.AddListener(() => SelectDino(GameManager.DinoType.Mosasaurus));
        startButton.onClick.AddListener(StartGame);

        startButton.interactable = false;
        ResetHighlights();
    }

    void SelectDino(GameManager.DinoType type)
    {
        selectedDino = type;
        GameManager.Instance?.SelectDino(type);
        startButton.interactable = true;

        // Подсветка выбранного
        trexIcon.color = type == GameManager.DinoType.TRex ? selectedColor : normalColor;
        quetzIcon.color = type == GameManager.DinoType.Quetzalcoatlus ? selectedColor : normalColor;
        mosasaurusIcon.color = type == GameManager.DinoType.Mosasaurus ? selectedColor : normalColor;
    }

    void StartGame()
    {
        if (selectedDino != GameManager.DinoType.None)
        {
            GameManager.Instance?.LoadGameScene();
        }
    }

    void ResetHighlights()
    {
        trexIcon.color = normalColor;
        quetzIcon.color = normalColor;
        mosasaurusIcon.color = normalColor;
    }
}