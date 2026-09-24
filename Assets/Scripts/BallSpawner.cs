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

        var ballServer = ball.GetComponent<BallServer>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform paddle = client.PlayerObject != null ? client.PlayerObject.transform : null;

            if (paddle == null) continue;

            if (paddle.position.x < 0f) ballServer.LeftPaddle = paddle;
            else ballServer.RightPaddle = paddle;
        }

        ball.Spawn();

        m_ballSpawn = true;
    }
}
