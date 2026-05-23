using UnityEngine;

public class SurvivalStats : MonoBehaviour
{
    [Header("Creature Type")]
    [SerializeField] private bool isAquatic = false;

    [Header("Hunger Settings")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDepleteRate = 2f;
    [SerializeField] private float hungerDamage = 5f;

    [Header("Thirst Settings")]
    [SerializeField] private float maxThirst = 100f;
    [SerializeField] private float thirstDepleteRate = 3f;
    [SerializeField] private float thirstDamage = 8f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegenRate = 1f;
    [SerializeField] private float regenThreshold = 70f;

    private float currentHunger;
    private float currentThirst;
    private float currentHealth;

    private DinoDiet diet;
    private InteractionSystem interaction;

    void Start()
    {
        currentHunger = maxHunger;
        currentHealth = maxHealth;

        // Водоплавающим жажда всегда фулл
        currentThirst = isAquatic ? maxThirst : maxThirst;

        diet = GetComponent<DinoDiet>();
        interaction = GetComponent<InteractionSystem>();
    }

    void Update()
    {
        DepleteStats();
        RegenerateHealth();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void DepleteStats()
    {
        // Голод всегда тратится
        currentHunger -= hungerDepleteRate * Time.deltaTime;

        // Жажда только у НЕ водоплавающих
        if (!isAquatic)
        {
            currentThirst -= thirstDepleteRate * Time.deltaTime;
        }
        else
        {
            currentThirst = maxThirst;
        }

        // Урон от голода
        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHealth -= hungerDamage * Time.deltaTime;
        }

        // Урон от жажды только наземным
        if (!isAquatic && currentThirst <= 0)
        {
            currentThirst = 0;
            currentHealth -= thirstDamage * Time.deltaTime;
        }
    }

    void RegenerateHealth()
    {
        if (currentHunger > regenThreshold && currentThirst > regenThreshold)
        {
            currentHealth = Mathf.Min(
                currentHealth + healthRegenRate * Time.deltaTime,
                maxHealth
            );
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
        // Водоплавающие не пьют вообще
        if (isAquatic) return;

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

    public bool IsAquatic() => isAquatic;
}