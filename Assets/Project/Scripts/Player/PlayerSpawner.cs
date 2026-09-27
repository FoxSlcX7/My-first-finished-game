using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Префаб игрока")]
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        SpawnOrPositionPlayer();
    }

    private void SpawnOrPositionPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawner] Не назначен Player Prefab!");
                return;
            }

            player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Если персонаж уже есть на сцене (например, перенесен вручную)
            player.transform.position = transform.position;
        }

        // Обновляем ссылку в GameManager
        GameManager.Instance?.RegisterPlayer(player.transform);

        // Привязываем камеру Cinemachine в текущей сцене
        var vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Target.TrackingTarget = player.transform;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
    }
}