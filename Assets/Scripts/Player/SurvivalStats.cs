using UnityEngine;

public class SurvivalStats : MonoBehaviour
{
    [Header("Hunger Settings")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDepleteRate = 2f; // Единиц в секунду
    [SerializeField] private float hungerDamage = 5f; // Урон когда голод на нуле

    [Header("Thirst Settings")]
    [SerializeField] private float maxThirst = 100f;
    [SerializeField] private float thirstDepleteRate = 3f; // Жажда быстрее чем голод
    [SerializeField] private float thirstDamage = 8f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegenRate = 1f; // Реген при хороших показателях
    [SerializeField] private float regenThreshold = 70f; // Выше этого — регеним

    private float currentHunger;
    private float currentThirst;
    private float currentHealth;

    private DinoDiet diet;
    private InteractionSystem interaction;

    void Start()
    {
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentHealth = maxHealth;

        diet = GetComponent<DinoDiet>();
        interaction = GetComponent<InteractionSystem>();
    }

    void Update()
    {
        // Истощение
        DepleteStats();

        // Регенерация
        RegenerateHealth();

        // Смерть от голода/жажды
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void DepleteStats()
    {
        currentHunger -= hungerDepleteRate * Time.deltaTime;
        currentThirst -= thirstDepleteRate * Time.deltaTime;

        // Урон если на нуле
        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHealth -= hungerDamage * Time.deltaTime;
        }

        if (currentThirst <= 0)
        {
            currentThirst = 0;
            currentHealth -= thirstDamage * Time.deltaTime;
        }
    }

    void RegenerateHealth()
    {
        if (currentHunger > regenThreshold && currentThirst > regenThreshold)
        {
            currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
        }
    }

    public void Eat(FoodSource food)
    {
        if (food == null || !food.IsAvailable) return;
        if (diet != null && !diet.CanEat(food)) return;

        if (food.TryEat(out float value))
        {
            currentHunger = Mathf.Min(currentHunger + value, maxHunger);
            Debug.Log($"Съедено: +{value} голода. Текущий голод: {currentHunger}");
        }
    }

    public void Drink(WaterSource water)
    {
        if (water == null || !water.IsAvailable) return;

        if (water.TryDrink(out float value))
        {
            currentThirst = Mathf.Min(currentThirst + value, maxThirst);
            Debug.Log($"Выпито: +{value} жажды. Текущая жажда: {currentThirst}");
        }
    }

    void Die()
    {
        Debug.Log("Динозавр умер от голода/жажды!");
        // Здесь можно добавить респавн или экран смерти
    }

    // Публичные геттеры для UI
    public float GetHungerPercent() => currentHunger / maxHunger;
    public float GetThirstPercent() => currentThirst / maxThirst;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetCurrentHunger() => currentHunger;
    public float GetCurrentThirst() => currentThirst;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHunger() => maxHunger;
    public float GetMaxThirst() => maxThirst;
    public float GetMaxHealth() => maxHealth;
}