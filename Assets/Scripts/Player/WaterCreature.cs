using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WaterCreature : MonoBehaviour
{
    [Header("Swimming Movement")]
    [SerializeField] private float swimSpeed = 10f;
    [SerializeField] private float fastSwimSpeed = 16f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Vertical Control")]
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float verticalDamping = 4f;

    [Header("Water Limits")]
    [SerializeField] private float surfaceOffset = 2f;

    private CharacterController controller;
    private PlayerMovement groundMovement;
    private Animator animator;
    private WaterBody currentWater;

    private float currentSpeed;
    private float verticalVelocity;
    private Vector3 swimDirection;
    private Vector3 lastSwimDirection;

    private int speedHash;
    private int isSwimmingHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        if (groundMovement != null)
            groundMovement.enabled = false;

        speedHash = Animator.StringToHash("Speed");
        isSwimmingHash = Animator.StringToHash("IsSwimming");

        swimDirection = transform.forward;
        lastSwimDirection = swimDirection;

        currentWater = FindObjectOfType<WaterBody>();

        if (currentWater != null)
        {
            Vector3 pos = transform.position;
            float maxY = currentWater.SurfaceY - surfaceOffset;

            if (pos.y > maxY)
                pos.y = maxY;

            transform.position = pos;
        }

        if (animator != null)
        {
            animator.SetBool(isSwimmingHash, true);
            animator.SetFloat(speedHash, 0f);
        }
    }

    void Update()
    {
        if (currentWater == null)
        {
            currentWater = FindObjectOfType<WaterBody>();
            if (currentWater == null) return;
        }

        HandleMovement();
        ClampToWater();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isFreeLook = Input.GetMouseButton(1);

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = (camForward * v + camRight * h).normalized;
        bool hasInput = inputDir.magnitude > 0.1f;

        if (!isFreeLook && hasInput)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );

            swimDirection = transform.forward;
            lastSwimDirection = swimDirection;
        }
        else if (isFreeLook)
        {
            swimDirection = lastSwimDirection;
        }

        // Горизонтальная скорость
        float targetSpeed = 0f;

        if (hasInput)
            targetSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSwimSpeed : swimSpeed;

        currentSpeed = Mathf.Lerp(
            currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        if (!hasInput && currentSpeed < 0.1f)
            currentSpeed = 0f;

        // Вертикальная скорость
        float desiredVertical = 0f;

        if (Input.GetKey(KeyCode.Space))
            desiredVertical = verticalSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            desiredVertical = -verticalSpeed;

        verticalVelocity = Mathf.Lerp(
            verticalVelocity,
            desiredVertical,
            verticalDamping * Time.deltaTime
        );

        if (!Input.GetKey(KeyCode.Space) &&
            !Input.GetKey(KeyCode.LeftControl) &&
            Mathf.Abs(verticalVelocity) < 0.1f)
        {
            verticalVelocity = 0f;
        }

        Vector3 move = swimDirection * currentSpeed;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void ClampToWater()
    {
        if (currentWater == null) return;

        float maxY = currentWater.SurfaceY - surfaceOffset;

        if (transform.position.y > maxY)
        {
            Vector3 pos = transform.position;
            pos.y = maxY;
            transform.position = pos;

            if (verticalVelocity > 0f)
                verticalVelocity = 0f;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float horizontalAnimSpeed = 0f;

        if (currentSpeed > swimSpeed * 1.3f)
            horizontalAnimSpeed = 2f;
        else if (currentSpeed > 0.5f)
            horizontalAnimSpeed = 1f;

        float verticalAnimSpeed = 0f;
        float absVertical = Mathf.Abs(verticalVelocity);

        if (absVertical > 1f)
            verticalAnimSpeed = 1f;
        if (absVertical > verticalSpeed * 0.7f)
            verticalAnimSpeed = 2f;

        float targetFs = Mathf.Max(horizontalAnimSpeed, verticalAnimSpeed);

        float currentFs = animator.GetFloat(speedHash);
        float newFs = Mathf.Lerp(currentFs, targetFs, Time.deltaTime * 10f);

        animator.SetFloat(speedHash, newFs);
    }

    public bool IsActive() => currentWater != null;
}