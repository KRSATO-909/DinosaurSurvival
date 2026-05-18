using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterSource : MonoBehaviour
{
    [Header("Water Settings")]
    [SerializeField] private float waterValue = 30f;
    [SerializeField] private float useCooldown = 1f;

    private BoxCollider waterCollider;
    private float lastUseTime;

    public float WaterValue => waterValue;
    public bool IsAvailable => true; // Бесконечная вода всегда доступна

    void Start()
    {
        waterCollider = GetComponent<BoxCollider>();
        waterCollider.isTrigger = true;
    }

    public bool TryDrink(out float value)
    {
        value = 0;

        if (Time.time < lastUseTime + useCooldown)
            return false;

        value = waterValue;
        lastUseTime = Time.time;
        return true;
    }

    // Проверка: точка внутри коллайдера?
    public bool IsPointInZone(Vector3 point)
    {
        if (waterCollider == null) return false;

        Vector3 closestPoint = waterCollider.ClosestPoint(point);
        return Vector3.Distance(point, closestPoint) < 0.2f;
    }

    // Для InteractionSystem — игрок рядом с водой?
    public bool IsInRange(Vector3 point)
    {
        if (waterCollider == null) return false;

        Vector3 closestPoint = waterCollider.ClosestPoint(point);
        return Vector3.Distance(point, closestPoint) < 2f;
    }

    void OnDrawGizmos()
    {
        if (waterCollider == null)
            waterCollider = GetComponent<BoxCollider>();

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(waterCollider.center, waterCollider.size);
    }

    void OnDrawGizmosSelected()
    {
        if (waterCollider == null)
            waterCollider = GetComponent<BoxCollider>();

        Gizmos.color = new Color(0f, 0.7f, 1f, 0.6f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(waterCollider.center, waterCollider.size);
    }
}