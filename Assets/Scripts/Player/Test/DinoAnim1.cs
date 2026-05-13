using UnityEngine;

public class DinoAnim1 : MonoBehaviour
{
    [Header("Ссылка на движение")]
    public DinoMovement1 moveScript; // Перетащи скрипт движения

    [Header("Параметры Animator (должны совпадать!)")]
    public string speedParam = "Speed";      // Float 0..1
    public string groundedParam = "IsGrounded"; // Bool
    public string biteParam = "Attack";        // Trigger

    private Animator _anim;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (moveScript == null || moveScript.IsBiting()) return;

        // Обновляем параметры
        _anim.SetFloat(speedParam, moveScript.GetSpeedRatio(), 0.1f, Time.deltaTime);
        _anim.SetBool(groundedParam, moveScript.IsGrounded());
    }

    // Вызови эту функцию из движения при нажатии укуса
    public void PlayBite()
    {
        _anim.SetTrigger(biteParam);
    }
}
