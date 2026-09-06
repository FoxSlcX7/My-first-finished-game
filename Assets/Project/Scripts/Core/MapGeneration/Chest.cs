using UnityEngine;

public class Chest : MonoBehaviour
{
    private SpellSO[] _rewardPool;
    private bool _opened;

    public void Init(SpellSO[] rewardPool) => _rewardPool = rewardPool;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_opened || !other.CompareTag("Player")) return;
        _opened = true;

        if (_rewardPool != null && _rewardPool.Length > 0)
        {
            SpellSO reward = _rewardPool[Random.Range(0, _rewardPool.Length)];
            Transform player = GameManager.Instance?.PlayerTransform;
            player?.GetComponent<SpellCaster>()?.EquipSpell(reward);
        }

        GetComponent<Collider2D>().enabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray; // заглушка «открыт», заменишь на анимацию
    }
}