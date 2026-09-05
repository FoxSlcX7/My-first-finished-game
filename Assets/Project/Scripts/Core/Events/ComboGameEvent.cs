using UnityEngine;

[CreateAssetMenu(fileName = "New Combo Event", menuName = "Game Event/Combo")]
public class ComboGameEvent : ScriptableObject
{
    private event System.Action<SpellComboSO> _eventAction;

    public void Raise(SpellComboSO combo)
    {
        _eventAction?.Invoke(combo);
    }

    public void AddListener(System.Action<SpellComboSO> listener)
    {
        _eventAction += listener;
    }

    public void RemoveListener(System.Action<SpellComboSO> listener)
    {
        _eventAction -= listener;
    }
}