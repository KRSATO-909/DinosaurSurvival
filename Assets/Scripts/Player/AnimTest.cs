using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerMovement movement;

    [Header("Idle Animations")]
    [SerializeField] private string[] idleAnimations = { "Idle1", "Idle2" };

    [Header("Attack Animations")]
    [SerializeField] private string[] attackAnimations = { "Bite1", "Bite2" };
    [SerializeField] private float attackMoveSpeedMultiplier = 0.5f;

    private string currentIdleAnim;
    private bool isAttacking;
    private bool waitingForNextIdle;
    private float attackEndTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        currentIdleAnim = idleAnimations[Random.Range(0, idleAnimations.Length)];
        animator.Play(currentIdleAnim, 0, 0f);
    }

    void Update()
    {
        if (movement == null || animator == null) return;

        HandleMovementAnimations();
        HandleIdleAnimations();
        HandleAttackAnimations();

        movement.SetAttacking(isAttacking);
        movement.SetSpeedMultiplier(isAttacking ? attackMoveSpeedMultiplier : 1f);
    }

    void HandleMovementAnimations()
    {
        float speed = 0f;
        if (movement.IsMoving())
            speed = movement.IsSprinting() ? 2f : 1f;

        float currentSpeed = animator.GetFloat("Speed");
        float newSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 10f);
        animator.SetFloat("Speed", newSpeed);
    }

    void HandleIdleAnimations()
    {
        if (animator.GetFloat("Speed") > 0.1f || isAttacking)
        {
            waitingForNextIdle = true;
            return;
        }

        if (waitingForNextIdle)
        {
            PlayRandomIdle();
            waitingForNextIdle = false;
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (IsIdleAnimation(stateInfo) && stateInfo.normalizedTime >= 1f)
        {
            PlayRandomIdle();
        }
    }

    void HandleAttackAnimations()
    {
        // Проверяем атаку через movement.CanAttack()
        if (Input.GetMouseButtonDown(0) && !isAttacking && movement.CanAttack())
        {
            PerformAttack();
        }

        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (IsAttackAnimation(stateInfo) && stateInfo.normalizedTime >= 0.95f)
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

    void PlayRandomIdle()
    {
        currentIdleAnim = idleAnimations[Random.Range(0, idleAnimations.Length)];
        animator.Play(currentIdleAnim, 0, 0f);
    }

    void PerformAttack()
    {
        string attackAnim = attackAnimations[Random.Range(0, attackAnimations.Length)];

        // Сообщаем движению что атака началась (для кулдауна)
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
        return false;
    }

    bool IsAttackAnimation(AnimatorStateInfo stateInfo)
    {
        foreach (string attack in attackAnimations)
            if (stateInfo.IsName(attack)) return true;
        return false;
    }

    public bool IsAttacking() => isAttacking;
}