using UnityEngine;

public class ComboVFXPlayer : MonoBehaviour
{
    [SerializeField] private RingVFX ringPrefab;

    private void OnEnable()
    {
        GameEvents.OnComboCast?.AddListener(OnComboCast);
    }

    private void OnDisable()
    {
        GameEvents.OnComboCast?.RemoveListener(OnComboCast);
    }

    private void OnComboCast(SpellComboSO combo)
    {
        if (ringPrefab == null) return;

        RingVFX ring = Instantiate(ringPrefab, transform.position, Quaternion.identity);
        ring.Init(combo.projectileColor);
    }
}