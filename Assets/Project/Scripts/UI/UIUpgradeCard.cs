using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIUpgradeCard : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image rarityFrame;
    [SerializeField] private TextMeshProUGUI rarityLabel;

    [Header("Защита от случайного клика")]
    [SerializeField] private float inputGraceDelay = 0.35f;

    private Button _button;
    private UpgradeDataSO _data;
    private Action<UpgradeDataSO> _onPick;
    private float _inputUnlockTime;

    private void Awake()
    {
        CacheButton();
        _button.onClick.AddListener(() => _onPick?.Invoke(_data));
    }

    public void Setup(UpgradeDataSO data, Action<UpgradeDataSO> onPick)
    {
        if (data == null)
        {
            Debug.LogWarning("UIUpgradeCard.Setup: data == null, карточка скрыта.");
            gameObject.SetActive(false);
            return;
        }

        _data = data;
        _onPick = onPick;
        gameObject.SetActive(true);

        // Awake мог ещё не пройти, если карточка была неактивна в иерархии.
        // Кнопку кэшируем лениво — не зависим от порядка активации.
        CacheButton();

        if (!gameObject.activeInHierarchy)
            Debug.LogWarning($"UIUpgradeCard '{gameObject.name}': после SetActive(true) НЕ активна в иерархии. " +
                             "Проверь: (1) родитель панели (Canvas/контейнер) активен; " +
                             "(2) в массив cards UIUpgradePanel попали объекты СЦЕНЫ, а не префаб-ассеты.");

        _button.interactable = false;
        _inputUnlockTime = Time.unscaledTime + inputGraceDelay;

        if (icon != null) { icon.sprite = data.icon; icon.enabled = data.icon != null; }
        if (title != null) title.text = data.upgradeName;
        if (description != null) description.text = data.description;

        if (rarityFrame != null) rarityFrame.color = data.rarity.GetColor();
        if (rarityLabel != null)
        {
            rarityLabel.text = data.rarity.GetDisplayName();
            rarityLabel.color = data.rarity.GetColor();
        }
    }

    private void CacheButton()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    private void Update()
    {
        if (_button == null) return;
        if (!_button.interactable && Time.unscaledTime >= _inputUnlockTime)
            _button.interactable = true;
    }
}