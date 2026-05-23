using UnityEngine;
using System.Collections;

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

    // Смерть
    private bool isDead = false;
    private float deathAlpha = 0f;          // прозрачность красного
    private float deathTextAlpha = 0f;      // прозрачность текста
    private float deathTimer = 0f;

    private const float RED_DURATION = 2f;      // сколько длится покраснение
    private const float TEXT_APPEAR_TIME = 1f;  // когда появляется текст после начала
    private const float TEXT_STAY_TIME = 3f;    // сколько висит текст

    void Start()
    {
        currentHunger = maxHunger;
        currentHealth = maxHealth;
        currentThirst = isAquatic ? maxThirst : maxThirst;

        diet = GetComponent<DinoDiet>();
        interaction = GetComponent<InteractionSystem>();
    }

    void Update()
    {
        if (isDead) return; // мёртвый ничего не делает

        DepleteStats();
        RegenerateHealth();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void DepleteStats()
    {
        currentHunger -= hungerDepleteRate * Time.deltaTime;

        if (!isAquatic)
        {
            currentThirst -= thirstDepleteRate * Time.deltaTime;
        }
        else
        {
            currentThirst = maxThirst;
        }

        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHealth -= hungerDamage * Time.deltaTime;
        }

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
        }
    }

    public void Drink(WaterSource water)
    {
        if (isAquatic) return;
        if (water == null || !water.IsAvailable) return;

        if (water.TryDrink(out float value))
        {
            currentThirst = Mathf.Min(currentThirst + value, maxThirst);
        }
    }

    void Die()
    {
        isDead = true;

        // Отключаем управление
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm) pm.enabled = false;

        FlyingSystem fs = GetComponent<FlyingSystem>();
        if (fs) fs.enabled = false;

        WaterCreature wc = GetComponent<WaterCreature>();
        if (wc) wc.enabled = false;

        AnimationController ac = GetComponent<AnimationController>();
        if (ac) ac.enabled = false;

        // Блокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // Фаза 1: красный экран нарастает 2 секунды
        float elapsed = 0f;
        while (elapsed < RED_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            deathAlpha = Mathf.Lerp(0f, 0.7f, elapsed / RED_DURATION);
            yield return null;
        }
        deathAlpha = 0.7f;

        // Фаза 2: появляется текст "ПОТРАЧЕНО"
        elapsed = 0f;
        while (elapsed < TEXT_APPEAR_TIME)
        {
            elapsed += Time.unscaledDeltaTime;
            deathTextAlpha = Mathf.Lerp(0f, 1f, elapsed / TEXT_APPEAR_TIME);
            yield return null;
        }
        deathTextAlpha = 1f;

        // Фаза 3: висит текст 3 секунды
        yield return new WaitForSecondsRealtime(TEXT_STAY_TIME);

        // Фаза 4: выход из игры
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Рисуем красный экран и текст
    void OnGUI()
    {
        if (!isDead) return;

        // Красный фон
        GUI.color = new Color(1f, 0f, 0f, deathAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        // Текст "ПОТРАЧЕНО"
        if (deathTextAlpha > 0f)
        {
            GUI.color = new Color(1f, 1f, 0f, deathTextAlpha); // жёлтый текст

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = Screen.height / 8;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.yellow;

            GUI.Label(
                new Rect(0, 0, Screen.width, Screen.height),
                "ПОТРАЧЕНО",
                style
            );
        }
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