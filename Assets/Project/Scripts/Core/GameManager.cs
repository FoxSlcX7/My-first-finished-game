using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;

    private bool _isGameOver;
    private Transform _playerTransform;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void GameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        Time.timeScale = 0f; // Останавливаем время

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
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

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}