using UnityEngine;

public class Camera1 : MonoBehaviour
{
    [Header("Настройки")]
    public Transform player;        // Перетащи сюда динозавра
    public Vector3 offset = new Vector3(0, 2, -4); // Камера: выше на 2м, сзади на 4м
    public float mouseSensitivity = 3f;
    public float verticalLimit = 60f; // Макс. наклон камеры вверх/вниз

    private float _xRot; // Вертикальный угол камеры

    void LateUpdate()
    {
        if (player == null) return;

        // === Вращение игрока по горизонтали (мышь X) ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        player.Rotate(0, mouseX, 0);

        // === Вращение камеры по вертикали (мышь Y) ===
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        _xRot = Mathf.Clamp(_xRot - mouseY, -verticalLimit, verticalLimit);
        transform.localRotation = Quaternion.Euler(_xRot, 0, 0);

        // === Позиция камеры: всегда сзади игрока ===
        transform.position = player.position + player.TransformDirection(offset);
    }
}
