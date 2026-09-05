using UnityEngine;

[CreateAssetMenu(fileName = "New Game Event", menuName = "Game Event/Base")]
public class GameEvent : ScriptableObject
{
    private event System.Action _eventAction;

    public void Raise()
    {
        _eventAction?.Invoke();
    }

    public void AddListener(System.Action listener)
    {
        _eventAction += listener;
    }

    public void RemoveListener(System.Action listener)
    {
        _eventAction -= listener;
    }
}