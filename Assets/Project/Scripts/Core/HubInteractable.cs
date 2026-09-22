using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class HubInteractable : MonoBehaviour
{
    [SerializeField] private GameObject hintObject; // Дочерний объект с текстом "[E] Изучить книги"

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
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (UIMetaUpgradePanel.Instance != null)
        {
            if (UIMetaUpgradePanel.Instance.IsOpen)
                UIMetaUpgradePanel.Instance.Close();
            else
                UIMetaUpgradePanel.Instance.Open();
        }
    }
}