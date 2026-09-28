using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Панели окончания игры")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    private bool _isGameOver;
    private Transform _playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Transform PlayerTransform
    {
        get
        {
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }
            return _playerTransform;
        }
        private set => _playerTransform = value;
    }

    public void RegisterPlayer(Transform player)
    {
        _playerTransform = player;
    }

    /// <summary>
    /// Поражение игрока
    /// </summary>
    public void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Победа в забеге (после убийства босса 10 этажа)
    /// </summary>
    public void Victory()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        Time.timeScale = 0f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else if (gameOverPanel != null)
        {
            // Фоллбэк, если отдельной панели победы еще нет
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Перезапуск забега заново с 1 этажа
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Возврат в Хаб к алтарю мета-прокачки
    /// Привяжи эту функцию на кнопку 'В Хаб' в UI GameOver и Victory
    /// </summary>
    public void ReturnToHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Hub");
    }
}