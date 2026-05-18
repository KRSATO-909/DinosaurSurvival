using UnityEngine;
using System.Collections.Generic;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float autoDetectRadius = 10f; // Авто-поиск в радиусе

    private SurvivalStats survival;
    private DinoDiet diet;

    // Списки для кеширования
    private List<FoodSource> nearbyFood = new List<FoodSource>();
    private List<WaterSource> nearbyWater = new List<WaterSource>();

    void Start()
    {
        survival = GetComponent<SurvivalStats>();
        diet = GetComponent<DinoDiet>();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // Собираем ближайшие объекты
        FindNearbyObjects();

        // Ищем ближайшую еду
        FoodSource bestFood = FindBestFood();
        if (bestFood != null)
        {
            if (diet == null || diet.CanEat(bestFood))
            {
                survival?.Eat(bestFood);
                Debug.Log($"Съедено: {bestFood.Type} (+{bestFood.FoodValue})");
                return;
            }
            else
            {
                Debug.Log($"Не могу есть {bestFood.Type} (диета: {diet.DietType})");
            }
        }

        // Ищем ближайшую воду
        WaterSource bestWater = FindBestWater();
        if (bestWater != null)
        {
            survival?.Drink(bestWater);
            Debug.Log($"Выпита вода (+{bestWater.WaterValue})");
            return;
        }

        Debug.Log("Нет доступной еды или воды поблизости");
    }

    void FindNearbyObjects()
    {
        // Ищем все источники в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, autoDetectRadius);

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
    }

    FoodSource FindBestFood()
    {
        FoodSource best = null;
        float minDist = float.MaxValue;

        foreach (FoodSource food in nearbyFood)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < food.InteractionRadius && dist < minDist)
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

    // Показываем подсказку на GUI
    void OnGUI()
    {
        FindNearbyObjects();

        FoodSource bestFood = FindBestFood();
        WaterSource bestWater = FindBestWater();

        string hint = "";

        if (bestFood != null && (diet == null || diet.CanEat(bestFood)))
        {
            hint = $"Нажми E чтобы съесть {bestFood.Type}";
        }
        else if (bestWater != null)
        {
            hint = "Нажми E чтобы выпить воды";
        }

        if (!string.IsNullOrEmpty(hint))
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30), hint);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, autoDetectRadius);
    }
}