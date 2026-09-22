using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMetaUpgradePanel : MonoBehaviour
{
    public static UIMetaUpgradePanel Instance { get; private set; }
    public bool IsOpen => _canvasGroup != null && _canvasGroup.alpha > 0f;

    [Header("Ссылки")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI runeStonesCurrencyText;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UIMetaUpgradeCard cardPrefab;
    [SerializeField] private Button closeButton;

    private readonly List<UIMetaUpgradeCard> _spawnedCards = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        SetVisible(false);
    }

    private void Start()
    {
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.OnRuneStonesChanged += UpdateCurrencyDisplay;
        }
    }

    private void OnDestroy()
    {
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.OnRuneStonesChanged -= UpdateCurrencyDisplay;
        }
    }

    public void Open()
    {
        SetVisible(true);
        UpdateCurrencyDisplay();
        PopulateCards();
    }

    public void Close()
    {
        SetVisible(false);
    }

    private void PopulateCards()
    {
        if (cardContainer == null || cardPrefab == null || MetaProgressionManager.Instance == null) return;

        // Если карточки уже созданы, просто обновляем их состояние
        if (_spawnedCards.Count > 0)
        {
            foreach (var card in _spawnedCards)
                card.UpdateVisuals();
            return;
        }

        foreach (var upgrade in MetaProgressionManager.Instance.AvailableUpgrades)
        {
            if (upgrade == null) continue;
            UIMetaUpgradeCard card = Instantiate(cardPrefab, cardContainer);
            card.Setup(upgrade, OnCardPurchase);
            _spawnedCards.Add(card);
        }
    }

    private void OnCardPurchase()
    {
        UpdateCurrencyDisplay();
        foreach (var card in _spawnedCards)
            card.UpdateVisuals();
    }

    private void UpdateCurrencyDisplay()
    {
        if (runeStonesCurrencyText != null && MetaProgressionManager.Instance != null)
        {
            runeStonesCurrencyText.text = $"Рунные Камни: {MetaProgressionManager.Instance.RuneStones}";
        }
    }

    private void SetVisible(bool visible)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = visible;
        _canvasGroup.interactable = visible;
    }
}