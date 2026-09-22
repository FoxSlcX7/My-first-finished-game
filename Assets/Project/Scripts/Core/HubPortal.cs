using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class HubPortal : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private GameObject hintObject; // Дочерний объект "[E] Отправиться в подземелье"

    private bool _playerInside;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (hintObject != null) hintObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        if (hintObject != null) hintObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        if (hintObject != null) hintObject.SetActive(false);
    }

    private void Update()
    {
        if (!_playerInside) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}