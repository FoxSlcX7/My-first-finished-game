using UnityEngine;

public class RingVFX : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.4f;
    [SerializeField] private float targetScale = 3f;

    private SpriteRenderer _sr;
    private float _timer;
    private Vector3 _startScale;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _startScale = transform.localScale;
    }

    public void Init(Color color)
    {
        if (_sr != null)
        {
            _sr.color = color;
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / lifetime);

        transform.localScale = Vector3.Lerp(_startScale, Vector3.one * targetScale, t);

        if (_sr != null)
        {
            Color c = _sr.color;
            c.a = 1f - t;
            _sr.color = c;
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}