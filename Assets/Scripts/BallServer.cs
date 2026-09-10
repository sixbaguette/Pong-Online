using Unity.Netcode;
using UnityEngine;

public class BallServer : NetworkBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private Transform visual;

    private Vector2 direction = Vector2.right;

    public NetworkVariable<Vector2> NetPos = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        transform.position = Vector3.zero;
        NetPos.Value = transform.position;
    }

    private void Update()
    {
        if (!IsServer) return;

        Vector2 p = transform.position;
        p += direction * speed * Time.deltaTime;
        transform.position = p;
        NetPos.Value = p;
    }
}
