using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemachinePlayerBinder : MonoBehaviour
{
    private CinemachineCamera _vcam;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        Bind();
    }

    public void Bind()
    {
        if (_vcam == null) return;

        // Если игрок уже на сцене
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _vcam.Target.TrackingTarget = player.transform;
        }
    }
}