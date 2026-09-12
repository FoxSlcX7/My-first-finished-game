using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool _opened;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_opened || !other.CompareTag("Player")) return;
        _opened = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.gray;

        UpgradeManager.Instance?.GrantUpgradeOffer();
        Debug.Log("[Chest] Открыт: начислен выбор апгрейда.");
    }
}