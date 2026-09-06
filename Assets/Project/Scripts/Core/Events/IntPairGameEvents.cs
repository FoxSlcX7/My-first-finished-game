using UnityEngine;

/// <summary>
/// Универсальное событие с двумя int-параметрами.
/// Используется для: XP (current/required), статистики (kills/deaths) и т.п.
/// </summary>
[CreateAssetMenu(fileName = "New IntPair Event", menuName = "Game Event/IntPair")]
public class IntPairGameEvent : ScriptableObject
{
    private event System.Action<int, int> _eventAction;

    public void Raise(int a, int b) => _eventAction?.Invoke(a, b);

    public void AddListener(System.Action<int, int> listener) => _eventAction += listener;

    public void RemoveListener(System.Action<int, int> listener) => _eventAction -= listener;
}