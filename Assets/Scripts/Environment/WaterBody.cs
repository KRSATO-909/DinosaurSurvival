using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterBody : MonoBehaviour
{
    private Collider waterCollider;
    public float SurfaceY => waterCollider.bounds.max.y;

    void Awake()
    {
        waterCollider = GetComponent<Collider>();
        waterCollider.isTrigger = true;
    }

    public bool IsUnderwater(Vector3 point)
    {
        if (waterCollider == null) return false;
        return waterCollider.bounds.Contains(point);
    }

    // Возвращает точку на поверхности воды прямо над заданной позицией
    public Vector3 GetSurfacePoint(Vector3 point)
    {
        return new Vector3(point.x, SurfaceY, point.z);
    }

    // Проверяет, находится ли точка над водой (выше поверхности)
    public bool IsAboveWater(Vector3 point)
    {
        return point.y > SurfaceY;
    }
}