using UnityEngine;
using UnityEngine.UI;

public class SurvivalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SurvivalStats playerStats;

    [Header("Hunger Bar")]
    [SerializeField] private Image hungerFill;
    [SerializeField] private Text hungerText;

    [Header("Thirst Bar")]
    [SerializeField] private Image thirstFill;
    [SerializeField] private Text thirstText;

    [Header("Health Bar")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthText;

    [Header("Interaction Hint")]
    [SerializeField] private Text interactionHint;

    void Update()
    {
        if (playerStats == null)
        {
            // Авто-поиск игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerStats = player.GetComponent<SurvivalStats>();
            return;
        }

        UpdateBars();
        UpdateHint();
    }

    void UpdateBars()
    {
        float hunger = playerStats.GetHungerPercent();
        float thirst = playerStats.GetThirstPercent();
        float health = playerStats.GetHealthPercent();

        // Заполнение
        if (hungerFill != null) hungerFill.fillAmount = hunger;
        if (thirstFill != null) thirstFill.fillAmount = thirst;
        if (healthFill != null) healthFill.fillAmount = health;

        // Текст
        if (hungerText != null) hungerText.text = $"Голод: {playerStats.GetCurrentHunger():F0}/{playerStats.GetMaxHunger()}";
        if (thirstText != null) thirstText.text = $"Жажда: {playerStats.GetCurrentThirst():F0}/{playerStats.GetMaxThirst()}";
        if (healthText != null) healthText.text = $"Здоровье: {playerStats.GetCurrentHealth():F0}/{playerStats.GetMaxHealth()}";

        // Цвета (красный когда мало)
        if (hungerFill != null) hungerFill.color = Color.Lerp(Color.red, Color.green, hunger);
        if (thirstFill != null) thirstFill.color = Color.Lerp(Color.red, Color.blue, thirst);
        if (healthFill != null) healthFill.color = Color.Lerp(Color.red, Color.green, health);
    }

    void UpdateHint()
    {
        if (interactionHint == null) return;

        // Здесь можно показывать подсказку о ближайшей еде/воде
        interactionHint.text = "Нажми E для взаимодействия";
    }
}