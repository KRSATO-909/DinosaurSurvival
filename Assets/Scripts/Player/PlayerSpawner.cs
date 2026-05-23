using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform trexSpawnPoint;
    public Transform quetzSpawnPoint;
    public Transform mosasaurusSpawnPoint;

    [Header("Dino Prefabs")]
    public GameObject trexPrefab;
    public GameObject quetzPrefab;
    public GameObject mosasaurusPrefab;

    void Start()
    {
        SpawnSelectedDino();
    }

    void SpawnSelectedDino()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        GameObject dinoPrefab = null;
        Transform spawnPoint = null;

        switch (GameManager.Instance.selectedDino)
        {
            case GameManager.DinoType.TRex:
                dinoPrefab = trexPrefab;
                spawnPoint = trexSpawnPoint;
                break;
            case GameManager.DinoType.Quetzalcoatlus:
                dinoPrefab = quetzPrefab;
                spawnPoint = quetzSpawnPoint;
                break;
            case GameManager.DinoType.Mosasaurus:
                dinoPrefab = mosasaurusPrefab;
                spawnPoint = mosasaurusSpawnPoint;
                break;
        }

        if (dinoPrefab != null && spawnPoint != null)
        {
            Instantiate(dinoPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("Dino prefab or spawn point not assigned!");
        }
    }
}