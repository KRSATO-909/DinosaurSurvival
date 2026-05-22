using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Swimming : MonoBehaviour
{
    [Header("Swimming Settings")]
    [SerializeField] private float swimSpeed = 4f;
    [SerializeField] private float sprintSwimSpeed = 7f;
    [SerializeField] private float rotationSpeed = 5f;

    private CharacterController controller;
    private PlayerMovement groundMovement;
    private Animator animator;
    private WaterBody currentWater;
    private bool isSwimming = false;
    private float enteredWaterY;

    private int isSwimmingHash = Animator.StringToHash("IsSwimming");
    private int swimSpeedHash = Animator.StringToHash("SwimSpeed");


    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isSwimming)
        {
            CheckForWater();
            return;
        }

        HandleSwimming();

        // Выход: верхняя точка сферы GroundCheck стала выше поверхности воды
        if (currentWater == null || GetGroundCheckTop() > currentWater.SurfaceY)
        {
            ExitWater();
        }
    }

    void CheckForWater()
    {
        WaterBody[] waters = FindObjectsByType<WaterBody>(FindObjectsSortMode.None);
        foreach (WaterBody water in waters)
        {
            if (GetGroundCheckTop() < water.SurfaceY)
            {
                EnterWater(water);
                break;
            }
        }
    }

    void EnterWater(WaterBody water)
    {
        currentWater = water;
        isSwimming = true;
        enteredWaterY = transform.position.y;

        if (groundMovement)
        {
            groundMovement.enabled = false;
            groundMovement.ResetVerticalVelocity();
        }

        if (animator)
        {
            animator.SetBool(isSwimmingHash, true);
            animator.SetTrigger("StartSwimming");   // <-- триггер для однократного входа
            animator.SetFloat(swimSpeedHash, 0f);
        }
    }

    void ExitWater()
    {
        isSwimming = false;
        currentWater = null;

        if (groundMovement) groundMovement.enabled = true;

        if (animator)
        {
            animator.SetBool(isSwimmingHash, false);
            animator.SetFloat(swimSpeedHash, 0f);
        }
    }

    void HandleSwimming()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        bool hasInput = inputDir.magnitude > 0.1f;

        if (hasInput)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        float targetSpeed = 0f;
        if (hasInput)
            targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSwimSpeed : swimSpeed;

        Vector3 move = inputDir * (targetSpeed * Time.deltaTime);
        move.y = 0f;
        controller.Move(move);

        // Фиксация высоты
        if (Mathf.Abs(transform.position.y - enteredWaterY) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y = enteredWaterY;
            transform.position = pos;
        }

        // Анимация
        if (animator)
        {
            float animSpeed = 0f;
            if (targetSpeed > swimSpeed * 1.3f) animSpeed = 2f;
            else if (targetSpeed > 0.1f) animSpeed = 1f;
            animator.SetFloat(swimSpeedHash, animSpeed);
        }
    }

    float GetGroundCheckTop()
    {
        Vector3 center = GetGroundCheckCenter();
        float radius = groundMovement ? groundMovement.GetGroundCheckRadius() : 0.2f;
        return center.y + radius;
    }

    Vector3 GetGroundCheckCenter()
    {
        if (groundMovement != null)
            return groundMovement.GetGroundCheckCenter();
        return transform.position + Vector3.down * 1f;
    }

    public bool IsSwimming() => isSwimming;
}