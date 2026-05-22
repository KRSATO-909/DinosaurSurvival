using UnityEngine;
using System.Collections.Generic;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Keys")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;   // Съесть/попить
    [SerializeField] private KeyCode scentKey = KeyCode.R;     // Нюх

    [Header("Scent Settings")]
    [SerializeField] private float autoDetectRadius = 10f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float scentDuration = 4f;          // Длительность нюха
    [SerializeField] private float scentCooldown = 8f;          // Кулдаун между нюхами
    [SerializeField] private Material rayMaterial;

    private SurvivalStats survival;
    private DinoDiet diet;

    private List<FoodSource> nearbyFood = new List<FoodSource>();
    private List<WaterSource> nearbyWater = new List<WaterSource>();

    private FoodSource bestFood;
    private WaterSource bestWater;

    // Лучи нюха
    private List<LineRenderer> activeRays = new List<LineRenderer>();
    private List<Component> rayTargets = new List<Component>();

    // Таймеры нюха
    private bool scentActive = false;
    private float scentTimer = 0f;
    private float scentCooldownTimer = 0f;

    void Start()
    {
        survival = GetComponent<SurvivalStats>();
        diet = GetComponent<DinoDiet>();
    }

    void Update()
    {
        // Обновление таймеров нюха
        if (scentActive)
        {
            scentTimer -= Time.deltaTime;
            if (scentTimer <= 0f)
            {
                scentActive = false;
                ClearScentRays();
            }
        }
        if (scentCooldownTimer > 0f && !scentActive)
            scentCooldownTimer -= Time.deltaTime;

        // Сканирование окружения
        ScanNearby();

        // Включение/выключение нюха
        if (Input.GetKeyDown(scentKey))
        {
            if (!scentActive && scentCooldownTimer <= 0f)
            {
                StartScent();
            }
        }

        // Обновление лучей (только при активном нюхе)
        if (scentActive)
            UpdateScentRays();
        else
            ClearScentRays();

        // Взаимодействие с объектом
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void ScanNearby()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, autoDetectRadius, interactableLayers);
        nearbyFood.Clear();
        nearbyWater.Clear();

        foreach (Collider col in colliders)
        {
            FoodSource food = col.GetComponent<FoodSource>();
            if (food != null && food.IsAvailable)
                nearbyFood.Add(food);

            WaterSource water = col.GetComponent<WaterSource>();
            if (water != null && water.IsAvailable)
                nearbyWater.Add(water);
        }

        bestFood = FindBestFood();
        bestWater = FindBestWater();
    }

    FoodSource FindBestFood()
    {
        FoodSource best = null;
        float minDist = float.MaxValue;
        foreach (FoodSource food in nearbyFood)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < minDist && dist <= food.InteractionRadius)
            {
                minDist = dist;
                best = food;
            }
        }
        return best;
    }

    WaterSource FindBestWater()
    {
        WaterSource best = null;
        float minDist = float.MaxValue;
        foreach (WaterSource water in nearbyWater)
        {
            if (water.IsInRange(transform.position))
            {
                float dist = Vector3.Distance(transform.position, water.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = water;
                }
            }
        }
        return best;
    }

    void TryInteract()
    {
        if (bestFood != null && diet != null && diet.CanEat(bestFood))
        {
            survival?.Eat(bestFood);
            Debug.Log($"Съедено: {bestFood.Type} (+{bestFood.FoodValue})");
            return;
        }
        if (bestWater != null)
        {
            survival?.Drink(bestWater);
            Debug.Log($"Выпита вода (+{bestWater.WaterValue})");
            return;
        }
    }

    void StartScent()
    {
        scentActive = true;
        scentTimer = scentDuration;
        scentCooldownTimer = scentCooldown;
        Debug.Log("[Scent] Нюх активирован");
    }

    void UpdateScentRays()
    {
        // Собираем все подходящие цели
        List<Component> targets = new List<Component>();
        foreach (FoodSource food in nearbyFood)
        {
            if (diet != null && diet.CanEat(food))
                targets.Add(food);
        }
        foreach (WaterSource water in nearbyWater)
        {
            targets.Add(water);
        }

        // Подгоняем количество лучей
        while (activeRays.Count > targets.Count)
        {
            Destroy(activeRays[activeRays.Count - 1].gameObject);
            activeRays.RemoveAt(activeRays.Count - 1);
            rayTargets.RemoveAt(rayTargets.Count - 1);
        }
        while (activeRays.Count < targets.Count)
        {
            GameObject lineObj = new GameObject("ScentRay");
            lineObj.transform.parent = transform;
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = rayMaterial != null ? rayMaterial : new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.05f;
            lr.endWidth = 0.02f;
            lr.positionCount = 2;
            activeRays.Add(lr);
            rayTargets.Add(null);
        }

        // Обновляем лучи
        for (int i = 0; i < targets.Count; i++)
        {
            LineRenderer lr = activeRays[i];
            lr.enabled = true;
            rayTargets[i] = targets[i];

            Vector3 start = transform.position + Vector3.up * 0.5f;
            Vector3 end = targets[i].transform.position;

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            if (targets[i] is FoodSource food)
                lr.startColor = lr.endColor = GetFoodColor(food.Type);
            else if (targets[i] is WaterSource)
                lr.startColor = lr.endColor = Color.blue;
        }
    }

    void ClearScentRays()
    {
        foreach (LineRenderer lr in activeRays)
        {
            if (lr != null) lr.enabled = false;
        }
    }

    Color GetFoodColor(FoodType type)
    {
        switch (type)
        {
            case FoodType.Meat: return Color.red;
            case FoodType.Grass: return Color.green;
            case FoodType.Fish: return Color.cyan;
            case FoodType.Insect: return Color.yellow;
            case FoodType.Mollusk: return Color.magenta;
            case FoodType.Carrion: return new Color(0.5f, 0.2f, 0.2f);
            default: return Color.white;
        }
    }

    void OnGUI()
    {
        // Если подсказки отключены в настройках – ничего не рисуем
        if (GameManager.Instance == null || !GameManager.Instance.showHints)
            return;

        string hint = "";
        if (bestFood != null && diet != null && diet.CanEat(bestFood))
            hint = $"Нажми E, чтобы съесть {bestFood.Type}";
        else if (bestWater != null)
            hint = "Нажми E, чтобы выпить воды";

        if (!string.IsNullOrEmpty(hint))
        {
            GUI.Label(new Rect(Screen.width / 2 - 120, Screen.height / 2 + 60, 240, 30), hint);
        }

        // Отображение таймера нюха
        if (scentActive)
        {
            GUI.Label(new Rect(Screen.width / 2 - 60, Screen.height / 2 + 80, 120, 30), $"Нюх: {scentTimer:F1}с");
        }
        else if (scentCooldownTimer > 0f)
        {
            GUI.Label(new Rect(Screen.width / 2 - 60, Screen.height / 2 + 80, 120, 30), $"Перезарядка: {scentCooldownTimer:F1}с");
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 60, Screen.height / 2 + 80, 120, 30), "Нажми R для нюха");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, autoDetectRadius);
    }
}