using Unity.Netcode;
using UnityEngine;

public class BallView : NetworkBehaviour
{
    [SerializeField] private BallServer source;
    [SerializeField] private Transform target;

    [SerializeField] private float smoothTime = 0.08f;

    private Vector3 m_vel;

    public Transform GetVisual() => target;

    public override void OnNetworkSpawn()
    {
        if (source == null) source = GetComponent<BallServer>();
        if (target == null) target = transform.Find("Visual");

        target.SetParent(null);
    }

    public override void OnNetworkDespawn()
    {
        if (source == null) Destroy(target.gameObject);
    }

    public void Update()
    {
        if (source == null && target == null) return;

        Vector3 sourcePos = source.NetPos.Value;
        target.position = Vector3.SmoothDamp(target.position, sourcePos, ref m_vel, smoothTime);
    }
}
