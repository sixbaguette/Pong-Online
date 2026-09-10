using Unity.Netcode;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject ballPrefab;

    private bool m_ballSpawn;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (m_ballSpawn == true) return;

        if (IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        NetworkObject ball = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);

        ball.Spawn();
    }
}
