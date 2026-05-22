using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerMovement movement;
    private FlyingSystem flying;

    [Header("Idle Animations")]
    [SerializeField] private string[] idleAnimations = { "Idle1", "Idle2" };

    [Header("Attack Animations")]
    [SerializeField] private string[] attackAnimations = { "Bite1", "Bite2" };
    [SerializeField] private float attackMoveSpeedMultiplier = 0.5f;

    [Header("Flight Idle Animations")]
    [SerializeField] private string[] flightIdleAnimations = { "FlightIdle1", "FlightIdle2" };

    [Header("Flight Attack Animations")]
    [SerializeField] private string[] flightAttackAnimations = { "FlightBite1", "FlightBite2" };

    private string currentIdleAnim;
    private bool isAttacking;
    private bool waitingForNextIdle;
    private float attackEndTime;
    private bool wasFlyingLastFrame;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        flying = GetComponent<FlyingSystem>();

        currentIdleAnim = idleAnimations[Random.Range(0, idleAnimations.Length)];
        animator.Play(currentIdleAnim, 0, 0f);
    }

    void Update()
    {
        if (animator == null) return;

        HandleMovementAnimations();
        HandleIdleAnimations();
        HandleAttackAnimations();

        // Замедление при атаке
        if (movement != null)
        {
            movement.SetAttacking(isAttacking);
            movement.SetSpeedMultiplier(isAttacking ? attackMoveSpeedMultiplier : 1f);
        }
    }

    void HandleMovementAnimations()
    {
        float speed = 0f;

        bool isFlying = flying != null && flying.IsFlying();

        if (!isFlying && movement != null && movement.enabled && movement.IsMoving())
        {
            speed = movement.IsSprinting() ? 2f : 1f;
        }

        float currentSpeed = animator.GetFloat("Speed");
        float newSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 10f);
        animator.SetFloat("Speed", newSpeed);

        // Сброс идла при смене режима
        if (isFlying != wasFlyingLastFrame)
        {
            waitingForNextIdle = true;
        }
        wasFlyingLastFrame = isFlying;
    }

    void HandleIdleAnimations()
    {
        bool isFlying = flying != null && flying.IsFlying();
        // В полёте мы не управляем идлами — аниматор сам переключает их через переходы
        if (isFlying)
        {
            waitingForNextIdle = false;
            return;
        }

        bool isMoving = animator.GetFloat("Speed") > 0.1f;

        if (isMoving || isAttacking)
        {
            waitingForNextIdle = true;
            return;
        }

        if (waitingForNextIdle)
        {
            PlayRandomIdle(false);
            waitingForNextIdle = false;
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (IsIdleAnimation(stateInfo) && stateInfo.normalizedTime >= 1f)
        {
            PlayRandomIdle(false);
        }
        else if (!IsIdleAnimation(stateInfo) && !isFlying)
        {
            PlayRandomIdle(false);
        }
    }

    void HandleAttackAnimations()
    {
        bool canAttack = movement != null && movement.CanAttack();
        bool isFlying = flying != null && flying.IsFlying();

        if (Input.GetMouseButtonDown(0) && !isAttacking && canAttack)
        {
            PerformAttack(isFlying);
        }

        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            bool isInAttack = IsAttackAnimation(stateInfo, isFlying);

            if (isInAttack && stateInfo.normalizedTime >= 0.95f)
            {
                isAttacking = false;
                waitingForNextIdle = true;
            }

            if (Time.time > attackEndTime)
            {
                isAttacking = false;
                waitingForNextIdle = true;
            }
        }
    }

    void PlayRandomIdle(bool isFlying)
    {
        string[] animations = isFlying ? flightIdleAnimations : idleAnimations;

        if (animations.Length == 0) return;

        currentIdleAnim = animations[Random.Range(0, animations.Length)];
        animator.Play(currentIdleAnim, 0, 0f);
    }

    void PerformAttack(bool isFlying)
    {
        string[] animations = isFlying ? flightAttackAnimations : attackAnimations;

        if (animations.Length == 0) return;

        string attackAnim = animations[Random.Range(0, animations.Length)];

        if (movement != null)
            movement.OnAttackStarted();

        float animLength = 0.5f;
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == attackAnim)
            {
                animLength = clip.length;
                break;
            }
        }

        attackEndTime = Time.time + animLength + 0.3f;
        animator.Play(attackAnim, 0, 0f);
        isAttacking = true;
    }

    bool IsIdleAnimation(AnimatorStateInfo stateInfo)
    {
        foreach (string idle in idleAnimations)
            if (stateInfo.IsName(idle)) return true;
        foreach (string idle in flightIdleAnimations)
            if (stateInfo.IsName(idle)) return true;
        return false;
    }

    bool IsAttackAnimation(AnimatorStateInfo stateInfo, bool isFlying)
    {
        string[] animations = isFlying ? flightAttackAnimations : attackAnimations;
        foreach (string attack in animations)
            if (stateInfo.IsName(attack)) return true;
        return false;
    }

    public bool IsAttacking() => isAttacking;
}