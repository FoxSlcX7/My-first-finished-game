using UnityEngine;

public class PlayerDamageFeedback : MonoBehaviour
{
    [SerializeField] private ScreenShake screenShake;

    private void OnEnable()
    {
            GameEvents.OnPlayerDamaged.AddListener(HandleDamaged);
    }

    private void OnDisable()
    {
        if (GameEvents.OnPlayerDamaged != null)
        {
            GameEvents.OnPlayerDamaged.RemoveListener(HandleDamaged);
        }
    }

    private void HandleDamaged(int damage)
    {
        if (screenShake != null)
        {
            screenShake.Shake();
        }
    }
}