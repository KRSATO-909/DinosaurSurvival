using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Transform headPosition;

    [Header("Third Person Settings")]
    public Vector3 thirdPersonOffset = new Vector3(0, 2f, -5f);
    public float thirdPersonSmooth = 10f;
    public float minDistance = 1f;
    public float maxDistance = 8f;
    public float zoomSpeed = 3f;
    public float collisionOffset = 0.5f; // Отступ от стен (увеличено с 0.3)
    public LayerMask cameraCollisionLayers;

    [Header("First Person Settings")]
    public Vector3 firstPersonOffset = new Vector3(0, 1.5f, 0.5f);

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Free Look (ПКМ)")]
    public float freeLookSensitivity = 3f;
    public float returnSpeed = 5f;

    [Header("Camera Switch")]
    public float switchSpeed = 8f;

    private float yaw = 0f;
    private float pitch = 20f;
    private float freeLookYaw = 0f;
    private float freeLookPitch = 0f;
    private bool isFirstPerson = false;
    private float currentZoom;

    // Для плавного переключения
    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (headPosition == null)
        {
            GameObject head = GameObject.Find("HeadCameraPosition");
            if (head != null) headPosition = head.transform;
        }

        if (cameraCollisionLayers == 0)
            cameraCollisionLayers = ~0;

        yaw = player.eulerAngles.y;
        currentZoom = thirdPersonOffset.magnitude;

        // Начальная позиция камеры
        Vector3 startPos = GetThirdPersonTargetPosition();
        transform.position = startPos;
        transform.LookAt(GetLookTarget());
    }

    void LateUpdate()
    {

        // Если игра на паузе – не обрабатываем камеру
        if (Time.timeScale == 0f) return;

        if (player == null) return;

        // Переключение камеры
        if (Input.GetKeyDown(KeyCode.V) && !isTransitioning)
        {
            StartTransition();
        }

        // Зум скроллом (в 3-м лице и не в переходе)
        if (!isFirstPerson && !isTransitioning)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minDistance, maxDistance);
            }
        }

        // Ввод мыши (только если не в переходе)
        if (!isTransitioning)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            bool isFreeLook = Input.GetMouseButton(1) && !isFirstPerson;

            if (isFreeLook)
            {
                freeLookYaw += mouseX * (freeLookSensitivity / mouseSensitivity);
                freeLookPitch -= mouseY * (freeLookSensitivity / mouseSensitivity);
                freeLookPitch = Mathf.Clamp(freeLookPitch, minVerticalAngle, maxVerticalAngle);
            }
            else
            {
                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

                freeLookYaw = Mathf.Lerp(freeLookYaw, 0, returnSpeed * Time.deltaTime);
                freeLookPitch = Mathf.Lerp(freeLookPitch, 0, returnSpeed * Time.deltaTime);
            }

            if (isFirstPerson)
                UpdateFirstPerson();
            else
                UpdateThirdPerson();
        }
        else
        {
            UpdateTransition();
        }
    }

    void StartTransition()
    {
        isTransitioning = true;
        transitionProgress = 0f;
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        isFirstPerson = !isFirstPerson;
    }

    void UpdateTransition()
    {
        transitionProgress += Time.deltaTime * switchSpeed;

        Vector3 targetPos;
        Quaternion targetRot;

        if (isFirstPerson)
        {
            // Переход В первое лицо
            targetPos = GetFirstPersonPosition();
            targetRot = Quaternion.Euler(pitch, yaw, 0);
        }
        else
        {
            // Переход В третье лицо - НЕ используем LookAt в конце
            targetPos = GetThirdPersonTargetPosition();
            Vector3 lookTarget = GetLookTarget();
            targetRot = Quaternion.LookRotation(lookTarget - targetPos);
        }

        // Плавная интерполяция
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(transitionProgress));

        // Если переход почти завершён — просто ставим в целевую позицию
        if (transitionProgress >= 1f)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
            isTransitioning = false;
        }
        else
        {
            transform.position = Vector3.Lerp(transitionStartPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(transitionStartRot, targetRot, t);
        }
    }

    Vector3 GetLookTarget()
    {
        return player.position + Vector3.up * 1.5f;
    }

    Vector3 GetFirstPersonPosition()
    {
        if (headPosition != null)
            return headPosition.position;
        else
            return player.position + firstPersonOffset;
    }

    Vector3 GetThirdPersonTargetPosition()
    {
        float targetYaw = yaw + freeLookYaw;
        float targetPitch = pitch + freeLookPitch;
        Quaternion rotation = Quaternion.Euler(targetPitch, targetYaw, 0);

        Vector3 zoomedOffset = thirdPersonOffset.normalized * currentZoom;
        Vector3 desiredPosition = player.position + rotation * zoomedOffset;

        return CheckCameraCollision(player.position, desiredPosition);
    }

    void UpdateThirdPerson()
    {
        Vector3 targetPos = GetThirdPersonTargetPosition();
        transform.position = Vector3.Lerp(transform.position, targetPos, thirdPersonSmooth * Time.deltaTime);

        Vector3 lookTarget = GetLookTarget();
        transform.LookAt(lookTarget);
    }

    void UpdateFirstPerson()
    {
        transform.position = GetFirstPersonPosition();
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    Vector3 CheckCameraCollision(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float maxDist = Vector3.Distance(from, to);

        // Основной луч
        RaycastHit hit;
        if (Physics.Raycast(from, direction, out hit, maxDist, cameraCollisionLayers))
        {
            float targetDist = Mathf.Max(hit.distance - collisionOffset, minDistance);
            Vector3 newPosition = from + direction * targetDist;

            // Дополнительная проверка: сфера вокруг новой позиции
            // чтобы камера не застревала в геометрии
            float sphereRadius = 0.3f;
            Collider[] colliders = Physics.OverlapSphere(newPosition, sphereRadius, cameraCollisionLayers);

            if (colliders.Length > 0)
            {
                // Если есть коллизии — отодвигаем ещё ближе к игроку
                float extraPush = sphereRadius + collisionOffset;
                newPosition = from + direction * Mathf.Max(targetDist - extraPush, minDistance);
            }

            Debug.DrawLine(from, hit.point, Color.red);
            Debug.DrawRay(newPosition, Vector3.up * 0.5f, Color.green);

            return newPosition;
        }

        Debug.DrawRay(from, direction * maxDist, Color.green);
        return to;
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Показываем целевую позицию
            Gizmos.color = Color.green;
            Vector3 target = GetThirdPersonTargetPosition();
            Gizmos.DrawWireSphere(target, 0.2f);
            Gizmos.DrawLine(player.position, target);

            // Показываем сферу проверки коллизий
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target, 0.3f);

            // Показываем точку взгляда
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(GetLookTarget(), 0.15f);
        }
    }
}