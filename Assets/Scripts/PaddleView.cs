using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class PaddleView : NetworkBehaviour
{
    [SerializeField] private PaddleInput source;
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.08f;

    private float m_velY;

    public override void OnNetworkSpawn()
    {
        if (source == null) source = GetComponent<PaddleInput>();

        if (target == null) target = transform.Find("Visual");

        target.SetParent(null);

        ApplyColor();
    }

    private void ApplyColor()
    {
        if (target == null) return;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = transform.position.x < 0f ? Color.blue : Color.red;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (target != null) Destroy(target.gameObject);
    }

    public Transform GetVisual() => target;

    private void Update()
    {
        if (source == null || target == null) return;

        Vector3 p = target.position;

        p.x = transform.position.x;

        p.y = Mathf.SmoothDamp(p.y, source.transform.position.y, ref m_velY, smoothTime);

        target.position = p;
    }
}
