using UnityEngine;

// Событие с параметром SpellSO
[CreateAssetMenu(fileName = "New Spell Event", menuName = "Game Event/Spell")]
public class SpellGameEvent : ScriptableObject
{
    private event System.Action<SpellSO> _eventAction;

    public void Raise(SpellSO spell)
    {
        _eventAction?.Invoke(spell);
    }

    public void AddListener(System.Action<SpellSO> listener)
    {
        _eventAction += listener;
    }

    public void RemoveListener(System.Action<SpellSO> listener)
    {
        _eventAction -= listener;
    }
}