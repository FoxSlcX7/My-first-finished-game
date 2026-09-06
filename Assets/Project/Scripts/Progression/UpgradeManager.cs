using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private UpgradeDataSO[] allUpgrades;
    [SerializeField] private UIUpgradePanel panel;

    [Header("Debug")]
    [SerializeField] private bool debugOfferOnU = true;

    private int _pendingOffers;
    private bool _showing;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => Subscribe();
    private void Start() => Subscribe();

    private void Subscribe()
    {
        if (GameEvents.OnLevelUp == null) return;
        GameEvents.OnLevelUp.RemoveListener(OfferUpgrades);
        GameEvents.OnLevelUp.AddListener(OfferUpgrades);
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUp?.RemoveListener(OfferUpgrades);
    }

    private void Update()
    {
        if (debugOfferOnU && Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
            OfferUpgrades();
    }

    public void OfferUpgrades()
    {
        _pendingOffers++;
        if (!_showing) ShowNext();
    }

    private void ShowNext()
    {
        if (_pendingOffers <= 0 || allUpgrades == null || allUpgrades.Length == 0) return;

        _pendingOffers--;
        _showing = true;
        Time.timeScale = 0f;
        panel.Show(GetRandomChoices(3), Pick);
    }

    private void Pick(UpgradeDataSO upgrade)
    {
        PlayerStats.Instance?.ApplyUpgrade(upgrade);
        panel.Hide();

        if (_pendingOffers > 0)
        {
            ShowNext();
        }
        else
        {
            _showing = false;
            Time.timeScale = 1f;
        }
    }

    private UpgradeDataSO[] GetRandomChoices(int count)
    {
        List<UpgradeDataSO> pool = new List<UpgradeDataSO>(allUpgrades);
        List<UpgradeDataSO> result = new List<UpgradeDataSO>();
        while (result.Count < count && pool.Count > 0)
        {
            int i = Random.Range(0, pool.Count);
            result.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return result.ToArray();
    }
}