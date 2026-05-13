using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerMovement movement;

    [Header("Idle Settings")]
    [SerializeField] private float minIdleTime = 3f;
    [SerializeField] private float maxIdleTime = 6f;
    [SerializeField] private string[] idleAnimations = { "Idle1", "Idle2" }; // Имена состояний

    [Header("Attack Settings")]
    [SerializeField] private string[] attackAnimations = { "Bite1", "Bite2" }; // Имена состояний

    private float idleTimer;
    private float currentIdleTime;
    private string currentIdleAnim;
    private bool isAttacking;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        // Начинаем с первой идл анимации
        currentIdleAnim = idleAnimations[0];
        SetRandomIdleTime();
    }

    void Update()
    {
        if (movement == null || animator == null) return;

        HandleMovementAnimations();
        HandleIdleAnimations();
        HandleAttackAnimations();
    }

    void HandleMovementAnimations()
    {
        float speed = 0f;

        if (movement.IsMoving())
        {
            speed = movement.IsSprinting() ? 2f : 1f;
        }

        animator.SetFloat("Speed", speed);
    }

    void HandleIdleAnimations()
    {
        // Меняем идл только если стоим и не атакуем
        if (animator.GetFloat("Speed") > 0.1f || isAttacking)
        {
            idleTimer = 0f;
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= currentIdleTime)
        {
            PlayRandomIdle();
        }
    }

    void HandleAttackAnimations()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            PerformAttack();
        }

        // Проверяем окончание атаки
        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Проверяем что это анимация атаки и она почти закончилась
            bool isAttackAnim = false;
            foreach (string attackAnim in attackAnimations)
            {
                if (stateInfo.IsName(attackAnim))
                {
                    isAttackAnim = true;
                    break;
                }
            }

            if (isAttackAnim && stateInfo.normalizedTime >= 0.9f)
            {
                isAttacking = false;
            }
        }
    }

    void PlayRandomIdle()
    {
        // Выбираем случайную идл анимацию, но не ту же самую
        string newIdle;
        do
        {
            newIdle = idleAnimations[Random.Range(0, idleAnimations.Length)];
        } while (newIdle == currentIdleAnim && idleAnimations.Length > 1);

        currentIdleAnim = newIdle;

        // Проигрываем выбранную анимацию
        animator.Play(currentIdleAnim, 0, 0f);

        // Сбрасываем таймер
        SetRandomIdleTime();
        idleTimer = 0f;
    }

    void PerformAttack()
    {
        if (isAttacking) return;

        // Выбираем случайную атаку
        string attackAnim = attackAnimations[Random.Range(0, attackAnimations.Length)];

        // Устанавливаем триггер и проигрываем
        animator.SetTrigger("Attack");
        animator.Play(attackAnim, 0, 0f);

        isAttacking = true;
    }

    void SetRandomIdleTime()
    {
        currentIdleTime = Random.Range(minIdleTime, maxIdleTime);
    }

    // Публичные методы
    public bool IsAttacking()
    {
        return isAttacking;
    }
}