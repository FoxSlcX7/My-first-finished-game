using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("🚀 Bootstrap: Начинаю инициализацию GameEvents...");
        GameEvents.Initialize();
        Debug.Log("✅ Bootstrap: GameEvents инициализированы!");
    }
}