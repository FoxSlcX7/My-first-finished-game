using UnityEngine;

[CreateAssetMenu(fileName = "New Int Game Event", menuName = "Game Event/Int")]
public class IntGameEvent : ScriptableObject
{
    private event System.Action<int> _eventAction;

    public void Raise(int value)
    {
        _eventAction?.Invoke(value);
    }

    public void AddListener(System.Action<int> listener)
    {
        _eventAction += listener;
    }

    public void RemoveListener(System.Action<int> listener)
    {
        _eventAction -= listener;
    }
}