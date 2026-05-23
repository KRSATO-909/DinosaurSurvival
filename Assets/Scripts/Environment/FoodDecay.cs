using UnityEngine;

[RequireComponent(typeof(FoodSource))]
public class FoodDecay : MonoBehaviour
{
    [Header("Decay Settings")]
    [SerializeField] private float decayTime = 120f;
    [SerializeField] private Renderer foodRenderer;
    [SerializeField] private float darkenAmount = 0.7f;      // насколько темнеет (0 = без изменений, 1 = полностью чёрный)
    [SerializeField] private GameObject decayEffect;

    [Header("Despawn")]
    [SerializeField] private float despawnDelay = 10f;
    [SerializeField] private bool destroyAfterDecay = true;

    private FoodSource foodSource;
    private float currentTime;
    private float decayProgress;
    private bool isDecayed = false;
    private float despawnTimer;
    private Color originalColor;

    void Start()
    {
        foodSource = GetComponent<FoodSource>();

        if (foodRenderer == null)
            foodRenderer = GetComponentInChildren<Renderer>();

        // Запоминаем ИСХОДНЫЙ цвет материала
        if (foodRenderer != null && foodRenderer.material != null)
        {
            originalColor = foodRenderer.material.color;
        }
        else
        {
            originalColor = Color.white;
        }

        currentTime = decayTime;
        UpdateVisual();
    }

    void Update()
    {
        if (!isDecayed)
        {
            currentTime -= Time.deltaTime;
            decayProgress = 1f - (currentTime / decayTime);
            UpdateVisual();

            if (currentTime <= 0f)
            {
                Decay();
            }
        }
        else if (destroyAfterDecay)
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0f)
            {
                foodSource.TryEat(out _);
                if (foodSource != null && !foodSource.IsAvailable)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    void UpdateVisual()
    {
        if (foodRenderer != null && foodRenderer.material != null)
        {
            // Затемняем исходный цвет пропорционально прогрессу тухления
            Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount * decayProgress);
            foodRenderer.material.color = darkenedColor;
        }
    }

    void Decay()
    {
        isDecayed = true;
        despawnTimer = despawnDelay;
        decayProgress = 1f;

        if (foodRenderer != null)
        {
            Color fullyDark = Color.Lerp(originalColor, Color.black, darkenAmount);
            foodRenderer.material.color = fullyDark;
        }

        if (decayEffect != null)
        {
            Instantiate(decayEffect, transform.position, Quaternion.identity);
        }
    }

    public bool IsDecayed() => isDecayed;
    public float GetDecayProgress() => decayProgress;
}