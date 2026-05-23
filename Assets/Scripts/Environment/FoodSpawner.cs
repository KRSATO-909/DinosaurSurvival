using UnityEngine;
using System.Collections.Generic;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> foodPrefabs;
    [SerializeField] private int maxFoodOnMap = 20;
    [SerializeField] private float spawnInterval = 10f;     // уменьшил для теста
    [SerializeField] private float spawnRadius = 100f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Terrain Settings")]
    [SerializeField] private Terrain terrain;                // ссылка на террейн
    [SerializeField] private float heightOffset = 0.5f;     // насколько выше земли спавнить

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

    private float spawnTimer;
    private int currentFoodCount;
    private float terrainMinX, terrainMaxX, terrainMinZ, terrainMaxZ;

    void Start()
    {
        // Если террейн не назначен, ищем автоматически
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                terrain = FindObjectOfType<Terrain>();
            }
        }

        if (terrain != null)
        {
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;
            terrainMinX = terrainPos.x;
            terrainMaxX = terrainPos.x + terrainSize.x;
            terrainMinZ = terrainPos.z;
            terrainMaxZ = terrainPos.z + terrainSize.z;

            // Ставим спавнер в центр террейна
            transform.position = new Vector3(
                terrainPos.x + terrainSize.x / 2f,
                terrainPos.y + terrainSize.y,
                terrainPos.z + terrainSize.z / 2f
            );

            // Радиус спавна = половина меньшей стороны террейна
            spawnRadius = Mathf.Min(terrainSize.x, terrainSize.z) / 2f;

            if (showDebugLogs)
            {
                Debug.Log($"[FoodSpawner] Террейн найден: {terrain.name}");
                Debug.Log($"[FoodSpawner] Размер террейна: {terrainSize}");
                Debug.Log($"[FoodSpawner] Радиус спавна: {spawnRadius}");
                Debug.Log($"[FoodSpawner] Позиция спавнера: {transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("[FoodSpawner] Террейн не найден! Спавн будет в радиусе от текущей позиции.");
        }

        spawnTimer = 3f; // первый спавн через 3 секунды
        currentFoodCount = CountFoodOnMap();
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            currentFoodCount = CountFoodOnMap();

            if (showDebugLogs)
            {
                Debug.Log($"[FoodSpawner] Еды на карте: {currentFoodCount}/{maxFoodOnMap}");
            }

            if (currentFoodCount < maxFoodOnMap)
            {
                int toSpawn = maxFoodOnMap - currentFoodCount;
                int spawnCount = Mathf.Min(toSpawn, 3); // максимум 3 за раз

                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnFood();
                }
            }

            spawnTimer = spawnInterval;
        }
    }

    int CountFoodOnMap()
    {
        return FindObjectsByType<FoodSource>(FindObjectsSortMode.None).Length;
    }

    void SpawnFood()
    {
        if (foodPrefabs.Count == 0)
        {
            Debug.LogError("[FoodSpawner] Нет префабов еды в списке!");
            return;
        }

        Vector3 randomPos;

        if (terrain != null)
        {
            // Случайная позиция в пределах террейна
            float randomX = Random.Range(terrainMinX, terrainMaxX);
            float randomZ = Random.Range(terrainMinZ, terrainMaxZ);
            randomPos = new Vector3(randomX, terrain.transform.position.y + 100f, randomZ);
        }
        else
        {
            // Случайная позиция в радиусе от спавнера
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            randomPos = transform.position + new Vector3(randomCircle.x, 50f, randomCircle.y);
        }

        // Рейкаст вниз для поиска земли
        RaycastHit hit;
        float rayDistance = 200f;

        if (Physics.Raycast(randomPos, Vector3.down, out hit, rayDistance, groundLayer))
        {
            Vector3 spawnPos = hit.point;
            spawnPos.y += heightOffset;

            // Случайный префаб
            GameObject prefab = foodPrefabs[Random.Range(0, foodPrefabs.Count)];
            GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (showDebugLogs)
            {
                Debug.Log($"[FoodSpawner] Заспавнено: {prefab.name} в позиции {spawnPos}");
            }

            // Визуализация в редакторе
            Debug.DrawRay(randomPos, Vector3.down * hit.distance, Color.green, 5f);
            Debug.DrawLine(spawnPos, spawnPos + Vector3.up * 2f, Color.yellow, 5f);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.DrawRay(randomPos, Vector3.down * rayDistance, Color.red, 5f);
                Debug.LogWarning($"[FoodSpawner] Не удалось найти землю для спавна в позиции {randomPos}");
            }
        }
    }

    // Принудительный спавн (можно вызвать из инспектора)
    public void ForceSpawn()
    {
        if (showDebugLogs) Debug.Log("[FoodSpawner] Принудительный спавн!");
        SpawnFood();
    }

    // Принудительно заполнить всю карту едой
    public void FillMap()
    {
        int toSpawn = maxFoodOnMap - CountFoodOnMap();
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnFood();
        }
        if (showDebugLogs) Debug.Log($"[FoodSpawner] Заспавнено {toSpawn} объектов еды");
    }

    void OnDrawGizmosSelected()
    {
        // Радиус спавна
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Границы террейна
        if (terrain != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 terrainCenter = new Vector3(
                terrainMinX + (terrainMaxX - terrainMinX) / 2f,
                terrain.transform.position.y,
                terrainMinZ + (terrainMaxZ - terrainMinZ) / 2f
            );
            Vector3 terrainSize3D = new Vector3(
                terrainMaxX - terrainMinX,
                0.1f,
                terrainMaxZ - terrainMinZ
            );
            Gizmos.DrawWireCube(terrainCenter, terrainSize3D);
        }
    }
}