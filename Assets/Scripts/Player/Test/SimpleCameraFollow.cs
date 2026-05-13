using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("🎯 Целевой объект (динозавр)")]
    public Transform target; // Перетащи сюда игрока

    [Header("📐 Настройки позиции")]
    public Vector3 offset = new Vector3(0, 2, -4); // Камера сзади-сверху
    public float followSmoothness = 5f; // Чем больше = плавнее

    [Header("🖱️ Управление мышью")]
    public float mouseSensitivity = 2f;
    public float verticalLimit = 70f; // Макс. наклон вверх/вниз

    [Header("🧪 Отладка")]
    public bool debugLogs = true;

    private float _xRot; // Вертикальное вращение камеры

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("⚠️ CameraFollow: target не назначен! Камера будет статичной.");
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (debugLogs) Debug.Log("🎥 Камера инициализирована");
    }

    void LateUpdate() // LateUpdate — чтобы камера двигалась ПОСЛЕ игрока
    {
        if (target == null) return;

        // === 1. Вращение камеры мышью ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Горизонталь: вращаем ВСЮ сцену вокруг игрока (через target)
        target.Rotate(0, mouseX, 0);

        // Вертикаль: наклоняем ТОЛЬКО камеру
        _xRot = Mathf.Clamp(_xRot - mouseY, -verticalLimit, verticalLimit);
        transform.localRotation = Quaternion.Euler(_xRot, 0, 0);

        // === 2. Плавное следование за целью ===
        Vector3 targetPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothness * Time.deltaTime);

        // === 3. Лог для отладки ===
        if (debugLogs && Time.frameCount % 120 == 0)
            Debug.Log($"📷 Камера: позиция={transform.position:F1}, угол={_xRot:F0}°");
    }

    // Для визуализации в редакторе
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
