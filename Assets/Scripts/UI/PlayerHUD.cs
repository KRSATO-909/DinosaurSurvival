using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI dinoTypeText;
    public UnityEngine.UI.Image healthFill;
    public UnityEngine.UI.Image hungerFill;
    public UnityEngine.UI.Image thirstFill;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;

    private SurvivalStats stats;
    private DinoDiet diet;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player)
        {
            stats = player.GetComponent<SurvivalStats>();
            diet = player.GetComponent<DinoDiet>();
            if (dinoTypeText && diet)
                dinoTypeText.text = diet.DietType.ToString();
        }
    }

    void Update()
    {
        if (stats == null) return;

        float healthPct = stats.GetHealthPercent();
        float hungerPct = stats.GetHungerPercent();
        float thirstPct = stats.GetThirstPercent();

        if (healthFill) healthFill.fillAmount = healthPct;
        if (hungerFill) hungerFill.fillAmount = hungerPct;
        if (thirstFill) thirstFill.fillAmount = thirstPct;

        if (healthText) healthText.text = $"{stats.GetCurrentHealth():F0}/{stats.GetMaxHealth()}";
        if (hungerText) hungerText.text = $"{stats.GetCurrentHunger():F0}/{stats.GetMaxHunger()}";
        if (thirstText) thirstText.text = $"{stats.GetCurrentThirst():F0}/{stats.GetMaxThirst()}";
    }
}