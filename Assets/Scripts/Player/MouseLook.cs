using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // Куб игрока
    public Transform headPosition; // Точка для камеры 1-го лица

    [Header("Third Person Settings")]
    public Vector3 thirdPersonOffset = new Vector3(0, 2f, -5f);
    public float thirdPersonSmooth = 10f;
    public float minDistance = 1f; // Минимальная дистанция камеры при коллизии
    public LayerMask cameraCollisionLayers; // Слои, которые блокируют камеру

    [Header("First Person Settings")]
    public Vector3 firstPersonOffset = new Vector3(0, 1.5f, 0.5f);

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Free Look (ПКМ)")]
    public float freeLookSensitivity = 3f;
    public float returnSpeed = 5f;

    private float yaw = 0f;
    private float pitch = 20f;

    private float freeLookYaw = 0f;
    private float freeLookPitch = 0f;

    private bool isFirstPerson = false;
    private float currentCameraDistance; // Текущая реальная дистанция камеры

    void Start()
    {
        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;

        // Если headPosition не назначен, ищем автоматически
        if (headPosition == null)
        {
            GameObject head = GameObject.Find("HeadCameraPosition");
            if (head != null)
                headPosition = head.transform;
        }

        // Если слои не назначены, используем всё кроме Player
        if (cameraCollisionLayers == 0)
            cameraCollisionLayers = ~0; // Всё

        // Запоминаем начальные углы и дистанцию
        yaw = transform.eulerAngles.y;
        currentCameraDistance = thirdPersonOffset.magnitude;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogError("Не назначен player в DinoCamera!");
            return;
        }

        // Переключение камеры по V
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
        }

        // Ввод мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Обработка свободного обзора (ПКМ) только в 3-м лице
        bool isFreeLook = Input.GetMouseButton(1) && !isFirstPerson;

        if (isFreeLook)
        {
            // Свободный обзор
            freeLookYaw += mouseX * (freeLookSensitivity / mouseSensitivity);
            freeLookPitch -= mouseY * (freeLookSensitivity / mouseSensitivity);
            freeLookPitch = Mathf.Clamp(freeLookPitch, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            // Обычный поворот камеры
            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            // Плавный возврат из свободного обзора
            freeLookYaw = Mathf.Lerp(freeLookYaw, 0, returnSpeed * Time.deltaTime);
            freeLookPitch = Mathf.Lerp(freeLookPitch, 0, returnSpeed * Time.deltaTime);
        }

        // Применяем позицию и поворот
        if (isFirstPerson)
        {
            UpdateFirstPerson();
        }
        else
        {
            UpdateThirdPerson(isFreeLook);
        }
    }

    void UpdateThirdPerson(bool isFreeLook)
    {
        // Вычисляем поворот
        float targetYaw = yaw + freeLookYaw;
        float targetPitch = pitch + freeLookPitch;
        Quaternion rotation = Quaternion.Euler(targetPitch, targetYaw, 0);

        // Базовая позиция камеры
        Vector3 desiredPosition = player.position + rotation * thirdPersonOffset;

        // Raycast для проверки коллизий камеры
        Vector3 cameraPosition = CheckCameraCollision(player.position, desiredPosition);

        // Плавное движение камеры
        transform.position = Vector3.Lerp(transform.position, cameraPosition, thirdPersonSmooth * Time.deltaTime);

        // Смотрим на игрока
        Vector3 lookTarget = player.position + Vector3.up * 1.5f;
        transform.LookAt(lookTarget);
    }

    Vector3 CheckCameraCollision(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float maxDistance = Vector3.Distance(from, to);

        RaycastHit hit;

        // Пускаем луч от игрока к желаемой позиции камеры
        if (Physics.Raycast(from, direction, out hit, maxDistance, cameraCollisionLayers))
        {
            // Если попали в препятствие, отодвигаем камеру
            float hitDistance = hit.distance;
            float targetDistance = Mathf.Max(hitDistance - 0.3f, minDistance); // Отступ 0.3 от стены
            Vector3 newPosition = from + direction * targetDistance;

            // Визуализация для отладки
            Debug.DrawLine(from, hit.point, Color.red);
            Debug.DrawRay(from, direction * targetDistance, Color.green);

            return newPosition;
        }

        // Если препятствий нет, возвращаем желаемую позицию
        Debug.DrawRay(from, direction * maxDistance, Color.green);
        return to;
    }

    void UpdateFirstPerson()
    {
        // Позиция камеры у головы с проверкой коллизий
        if (headPosition != null)
        {
            transform.position = headPosition.position;
        }
        else
        {
            transform.position = player.position + firstPersonOffset;
        }

        // Поворот
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    // Для отладки
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position + thirdPersonOffset, 0.3f);

            // Показываем минимальную дистанцию
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistance);
        }
    }
}
