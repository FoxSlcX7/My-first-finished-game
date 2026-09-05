using UnityEngine;

// Событие с двумя параметрами: текущее и максимальное HP
[CreateAssetMenu(fileName = "New Health Event", menuName = "Game Event/Health")]
public class HealthGameEvent : ScriptableObject
{
    private event System.Action<int, int> _eventAction;

    public void Raise(int current, int max)
    {
        _eventAction?.Invoke(current, max);
    }

    public void AddListener(System.Action<int, int> listener)
    {
        _eventAction += listener;
    }

    public void RemoveListener(System.Action<int, int> listener)
    {
        _eventAction -= listener;
    }
}