using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FlyingSystem : MonoBehaviour
{
    [Header("Takeoff")]
    [SerializeField] private float takeoffJumpHeight = 6f;
    [SerializeField] private string takeoffAnimName = "TakeoffQetz";
    [SerializeField] private string landingAnimName = "LandingQetz";

    [Header("Flight Movement")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float fastFlightSpeed = 16f;
    [SerializeField] private float flightAcceleration = 6f;
    [SerializeField] private float flightRotationSpeed = 8f;

    [Header("Vertical Control")]
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float verticalDamping = 4f;

    private WaterBody[] allWaters;
    private float waterCheckTimer = 0f;

    private CharacterController controller;
    private PlayerMovement groundMovement;
    private Animator animator;

    private enum FlightState { Grounded, TakingOff, Flying, Landing }
    private FlightState state = FlightState.Grounded;

    private float currentFlightSpeed;
    private float verticalVelocity;
    private Vector3 flightDirection;
    private Vector3 lastFlightDirection;

    private float landingTimer;

    private int isFlyingHash;
    private int flightSpeedHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        isFlyingHash = Animator.StringToHash("IsFlying");
        flightSpeedHash = Animator.StringToHash("FlightSpeed");

        flightDirection = transform.forward;
        lastFlightDirection = flightDirection;
    }

    void Update()
    {
        waterCheckTimer -= Time.deltaTime;
        if (waterCheckTimer <= 0f)
        {
            waterCheckTimer = 0.5f;
            allWaters = FindObjectsByType<WaterBody>(FindObjectsSortMode.None);
        }

        switch (state)
        {
            case FlightState.Grounded: GroundedUpdate(); break;
            case FlightState.TakingOff: TakingOffUpdate(); break;
            case FlightState.Flying: FlyingUpdate(); break;
            case FlightState.Landing: LandingUpdate(); break;
        }

        if (animator != null)
        {
            animator.SetBool(isFlyingHash, state != FlightState.Grounded);
            if (state == FlightState.Flying)
                UpdateFlightSpeedParam();
            else if (state == FlightState.Grounded || state == FlightState.Landing)
                animator.SetFloat(flightSpeedHash, 0f);
        }
    }

    // ───── ЗЕМЛЯ ─────
    void GroundedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            StartTakeoff();
        }
    }

    void StartTakeoff()
    {
        state = FlightState.TakingOff;
        if (groundMovement) groundMovement.enabled = false;

        float absGrav = Mathf.Abs(Physics.gravity.y);
        verticalVelocity = Mathf.Sqrt(2f * absGrav * takeoffJumpHeight);
        flightDirection = transform.forward;
        lastFlightDirection = flightDirection;
        currentFlightSpeed = 0f;

        animator.SetBool(isFlyingHash, true);
        animator.Play(takeoffAnimName, 0, 0f);
    }

    // ───── ВЗЛЁТ ─────
    void TakingOffUpdate()
    {
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        Vector3 move = Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);

        if (verticalVelocity <= 0f)
        {
            state = FlightState.Flying;
            verticalVelocity = 0f;
            currentFlightSpeed = 0f;
        }
    }

    // ───── ПОЛЁТ ─────
    void FlyingUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isFreeLook = Input.GetMouseButton(1);

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        bool hasMovementInput = inputDir.magnitude > 0.1f;
        bool hasVerticalInput = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl);

        if (!isFreeLook && hasMovementInput)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, flightRotationSpeed * Time.deltaTime);
            flightDirection = transform.forward;
            lastFlightDirection = flightDirection;
        }
        else if (isFreeLook)
        {
            flightDirection = lastFlightDirection;
        }

        float targetHorizontalSpeed = 0f;
        if (hasMovementInput)
            targetHorizontalSpeed = Input.GetKey(KeyCode.LeftShift) ? fastFlightSpeed : flightSpeed;

        currentFlightSpeed = Mathf.Lerp(currentFlightSpeed, targetHorizontalSpeed, flightAcceleration * Time.deltaTime);
        if (!hasMovementInput && currentFlightSpeed < 0.1f)
            currentFlightSpeed = 0f;

        float desiredVertical = 0f;
        if (Input.GetKey(KeyCode.Space)) desiredVertical = verticalSpeed;
        else if (Input.GetKey(KeyCode.LeftControl)) desiredVertical = -verticalSpeed;

        verticalVelocity = Mathf.Lerp(verticalVelocity, desiredVertical, verticalDamping * Time.deltaTime);
        if (!hasVerticalInput && Mathf.Abs(verticalVelocity) < 0.1f)
            verticalVelocity = 0f;

        Vector3 move = flightDirection * currentFlightSpeed;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // Мягкое скольжение по воде от нижней точки GroundCheck
        if (allWaters != null && groundMovement != null)
        {
            Vector3 groundBottom = groundMovement.GetGroundCheckCenter() + Vector3.down * groundMovement.GetGroundCheckRadius();
            foreach (WaterBody water in allWaters)
            {
                if (groundBottom.y < water.SurfaceY)
                {
                    // Не даём опуститься ниже, чем позволяет нижняя точка сферы
                    float minY = water.SurfaceY + groundMovement.GetGroundCheckRadius();
                    if (transform.position.y < minY)
                    {
                        Vector3 pos = transform.position;
                        pos.y = minY;
                        transform.position = pos;
                        if (verticalVelocity < 0f) verticalVelocity = 0f;
                    }
                    break;
                }
            }
        }

        if (desiredVertical < 0f && IsGrounded())
        {
            StartLanding();
        }
    }

    // ───── ПОСАДКА ─────
    void StartLanding()
    {
        state = FlightState.Landing;
        landingTimer = 0f;
        currentFlightSpeed = 0f;
        verticalVelocity = 0f;

        animator.SetBool(isFlyingHash, false);
        animator.Play(landingAnimName, 0, 0f);
    }

    void LandingUpdate()
    {
        landingTimer += Time.deltaTime;

        bool animDone = false;
        if (animator != null)
        {
            AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
            if (st.IsName(landingAnimName) && st.normalizedTime >= 0.9f)
                animDone = true;
        }
        if (animDone || landingTimer > 2f)
        {
            CompleteLanding();
        }
    }

    void CompleteLanding()
    {
        state = FlightState.Grounded;
        if (groundMovement) groundMovement.enabled = true;
    }

    // ───── АНИМАЦИИ ─────
    void UpdateFlightSpeedParam()
    {
        float horizontalAnimSpeed = 0f;
        if (currentFlightSpeed > flightSpeed * 1.3f)
            horizontalAnimSpeed = 2f;
        else if (currentFlightSpeed > 0.5f)
            horizontalAnimSpeed = 1f;

        float verticalAnimSpeed = 0f;
        float absVertical = Mathf.Abs(verticalVelocity);
        if (absVertical > 1f)
            verticalAnimSpeed = 1f;
        if (absVertical > verticalSpeed * 0.7f)
            verticalAnimSpeed = 2f;

        float targetFs = Mathf.Max(horizontalAnimSpeed, verticalAnimSpeed);
        float currentFs = animator.GetFloat(flightSpeedHash);
        float newFs = Mathf.MoveTowards(currentFs, targetFs, 4f * Time.deltaTime);
        animator.SetFloat(flightSpeedHash, newFs);
    }

    bool IsGrounded()
    {
        if (groundMovement == null) return false;
        return Physics.CheckSphere(groundMovement.GetGroundCheckCenter(), groundMovement.GetGroundCheckRadius(), groundMovement.GetGroundLayer());
    }

    public bool IsFlying() => state != FlightState.Grounded;
}