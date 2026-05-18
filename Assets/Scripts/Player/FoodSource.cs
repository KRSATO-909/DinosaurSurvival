using UnityEngine;
using System.Collections;

public enum FoodType
{
    Meat,
    Grass,
    Fish,
    Insect,
    Mollusk,
    Carrion
}

public class FoodSource : MonoBehaviour
{
    [Header("Food Settings")]
    [SerializeField] private FoodType foodType = FoodType.Meat;
    [SerializeField] private float foodValue = 30f;
    [SerializeField] private bool destroyOnUse = true;

    [Header("Respawn")]
    [SerializeField] private bool enableRespawn = false;
    [SerializeField] private float respawnTime = 60f;

    [Header("Interaction Zone")]
    [SerializeField] private float interactionRadius = 3f;

    private bool isAvailable = true;
    private Coroutine respawnCoroutine;

    // Для респавна — сохраняем оригинальные данные
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private GameObject originalPrefab; // Если это префаб — создаём заново

    public FoodType Type => foodType;
    public float FoodValue => foodValue;
    public bool IsAvailable => isAvailable;
    public float InteractionRadius => interactionRadius;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void OnDestroy()
    {
        // Очищаем корутину при уничтожении
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    public bool TryEat(out float value)
    {
        if (!isAvailable)
        {
            value = 0;
            return false;
        }

        value = foodValue;

        if (destroyOnUse)
        {
            isAvailable = false;

            if (enableRespawn)
            {
                // Запоминаем данные и запускаем корутину
                originalPosition = transform.position;
                originalRotation = transform.rotation;

                // Скрываем объект (выключаем рендерер и коллайдер)
                SetVisualActive(false);

                // Запускаем корутину респавна
                respawnCoroutine = StartCoroutine(RespawnRoutine());
            }
            else
            {
                // Полностью удаляем объект
                Destroy(gameObject);
            }
        }

        return true;
    }

    IEnumerator RespawnRoutine()
    {
        float timer = respawnTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Возрождаем
        isAvailable = true;
        SetVisualActive(true);

        respawnCoroutine = null;
    }

    void SetVisualActive(bool active)
    {
        // Отключаем/включаем рендереры
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = active;
        }

        // Отключаем/включаем коллайдеры
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = active;
        }
    }

    public bool IsInRange(Vector3 point)
    {
        if (!isAvailable) return false;
        return Vector3.Distance(transform.position, point) <= interactionRadius;
    }

    // Принудительная отмена респавна (если нужно)
    public void CancelRespawn()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (isAvailable)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        else
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        Gizmos.DrawSphere(transform.position, interactionRadius);

#if UNITY_EDITOR
        string status = isAvailable ? "Доступно" :
            (enableRespawn ? $"Возрождается..." : "Уничтожено");
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f,
            $"{foodType}\n{foodValue} еды\n{status}");
#endif
    }
}